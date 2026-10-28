using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;
/// <summary>Executive Overview only. No general-purpose DAX engine or database mutations.</summary>
public partial class DashboardService(VtbContext db)
{
 // The removed View column was CASE on employee.employment_status in schema.sql.
 // EXISTS preserves the View row cardinality (including multiple documents per employee).
 public IQueryable<string> KpiEmployeeIds()=>db.Employees.Where(e=>e.EmploymentStatus=="DangLamViec"||e.EmploymentStatus=="TamHoanHDLD").Select(e=>e.EmployeeId);
 public static long? DaxCount(long count)=>count==0?null:count;
 public static double? DaxDivide(long? numerator,long? denominator)=>!denominator.HasValue||denominator==0||!numerator.HasValue?null:(double)numerator.Value/denominator.Value;
 public static string? DocumentLabel(string status)=>status.ToUpperInvariant() switch{"HOP_LE"=>"Hợp lệ","SAP_HET_HAN"=>"Sắp hết hạn","DA_HET_HAN"=>"Đã hết hạn","KHONG_HOP_LE"=>"Không hợp lệ",_=>null};
 public static string? EmployeeLabel(string status)=>status.ToUpperInvariant() switch{"DAY_DU"=>"Đầy đủ","THIEU_HO_SO"=>"Thiếu hồ sơ",_=>null};
 public IQueryable<Employee> EmployeeScope(OverviewFilter f){
  var q=db.Employees.AsNoTracking();
  // Slicer is on employee, not vw_employee_quality.employment_status. Empty means no selection.
  if(f.Statuses.Length>0)q=q.Where(e=>f.Statuses.Contains(e.EmploymentStatus));
  if(f.DivisionIds.Length>0)q=q.Where(e=>db.Departments.Any(d=>d.DepartmentId==e.DepartmentId&&f.DivisionIds.Contains(d.DivisionId)));
  // Power BI hierarchy uses names, so equal department names under the same division select together.
  if(f.DepartmentIds.Length>0)q=q.Where(e=>db.Departments.Any(d=>d.DepartmentId==e.DepartmentId&&db.Departments.Any(selected=>f.DepartmentIds.Contains(selected.DepartmentId)&&selected.DivisionId==d.DivisionId&&selected.DepartmentName==d.DepartmentName)));
  return q;
 }
 public IQueryable<EmployeeQuality> EmployeeMeasureScope(OverviewFilter f){var ids=EmployeeScope(f).Select(x=>x.EmployeeId);return db.EmployeeQualities.AsNoTracking().Where(q=>ids.Contains(q.EmployeeId)&&(q.EmploymentStatus=="DangLamViec"||q.EmploymentStatus=="TamHoanHDLD"));}
 public IQueryable<DocumentQuality> DocumentMeasureScope(OverviewFilter f){var ids=EmployeeScope(f).Select(x=>x.EmployeeId);return db.DocumentQualities.AsNoTracking().Where(q=>ids.Contains(q.EmployeeId)&&q.IsKpiScope==1);}
 public IQueryable<MissingEmployeeRow> MissingQuery(OverviewFilter f){
  var kpiIds=KpiEmployeeIds();
  var ids=EmployeeScope(f).Select(x=>x.EmployeeId);
  // Visual filters, not a new completeness algorithm. employee_id NOT BLANK is explicit in PBIR.
  var q=from e in db.EmployeeQualities.AsNoTracking() join d in db.Departments on e.DepartmentId equals d.DepartmentId
    where ids.Contains(e.EmployeeId)&&e.EmployeeId!=null&&kpiIds.Contains(e.EmployeeId)&&e.EmployeeQualityStatus=="THIEU_HO_SO"
    select new MissingEmployeeRow{EmployeeId=e.EmployeeId,FullName=e.FullName,DepartmentName=d.DepartmentName,Required=e.TotalRequiredDocuments,Present=e.TotalDocumentsPresent,Missing=e.TotalDocumentsMissing,Issue=e.QualityIssue,Codes=e.MissingDocumentCodes};
  if(!string.IsNullOrWhiteSpace(f.Search)){var term=f.Search.Trim();q=q.Where(x=>x.EmployeeId.Contains(term)||x.FullName.Contains(term)||x.DepartmentName.Contains(term));}return q;
 }
 public async Task<OverviewOptions> Options(OverviewFilter f,CancellationToken ct){
  var employees=db.Employees.AsNoTracking();if(f.Statuses.Length>0)employees=employees.Where(x=>f.Statuses.Contains(x.EmploymentStatus));
  var departments=db.Departments.AsNoTracking().Where(d=>employees.Any(e=>e.DepartmentId==d.DepartmentId));
  var divisions=await db.Divisions.AsNoTracking().Where(d=>departments.Any(p=>p.DivisionId==d.DivisionId)).OrderBy(d=>d.DivisionName).Select(d=>new OrganizationOption(d.DivisionId,d.DivisionName,null)).ToListAsync(ct);
  if(f.DivisionIds.Length>0)departments=departments.Where(d=>f.DivisionIds.Contains(d.DivisionId));
  return new(divisions,await departments.OrderBy(d=>d.Division.DivisionName).ThenBy(d=>d.DepartmentName).Select(d=>new OrganizationOption(d.DepartmentId,d.DepartmentName,d.DivisionId)).ToListAsync(ct));
 }
 public async Task<OverviewResult> Overview(OverviewFilter f,CancellationToken ct){
  var employees=EmployeeMeasureScope(f);var docs=DocumentMeasureScope(f);
  var a=await employees.GroupBy(x=>1).Select(g=>new{Total=g.LongCount(),Complete=g.LongCount(x=>x.EmployeeQualityStatus=="DAY_DU"),Missing=g.LongCount(x=>x.EmployeeQualityStatus=="THIEU_HO_SO")}).SingleOrDefaultAsync(ct);
  var b=await docs.GroupBy(x=>1).Select(g=>new{Total=g.LongCount(),Expiring=g.LongCount(x=>x.QualityStatus=="SAP_HET_HAN"),Expired=g.LongCount(x=>x.QualityStatus=="DA_HET_HAN"),Invalid=g.LongCount(x=>x.QualityStatus=="KHONG_HOP_LE")}).SingleOrDefaultAsync(ct);
  var total=DaxCount(a?.Total??0);var complete=DaxCount(a?.Complete??0);
  var byDepartment=await (from e in employees join d in db.Departments on e.DepartmentId equals d.DepartmentId join v in db.Divisions on d.DivisionId equals v.DivisionId
   group e by new{v.DivisionId,d.DepartmentId,Division=v.DivisionName,Department=d.DepartmentName,Status=e.EmployeeQualityStatus} into g
   orderby g.Key.Division,g.Key.Department,g.Key.Status
   select new{g.Key.DivisionId,g.Key.DepartmentId,g.Key.Division,g.Key.Department,g.Key.Status,Count=g.LongCount()}).ToListAsync(ct);
  // Group using calculated SWITCH label. Unknown codes share BLANK, rather than invented labels.
  var grouped=await docs.GroupBy(x=>new{x.DocumentCode,x.QualityStatus}).Select(g=>new{g.Key.DocumentCode,g.Key.QualityStatus,Count=g.LongCount()}).ToListAsync(ct);
  var dist=grouped.GroupBy(x=>DocumentState(x.QualityStatus)).Select(g=>new ChartPoint(DocumentLabel(g.Key)??"(Trống)",DocumentLabel(g.Key),g.Sum(x=>x.Count),Code:g.Key)).OrderByDescending(x=>x.Value).ThenBy(x=>x.Category).ToList();
  var byType=grouped.GroupBy(x=>new{x.DocumentCode,Status=DocumentState(x.QualityStatus)}).Select(g=>new ChartPoint(g.Key.DocumentCode,DocumentLabel(g.Key.Status),g.Sum(x=>x.Count),Code:g.Key.Status)).ToList();
  // Display enrichment only: grouping, sort keys, totals and filter context remain code-based.
  var typeCodes=byType.Select(x=>x.Category).Distinct().ToArray();
  var names=await db.DocumentTypes.AsNoTracking().Where(x=>typeCodes.Contains(x.DocumentCode)).ToDictionaryAsync(x=>x.DocumentCode,x=>x.DocumentName,ct);
  byType=byType.Select(x=>x with{Label=names.TryGetValue(x.Category,out var name)&&!string.IsNullOrWhiteSpace(name)?name:"Chưa có tên loại tài liệu"}).ToList();
  var totals=byType.GroupBy(x=>x.Category).ToDictionary(g=>g.Key,g=>g.Sum(x=>x.Value));byType=byType.OrderByDescending(x=>totals[x.Category]).ThenBy(x=>x.Category,StringComparer.Ordinal).ThenBy(x=>x.Series).ToList();
  var dep=byDepartment.GroupBy(x=>new{x.DivisionId,x.DepartmentId,x.Division,x.Department,Status=EmployeeLabel(x.Status)}).Select(g=>new ChartPoint(g.Key.Department,g.Key.Status,g.Sum(x=>x.Count),g.Key.Division,g.Key.Status=="Đầy đủ"?"DAY_DU":g.Key.Status=="Thiếu hồ sơ"?"THIEU_HO_SO":"BLANK",g.Key.DivisionId,g.Key.DepartmentId)).ToList();
  return new(new(total,complete,DaxDivide(complete,total),DaxCount(a?.Missing??0),DaxCount(b?.Expiring??0),DaxCount(b?.Expired??0),DaxCount(b?.Invalid??0),DaxCount(b?.Total??0)),dep,dist,byType,DateTimeOffset.Now);
 }
 public async Task<MissingEmployeePage> Missing(OverviewFilter f,CancellationToken ct){
  var q=MissingQuery(f);var total=await q.CountAsync(ct);var page=Math.Min(f.Page,Math.Max(1,(int)Math.Ceiling(total/(double)f.PageSize)));
  IOrderedQueryable<MissingEmployeeRow> sorted=f.Sort switch{
   "id"=>f.Desc?q.OrderByDescending(x=>x.EmployeeId):q.OrderBy(x=>x.EmployeeId),"name"=>f.Desc?q.OrderByDescending(x=>x.FullName):q.OrderBy(x=>x.FullName),"department"=>f.Desc?q.OrderByDescending(x=>x.DepartmentName):q.OrderBy(x=>x.DepartmentName),
   "required"=>f.Desc?q.OrderByDescending(x=>x.Required):q.OrderBy(x=>x.Required),"present"=>f.Desc?q.OrderByDescending(x=>x.Present):q.OrderBy(x=>x.Present),"issue"=>f.Desc?q.OrderByDescending(x=>x.Issue):q.OrderBy(x=>x.Issue),"codes"=>f.Desc?q.OrderByDescending(x=>x.Codes):q.OrderBy(x=>x.Codes),_=>f.Desc?q.OrderByDescending(x=>x.Missing):q.OrderBy(x=>x.Missing)};
  return new(await sorted.ThenBy(x=>x.EmployeeId).Skip((page-1)*f.PageSize).Take(f.PageSize).ToListAsync(ct),total,page,f.PageSize);
 }
}

