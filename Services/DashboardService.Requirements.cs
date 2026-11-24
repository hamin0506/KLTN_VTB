using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;
public partial class DashboardService {
 // Count one applicable employee/type pair. Any current document means the pair is present,
 // regardless of that document's quality; never use employee_quality_status for this drill.
 public static IQueryable<RequirementPair> RequirementPairs(IQueryable<DocumentRequirement> source)=>
  source.Where(r=>r.EmploymentStatus=="DangLamViec"||r.EmploymentStatus=="TamHoanHDLD")
   .GroupBy(r=>new{r.EmployeeId,r.DocumentTypeId})
   .Select(g=>new RequirementPair{EmployeeId=g.Key.EmployeeId,DocumentTypeId=g.Key.DocumentTypeId,Missing=g.Min(r=>r.RequirementStatus=="THIEU_HO_SO"?1:0)});
 public async Task ValidateRequirementDrill(RequirementDrillFilter f,CancellationToken ct){
  var divisionIds=f.DivisionIds.Concat(f.DrillDivisionId.HasValue?[f.DrillDivisionId.Value]:[]).Distinct().ToArray();
  if(await db.Divisions.CountAsync(x=>divisionIds.Contains(x.DivisionId),ct)!=divisionIds.Length)throw new InvalidDataException("Đơn vị không tồn tại.");
  var departmentIds=f.DepartmentIds.Concat(f.DrillDepartmentId.HasValue?[f.DrillDepartmentId.Value]:[]).Distinct().ToArray();
  var deps=await db.Departments.Where(x=>departmentIds.Contains(x.DepartmentId)).Select(x=>new{x.DepartmentId,x.DivisionId}).ToListAsync(ct);
  if(deps.Count!=departmentIds.Length||deps.Any(x=>f.DivisionIds.Length>0&&!f.DivisionIds.Contains(x.DivisionId)))throw new InvalidDataException("Phòng ban không thuộc phạm vi đơn vị đã chọn.");
  if(f.DrillDivisionId.HasValue&&f.DivisionIds.Length>0&&!f.DivisionIds.Contains(f.DrillDivisionId.Value))throw new InvalidDataException("Đơn vị ngoài bộ lọc hiện tại.");
  if(f.DrillDepartmentId.HasValue&&(f.DepartmentIds.Length>0&&!f.DepartmentIds.Contains(f.DrillDepartmentId.Value)||f.DrillDivisionId.HasValue&&!deps.Any(x=>x.DepartmentId==f.DrillDepartmentId&&x.DivisionId==f.DrillDivisionId)))throw new InvalidDataException("Phòng ban không thuộc đơn vị hoặc bộ lọc đang xem.");
  var typeIds=f.TypeIds.Concat(f.DrillTypeId.HasValue?[f.DrillTypeId.Value]:[]).Distinct().ToArray();
  if(await db.DocumentTypes.CountAsync(x=>typeIds.Contains(x.DocumentTypeId),ct)!=typeIds.Length)throw new InvalidDataException("Loại tài liệu không tồn tại.");
  if(f.DrillTypeId.HasValue&&!await ReportTypes(f).AnyAsync(x=>x.DocumentTypeId==f.DrillTypeId,ct))throw new InvalidDataException("Loại tài liệu ngoài bộ lọc đang xem.");
 }
 public IQueryable<RequirementDrillRow> RequirementDrillQuery(RequirementDrillFilter f){
  // Exact IDs, unlike the legacy name-based employee hierarchy selection.
  var employees=db.Employees.AsNoTracking().Where(e=>e.EmploymentStatus=="DangLamViec"||e.EmploymentStatus=="TamHoanHDLD");
  if(f.EmployeeIds.Length>0)employees=employees.Where(e=>f.EmployeeIds.Contains(e.EmployeeId));
  if(f.DepartmentIds.Length>0)employees=employees.Where(e=>f.DepartmentIds.Contains(e.DepartmentId));
  var pairs=RequirementPairs(db.DocumentRequirements.AsNoTracking());
  var q=from p in pairs join e in employees on p.EmployeeId equals e.EmployeeId
   join d in db.Departments on e.DepartmentId equals d.DepartmentId join v in db.Divisions on d.DivisionId equals v.DivisionId
   join t in ReportTypes(f) on p.DocumentTypeId equals t.DocumentTypeId
   select new RequirementDrillRow{EmployeeId=e.EmployeeId,FullName=e.FullName,DivisionId=v.DivisionId,DivisionName=v.DivisionName,DepartmentId=d.DepartmentId,DepartmentName=d.DepartmentName,DocumentTypeId=t.DocumentTypeId,DocumentName=t.DocumentName,Missing=p.Missing};
  if(f.DivisionIds.Length>0)q=q.Where(x=>f.DivisionIds.Contains(x.DivisionId));
  if(f.DrillDivisionId.HasValue)q=q.Where(x=>x.DivisionId==f.DrillDivisionId);
  if(f.DrillDepartmentId.HasValue)q=q.Where(x=>x.DepartmentId==f.DrillDepartmentId);
  if(f.DrillTypeId.HasValue)q=q.Where(x=>x.DocumentTypeId==f.DrillTypeId);
  return q;
 }
 public static IQueryable<RequirementChartPoint> RequirementChartQuery(IQueryable<RequirementDrillRow> rows,string level){
  var keys=rows.Select(x=>new{Id=level=="types"?x.DocumentTypeId:level=="departments"?x.DepartmentId:x.DivisionId,Name=level=="types"?x.DocumentName:level=="departments"?x.DepartmentName:x.DivisionName,x.EmployeeId,x.Missing});
  return keys.GroupBy(x=>new{x.Id,x.Name}).Select(g=>new RequirementChartPoint{Id=g.Key.Id,Name=g.Key.Name,Total=g.LongCount(),Missing=g.Sum(x=>(long)x.Missing),Employees=g.Where(x=>x.Missing==1).Select(x=>x.EmployeeId).Distinct().Count()});
 }
 public async Task<List<RequirementChartPoint>> RequirementChart(RequirementDrillFilter f,CancellationToken ct)=>await RequirementChartQuery(RequirementDrillQuery(f),f.Level).OrderByDescending(x=>x.Missing).ThenBy(x=>x.Name).ThenBy(x=>x.Id).ToListAsync(ct);
 public IQueryable<RequirementPerson> RequirementPeopleQuery(RequirementDrillFilter f){
  var q=RequirementDrillQuery(f).Where(x=>x.Missing==1);
  if(!string.IsNullOrWhiteSpace(f.Search)){var term=f.Search.Trim();q=q.Where(x=>x.EmployeeId.Contains(term)||x.FullName.Contains(term)||x.DepartmentName.Contains(term));}
  return q.GroupBy(x=>new{x.EmployeeId,x.FullName,x.DepartmentName}).Select(g=>new RequirementPerson{EmployeeId=g.Key.EmployeeId,FullName=g.Key.FullName,Department=g.Key.DepartmentName,Missing=g.Count()});
 }
 public async Task<RequirementPeoplePage> RequirementPeople(RequirementDrillFilter f,CancellationToken ct){
  var q=RequirementPeopleQuery(f);var total=await q.CountAsync(ct);var missing=await q.SumAsync(x=>(long?)x.Missing,ct)??0;
  var page=Math.Min(f.Page,Math.Max(1,(int)Math.Ceiling(total/(double)f.PageSize)));
  IOrderedQueryable<RequirementPerson> ordered=f.DetailSort switch{
   "id"=>f.Desc?q.OrderByDescending(x=>x.EmployeeId):q.OrderBy(x=>x.EmployeeId),
   "name"=>f.Desc?q.OrderByDescending(x=>x.FullName):q.OrderBy(x=>x.FullName),
   "department"=>f.Desc?q.OrderByDescending(x=>x.Department):q.OrderBy(x=>x.Department),
   _=>f.Desc?q.OrderBy(x=>x.Missing):q.OrderByDescending(x=>x.Missing)};
  var rows=await ordered.ThenBy(x=>x.EmployeeId).Skip((page-1)*f.PageSize).Take(f.PageSize).ToListAsync(ct);
  var ids=rows.Select(x=>x.EmployeeId).ToArray();
  var details=await RequirementDrillQuery(f).Where(x=>x.Missing==1&&ids.Contains(x.EmployeeId)).Select(x=>new{x.EmployeeId,x.DocumentTypeId,x.DocumentName}).ToListAsync(ct);
  var types=details.Select(x=>x.DocumentTypeId).Distinct().ToArray();
  var order=await db.DocumentTypes.Where(x=>types.Contains(x.DocumentTypeId)).InRequirementOrder(db).Select(x=>x.DocumentTypeId).ToListAsync(ct);
  var rank=order.Select((id,index)=>new{id,index}).ToDictionary(x=>x.id,x=>x.index);
  foreach(var row in rows)row.Documents=details.Where(x=>x.EmployeeId==row.EmployeeId).OrderBy(x=>rank[x.DocumentTypeId]).Select(x=>x.DocumentName).ToList();
  return new(rows,total,missing,page,f.PageSize);
 }
}
