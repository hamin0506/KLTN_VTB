using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vtb.Hr.Web.Controllers;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Services;
using Vtb.Hr.Web.Security;
int passed=0;
void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);passed++;}
foreach(var value in new[]{"https://example.test","//example.test","/\\example.test","/Catalog/Delete/123","/Catalog/../Export/Employees","/%2fexample.test","/Export/Employees","javascript:alert(1)","/Catalog/Index\r\nX: bad"})Check(NavigationContext.Safe(value)==null,"reject unsafe navigation "+value.Replace("\r\n"," "));
foreach(var value in new[]{"/Catalog/Index?kind=employees&page=3&q=abc","/Quality/List?kind=employees&page=2","/Dashboard/ProblemDocuments?problem_page=4","/Profiles/Details/TEST0001?returnUrl=%2FCatalog%3Fpage%3D3"})Check(NavigationContext.Safe(value)==value,"retain local context "+value);
foreach(var org in new[]{"Departments","Divisions"}){
 var target="/"+org+"?divisionId=2&q=Ph%C3%B2ng&page=2&pageSize=20&sort=name";
 foreach(var value in new string?[]{target,null,"https://example.test","//example.test","/Catalog/Index","/"+org+"/Delete/1","/"+org+"/../Index"}){
  foreach(var post in new[]{false,true}){
   var context=new DefaultHttpContext();
   if(post){context.Request.ContentType="application/x-www-form-urlencoded";context.Request.Form=new FormCollection(new Dictionary<string,Microsoft.Extensions.Primitives.StringValues>{{"returnUrl",value??""}});}
   else context.Request.QueryString=new QueryString("?returnUrl="+Uri.EscapeDataString(value??""));
   var expected=value==target?target:null;Check(NavigationContext.OrganizationReturn(context.Request,org)==expected,"organization safe GET/POST return "+org+" "+post);
   Controller controller=org=="Departments"?new DepartmentsController(null!,null!,null!):new DivisionsController(null!,null!,null!);
   controller.ControllerContext=new ControllerContext{HttpContext=context};
   var result=(IActionResult)controller.GetType().GetMethod("ReturnToList",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(controller,null)!;
   Check(expected!=null?result is LocalRedirectResult local&&local.Url==target:result is RedirectToActionResult fallback&&fallback.ActionName=="Index","save-success return branch without database "+org);
  }
 }
}
var builder=WebApplication.CreateBuilder(new WebApplicationOptions{ContentRootPath=Directory.GetCurrentDirectory(),EnvironmentName="Testing"});builder.Logging.ClearProviders();builder.WebHost.UseTestServer();builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Configuration.AddInMemoryCollection(new Dictionary<string,string?>{{"Access:Administrators:0","test-admin"},{"Access:Readers:0","test-reader"}});
builder.Services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions,TestAuth>("Test",_=>{});
builder.Services.AddAuthorization(o=>{var read=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireAssertion(c=>Access.Read(c.User,builder.Configuration)).Build();o.DefaultPolicy=read;o.FallbackPolicy=read;o.AddPolicy("Admin",p=>p.RequireAuthenticatedUser().RequireAssertion(c=>Access.Admin(c.User,builder.Configuration)));});
builder.Services.AddControllersWithViews(o=>o.Filters.Add<WriteAccessFilter>()).AddApplicationPart(typeof(CatalogController).Assembly).AddApplicationPart(typeof(UiProbeController).Assembly);
builder.Services.AddDbContext<VtbContext>(o=>o.UseSqlServer("Server=invalid.example;Database=NoDatabase;Integrated Security=True;Connect Timeout=1"));builder.Services.AddScoped<CatalogService>();builder.Services.AddScoped<QualityService>();builder.Services.AddScoped<DashboardService>();builder.Services.AddScoped<DocumentFileStore>();
await using var app=builder.Build();app.UseRouting();app.UseAuthentication();app.UseAuthorization();app.MapControllerRoute("default","{controller}/{action}/{id?}");await app.StartAsync();using var client=app.GetTestClient();
async Task<string> Html(string path,string user="test-reader"){using var req=new HttpRequestMessage(HttpMethod.Get,path);req.Headers.Add("X-Test-User",user);using var res=await client.SendAsync(req);Check(res.StatusCode==HttpStatusCode.OK,"render "+path+" "+user);return WebUtility.HtmlDecode(await res.Content.ReadAsStringAsync());}
foreach(var endpoint in new[]{"RequirementChart","RequirementPeople"}){
 using(var response=await client.GetAsync("/Dashboard/"+endpoint))Check(response.StatusCode==HttpStatusCode.Unauthorized,"anonymous blocked requirement drill "+endpoint);
 foreach(var suffix in new[]{"?DrillDivisionId=-1","?Level=invalid","?DrillTypeId=0","?DetailSort=sql","?EmployeeIds=bad"}){
  using var request=new HttpRequestMessage(HttpMethod.Get,"/Dashboard/"+endpoint+suffix);request.Headers.Add("X-Test-User","test-reader");using var response=await client.SendAsync(request);Check(response.StatusCode==HttpStatusCode.BadRequest,"invalid requirement drill rejected before SQL "+endpoint+suffix);
 }
}
var quality=await Html("/ui-probe/grid?kind=employees&quality=true&page=3");
foreach(var label in UiProbeController.QualityHeaders)Check(Regex.IsMatch(quality,"<th[^>]*>"+Regex.Escape(label)+"</th>"),"Quality column "+label);
Check(!quality.Contains("employee-quality-legend")&&!quality.Contains("employee-table-scroll"),"Quality does not inherit Catalog layout/legend");
var catalog=await Html("/ui-probe/grid?kind=employees&page=3");Check(catalog.Contains("employee-quality-legend")&&catalog.Contains("employee-table-scroll")&&catalog.Contains("data-employee-export"),"Catalog special layout and employee export preserved");
var employeeHead=Regex.Match(catalog,@"<thead>(.*?)</thead>",RegexOptions.Singleline).Groups[1].Value;
var employeeHeaders=Regex.Matches(employeeHead,@"<th\b[^>]*>(.*?)</th>",RegexOptions.Singleline).Select(m=>m.Groups[1].Value.Trim()).ToArray();
Check(employeeHeaders.SequenceEqual(new[]{"Mã cán bộ","Họ và tên","Số điện thoại","Email","Giới tính","Phòng ban","Chức danh","Trạng thái làm việc","Thao tác"}),"exact nine employee columns in required order");
Check(catalog.Contains("employee-quality-caption")&&catalog.Contains("employee-employment-badge"),"quality below code and separate employment badge");
foreach(var label in employeeHeaders)Check(catalog.Contains("data-label=\""+label+"\""),"mobile retains field label "+label);
Check(catalog.Contains("table-row-actions")&&quality.Contains("table-row-actions")&&!catalog.Contains("employee-action-profile"),"shared horizontal text actions for Catalog and Quality");
Check(!quality.Contains("employee-results-card")&&!quality.Contains("employee-employment-badge"),"Quality isolated from employee-only presentation");
foreach(var state in Labels.Employment){
 foreach(var value in new[]{state.Key,state.Value}){
 var stateHtml=await Html("/ui-probe/grid?kind=employees&employment="+Uri.EscapeDataString(value));
 Check(stateHtml.Contains("employee-employment--"+state.Key+"\""),"employment CSS code from raw/display value "+value);
 }
}
var adminDirectory=await Html("/ui-probe/grid?kind=employees","test-admin");
foreach(var action in new[]{"Hồ sơ","Chi tiết","Sửa","Xóa"})Check(adminDirectory.Contains("aria-label=\""+action+"\""),"Admin employee action retained "+action);
Check(catalog.Contains("aria-label=\"Hồ sơ\"")&&catalog.Contains("aria-label=\"Chi tiết\"")&&!catalog.Contains("aria-label=\"Sửa\"")&&!catalog.Contains("aria-label=\"Xóa\""),"Reader employee actions retained without write actions");
Check(catalog.Contains("returnUrl=%2FCatalog")&&catalog.Contains("page%3D3"),"row links retain list page");
Check(catalog.Contains("data-division-id=\"1\"")&&catalog.Contains("data-division-id=\"2\""),"organization metadata rendered");
var degrees=await Html("/ui-probe/grid?kind=qualifications");Check(!degrees.Contains("name=\"q\"")&&degrees.Contains("name=\"EmployeeId\""),"qualifications hide unsupported search but retain employee filter");
var documents=await Html("/ui-probe/grid?kind=documents");Check(Regex.IsMatch(documents,@"href=""[^""]*employeeId=TEST0001[^""]*"">Xóa bộ lọc"),"reset document filter preserves employee");
foreach(var org in new[]{"Divisions","Departments"}){
 foreach(var detail in new[]{false,true}){
  var path="/ui-probe/organization?org="+org+"&detail="+detail;
  var reader=await Html(path);var admin=await Html(path,"test-admin");
  Check(!Regex.IsMatch(reader,@"href=""/"+org+@"/(Create|Edit|Delete)"),org+" Reader no write links "+detail);
  Check(admin.Contains("/"+org+"/Edit"),org+" Admin edit available "+detail);
 }
 foreach(var action in new[]{"Create","Edit","Delete"}){using var req=new HttpRequestMessage(HttpMethod.Get,"/"+org+"/"+action+"/1");req.Headers.Add("X-Test-User","test-reader");using var res=await client.SendAsync(req);Check(res.StatusCode==HttpStatusCode.Forbidden,"server rejects Reader "+org+"/"+action);}
}
var empty=await Html("/ui-probe/organization?org=Departments&empty=true&divisionId=1");Check(empty.Contains("Xóa bộ lọc")&&empty.Contains("Không tìm thấy phòng ban trong phạm vi đã chọn"),"department division-only empty/reset correct");Check(!empty.Contains("Bạn có thể thêm phòng ban mới"),"Reader empty state does not invite writes");
var unfiltered=await Html("/ui-probe/organization?org=Departments&empty=true");Check(!unfiltered.Contains(">Xóa bộ lọc</a>")&&unfiltered.Contains("Chưa có phòng ban"),"unfiltered department empty state");
var noQuality=await Html("/ui-probe/profile");Check(noQuality.Contains("Chưa có kết quả đánh giá hồ sơ của cán bộ")&&!noQuality.Contains("Xuất Excel"),"profile unknown state explicit and no export");
var complete=await Html("/ui-probe/profile?assessed=true");Check(complete.Contains("Không có mục hồ sơ thiếu trong kết quả hiện tại")&&complete.Contains("không đồng nghĩa mọi tài liệu"),"zero missing does not claim valid");
var record=await Html("/ui-probe/record?returnUrl="+Uri.EscapeDataString("/Catalog/Index?kind=documents&employeeId=TEST0001&page=3"));Check(record.Contains("name=\"returnUrl\"")&&record.Contains("/Catalog/Index?kind=documents&employeeId=TEST0001&page=3"),"record back link and form preserve context");
var badReturn=await Html("/ui-probe/record?returnUrl=https%3A%2F%2Fevil.test");Check(!badReturn.Contains("href=\"https://evil.test"),"record refuses external return URL");
var problem=await Html("/Dashboard/ProblemDocuments");Check(!problem.Contains("Phạm vi KPI và biểu đồ:")&&!problem.Contains("Tài liệu hiện hành không hợp lệ hoặc đã hết hạn")&&problem.Contains("requirement-detail")&&problem.Contains("expiring-panel"),"ProblemDocuments removes only the two requested notes");

Check(problem.Contains("for=\"search-problem\"")&&problem.Contains("id=\"search-problem\""),"report search visible label linked");
Check(!problem.Contains("Bộ lọc tự động cập nhật")&&!problem.Contains("KPI, biểu đồ và hai bảng có phạm vi khác nhau")&&problem.Contains("chart-drill-toolbar"),"problem instructions removed; common chart toolbar retained");
Check(quality.Contains("ui-number"),"numeric Quality columns aligned");
foreach(var kind in new[]{"employees","qualifications","types","rules","documents"}){
 var html=await Html("/ui-probe/form?kind="+kind,"test-admin");
 Check(html.Contains("class=\"ui-form\"")&&html.Contains("ui-form-actions"),"shared form components "+kind);
 Check(html.Contains("aria-describedby")&&html.Contains("-error"),"field errors linked "+kind);
 if(kind=="employees")Check(html.Contains("Đúng 8 ký tự ASCII")&&html.Contains("Thông tin công tác")&&html.Contains("aria-invalid=\"true\""),"employee hint grouping and invalid state");
 if(kind is "types" or "qualifications")Check(html.Contains("<textarea")&&html.Contains("Nội dung kiểm thử"),"textarea preserves content "+kind);
 if(kind=="documents")Check(html.Contains("attachment-help attachment-error Attachment-error"),"attachment error linked");
}
foreach(var org in new[]{"Divisions","Departments"}){
 var target="/"+org+"?divisionId=2&q=Test&page=2&pageSize=20&sort=name";
 foreach(var invalid in new[]{false,true}){
  var retained=WebUtility.HtmlDecode(await Html("/ui-probe/org-form?org="+org+"&invalid="+invalid+"&returnUrl="+Uri.EscapeDataString(target),"test-admin"));
  Check(retained.Contains("name=\"returnUrl\" value=\""+target+"\"")&&retained.Contains("href=\""+target+"\""),"form hidden return and cancel preserve complete query "+org+" "+invalid);
  if(invalid)Check(retained.Contains("Lỗi nhập liệu thử"),"validation rendered with retained return "+org);
 }
 var html=await Html("/ui-probe/org-form?org="+org,"test-admin");Check(html.Contains("ui-form")&&html.Contains("Các trường có dấu *")&&html.Contains("ui-form-actions"),"organization form UI "+org);
}
foreach(var mode in new[]{"create","edit","details","delete"}){
 var html=await Html("/ui-probe/form?kind=types&mode="+mode+"&returnUrl=%2FCatalog%3Fkind%3Dtypes%26page%3D2","test-admin");
 Check(html.Contains(mode=="details"?">Quay lại</a>":">Hủy</a>"),"record cancel/back label "+mode);
 Check(mode=="details"?!html.Contains(">Lưu</button>"):html.Contains(mode=="delete"?">Xác nhận xóa</button>":">Lưu</button>"),"record save/delete label "+mode);
}
var emptyGrid=await Html("/ui-probe/empty-grid");Check(emptyGrid.Contains("Chưa có dữ liệu trong danh sách"),"unfiltered empty list explicit");
var filteredGrid=await Html("/ui-probe/empty-grid?q=sample");Check(filteredGrid.Contains("Không có kết quả phù hợp"),"filtered empty list distinct");
foreach(var state in new[]{"DangLamViec","TamHoanHDLD","NghiHuu","ChamDutHDLD"}){
 var html=await Html("/ui-probe/archive?status="+state);
 Check(html.Contains(ProfilePresentation.EmploymentLabel(state))&&!html.Contains("Đã nghỉ việc"),"profile canonical employment label "+state);
 var printed=await Html("/ui-probe/archive?print=true&status="+state);
 Check(printed.Contains(Labels.Text(state))&&!printed.Contains("Đã nghỉ việc"),"print canonical employment label "+state);
 Check(html.Contains("data-document-id=\"901\"")&&html.Contains("data-document-id=\"902\""),"Razor current/history visible without quality "+state);
 Check(html.Contains("historical-documents")&&html.Contains("/Catalog/Details/901"),"archive accordion and document details retained "+state);
 Check(!html.Contains("Xuất Excel")&&html.Contains("In lý lịch trích ngang"),"profile print preserved without export "+state);
 if(state is "NghiHuu" or "ChamDutHDLD")Check(html.Contains("Không áp dụng cảnh báo hiện hành")&&!html.Contains("<article class=\"profile-warning\">"),"retired/terminated have no warning cards "+state);
}
var dated=await Html("/ui-probe/document-system");
Check(!dated.Contains("Thông tin ghi nhận hệ thống")&&dated.Contains("01/11/2026 09:00")&&dated.Contains("12/11/2026 18:25"),"document dates include zero/nonzero minutes without offset");
Check(!Regex.IsMatch(dated,@"<(input|select|textarea)[^>]*name=""(CreatedAt|UpdatedAt)"),"system timestamps have no editable controls");
foreach(var mode in new[]{"edit","create","delete"}){var other=await Html("/ui-probe/document-system?mode="+mode);Check(!other.Contains("document-system-meta"),"system info only in details, not "+mode);}
Check(dated.IndexOf("document-system-meta")>dated.IndexOf("data-attachment-editor"),"timestamps follow attachments");
var noUpdate=await Html("/ui-probe/system-empty");Check(noUpdate.Contains("Chưa cập nhật")&&noUpdate.Contains("01/11/2026 09:12"),"nullable presentation fallback preserves creation time");
Console.WriteLine($"{passed} phase 1 + phase 2 + Rule 9 checks passed; isolated HTTP/Razor, no live SQL or browser.");

public class TestAuth(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder):AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder){protected override Task<AuthenticateResult> HandleAuthenticateAsync(){var name=Request.Headers["X-Test-User"].ToString();if(name=="")return Task.FromResult(AuthenticateResult.NoResult());return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,name)},"Test")),"Test")));}}
[Route("ui-probe")]
public class UiProbeController:Controller{

