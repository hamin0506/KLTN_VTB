using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;
public partial class DashboardService {
 public static string? ContractLabel(string code)=>code.ToUpperInvariant() switch{"HOP_LE"=>"Hợp lệ","THIEU_HDXDTH1"=>"Thiếu HĐXĐTH1","CAN_KY_KET_TIEP"=>"Cần ký kết tiếp","KHONG_CANH_BAO"=>"Không cảnh báo",_=>null};
 public static double RequirementRate(long total,long missing)=>total==0?0:(double)(total-missing)/total;
 public IQueryable<DocumentType> ReportTypes(ReportFilter f){var q=db.DocumentTypes.AsNoTracking();if(f.TypeIds.Length>0)q=q.Where(x=>f.TypeIds.Contains(x.DocumentTypeId));if(f.Groups.Length>0)q=q.Where(x=>f.Groups.Contains(x.DocumentGroup));return q;}
 public IQueryable<Employee> ReportEmployees(ReportFilter f){var q=EmployeeScope(f.Scope());if(f.EmployeeIds.Length>0)q=q.Where(x=>f.EmployeeIds.Contains(x.EmployeeId));if(f.TypeIds.Length+f.Groups.Length>0){var types=ReportTypes(f).Select(x=>x.DocumentTypeId);q=q.Where(e=>db.Documents.Any(d=>d.EmployeeId==e.EmployeeId&&types.Contains(d.DocumentTypeId)));}return q;}
 public IQueryable<DocumentQuality> ReportDocuments(ReportFilter f){var ids=ReportEmployees(f).Select(x=>x.EmployeeId);var q=db.DocumentQualities.AsNoTracking().Where(x=>ids.Contains(x.EmployeeId));if(f.TypeIds.Length+f.Groups.Length>0){var codes=ReportTypes(f).Select(x=>x.DocumentCode);q=q.Where(x=>codes.Contains(x.DocumentCode));}return q;}
 public IQueryable<DocumentRequirement> ReportRequirements(ReportFilter f){
  var q=db.DocumentRequirements.AsNoTracking();
  // Only the document_id relationship is active. Without an external filter preserve NULL document_id rows.
  if(f.HasScope){var employees=ReportEmployees(f).Select(x=>x.EmployeeId);var types=ReportTypes(f).Select(x=>x.DocumentTypeId);var docs=db.Documents.Where(x=>employees.Contains(x.EmployeeId));if(f.TypeIds.Length+f.Groups.Length>0)docs=docs.Where(x=>types.Contains(x.DocumentTypeId));var ids=docs.Select(x=>(int?)x.DocumentId);q=q.Where(x=>ids.Contains(x.DocumentId));}return q;
 }
 public IQueryable<LaborContractCheck> ReportContracts(ReportFilter f){var ids=ReportEmployees(f).Select(x=>x.EmployeeId);return db.LaborContractChecks.AsNoTracking().Where(x=>ids.Contains(x.EmployeeId));}
 public IQueryable<ReportOrganizationRow> ProblemOrganizationQuery(ReportFilter f)=>
  from r in ReportRequirements(f)
  join e in db.Employees on r.EmployeeId equals e.EmployeeId
  join d in db.Departments on e.DepartmentId equals d.DepartmentId
  join v in db.Divisions on d.DivisionId equals v.DivisionId
  group r by new{v.DivisionId,v.DivisionName,d.DepartmentId,d.DepartmentName} into g
  select new ReportOrganizationRow{DivisionId=g.Key.DivisionId,DivisionName=g.Key.DivisionName,DepartmentId=g.Key.DepartmentId,DepartmentName=g.Key.DepartmentName,
   Total=g.LongCount(x=>x.EmploymentStatus=="DangLamViec"||x.EmploymentStatus=="TamHoanHDLD"),
   Missing=g.LongCount(x=>(x.EmploymentStatus=="DangLamViec"||x.EmploymentStatus=="TamHoanHDLD")&&x.RequirementStatus=="THIEU_HO_SO")};
 public async Task<ReportData> ProblemData(ReportFilter f,CancellationToken ct){
  var scope=ReportRequirements(f);var active=scope.Where(x=>x.EmploymentStatus=="DangLamViec"||x.EmploymentStatus=="TamHoanHDLD");
  var total=await active.LongCountAsync(ct);var missing=await active.LongCountAsync(x=>x.RequirementStatus=="THIEU_HO_SO",ct);
  var first=await active.Where(x=>x.RequirementStatus=="THIEU_HO_SO").GroupBy(x=>x.DocumentName).Select(g=>new{ Name=g.Key,Value=g.LongCount()}).OrderByDescending(x=>x.Value).ThenBy(x=>x.Name).ToListAsync(ct);
  var organization=await ProblemOrganizationQuery(f).ToListAsync(ct);
  var second=organization.GroupBy(x=>new{x.DivisionId,x.DivisionName}).Select(g=>new ReportPoint(g.Key.DivisionName,RequirementRate(g.Sum(x=>x.Total),g.Sum(x=>x.Missing)),g.Key.DivisionId.ToString())).OrderByDescending(x=>x.Value).ThenBy(x=>x.Name).ToList();
  return new(new(){["total"]=DaxCount(total),["rate"]=RequirementRate(total,missing),["missing"]=DaxCount(missing)},first.Select(x=>new ReportPoint(x.Name,x.Value)).ToList(),second,organization);
 }
 public IQueryable<ContractOrganizationRow> ContractOrganizationQuery(ReportFilter f)=>
  from c in ReportContracts(f)
  join e in db.Employees on c.EmployeeId equals e.EmployeeId
  join d in db.Departments on e.DepartmentId equals d.DepartmentId
  join v in db.Divisions on d.DivisionId equals v.DivisionId
  where c.LaborContractStatus=="CAN_KY_KET_TIEP"||c.LaborContractStatus=="THIEU_HDXDTH1"
  group e by new{v.DivisionId,v.DivisionName,d.DepartmentId,d.DepartmentName} into g
  select new ContractOrganizationRow{DivisionId=g.Key.DivisionId,DivisionName=g.Key.DivisionName,DepartmentId=g.Key.DepartmentId,DepartmentName=g.Key.DepartmentName,Count=g.Select(e=>e.EmployeeId).Distinct().Count()};
 public async Task<ReportData> ContractData(ReportFilter f,CancellationToken ct){
  var q=ReportContracts(f);var valid=await q.LongCountAsync(x=>x.EmployeeId!=null&&x.LaborContractStatus=="HOP_LE",ct);var missing=await q.LongCountAsync(x=>x.EmployeeId!=null&&x.LaborContractStatus=="THIEU_HDXDTH1",ct);var next=await q.LongCountAsync(x=>x.EmployeeId!=null&&x.LaborContractStatus=="CAN_KY_KET_TIEP",ct);
  var expiring=await ReportDocuments(f).LongCountAsync(x=>x.IsKpiScope==1&&x.QualityStatus=="SAP_HET_HAN"&&(x.DocumentCode=="HDTHUVIEC"||x.DocumentCode=="HDXDTH1"||x.DocumentCode=="HDXDTH2"),ct);
  var organization=await ContractOrganizationQuery(f).ToListAsync(ct);
  var first=organization.OrderByDescending(x=>x.Count).ThenBy(x=>x.DepartmentName).ThenBy(x=>x.DepartmentId).Select(x=>new ReportPoint(x.DepartmentName,x.Count,x.DepartmentId.ToString())).ToList();
  var second=await q.Where(x=>x.EmploymentStatus!=null&&x.EmploymentStatus!="ChamDutHDLD"&&x.EmploymentStatus!="NghiHuu").GroupBy(x=>x.LaborContractStatus).Select(g=>new{Code=g.Key,Value=g.LongCount(x=>x.EmployeeId!=null)}).ToListAsync(ct);
  return new(new(){["valid"]=DaxCount(valid),["missing"]=DaxCount(missing),["next"]=DaxCount(next),["expiring"]=DaxCount(expiring)},first,second.GroupBy(x=>ContractLabel(x.Code)).Select(g=>new ReportPoint(g.Key??"(Trống)",DaxCount(g.Sum(x=>x.Value)),g.Key==null?"BLANK":g.First().Code.ToUpperInvariant())).OrderByDescending(x=>x.Value).ToList(),ContractOrganization:organization);
 }
 public IQueryable<ReportRow> ReportTableQuery(ReportFilter f){
  IQueryable<ReportRow> q;
  if(f.Kind=="contractIssues"||f.Kind=="contractDetail"){
   var contracts=ReportContracts(f);
   if(f.Kind=="contractIssues")contracts=contracts.Where(x=>x.LaborContractStatus!=null&&x.LaborContractStatus!="HOP_LE"&&x.LaborContractStatus!="KHONG_CANH_BAO");
   else if(f.State=="BLANK")contracts=contracts.Where(x=>x.LaborContractStatus!="HOP_LE"&&x.LaborContractStatus!="THIEU_HDXDTH1"&&x.LaborContractStatus!="CAN_KY_KET_TIEP"&&x.LaborContractStatus!="KHONG_CANH_BAO");
   else contracts=contracts.Where(x=>x.LaborContractStatus==f.State);
   q=from c in contracts join e in db.Employees on c.EmployeeId equals e.EmployeeId select new ReportRow{Code=null,Document=null,Group=null,IssueDate=null,ExpiryDate=null,Days=null,Reason=null,Id=c.EmployeeId,Name=c.FullName,Department=c.DepartmentName,Employment=e.EmploymentStatus=="DangLamViec"?"Đang làm việc":e.EmploymentStatus=="TamHoanHDLD"?"Tạm hoãn HĐLĐ":e.EmploymentStatus=="ChamDutHDLD"?"Chấm dứt HĐLĐ":e.EmploymentStatus=="NghiHuu"?"Nghỉ hưu":"",Status=c.LaborContractStatus=="HOP_LE"?"Hợp lệ":c.LaborContractStatus=="THIEU_HDXDTH1"?"Thiếu HĐXĐTH1":c.LaborContractStatus=="CAN_KY_KET_TIEP"?"Cần ký kết tiếp":c.LaborContractStatus=="KHONG_CANH_BAO"?"Không cảnh báo":null};
  }else{
   var docs=ReportDocuments(f);
   if(f.Kind=="problem")docs=docs.Where(x=>x.DocumentCode!="HDXDTH1"&&x.DocumentCode!="HDXDTH2"&&x.DocumentCode!="HDTHUVIEC"&&x.QualityStatus!="HOP_LE");
   else if(f.Kind=="expiring")docs=docs.Where(x=>x.QualityStatus=="SAP_HET_HAN");
   else docs=docs.Where(x=>x.QualityStatus=="DA_HET_HAN"&&(x.DocumentCode=="HDKXDTH"||x.DocumentCode=="HDTHUVIEC"||x.DocumentCode=="HDXDTH1"||x.DocumentCode=="HDXDTH2"));
   var problem=f.Kind=="problem";var expiring=f.Kind=="expiring";
   q=docs.Select(x=>new ReportRow{Department=null,Employment=null,Id=x.EmployeeId,Name=x.FullName,Document=x.DocumentName,Code=problem?null:x.DocumentCode,Group=problem?x.DocumentGroup:null,IssueDate=expiring?null:x.IssueDate,ExpiryDate=x.ExpiryDate,Days=x.ExpiryDays,Reason=problem?x.InvalidReason:null,Status=expiring?null:x.QualityStatus=="HOP_LE"?"Hợp lệ":x.QualityStatus=="SAP_HET_HAN"?"Sắp hết hạn":x.QualityStatus=="DA_HET_HAN"?"Đã hết hạn":x.QualityStatus=="KHONG_HOP_LE"?"Không hợp lệ":null});
  }
  q=q.Distinct();if(!string.IsNullOrWhiteSpace(f.Search)){var term=f.Search.Trim();q=q.Where(x=>x.Id.Contains(term)||x.Name.Contains(term)||(x.Document!=null&&x.Document.Contains(term))||(x.Department!=null&&x.Department.Contains(term)));}return q;
 }
 public async Task<ReportTable> ReportRows(ReportFilter f,CancellationToken ct){
  var q=ReportTableQuery(f);var total=await q.CountAsync(ct);var page=Math.Min(f.Page,Math.Max(1,(int)Math.Ceiling(total/(double)f.PageSize)));
  return new(await ReportOrderQuery(f).Skip((page-1)*f.PageSize).Take(f.PageSize).ToListAsync(ct),total,page,f.PageSize);
 }
 public IOrderedQueryable<ReportRow> ReportOrderQuery(ReportFilter f){
  var q=ReportTableQuery(f);
  var key=f.Sort==""?f.Kind=="expiring"?"ExpiryDate":f.Kind=="contractDetail"?"Name":f.Kind=="contractIssues"?"Status":"Days":f.Sort;var desc=f.Sort==""?f.Kind=="expiredContracts":f.Desc;
  var parameter=Expression.Parameter(typeof(ReportRow),"row");var property=Expression.Property(parameter,key);var lambda=Expression.Lambda(property,parameter);
  var sorted=(IOrderedQueryable<ReportRow>)q.Provider.CreateQuery<ReportRow>(Expression.Call(typeof(Queryable),desc?"OrderByDescending":"OrderBy",[typeof(ReportRow),property.Type],q.Expression,Expression.Quote(lambda)));
  foreach(var tie in ReportFilter.Columns(f.Kind).Where(x=>x!=key)){
   var member=Expression.Property(parameter,tie);var order=Expression.Lambda(member,parameter);
   sorted=(IOrderedQueryable<ReportRow>)q.Provider.CreateQuery<ReportRow>(Expression.Call(typeof(Queryable),"ThenBy",[typeof(ReportRow),member.Type],sorted.Expression,Expression.Quote(order)));
  }
  return sorted;
 }
 public async Task<object> ReportOptions(ReportFilter f,CancellationToken ct){
  var employees=ReportEmployees(new ReportFilter{DivisionIds=f.DivisionIds,DepartmentIds=f.DepartmentIds,TypeIds=f.TypeIds,Groups=f.Groups});
  if(!string.IsNullOrWhiteSpace(f.OptionSearch)){var term=f.OptionSearch.Trim();employees=employees.Where(x=>x.EmployeeId.Contains(term)||x.FullName.Contains(term));}
  var people=await employees.OrderBy(x=>x.EmployeeId).Take(50).Select(x=>new ReportOption(x.EmployeeId,x.EmployeeId+" - "+x.FullName)).ToListAsync(ct);
  var selected=await db.Employees.Where(x=>f.EmployeeIds.Contains(x.EmployeeId)).Select(x=>new ReportOption(x.EmployeeId,x.EmployeeId+" - "+x.FullName)).ToListAsync(ct);
  var departments=db.Departments.AsNoTracking();if(f.DivisionIds.Length>0)departments=departments.Where(x=>f.DivisionIds.Contains(x.DivisionId));
  return new{employees=selected.Concat(people).DistinctBy(x=>x.Id),divisions=await db.Divisions.OrderBy(x=>x.DivisionName).Select(x=>new ReportOption(x.DivisionId.ToString(),x.DivisionName)).ToListAsync(ct),departments=await departments.OrderBy(x=>x.DepartmentName).Select(x=>new ReportOption(x.DepartmentId.ToString(),x.Division.DivisionName+" / "+x.DepartmentName)).ToListAsync(ct),types=await db.DocumentTypes.Where(x=>x.DocumentGroup=="Hồ sơ gốc"||x.DocumentGroup=="Quyết định / Văn bản khác").InRequirementOrder(db).Select(x=>new ReportOption(x.DocumentTypeId.ToString(),x.DocumentGroup+" / "+x.DocumentName+" — "+x.DocumentCode)).ToListAsync(ct)};
 }
}
