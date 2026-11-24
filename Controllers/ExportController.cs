using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Services;
using System.Globalization;
namespace Vtb.Hr.Web.Controllers;

[Authorize]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class ExportController(VtbContext db,CatalogService catalog,QualityService quality,DashboardService dashboard):Controller
{
 private string? exportScope;
 private static readonly string[] Employment=["DangLamViec","TamHoanHDLD","ChamDutHDLD","NghiHuu"];
 private static readonly string[] DocumentStates=["HOP_LE","SAP_HET_HAN","DA_HET_HAN","KHONG_HOP_LE","KHONG_XAC_DINH"];
 private async Task Organization(int[] divisions,int[] departments,CancellationToken ct){
  if(divisions.Length>200||departments.Length>500||divisions.Any(x=>x<=0)||departments.Any(x=>x<=0))throw new InvalidDataException("Bộ lọc tổ chức không hợp lệ.");
  if(await db.Divisions.CountAsync(x=>divisions.Contains(x.DivisionId),ct)!=divisions.Distinct().Count())throw new InvalidDataException("Khối không tồn tại.");
  var deps=await db.Departments.Where(x=>departments.Contains(x.DepartmentId)).Select(x=>new{x.DepartmentId,x.DivisionId}).ToListAsync(ct);
  if(deps.Count!=departments.Distinct().Count()||(divisions.Length>0&&deps.Any(d=>!divisions.Contains(d.DivisionId))))throw new InvalidDataException("Phòng ban không tồn tại hoặc không thuộc khối đã chọn.");
 }
 private async Task Validate(Filter f,string kind,CancellationToken ct){
  await Organization(f.DivisionId.HasValue?[f.DivisionId.Value]:[],f.DepartmentId.HasValue?[f.DepartmentId.Value]:[],ct);
  if(f.Q?.Length>200||f.EmployeeId?.Length>8||f.Group?.Length>100)throw new InvalidDataException("Bộ lọc quá dài.");
  if(!string.IsNullOrEmpty(f.EmployeeId)&&!await db.Employees.AnyAsync(e=>e.EmployeeId==f.EmployeeId,ct))throw new InvalidDataException("Cán bộ không tồn tại.");
  if(f.DocumentTypeId.HasValue&&!await db.DocumentTypes.AnyAsync(t=>t.DocumentTypeId==f.DocumentTypeId,ct))throw new InvalidDataException("Loại tài liệu không tồn tại.");
  var states=kind switch{"employees"=>Employment,"rules"=>new[]{"BAT_BUOC","CO_DIEU_KIEN","TUY_CHON"},"contracts"=>new[]{"HOP_LE","THIEU_HDXDTH1","CAN_KY_KET_TIEP","KHONG_CANH_BAO"},"requirements"=>new[]{"DA_CO","THIEU_HO_SO"},_=>DocumentStates};
  if(!string.IsNullOrEmpty(f.Status)&&!states.Contains(f.Status))throw new InvalidDataException("Trạng thái không hợp lệ.");
  if(!string.IsNullOrEmpty(f.Current)&&f.Current is not ("yes" or "no")||!string.IsNullOrEmpty(f.Term)&&f.Term is not ("dated" or "undated"))throw new InvalidDataException("Bộ lọc tài liệu không hợp lệ.");
  if(!string.IsNullOrEmpty(f.Group)&&!new[]{"Hồ sơ gốc","Hợp đồng lao động","Quyết định / Văn bản khác"}.Contains(f.Group))throw new InvalidDataException("Nhóm tài liệu không hợp lệ.");
 }
 private async Task<string> ScopeText(CancellationToken ct){
  var labels=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase){["q"]="Tìm kiếm",["search"]="Tìm kiếm",["status"]="Trạng thái",["statuses"]="Trạng thái làm việc",["quality"]="Chất lượng",["state"]="Trạng thái",["employeeId"]="Mã cán bộ",["employeeIds"]="Mã cán bộ",["current"]="Phiên bản (yes: hiện hành, no: lịch sử)",["group"]="Nhóm",["groups"]="Nhóm",["term"]="Hạn tài liệu",["mode"]="Phạm vi",["kind"]="Danh sách",["kpiOnly"]="Chỉ nhân sự hiện tại"};
  var parts=new List<string>();
  foreach(var item in Request.Query){if(item.Value.All(string.IsNullOrWhiteSpace))continue;
   var key=item.Key.ToLowerInvariant();if(key.StartsWith("drill"))key=key[5..];if(key=="typeid")key="documenttypeid";if(key=="issuedepartmentid")key="departmentid";
   if(key is "divisionid" or "divisionids" or "departmentid" or "departmentids" or "documenttypeid" or "typeids"){
    var ids=item.Value.Select(x=>int.TryParse(x,out var id)?id:0).ToArray();
    var names=key.StartsWith("division")?await db.Divisions.Where(x=>ids.Contains(x.DivisionId)).Select(x=>x.DivisionName).ToListAsync(ct):key.StartsWith("department")?await db.Departments.Where(x=>ids.Contains(x.DepartmentId)).Select(x=>x.DepartmentName).ToListAsync(ct):await db.DocumentTypes.Where(x=>ids.Contains(x.DocumentTypeId)).Select(x=>x.DocumentName).ToListAsync(ct);
    parts.Add((key.StartsWith("division")?"Khối":key.StartsWith("department")?"Phòng ban":"Loại tài liệu")+": "+string.Join(", ",names));
   }else if(labels.TryGetValue(item.Key,out var label))parts.Add(label+": "+string.Join(", ",item.Value.Select(x=>key is "q" or "search" or "employeeid" or "employeeids"?x:DisplayScopeValue(x))));
  }return parts.Count==0?"Toàn bộ dữ liệu được phép xem":string.Join("; ",parts);
 }
 private static string DisplayScopeValue(string? value)=>value switch{
  "all"=>"Toàn bộ cán bộ","current"=>"Theo bộ lọc hiện tại","division"=>"Theo khối","department"=>"Theo phòng ban",
  "yes"=>"Hiện hành","no"=>"Lịch sử","dated"=>"Có ngày hết hạn","undated"=>"Không có ngày hết hạn",
  "documents"=>"Tài liệu","types"=>"Loại tài liệu","rules"=>"Quy tắc yêu cầu","employees"=>"Cán bộ","qualifications"=>"Văn bằng","requirements"=>"Hồ sơ bắt buộc","contracts"=>"Hợp đồng",
  "problem"=>"Tài liệu có vấn đề","expiring"=>"Tài liệu sắp hết hạn","expiredContracts"=>"Hợp đồng hết hạn","contractIssues"=>"Cán bộ cần xử lý hợp đồng","contractDetail"=>"Chi tiết hợp đồng",
  "True" or "true"=>"Có","False" or "false"=>"Không",_=>Labels.Text(value)};
 private async Task<IActionResult> Run(Func<CancellationToken,Task<ExcelTable>> read){
  if(!ModelState.IsValid)return BadRequest(new{message="Bộ lọc xuất dữ liệu không hợp lệ."});
  try{var ct=HttpContext.RequestAborted;var table=await read(ct);ct.ThrowIfCancellationRequested();if(table.Rows.Count==0)return NotFound(new{message="Không có dữ liệu phù hợp để xuất."});return File(ExcelExport.Write(table,exportScope??await ScopeText(ct)),ExcelExport.Mime,ExcelExport.FileName(table.Title));}
  catch(InvalidDataException e){return BadRequest(new{message=e.Message});}
  catch(OperationCanceledException) when(HttpContext.RequestAborted.IsCancellationRequested){return StatusCode(499);}
  catch(Exception e) when(e is Microsoft.Data.SqlClient.SqlException or DashboardListReadException or InvalidOperationException){return StatusCode(503,new{message="Không thể xuất dữ liệu. Vui lòng kiểm tra kết nối SQL Server hoặc thử lại."});}
 }
 [HttpGet]public Task<IActionResult> EmployeeOptions(CancellationToken ct)=>Options(ct);
 private async Task<IActionResult> Options(CancellationToken ct)=>Json(new{divisions=await db.Divisions.AsNoTracking().OrderBy(x=>x.DivisionName).Select(x=>new{id=x.DivisionId,name=x.DivisionName}).ToListAsync(ct),departments=await db.Departments.AsNoTracking().OrderBy(x=>x.DepartmentName).Select(x=>new{id=x.DepartmentId,divisionId=x.DivisionId,name=x.DepartmentName}).ToListAsync(ct)});
 [HttpGet]public Task<IActionResult> Employees(Filter f,string mode="current")=>Run(async ct=>{
  if(mode is not ("current" or "all" or "division" or "department"))throw new InvalidDataException("Phạm vi xuất không hợp lệ.");
  if(mode!="current"){f.Q=null;f.Quality=null;f.EmployeeId=null;}
  if(mode=="all"){f.DivisionId=null;f.DepartmentId=null;}
  if(mode=="division"){if(!f.DivisionId.HasValue)throw new InvalidDataException("Vui lòng chọn khối.");f.DepartmentId=null;}
  if(mode=="department"&&!f.DepartmentId.HasValue)throw new InvalidDataException("Vui lòng chọn phòng ban.");
  await Validate(f,"employees",ct);
  var organization=f.DepartmentId.HasValue?"Phòng ban: "+await db.Departments.Where(d=>d.DepartmentId==f.DepartmentId).Select(d=>d.DepartmentName).SingleAsync(ct):f.DivisionId.HasValue?"Khối: "+await db.Divisions.Where(d=>d.DivisionId==f.DivisionId).Select(d=>d.DivisionName).SingleAsync(ct):"Toàn bộ cán bộ";
  exportScope=organization+(!string.IsNullOrEmpty(f.Status)?"; Trạng thái: "+Labels.Text(f.Status):"; Tất cả trạng thái")+(!string.IsNullOrEmpty(f.Q)?"; Tìm kiếm: "+f.Q:"")+(f.Quality!=null?"; Chất lượng: "+Labels.Text(f.Quality):"");
  var q=catalog.Employees(f);
  if(!string.IsNullOrEmpty(f.Status))q=q.Where(x=>x.EmploymentStatus==f.Status);
  if(f.Quality!=null)q=q.Where(x=>db.EmployeeQualities.Any(v=>v.EmployeeId==x.EmployeeId&&v.EmployeeQualityStatus==f.Quality));
  ExcelExport.CheckCount(await q.CountAsync(ct));
  var rows=await (from e in q join d in db.Departments on e.DepartmentId equals d.DepartmentId join v in db.Divisions on d.DivisionId equals v.DivisionId orderby e.EmployeeId select new{e.EmployeeId,e.FullName,e.Gender,v.DivisionName,d.DepartmentName,e.PositionName,e.EmploymentStatus,e.JoinDate}).ToListAsync(ct);
  return new ExcelTable("Danh sách cán bộ",["Mã cán bộ","Họ và tên","Giới tính","Khối","Phòng ban","Chức danh/vị trí","Trạng thái làm việc","Ngày vào làm"],rows.Select(x=>new object?[]{x.EmployeeId,x.FullName,Labels.Text(x.Gender),x.DivisionName,x.DepartmentName,x.PositionName,Labels.Text(x.EmploymentStatus),x.JoinDate}).ToList());
 });
 private static object? Value(string header,string? cell){if(header.Contains("Ngày",StringComparison.OrdinalIgnoreCase)||header.Contains("Hiệu lực")||header=="Hết hạn"){if(DateOnly.TryParseExact(cell,"dd/MM/yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date))return date;}return string.IsNullOrWhiteSpace(cell)?"--":Labels.Text(cell);}
 private async Task<ExcelTable> Grid(Func<Task<GridModel>> read,Filter f,CancellationToken ct){
  f.Page=1;var first=await read();ExcelExport.CheckCount(first.Total);var result=new ExcelTable(first.Title,first.Headers,[]);
  void Add(GridModel grid){result.Rows.AddRange(grid.Rows.Select(r=>r.Cells.Select((v,i)=>Value(grid.Headers[i],v)).ToArray()));ExcelExport.CheckCount(result.Rows.Count);}
  ct.ThrowIfCancellationRequested();Add(first);return result;
 }
 [HttpGet]public Task<IActionResult> Catalog(string kind,Filter f)=>Run(async ct=>{
  if(kind is not ("documents" or "types" or "rules" or "qualifications"))throw new InvalidDataException("Danh sách không hợp lệ.");await Validate(f,kind,ct);
  if(kind=="documents"&&string.IsNullOrWhiteSpace(f.EmployeeId))throw new InvalidDataException("Vui lòng chọn cán bộ.");
  return await Grid(()=>kind switch{"documents"=>catalog.List<Document>(kind,f,forExport:true),"types"=>catalog.List<DocumentType>(kind,f,forExport:true),"qualifications"=>catalog.List<Qualification>(kind,f,forExport:true),_=>catalog.List<RequirementRule>(kind,f,forExport:true)},f,ct);
 });
 [HttpGet]public Task<IActionResult> Quality(string kind,Filter f)=>Run(async ct=>{
  if(kind is not ("documents" or "requirements" or "contracts" or "employees"))throw new InvalidDataException("Danh sách không hợp lệ.");
  // Employee quality Status is a quality state, not employment status.
  if(kind=="employees"){var status=f.Status;if(!string.IsNullOrEmpty(status)&&status is not ("DAY_DU" or "THIEU_HO_SO"))throw new InvalidDataException("Trạng thái không hợp lệ.");f.Status=null;await Validate(f,kind,ct);f.Status=status;}else await Validate(f,kind,ct);
  return await Grid(()=>quality.List(kind,f,forExport:true),f,ct);
 });
 [HttpGet]public Task<IActionResult> Organization(string kind,Filter f)=>Run(async ct=>{
  if(kind is not ("divisions" or "departments"))throw new InvalidDataException("Danh sách không hợp lệ.");await Validate(f,kind,ct);var term=f.Q?.Trim();
  if(kind=="divisions"){var q=db.Divisions.AsNoTracking();if(!string.IsNullOrEmpty(term))q=q.Where(x=>x.DivisionCode.Contains(term)||x.DivisionName.Contains(term));ExcelExport.CheckCount(await q.CountAsync(ct));var rows=await q.OrderBy(x=>x.DivisionCode).ToListAsync(ct);return new ExcelTable("Danh sách khối",["Mã khối","Tên khối"],rows.Select(x=>new object?[]{x.DivisionCode,x.DivisionName}).ToList());}
  else {var q=db.Departments.AsNoTracking().Include(x=>x.Division).AsQueryable();if(f.DivisionId.HasValue)q=q.Where(x=>x.DivisionId==f.DivisionId);if(!string.IsNullOrEmpty(term))q=q.Where(x=>x.DepartmentCode.Contains(term)||x.DepartmentName.Contains(term)||x.Division.DivisionName.Contains(term));ExcelExport.CheckCount(await q.CountAsync(ct));var rows=await q.OrderBy(x=>x.DepartmentCode).ToListAsync(ct);return new ExcelTable("Danh sách phòng ban",["Mã phòng ban","Tên phòng ban","Khối"],rows.Select(x=>new object?[]{x.DepartmentCode,x.DepartmentName,x.Division.DivisionName}).ToList());}
 });
 [HttpGet]public Task<IActionResult> RequirementPeople(RequirementDrillFilter f)=>Run(async ct=>{
  await dashboard.ValidateRequirementDrill(f,ct);
  ExcelExport.CheckCount(await dashboard.RequirementPeopleQuery(f).CountAsync(ct));
  f.Page=1;f.PageSize=100;var first=await dashboard.RequirementPeople(f,ct);var rows=first.Rows;
  for(int page=2;page<=Math.Ceiling(first.Total/100d);page++){f.Page=page;rows.AddRange((await dashboard.RequirementPeople(f,ct)).Rows);ExcelExport.CheckCount(rows.Count);}
  exportScope="Nhân sự đang làm việc/tạm hoãn HĐLĐ; chỉ yêu cầu chưa có tài liệu hiện hành. "+await ScopeText(ct);
  return new ExcelTable("Cán bộ thiếu tài liệu",["Mã cán bộ","Họ tên","Phòng ban","Số yêu cầu thiếu","Tài liệu cần bổ sung"],rows.Select(x=>new object?[]{x.EmployeeId,x.FullName,x.Department,x.Missing,string.Join("; ",x.Documents)}).ToList());
 });
 [HttpGet]public Task<IActionResult> Overview(OverviewFilter f)=>Run(async ct=>{
  await Organization(f.DivisionIds,f.DepartmentIds,ct);f.Page=1;f.PageSize=100;var first=await dashboard.Missing(f,ct);ExcelExport.CheckCount(first.Total);var rows=first.Rows;
  for(int p=2;p<=Math.Ceiling(first.Total/100d);p++){f.Page=p;rows.AddRange((await dashboard.Missing(f,ct)).Rows);ExcelExport.CheckCount(rows.Count);}
  return new ExcelTable("Cán bộ cần bổ sung hoặc xử lý hồ sơ",["Mã cán bộ","Họ tên","Phòng ban","Số tài liệu thiếu","Vấn đề cần xử lý","Tài liệu cần bổ sung"],rows.Select(x=>new object?[]{x.EmployeeId,x.FullName,x.DepartmentName,x.Missing,x.Issue,x.MissingDocuments.Count>0?string.Join("; ",x.MissingDocuments):"--"}).ToList());
 });
 [HttpGet]public Task<IActionResult> Report(ReportFilter f)=>Run(async ct=>{
  await dashboard.ValidateContractDrill(f,ct);
  await Organization(f.DivisionIds,f.DepartmentIds,ct);
  if(await db.Employees.CountAsync(e=>f.EmployeeIds.Contains(e.EmployeeId),ct)!=f.EmployeeIds.Distinct().Count()||await db.DocumentTypes.CountAsync(t=>f.TypeIds.Contains(t.DocumentTypeId),ct)!=f.TypeIds.Distinct().Count())throw new InvalidDataException("Cán bộ hoặc loại tài liệu không tồn tại.");
  var q=dashboard.ReportOrderQuery(f);ExcelExport.CheckCount(await q.CountAsync(ct));var rows=await q.ToListAsync(ct);
  var keys=f.Kind switch{"problem"=>new[]{"Id","Name","Document","Status","Reason","Days"},"expiring"=>new[]{"Id","Name","Document","ExpiryDate","Days"},"contractDetail"=>new[]{"Id","Name","Employment","Department","Status"},_=>ReportFilter.Columns(f.Kind)};var labels=new Dictionary<string,string>{{"Id","Mã cán bộ"},{"Name","Họ tên"},{"Document","Loại tài liệu"},{"Code","Mã loại tài liệu"},{"Group","Nhóm tài liệu"},{"IssueDate","Ngày cấp"},{"ExpiryDate","Ngày hết hạn"},{"Days","Số ngày còn lại"},{"Status","Trạng thái"},{"Reason","Lý do không hợp lệ"},{"Department","Phòng ban"},{"Employment","Trạng thái làm việc"}};
  var title=f.Kind switch{"problem"=>"Tài liệu có vấn đề","expiring"=>"Tài liệu sắp hết hạn","expiredContracts"=>"Hợp đồng đã hết hạn","contractIssues"=>"Cán bộ cần xử lý hợp đồng",_=>f.IssuesOnly?"Cán bộ cần xử lý hợp đồng theo phòng ban":"Chi tiết hợp đồng - "+Labels.Text(f.State)};
  return new ExcelTable(title,keys.Select(k=>labels[k]).ToArray(),rows.Select(r=>keys.Select(k=>typeof(ReportRow).GetProperty(k)!.GetValue(r)).ToArray()).ToList());
 });
 [HttpGet]public Task<IActionResult> Documents(DocumentDrillFilter f)=>Run(async ct=>{
  await Organization(f.DivisionIds,f.DepartmentIds,ct);f.Page=1;f.PageSize=100;var first=await dashboard.DocumentDetails(f,ct);ExcelExport.CheckCount(first.Total);var rows=first.Rows;
  for(int p=2;p<=Math.Ceiling(first.Total/100d);p++){f.Page=p;rows.AddRange((await dashboard.DocumentDetails(f,ct)).Rows);ExcelExport.CheckCount(rows.Count);}
  var reason=f.State is not ("HOP_LE" or "DA_HET_HAN" or "SAP_HET_HAN");var headers=new List<string>{"Mã cán bộ","Họ tên","Phòng ban","Mã loại tài liệu","Loại tài liệu","Ngày cấp","Ngày hết hạn","Số ngày còn lại","Lưu trữ yêu cầu","Hình thức lưu trữ"};if(reason)headers.Add("Lý do không hợp lệ");
  return new ExcelTable("Danh sách tài liệu - "+Labels.Text(f.State),headers.ToArray(),rows.Select(r=>{var cells=new List<object?>{r.EmployeeId,r.FullName,r.Department,r.Code,r.Document,r.IssueDate,r.ExpiryDate,r.Days,r.RequiredStorage,r.StorageDisplay};if(reason)cells.Add(r.Reason);return cells.ToArray();}).ToList());
 });
}