 [HttpGet("empty-grid")]public IActionResult EmptyGrid(string? q=null){RouteData.Values["controller"]="Catalog";return View("~/Views/Shared/Grid.cshtml",new GridModel{Kind="types",Filter=new(){Q=q}});}
 [HttpGet("form")]public IActionResult Form(string kind,string mode="create"){
  object entity=kind switch{"employees"=>new Employee{EmployeeId="TEST0001"},"qualifications"=>new Qualification{Notes="Nội dung kiểm thử"},"types"=>new DocumentType{Description="Nội dung kiểm thử"},"rules"=>new RequirementRule(),_=>new Document()};
  List<Field> fields=kind switch{
   "employees"=>[new("EmployeeId","Mã cán bộ",Required:true,Max:8),new("Email","Email","email",Max:254),new("DepartmentId","Phòng ban","select",true)],
   "qualifications"=>[new("Notes","Ghi chú",Max:500)],"types"=>[new("Description","Mô tả",Max:200)],"rules"=>[new("WarningThresholdDays","Ngưỡng cảnh báo","number")],_=>[new("StorageForm","Hình thức lưu trữ","select")]};
  if(kind=="employees")ModelState.AddModelError("Email","Email không đúng định dạng.");
  return View("~/Views/Shared/Record.cshtml",new EditRecord{Kind=kind,Entity=entity,Fields=fields,Editing=mode=="edit",ReadOnly=mode is "details" or "delete",Deleting=mode=="delete"});
 }
 [HttpGet("org-form")]public IActionResult OrgForm(string org,bool invalid=false){if(invalid){Request.ContentType="application/x-www-form-urlencoded";Request.Form=new FormCollection(new Dictionary<string,Microsoft.Extensions.Primitives.StringValues>{{"returnUrl",Request.Query["returnUrl"]}});ModelState.AddModelError("","Lỗi nhập liệu thử");}return View("~/Views/"+org+"/Form.cshtml",org=="Divisions"?(object)new DivisionForm{DivisionId=1}:new DepartmentForm{DepartmentId=1});}

