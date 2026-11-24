using System.IO.Compression;
using System.Xml.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vtb.Hr.Web.Controllers;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Services;
using Xlsx=Vtb.Hr.Web.Services.ExcelExport;
int passed=0;
void Check(bool value,string name){if(!value)throw new Exception(name);Console.WriteLine("PASS "+name);passed++;}
var statusRows=Labels.Employment.Select(x=>new object?[]{x.Key,Labels.Text(x.Key)}).ToList();
using(var statusZip=new ZipArchive(new MemoryStream(Xlsx.Write(new ExcelTable("Nhãn kiểm thử",["Mã","Trạng thái"],statusRows),"Kiểm thử")))){
 XNamespace statusNs="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
 var statusSheet=XDocument.Load(statusZip.GetEntry("xl/worksheets/sheet1.xml")!.Open());
 foreach(var pair in Labels.Employment)Check(statusSheet.Descendants(statusNs+"t").Any(t=>t.Value==pair.Value),"XLSX employment label "+pair.Key);
}
var rows=Enumerable.Range(0,250).Select(i=>new object?[]{i.ToString("D8"),i==0?"=HYPERLINK(\"https://example.test\")":"Nguyễn Văn Kiểm thử",new DateOnly(2020,6,1),i,(i%2==0?null:"+SUM(1,2)")}).ToList();
var bytes=Xlsx.Write(new ExcelTable("Danh sách kiểm thử",["Mã","Họ tên","Ngày vào làm","Số lượng","Ghi chú"],rows),"Phòng ban kiểm thử");
Directory.CreateDirectory(".scaffold/export-test");File.WriteAllBytes(".scaffold/export-test/format-test.xlsx",bytes);
using(var zip=new ZipArchive(new MemoryStream(bytes))){
 XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
 var sheet=XDocument.Load(zip.GetEntry("xl/worksheets/sheet1.xml")!.Open());
 Check(sheet.Descendants(ns+"row").Count()==254,"250 data rows plus metadata/header, no pagination truncation");
 Check(!sheet.Descendants(ns+"f").Any(),"no formula elements");
 Check(sheet.Descendants(ns+"c").Single(c=>(string?)c.Attribute("r")=="B6").Attribute("t")?.Value=="inlineStr","formula-like input explicitly text");
 Check(sheet.Descendants(ns+"c").Single(c=>(string?)c.Attribute("r")=="A6").Value=="00000000","leading zero employee ID preserved");
 Check(sheet.Descendants(ns+"autoFilter").Single().Attribute("ref")?.Value=="A5:E255","filter covers all records");
 Check(sheet.Descendants(ns+"pane").Single().Attribute("ySplit")?.Value=="5","header frozen");
 Check(sheet.Descendants(ns+"c").Single(c=>(string?)c.Attribute("r")=="C6").Attribute("s")?.Value=="4","dates use date number format");
 Check(sheet.Descendants(ns+"c").Single(c=>(string?)c.Attribute("r")=="C7").Attribute("s")?.Value=="5","alternating row preserves date format");
 Check(sheet.Descendants(ns+"t").Any(t=>t.Value=="Nguyễn Văn Kiểm thử"),"Vietnamese Unicode preserved");
 foreach(var entry in zip.Entries)using(var stream=entry.Open())XDocument.Load(stream);
 Check(true,"all Open XML parts parse");
}
Check(!Xlsx.FileName("../test:bad/path").Contains('/')&&!Xlsx.FileName("test:bad").Contains(':'),"safe download name");
try{Xlsx.CheckCount(20001);throw new Exception("missing row limit");}catch(InvalidDataException){Check(true,"oversized export rejected instead of truncated");}
var dbOptions=new DbContextOptionsBuilder<VtbContext>().UseSqlServer("Server=invalid.example;Database=NoConnection;Integrated Security=True;Connect Timeout=1").Options;
using(var db=new VtbContext(dbOptions)){
 var catalog=new CatalogService(db);var controller=new ExportController(db,catalog,new QualityService(db,catalog),new DashboardService(db)){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext()}};
 var run=typeof(ExportController).GetMethod("Run",BindingFlags.NonPublic|BindingFlags.Instance)!;
 async Task<IActionResult> Run(ExcelTable table)=>await (Task<IActionResult>)run.Invoke(controller,new object[]{(Func<CancellationToken,Task<ExcelTable>>)(_=>Task.FromResult(table))})!;
 Check(await Run(new ExcelTable("Trống",["Mã"],[])) is NotFoundObjectResult,"empty results produce clear no-data response");
 Check(await Run(new ExcelTable("Danh sách",["Mã"],[new object?[]{"001"}])) is FileContentResult {ContentType:Xlsx.Mime},"download result is actual xlsx MIME");
 Check(await controller.Employees(new Filter(),"division") is BadRequestObjectResult,"division mode requires selection before SQL");
 Check(await controller.Employees(new Filter(),"department") is BadRequestObjectResult,"department mode requires selection before SQL");
 Check(await controller.Employees(new Filter(),"arbitrary") is BadRequestObjectResult,"invalid mode rejected");
 Check(await controller.Catalog("secrets",new Filter()) is BadRequestObjectResult,"unknown export category rejected");
}
var builder=WebApplication.CreateBuilder(new WebApplicationOptions{ContentRootPath=Directory.GetCurrentDirectory(),EnvironmentName="Testing"});
builder.Logging.ClearProviders();builder.WebHost.UseTestServer();
builder.Services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions,TestAuth>("Test",_=>{});
builder.Services.AddAuthorization(o=>{var read=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireClaim("read","yes").Build();o.DefaultPolicy=read;o.FallbackPolicy=read;});
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(ExportController).Assembly).AddApplicationPart(typeof(ExportProbeController).Assembly);
builder.Services.AddDbContext<VtbContext>(o=>o.UseSqlServer("Server=invalid.example;Database=NoConnection;Integrated Security=True;Connect Timeout=1"));
builder.Services.AddScoped<CatalogService>();builder.Services.AddScoped<QualityService>();builder.Services.AddScoped<DashboardService>();
await using var app=builder.Build();app.UseRouting();app.UseAuthentication();app.UseAuthorization();app.MapControllerRoute("default","{controller}/{action}/{id?}");await app.StartAsync();
using var client=app.GetTestClient();
foreach(var endpoint in new[]{"Employees","Catalog","Organization","Overview","Report","Documents","Quality","EmployeeOptions","RequirementPeople"}){
 using var anon=await client.GetAsync("/Export/"+endpoint);Check((int)anon.StatusCode==401,"anonymous blocked "+endpoint);
 using var request=new HttpRequestMessage(HttpMethod.Get,"/Export/"+endpoint);request.Headers.Add("X-Test-User","denied");using var denied=await client.SendAsync(request);Check((int)denied.StatusCode==403,"unpermitted account blocked "+endpoint);
}
foreach(var url in new[]{"/Export/RequirementPeople?DrillDivisionId=-1","/Export/RequirementPeople?DetailSort=sql","/Export/Employees?mode=division","/Export/Employees?mode=department","/Export/Overview?Statuses=invalid","/Export/Report?Kind=contractDetail","/Export/Documents?State=invalid","/Export/Overview?DivisionIds=-1","/Export/Report?Sort=SQL"}){
 using var request=new HttpRequestMessage(HttpMethod.Get,url);request.Headers.Add("X-Test-User","reader");using var response=await client.SendAsync(request);Check((int)response.StatusCode==400,"invalid parameters rejected over HTTP "+url);
}
foreach(var url in new[]{"/export-probe/grid","/export-probe/employees","/Dashboard/ProblemDocuments","/Dashboard/Contract","/Dashboard/ContractDetails?Kind=contractDetail&State=HOP_LE","/Dashboard/DocumentDetails?State=SAP_HET_HAN"}){
 using var request=new HttpRequestMessage(HttpMethod.Get,url);request.Headers.Add("X-Test-User","reader");using var response=await client.SendAsync(request);var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 Check((int)response.StatusCode==200&&html.Contains("excel-export.js"),"Razor renders export script "+url);
 if(url.EndsWith("/grid"))Check(html.Contains("/Export/Catalog?")&&html.Contains("q=Test")&&html.Contains("kind=types"),"Razor export URL carries exact grid filter route values");
 if(url.EndsWith("/employees"))Check(html.Contains("data-employee-export")&&html.Contains("Xuất danh sách cán bộ"),"employee modal trigger rendered");
}
Console.WriteLine($"{passed} checks passed. No live SQL connection or writes.");
public class TestAuth(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder):AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder){
 protected override Task<AuthenticateResult> HandleAuthenticateAsync(){var user=Request.Headers["X-Test-User"].ToString();if(user=="")return Task.FromResult(AuthenticateResult.NoResult());var claims=new[]{new Claim(ClaimTypes.Name,user),new Claim("read",user=="reader"?"yes":"no")};return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims,"Test")),"Test")));}
}

// Test-only rendering endpoints; never included in the production project.
[Route("export-probe")]
public class ExportProbeController:Controller{
 [HttpGet("grid"),HttpGet("employees")]
 public IActionResult Grid(){var employees=Request.Path.Value!.EndsWith("employees");RouteData.Values["controller"]="Catalog";return View("~/Views/Shared/Grid.cshtml",new GridModel{Kind=employees?"employees":"types",Title="Kiểm thử giao diện",Headers=employees?["Mã cán bộ","Họ và tên"]:["Mã","Tên"],Filter=new Filter{Q="Test",DivisionId=1}});}
}
