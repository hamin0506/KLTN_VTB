using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;
public class QualityService(VtbContext db,CatalogService catalog)
{
 public IQueryable<string> Scope(Filter f){var ids=catalog.Employees(f).Select(x=>x.EmployeeId);return f.KpiOnly?ids.Where(id=>db.Employees.Any(q=>q.EmployeeId==id&&(q.EmploymentStatus=="DangLamViec"||q.EmploymentStatus=="TamHoanHDLD"))):ids;}
 public async Task<DashboardModel> Dashboard(Filter f)
 {
  var ids=Scope(f);var eq=db.EmployeeQualities.Where(x=>ids.Contains(x.EmployeeId));var dq=db.DocumentQualities.Where(x=>ids.Contains(x.EmployeeId));
  var m=new DashboardModel{Filter=f,Options=await catalog.Options(),Employees=await ids.CountAsync(),Documents=await db.Documents.CountAsync(x=>ids.Contains(x.EmployeeId)),Complete=await eq.CountAsync(x=>x.EmployeeQualityStatus=="DAY_DU"),Missing=await eq.CountAsync(x=>x.EmployeeQualityStatus=="THIEU_HO_SO")};
  m.Valid=await dq.Where(x=>x.QualityStatus=="HOP_LE").Select(x=>x.DocumentId).Distinct().CountAsync();
  m.Expiring=await dq.Where(x=>x.QualityStatus=="SAP_HET_HAN").Select(x=>x.DocumentId).Distinct().CountAsync();
  m.Expired=await dq.Where(x=>x.QualityStatus=="DA_HET_HAN").Select(x=>x.DocumentId).Distinct().CountAsync();
  m.Invalid=await dq.Where(x=>x.QualityStatus=="KHONG_HOP_LE").Select(x=>x.DocumentId).Distinct().CountAsync();
  m.QualityRows=await dq.CountAsync();m.QualityDocuments=await dq.Select(x=>x.DocumentId).Distinct().CountAsync();
  m.Conflicts=await ConflictingDocuments(dq).CountAsync();
  m.Alerts=await eq.Where(x=>x.EmployeeQualityStatus=="THIEU_HO_SO").OrderByDescending(x=>x.TotalDocumentsMissing).ThenBy(x=>x.EmployeeId).Take(8).ToListAsync();return m;
 }
 public async Task<GridModel> List(string kind,Filter f)
 {
  var ids=Scope(f);var g=new GridModel{Kind=kind,Filter=f,Options=await catalog.Options(),Filters=["DivisionId","DepartmentId","EmployeeId","Status"]};
  switch(kind){
   case "documents":
    g.Title="Chất lượng tài liệu";g.Headers=["Cán bộ","Loại tài liệu","Ngày cấp","Hết hạn","Còn (ngày)","Trạng thái","Nguyên nhân"];
    g.Filters.Add("DocumentTypeId");g.Options["Status"]=CatalogService.Choices("HOP_LE","SAP_HET_HAN","DA_HET_HAN","KHONG_HOP_LE","KHONG_XAC_DINH");
    var dq=db.DocumentQualities.AsNoTracking().Where(x=>ids.Contains(x.EmployeeId));
    if(!string.IsNullOrEmpty(f.Status))dq=dq.Where(x=>x.QualityStatus==f.Status);
    if(f.DocumentTypeId.HasValue)dq=dq.Where(x=>db.Documents.Any(d=>d.DocumentId==x.DocumentId&&d.DocumentTypeId==f.DocumentTypeId));
    g.Total=await dq.CountAsync();Page(g);foreach(var v in await dq.OrderBy(x=>x.EmployeeId).ThenBy(x=>x.DocumentId).ThenBy(x=>x.QualityStatus).ThenBy(x=>x.RequiredStorageForm).ThenBy(x=>x.WarningThresholdDays).Skip((f.Page-1)*20).Take(20).ToListAsync())g.Rows.Add(new(){Cells=[v.EmployeeId+" — "+v.FullName,v.DocumentName,Date(v.IssueDate),Date(v.ExpiryDate),v.ExpiryDays?.ToString(),v.QualityStatus,v.InvalidReason],Links=[new("Sửa tài liệu","Catalog","Edit",v.DocumentId.ToString(),"documents"),new("Hồ sơ","Profiles","Details",v.EmployeeId)]});
    g.Note="Dữ liệu đọc trực tiếp từ View, chỉ gồm tài liệu hiện hành. Nếu nhiều quy tắc áp dụng, một tài liệu có thể xuất hiện nhiều dòng.";break;
   case "requirements":
    g.Title="Kiểm tra hồ sơ bắt buộc";g.Headers=["Cán bộ","Phòng ban","Loại tài liệu","Mức độ","Lý do yêu cầu","Trạng thái"];g.Filters.Add("DocumentTypeId");g.Options["Status"]=CatalogService.Choices("DA_CO","THIEU_HO_SO");
    var rq=db.DocumentRequirements.AsNoTracking().Where(x=>ids.Contains(x.EmployeeId));if(!string.IsNullOrEmpty(f.Status))rq=rq.Where(x=>x.RequirementStatus==f.Status);if(f.DocumentTypeId.HasValue)rq=rq.Where(x=>x.DocumentTypeId==f.DocumentTypeId);
    g.Total=await rq.CountAsync();Page(g);foreach(var v in await rq.OrderBy(x=>x.EmployeeId).ThenBy(x=>x.RequirementId).Skip((f.Page-1)*20).Take(20).ToListAsync())g.Rows.Add(new(){Cells=[v.EmployeeId+" — "+v.FullName,v.DepartmentName,v.DocumentName,v.RequirementLevel,v.RequirementReason,v.RequirementStatus],Links=[new("Hồ sơ","Profiles","Details",v.EmployeeId),v.DocumentId.HasValue?new("Sửa tài liệu","Catalog","Edit",v.DocumentId.ToString(),"documents"):new("Bổ sung","Catalog","Create",Kind:"documents",EmployeeId:v.EmployeeId,DocumentTypeId:v.DocumentTypeId)]});break;
   case "contracts":
    g.Title="Kiểm tra hợp đồng lao động";g.Headers=["Mã cán bộ","Họ tên","Phòng ban","Trạng thái làm việc","Kết quả hợp đồng"];g.Options["Status"]=CatalogService.Choices("HOP_LE","THIEU_HDXDTH1","CAN_KY_KET_TIEP","KHONG_CANH_BAO");
    var cq=db.LaborContractChecks.AsNoTracking().Where(x=>ids.Contains(x.EmployeeId));if(!string.IsNullOrEmpty(f.Status))cq=cq.Where(x=>x.LaborContractStatus==f.Status);
    g.Total=await cq.CountAsync();Page(g);foreach(var v in await cq.OrderBy(x=>x.EmployeeId).Skip((f.Page-1)*20).Take(20).ToListAsync())g.Rows.Add(new(){Cells=[v.EmployeeId,v.FullName,v.DepartmentName,v.EmploymentStatus,v.LaborContractStatus],Links=[new("Hồ sơ","Profiles","Details",v.EmployeeId)]});g.Note="Không cảnh báo là kết quả từ View, không đồng nghĩa với đã xác nhận mọi hợp đồng đều hợp lệ.";break;
   default:
    g.Title="Chất lượng hồ sơ cán bộ";g.Headers=["Cán bộ","Yêu cầu","Đã có","Còn thiếu","Mã tài liệu thiếu","Cảnh báo","Trạng thái"];g.Options["Status"]=CatalogService.Choices("DAY_DU","THIEU_HO_SO");
    var eq=db.EmployeeQualities.AsNoTracking().Where(x=>ids.Contains(x.EmployeeId));if(!string.IsNullOrEmpty(f.Status))eq=eq.Where(x=>x.EmployeeQualityStatus==f.Status);
    g.Total=await eq.CountAsync();Page(g);foreach(var v in await eq.OrderBy(x=>x.EmployeeId).Skip((f.Page-1)*20).Take(20).ToListAsync())g.Rows.Add(new(){Cells=[v.EmployeeId+" — "+v.FullName,v.TotalRequiredDocuments.ToString(),v.TotalDocumentsPresent.ToString(),v.TotalDocumentsMissing.ToString(),v.MissingDocumentCodes,v.QualityIssue,v.EmployeeQualityStatus],Links=[new("Hồ sơ","Profiles","Details",v.EmployeeId)]});g.Note="Đầy đủ phản ánh mức độ đầy đủ và kiểm tra chuỗi hợp đồng của View, không thay thế kết quả tính hợp lệ của từng tài liệu.";break;
  }
  return g;
 }
 public static IQueryable<int> ConflictingDocuments(IQueryable<DocumentQuality> q)=>q.Where(x=>q.Any(y=>y.DocumentId==x.DocumentId&&y.QualityStatus!=x.QualityStatus)).Select(x=>x.DocumentId).Distinct();
 private static void Page(GridModel g)=>g.Filter.Page=Math.Clamp(g.Filter.Page,1,g.Pages);
 public static string Date(DateOnly? d)=>d?.ToString("dd/MM/yyyy")??"—";
}
