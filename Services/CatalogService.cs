using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;
public class CatalogService(VtbContext db)
{
 public static string Title(string kind)=>kind switch{"employees"=>"Danh sách cán bộ","qualifications"=>"Trình độ chuyên môn","documents"=>"Danh sách tài liệu","types"=>"Danh sách loại tài liệu","rules"=>"Yêu cầu hồ sơ theo loại tài liệu",_=>throw new ArgumentException("Danh mục không hợp lệ")};
 public static bool Known(string kind)=>kind is "qualifications" or "employees" or "documents" or "types" or "rules";
 public static List<Field> Fields(string kind)=>kind switch {
 "employees"=>[new("EmployeeId","Mã cán bộ","text",true,8),new("FullName","Họ và tên","text",true,50),new("PhoneNumber","Số điện thoại","tel",false,10),new("Email","Email","email",false,254),new("Hometown","Quê quán","text",false,255),new("DateOfBirth","Ngày sinh","date"),new("Gender","Giới tính","select",true),new("DepartmentId","Phòng ban","select",true),new("PositionName","Chức danh","text",false,200),new("IsManagerial","Cán bộ quản lý","checkbox"),new("EmploymentStatus","Trạng thái làm việc","select",true),new("JoinDate","Ngày vào làm","date",true),new("LeaveDate","Ngày nghỉ","date")],
 "qualifications"=>[new("EmployeeId","Cán bộ","select",true),new("QualificationLevel","Trình độ","text",true,50),new("DegreeName","Tên văn bằng","text",false,200),new("Major","Chuyên ngành","text",false,200),new("Institution","Cơ sở đào tạo","text",false,255),new("GraduationDate","Ngày tốt nghiệp","date"),new("Notes","Ghi chú","text",false,500)],
 "documents"=>[new("EmployeeId","Cán bộ","select",true),new("DocumentTypeId","Loại tài liệu","select",true),new("SpecificName","Tên cụ thể","text",false,300),new("IssueDate","Ngày cấp","date",true),new("ExpiryDate","Ngày hết hạn","date"),new("StorageForm","Hình thức lưu trữ","select")],
 "types"=>[new("DocumentCode","Mã loại tài liệu","text",true,50),new("DocumentName","Tên loại tài liệu","text",true,200),new("DocumentGroup","Nhóm tài liệu","select",true),new("Description","Mô tả","text",false,200)],
 "rules"=>[new("DocumentTypeId","Loại tài liệu","select",true),new("RequirementLevel","Mức độ yêu cầu","select",true),new("RequiredStorageForm","Hình thức lưu trữ yêu cầu","text",false,50),new("WarningThresholdDays","Ngưỡng cảnh báo (ngày)","number"),new("EffectiveFrom","Hiệu lực từ","date",true),new("EffectiveTo","Hiệu lực đến","date"),new("IsActive","Đang áp dụng","checkbox")],_=>[]};
 public static List<SelectListItem> Choices(params string[] values)=>values.Select(x=>new SelectListItem(Labels.Text(x),x)).ToList();
 public async Task<Dictionary<string,List<SelectListItem>>> Options(string? kind=null)
 {
  if(kind=="types")return new(){["DocumentGroup"]=Choices("Hồ sơ gốc","Hợp đồng lao động","Quyết định / Văn bản khác")};
  var departments=await db.Departments.AsNoTracking().Include(x=>x.Division).OrderBy(x=>x.DepartmentName).ToListAsync();
  return new(){
   ["DivisionId"]=await db.Divisions.AsNoTracking().OrderBy(x=>x.DivisionName).Select(x=>new SelectListItem(x.DivisionName,x.DivisionId.ToString())).ToListAsync(),
   ["DepartmentId"]=departments.Select(x=>new SelectListItem(x.Division.DivisionName+" / "+x.DepartmentName,x.DepartmentId.ToString())).ToList(),
   ["EmployeeId"]=await db.Employees.AsNoTracking().OrderBy(x=>x.EmployeeId).Select(x=>new SelectListItem(x.EmployeeId+" — "+x.FullName,x.EmployeeId)).ToListAsync(),
   ["DocumentTypeId"]=await db.DocumentTypes.AsNoTracking().InRequirementOrder(db).Select(x=>new SelectListItem(x.DocumentCode+" — "+x.DocumentName,x.DocumentTypeId.ToString())).ToListAsync(),
   ["StorageForm"]=DocumentStorageForms.Options(),
   ["Gender"]=Choices("Nam","Nu"),["EmploymentStatus"]=Choices("DangLamViec","TamHoanHDLD","ChamDutHDLD","NghiHuu"),
   ["DocumentGroup"]=Choices("Hồ sơ gốc","Hợp đồng lao động","Quyết định / Văn bản khác"),["RequirementLevel"]=Choices("BAT_BUOC","TUY_CHON","CO_DIEU_KIEN"),
   ["Current"]=new(){new("Hiện hành","yes"),new("Lịch sử","no")},["Term"]=new(){new("Có ngày hết hạn","dated"),new("Không có ngày hết hạn","undated")}
  };
 }
 public IQueryable<Employee> Employees(Filter f)
 {
  var q=db.Employees.AsNoTracking();if(!string.IsNullOrWhiteSpace(f.Q))q=q.Where(x=>x.EmployeeId.Contains(f.Q)||x.FullName.Contains(f.Q));
  if(f.DivisionId.HasValue)q=q.Where(x=>db.Departments.Any(d=>d.DepartmentId==x.DepartmentId&&d.DivisionId==f.DivisionId));
  if(f.DepartmentId.HasValue)q=q.Where(x=>x.DepartmentId==f.DepartmentId);
  if(!string.IsNullOrEmpty(f.EmployeeId))q=q.Where(x=>x.EmployeeId==f.EmployeeId);
  return q;
 }
 public async Task<GridModel> List<T>(string kind,Filter f) where T:class
 {
  IQueryable<T> query;
  switch(kind){
   case "employees":var e=Employees(f);if(!string.IsNullOrEmpty(f.Status))e=e.Where(x=>x.EmploymentStatus==f.Status);query=(IQueryable<T>)e.OrderBy(x=>x.EmployeeId);break;
   case "qualifications":
    var degrees=db.Qualifications.AsNoTracking();if(!string.IsNullOrWhiteSpace(f.EmployeeId))degrees=degrees.Where(x=>x.EmployeeId==f.EmployeeId);
    query=(IQueryable<T>)degrees.ByGraduation();break;
   case "documents":
    var employeeIds=Employees(new Filter{DivisionId=f.DivisionId,DepartmentId=f.DepartmentId,EmployeeId=f.EmployeeId}).Select(e=>e.EmployeeId);
    var d=db.Documents.AsNoTracking().Where(x=>employeeIds.Contains(x.EmployeeId));
    if(!string.IsNullOrWhiteSpace(f.Q))d=d.Where(x=>x.EmployeeId.Contains(f.Q)||(x.SpecificName!=null&&x.SpecificName.Contains(f.Q))||db.DocumentTypes.Any(t=>t.DocumentTypeId==x.DocumentTypeId&&t.DocumentName.Contains(f.Q))||db.Employees.Any(e=>e.EmployeeId==x.EmployeeId&&e.FullName.Contains(f.Q)));
    if(f.DocumentTypeId.HasValue)d=d.Where(x=>x.DocumentTypeId==f.DocumentTypeId);
    if(f.Current=="yes")d=d.Where(x=>x.IsCurrent);if(f.Current=="no")d=d.Where(x=>!x.IsCurrent);
    if(f.Term=="dated")d=d.Where(x=>x.ExpiryDate!=null);if(f.Term=="undated")d=d.Where(x=>x.ExpiryDate==null);
    if(!string.IsNullOrEmpty(f.Status))d=d.Where(x=>db.DocumentQualities.Any(v=>v.DocumentId==x.DocumentId&&v.QualityStatus==f.Status));
    query=(IQueryable<T>)d.InRequirementOrder(db);break;
   case "types":var t=db.DocumentTypes.AsNoTracking();if(!string.IsNullOrWhiteSpace(f.Q))t=t.Where(x=>x.DocumentCode.Contains(f.Q)||x.DocumentName.Contains(f.Q));if(!string.IsNullOrEmpty(f.Group))t=t.Where(x=>x.DocumentGroup==f.Group);query=(IQueryable<T>)t.InRequirementOrder(db);break;
   default:var r=db.RequirementRules.AsNoTracking();if(f.DocumentTypeId.HasValue)r=r.Where(x=>x.DocumentTypeId==f.DocumentTypeId);if(!string.IsNullOrEmpty(f.Status))r=r.Where(x=>x.RequirementLevel==f.Status);if(!string.IsNullOrWhiteSpace(f.Q))r=r.Where(x=>db.DocumentTypes.Any(t=>t.DocumentTypeId==x.DocumentTypeId&&(t.DocumentCode.Contains(f.Q)||t.DocumentName.Contains(f.Q))));query=(IQueryable<T>)r.InPageOrder();break;
  }
  var total=await query.CountAsync();f.Page=Math.Clamp(f.Page,1,Math.Max(1,(int)Math.Ceiling(total/20d)));
  var items=await query.Skip((f.Page-1)*20).Take(20).ToListAsync();var options=await Options(kind);
  options["Status"]=kind switch{"employees"=>options["EmploymentStatus"],"rules"=>options["RequirementLevel"],_=>Choices("HOP_LE","SAP_HET_HAN","DA_HET_HAN","KHONG_HOP_LE","KHONG_XAC_DINH")};options["Group"]=options["DocumentGroup"];
  var employeeDepartmentIds=kind=="employees"?items.Cast<Employee>().Select(e=>e.DepartmentId).Distinct().ToArray():Array.Empty<int>();
  var employeeDepartments=kind=="employees"?await db.Departments.AsNoTracking().Where(d=>employeeDepartmentIds.Contains(d.DepartmentId)).ToDictionaryAsync(d=>d.DepartmentId,d=>d.DepartmentName):new Dictionary<int,string>();
  var fields=Fields(kind).Where(x=>x.Name is not "FilePath" and not "DateOfBirth" and not "IsManagerial").ToList();
  var grid=new GridModel{Title=Title(kind),Kind=kind,Filter=f,Total=total,Options=options,Headers=fields.Select(x=>x.Label).ToArray(),Create=new("Thêm mới","Catalog","Create",Kind:kind),Filters=kind switch{"qualifications"=>["EmployeeId"],"employees"=>["DivisionId","DepartmentId","Status"],"documents"=>["EmployeeId","DocumentTypeId","Status","Current","Term"],"types"=>["Group"],_=>["DocumentTypeId","Status"]}};
  if(kind=="rules")grid.Note="Quy tắc tắt hoặc hết hiệu lực vẫn có thể ảnh hưởng báo cáo hiện tại.";
  if(kind=="documents")grid.Note="Bộ lọc chất lượng lấy từ View và chỉ áp dụng tài liệu hiện hành. Tài liệu lịch sử không được View đánh giá.";
    var qualityRows = new List<DocumentQuality>();
  if(kind=="documents") {
   var pageIds=items.Cast<Document>().Select(x=>x.DocumentId).ToList();
   qualityRows=await db.DocumentQualities.Where(x=>pageIds.Contains(x.DocumentId)).ToListAsync();
   grid.Headers=grid.Headers.Concat(new[]{"Chất lượng hiện hành"}).ToArray();
  }
  if(kind=="types")grid.Headers=new[]{"STT"}.Concat(grid.Headers).ToArray();
  foreach(var item in items){var id=Id(item);var row=new GridRow();if(kind=="types")row.Cells.Add(((f.Page-1)*20+grid.Rows.Count+1).ToString());foreach(var field in fields){var value=Value(item,field.Name);var display=item is Document storageDocument&&field.Name=="StorageForm"?DocumentStorageForms.Display(storageDocument.StorageForm):options.TryGetValue(field.Name,out var choices)?choices.FirstOrDefault(x=>x.Value==value)?.Text??value:value;row.Cells.Add(item is Employee employee?EmployeeListValue(employee,field.Name,display,employeeDepartments):display);}
      if(item is Document document) {
    var results=qualityRows.Where(x=>x.DocumentId==document.DocumentId).ToList();
    row.Cells.Add(!document.IsCurrent?"Lịch sử — không đánh giá":results.Count==0?"Chưa có kết quả":string.Join("; ",results.Select(x=>Labels.Text(x.QualityStatus)+(string.IsNullOrEmpty(x.InvalidReason)?"":" — "+x.InvalidReason)).Distinct()));
   }
   row.Links.Add(new("Chi tiết","Catalog","Details",id,kind));row.Links.Add(new("Sửa","Catalog","Edit",id,kind));row.Links.Add(new("Xóa","Catalog","Delete",id,kind));if(kind=="employees")row.Links.Insert(0,new("Hồ sơ","Profiles","Details",id));grid.Rows.Add(row);}
  return grid;
 }
 // Table cells must not reuse compound dropdown labels for employee identity or department.
 public static string? EmployeeListValue(Employee employee,string field,string? fallback,IReadOnlyDictionary<int,string> departments)=>field switch{
  "EmployeeId"=>employee.EmployeeId,"FullName"=>employee.FullName,"DepartmentId"=>departments.GetValueOrDefault(employee.DepartmentId),_=>fallback};
 public static string Id(object item)=>item switch{Qualification q=>q.QualificationId.ToString(),Employee e=>e.EmployeeId,Document d=>d.DocumentId.ToString(),DocumentType t=>t.DocumentTypeId.ToString(),RequirementRule r=>r.RequirementId.ToString(),_=>""};
 public static string? Value(object item,string property)=>item.GetType().GetProperty(property)?.GetValue(item) switch {DateOnly d=>d.ToString("dd/MM/yyyy"),bool b=>b?"Có":"Không",var v=>v?.ToString()};
 public async Task<List<(string Field,string Error)>> Validate(object entity,bool editing)
 {
  var errors=new List<(string,string)>();
  switch(entity){
   case Qualification q:
    if(!await db.Employees.AnyAsync(x=>x.EmployeeId==q.EmployeeId))errors.Add(("EmployeeId","Cán bộ không tồn tại."));break;
   case Employee e:
    if(!await db.Departments.AnyAsync(x=>x.DepartmentId==e.DepartmentId))errors.Add(("DepartmentId","Phòng ban không tồn tại."));
    if(!editing&&await db.Employees.AnyAsync(x=>x.EmployeeId==e.EmployeeId))errors.Add(("EmployeeId","Mã cán bộ đã tồn tại."));break;
   case Document d:
    if(!await db.Employees.AnyAsync(x=>x.EmployeeId==d.EmployeeId))errors.Add(("EmployeeId","Cán bộ không tồn tại."));
    if(!await db.DocumentTypes.AnyAsync(x=>x.DocumentTypeId==d.DocumentTypeId))errors.Add(("DocumentTypeId","Loại tài liệu không tồn tại."));
    if(editing&&d.IsCurrent&&await db.Documents.AnyAsync(x=>x.EmployeeId==d.EmployeeId&&x.DocumentTypeId==d.DocumentTypeId&&x.IsCurrent&&x.DocumentId!=d.DocumentId))errors.Add(("DocumentTypeId","Cán bộ đã có tài liệu hiện hành thuộc loại này. Không thể chuyển sang loại đã có bản hiện hành."));
    break;
   case DocumentType t:
    if(await db.DocumentTypes.AnyAsync(x=>x.DocumentTypeId!=t.DocumentTypeId&&x.DocumentCode==t.DocumentCode))errors.Add(("DocumentCode","Mã loại tài liệu đã tồn tại."));
    if(await db.DocumentTypes.AnyAsync(x=>x.DocumentTypeId!=t.DocumentTypeId&&x.DocumentName==t.DocumentName))errors.Add(("DocumentName","Tên loại tài liệu đã tồn tại."));
    if(editing&&db.Entry(t).Property(x=>x.DocumentCode).OriginalValue!=t.DocumentCode&&(new[]{"HDTHUVIEC","HDXDTH1","HDXDTH2","HDKXDTH","QDBONHIEM","HSBONHIEM","KEKHAITS","QDTAMHOAN","QDNGHIHUU"}.Contains(db.Entry(t).Property(x=>x.DocumentCode).OriginalValue,StringComparer.OrdinalIgnoreCase)||await db.RequirementRules.AnyAsync(x=>x.DocumentTypeId==t.DocumentTypeId)))errors.Add(("DocumentCode","Không đổi mã loại tài liệu đang có quy tắc hoặc mã nghiệp vụ được View sử dụng để đánh giá."));break;
   case RequirementRule r:
    var type=await db.DocumentTypes.AsNoTracking().SingleOrDefaultAsync(x=>x.DocumentTypeId==r.DocumentTypeId);
    if(type==null)errors.Add(("DocumentTypeId","Loại tài liệu không tồn tại."));
    if(await db.RequirementRules.AnyAsync(x=>x.DocumentTypeId==r.DocumentTypeId&&x.RequirementId!=r.RequirementId))errors.Add(("DocumentTypeId","Đã có quy tắc cho loại này. Nhiều quy tắc làm nhân bản dòng trong View; cần rà soát SQL trước khi tạo thêm hoặc sửa bản trùng."));
    if(r.RequirementLevel=="CO_DIEU_KIEN"&&type!=null&&!new[]{"QDBONHIEM","HSBONHIEM","KEKHAITS","QDTAMHOAN","QDNGHIHUU"}.Contains(type.DocumentCode))errors.Add(("RequirementLevel","View hiện tại chưa định nghĩa điều kiện cho mã loại tài liệu này."));break;
  }
  return errors;
 }
}