 [HttpGet("archive")]public IActionResult Archive(string status,bool print=false){
  return View(print?"~/Views/Profiles/Print.cshtml":"~/Views/Profiles/Details.cshtml",new ProfileModel{Employee=new Employee{EmployeeId="TEST9001",FullName="Kiểm thử lưu trữ",EmploymentStatus=status},Department=new Department{Division=new Division()},DocumentTypes=new(){[901]=new(){DocumentTypeId=901,DocumentCode="HDXDTH1",DocumentName="Hợp đồng kiểm thử",DocumentGroup="Hợp đồng lao động"}},Documents=[new(){DocumentId=901,EmployeeId="TEST9001",DocumentTypeId=901,IsCurrent=true},new(){DocumentId=902,EmployeeId="TEST9001",DocumentTypeId=901,IsCurrent=false}],Warnings=[new(){DocumentId=901,DocumentName="Cảnh báo kiểm thử",QualityStatus="DA_HET_HAN"}]});
 }
 [HttpGet("document-system")]public IActionResult DocumentSystem(string mode="details")=>View("~/Views/Shared/Record.cshtml",new EditRecord{Kind="documents",ReadOnly=mode is "details" or "delete",Deleting=mode=="delete",Editing=mode!="create",Entity=new Document{EmployeeId="TEST9001",CreatedAt=new DateTime(2026,11,1,9,0,34),UpdatedAt=new DateTime(2026,11,12,18,25,45)},Fields=[]});
 [HttpGet("system-empty")]public IActionResult SystemEmpty()=>PartialView("~/Views/Shared/_DocumentSystemInfo.cshtml",new DocumentSystemInfo(new DateTime(2026,11,1,9,12,34),null));
 public static readonly string[] QualityHeaders=["Cán bộ","Yêu cầu","Đã có","Còn thiếu","Mã tài liệu thiếu","Tình trạng hợp đồng","Cảnh báo","Trạng thái"];
 [HttpGet("grid")]public IActionResult Grid(string kind="employees",bool quality=false,int page=1,string employment="DangLamViec"){RouteData.Values["controller"]=quality?"Quality":"Catalog";RouteData.Values["action"]=quality?"List":"Index";Request.Path=quality?"/Quality/List":"/Catalog/Index";var headers=quality?QualityHeaders:kind=="employees"?new[]{"Mã cán bộ","Họ và tên","Số điện thoại","Email","Giới tính","Phòng ban","Chức danh","Trạng thái làm việc"}:new[]{"Mã","Tên"};return View("~/Views/Shared/Grid.cshtml",new GridModel{Kind=kind,Title="Danh sách kiểm thử",Headers=headers,Total=60,Filter=new Filter{Page=page,EmployeeId=kind is "documents" or "qualifications"?"TEST0001":null},Filters=kind=="employees"?["DivisionId","DepartmentId"]:["EmployeeId"],Options=new(){["DivisionId"]=[new("A","1"),new("B","2")],["DepartmentId"]=[new("A1","11"){Group=new(){Name="1"}},new("B1","21"){Group=new(){Name="2"}}]},Rows=[new(){Cells=headers.Select((h,i)=>(string?)(i==0?"TEST0001":h=="Trạng thái làm việc"?employment:"Giá trị kiểm thử")).ToList(),Links=[new("Hồ sơ","Profiles","Details","TEST0001"),new("Chi tiết","Catalog","Details","TEST0001",kind),new("Sửa","Catalog","Edit","TEST0001",kind),new("Xóa","Catalog","Delete","TEST0001",kind)]}]});}
 [HttpGet("organization")]public IActionResult Organization(string org,bool detail=false,bool empty=false,int? divisionId=null){RouteData.Values["controller"]=org;RouteData.Values["action"]=detail?"Details":"Index";Request.Path="/"+org+"/Index";var division=new Division{DivisionId=1,DivisionCode="TEST",DivisionName="Đơn vị kiểm thử"};var department=new Department{DepartmentId=11,DivisionId=1,Division=division,DepartmentCode="TEST",DepartmentName="Phòng kiểm thử"};object model=org=="Divisions"?detail?division:new DivisionList{Items=empty?[]:[division],Total=empty?0:1}:detail?department:new DepartmentList{Items=empty?[]:[department],Total=empty?0:1,DivisionId=divisionId};return View("~/Views/"+org+"/"+(detail?"Details":"Index")+".cshtml",model);}
 [HttpGet("profile")]public IActionResult Profile(bool assessed=false){Request.Path="/Profiles/Details/TEST0001";RouteData.Values["controller"]="Profiles";return View("~/Views/Profiles/Details.cshtml",new ProfileModel{Employee=new Employee{EmployeeId="TEST0001",FullName="Kiểm thử",EmploymentStatus="DangLamViec"},Department=new Department{Division=new Division()},Quality=assessed?new EmployeeQuality{EmployeeQualityStatus="DAY_DU"}:null});}
 [HttpGet("record")]public IActionResult Record()=>View("~/Views/Shared/Record.cshtml",new EditRecord{Kind="documents",ReadOnly=true,Entity=new Document{EmployeeId="TEST0001"},Fields=[]});
}
