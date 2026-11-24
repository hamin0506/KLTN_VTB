using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vtb.Hr.Web.Controllers;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Security;
using Vtb.Hr.Web.Services;
// Run from the repository, or pass --repo-root <checkout>. No application configuration is loaded.
string? Argument(string name){var i=Array.IndexOf(args,name);if(i<0)return null;if(i+1>=args.Length)throw new ArgumentException("Missing value for "+name);return args[i+1];}
string FindRoot(){
 foreach(var start in new[]{Directory.GetCurrentDirectory(),AppContext.BaseDirectory})
  for(var dir=new DirectoryInfo(start);dir!=null;dir=dir.Parent)
   if(File.Exists(Path.Combine(dir.FullName,"Vtb.Hr.Web.csproj")))return dir.FullName;
 throw new InvalidOperationException("Run from the repository or supply --repo-root <checkout>.");
}
var repository=Path.GetFullPath(Argument("--repo-root")??FindRoot());
if(!File.Exists(Path.Combine(repository,"Vtb.Hr.Web.csproj")))throw new ArgumentException("Invalid --repo-root: project not found.");
using var scratch=new VerificationScratch();
// Reserved .invalid host plus a fail-closed interceptor: translation only, never connect to SQL.
const string translationConnection="Server=verification.invalid;Database=VerificationOnly;Integrated Security=True;Connect Timeout=1";
var offline=new OfflineDatabase();
DbContextOptionsBuilder<VtbContext> ConfigureDb(DbContextOptionsBuilder<VtbContext> b)=>b.UseSqlServer(translationConnection).AddInterceptors(offline);
var failures=new List<string>();var passed=0;var skipped=0;
void Skip(string label){skipped++;Console.WriteLine("SKIP "+label);}
void Check(bool value,string label){Console.WriteLine((value?"PASS ":"FAIL ")+label);if(value)passed++;else failures.Add(label);}
// Read-only LINQ fixtures exercise the same projection/predicate as the SQL query.
{
 var staff=Enumerable.Range(1,7).Select(i=>new Employee{EmployeeId="CASE"+i,FullName="Case "+i,DepartmentId=1}).AsQueryable();
 var counts=new[]{3,0,0,2,0,0,0};
 var issues=new[]{"Thiếu hồ sơ","Cần ký tiếp HĐLĐ","Thiếu HĐXĐTH1","Thiếu hồ sơ; Cần ký tiếp HĐLĐ","Không có cảnh báo","Không có cảnh báo","Không có cảnh báo"};
 var quality=staff.Select(e=>new EmployeeQuality{EmployeeId=e.EmployeeId,TotalDocumentsMissing=counts[int.Parse(e.EmployeeId.Substring(4))-1],EmployeeQualityStatus=int.Parse(e.EmployeeId.Substring(4))<=4?"THIEU_HO_SO":"DAY_DU",QualityIssue=issues[int.Parse(e.EmployeeId.Substring(4))-1]}).ToList().AsQueryable();
 var states=new[]{"HOP_LE","CAN_KY_KET_TIEP","THIEU_HDXDTH1","CAN_KY_KET_TIEP","HOP_LE","HOP_LE","KHONG_CANH_BAO"};
 var contracts=staff.Select(e=>new LaborContractCheck{EmployeeId=e.EmployeeId,LaborContractStatus=states[int.Parse(e.EmployeeId.Substring(4))-1]}).ToList();
 contracts.Add(contracts[3]);
 var rows=DashboardService.FollowupQuery(staff,quality,contracts.AsQueryable(),new[]{new Department{DepartmentId=1,DepartmentName="Test"}}.AsQueryable()).OrderByDescending(x=>x.Missing).ThenBy(x=>x.EmployeeId).ToList();
 Check(rows.Select(x=>x.EmployeeId).SequenceEqual(new[]{"CASE1","CASE4","CASE2","CASE3"}),"followup missing desc, contract-only zero included, valid and neutral excluded");
 Check(rows.Single(x=>x.EmployeeId=="CASE1").Issue=="Thiếu hồ sơ"&&rows.Single(x=>x.EmployeeId=="CASE1").Missing==3,"followup missing with valid contract keeps actual count");
 Check(rows.Single(x=>x.EmployeeId=="CASE2") is {Missing:0,Issue:"Cần ký tiếp HĐLĐ"},"followup renewal does not increment missing count");
 Check(rows.Single(x=>x.EmployeeId=="CASE3").Issue=="Thiếu HĐXĐTH1","followup missing first contract abbreviated label");
 Check(rows.Single(x=>x.EmployeeId=="CASE4") is {Missing:2,Issue:"Thiếu hồ sơ; Cần ký tiếp HĐLĐ"},"followup combined issues one row despite duplicate contract source");
 Check(rows.Skip(2).Take(2).All(x=>x.Missing==0),"followup paging retains zero missing employees");
 foreach(var issue in new[]{"Tài liệu bắt buộc đã hết hạn","Tài liệu bắt buộc không hợp lệ"}){
  var assessed=new[]{new EmployeeQuality{EmployeeId="CASE5",EmployeeQualityStatus="THIEU_HO_SO",QualityIssue=issue,TotalDocumentsMissing=0}}.AsQueryable();
  var row=DashboardService.FollowupQuery(staff,assessed,contracts.AsQueryable(),new[]{new Department{DepartmentId=1}}.AsQueryable()).Single();
  Check(row.Missing==0&&row.Issue==issue,"followup keeps zero-missing View issue: "+issue);
 }
 var noQuality=DashboardService.FollowupQuery(staff.Where(x=>x.EmployeeId=="CASE2"),Array.Empty<EmployeeQuality>().AsQueryable(),contracts.AsQueryable(),new[]{new Department{DepartmentId=1}}.AsQueryable()).ToList();
 Check(noQuality.Count==0,"no assessment row is not synthesized from contract rows");
 Check(!DashboardService.FollowupQuery(staff,Array.Empty<EmployeeQuality>().AsQueryable(),Array.Empty<LaborContractCheck>().AsQueryable(),new[]{new Department{DepartmentId=1}}.AsQueryable()).Any(),"empty source evaluations yield no followup rows");

}
{
 var filter=new Filter{Q="test",DivisionId=2,DepartmentId=3,Status="TamHoanHDLD",Quality="error",Page=4};
 var routes=filter.Routes("employees");Check(routes["quality"]=="THIEU_HO_SO"&&routes["status"]=="TamHoanHDLD","quality and employment persist independently in pagination routes");
 var toggle=filter.QualityRoutes("employees","THIEU_HO_SO");Check(toggle["quality"]==""&&toggle["page"]=="1"&&toggle["divisionId"]=="2"&&toggle["departmentId"]=="3"&&toggle["q"]=="test","quality toggle resets page and retains all other scope filters");
 Check(filter.QualityRoutes("employees","DAY_DU")["quality"]=="DAY_DU"&&filter.QualityRoutes("employees",null)["quality"]=="","quality switch and all states");
 filter.Quality="unknown";Check(filter.Quality==null,"quality whitelist excludes unknown from color choices");
}
Check(DashboardListReadException.Describe(207).Status==500&&DashboardListReadException.Describe(208).Status==500,"invalid column/object is a query error, not connection");
Check(DashboardListReadException.Describe(53).Status==503&&DashboardListReadException.Describe(-2).Message.Contains("thời gian"),"connection and timeout are distinguished");
Check(DashboardListReadException.Describe(null).Status==500,"mapping and unknown failures not mislabeled as connection");
async Task VerifyCancellation(DbContextOptions<VtbContext> options){
// Exercise the actual controller read boundary without touching SQL.
using(var cancelContext=new VtbContext(options)){
 var controller=new DashboardController(new DashboardService(cancelContext),Microsoft.Extensions.Logging.Abstractions.NullLogger<DashboardController>.Instance){ControllerContext=new ControllerContext{HttpContext=new DefaultHttpContext()}};
 var read=typeof(DashboardController).GetMethod("Read",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.MakeGenericMethod(typeof(int));
 Func<Task<int>> canceled=()=>Task.FromException<int>(new TaskCanceledException("simulated request cancellation"));
 using var cancellation=new CancellationTokenSource();cancellation.Cancel();controller.HttpContext.RequestAborted=cancellation.Token;
 var result=await (Task<IActionResult>)read.Invoke(controller,new object[]{new OverviewFilter(),canceled})!;
 Check(result is StatusCodeResult status&&status.StatusCode==499,"client request cancellation handled as 499");
 controller.HttpContext.RequestAborted=CancellationToken.None;
 try{await (Task<IActionResult>)read.Invoke(controller,new object[]{new OverviewFilter(),canceled})!;Check(false,"unrelated cancellation remains visible");}catch(TaskCanceledException){Check(true,"unrelated cancellation remains visible");}
}
}
if(args.Contains("--cancellation-only")){
 await VerifyCancellation(ConfigureDb(new DbContextOptionsBuilder<VtbContext>()).Options);
 Console.WriteLine($"RESULT: {passed} passed, {failures.Count} failed. No database connection attempted.");Environment.ExitCode=failures.Count==0?0:1;return;
}
bool Valid(object item){var errors=new List<ValidationResult>();return Validator.TryValidateObject(item,new ValidationContext(item),errors,true);}
Check(!Valid(new Employee{EmployeeId="SHORT",FullName="Test",DepartmentId=1}),"employee PK length");
Check(!Valid(new Employee{EmployeeId="TEST0001",FullName="Test",DepartmentId=1,Gender="Invalid"}),"employee gender CHECK");
Check(!Valid(new Employee{EmployeeId="TEST0001",FullName="Test",DepartmentId=1,LeaveDate=new DateOnly(2000,1,1)}),"employee leave/join dates");
Check(!Valid(new Document{EmployeeId="TEST0001",DocumentTypeId=1,IssueDate=new DateOnly(2026,1,2),ExpiryDate=new DateOnly(2026,1,1)}),"document expiry CHECK");
Check(!Valid(new DocumentType{DocumentCode="X",DocumentName="Test",DocumentGroup="Invalid"}),"document group CHECK");
Check(!Valid(new RequirementRule{DocumentTypeId=1,WarningThresholdDays=-1}),"rule threshold CHECK");
Check(!Valid(new RequirementRule{DocumentTypeId=1,EffectiveFrom=new DateOnly(2026,2,1),EffectiveTo=new DateOnly(2026,1,1)}),"rule date CHECK");
Check(!Valid(new RequirementRule{DocumentTypeId=1,RequirementLevel="Invalid"}),"rule level CHECK");
Check(SecureEmbed.Validate("https://app.powerbi.com/reportEmbed?reportId=11111111-1111-1111-1111-111111111111")!=null,"Secure Embed allow valid URL");
foreach(var url in new[]{"https://app.powerbi.com/view?r=public","http://app.powerbi.com/reportEmbed?reportId=x","https://evil.example/reportEmbed?reportId=x","javascript:alert(1)","https://app.powerbi.com.evil.example/reportEmbed?reportId=x"})Check(SecureEmbed.Validate(url)==null,"Secure Embed rejects "+url.Split('?')[0]);
var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{["ConnectionStrings:VTB2"]=translationConnection,["Access:Administrators:0"]="VERIFICATION\\admin",["Access:Readers:0"]="TEST\\reader"}).Build();
ClaimsPrincipal User(string name,bool auth=true)=>new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,name)},auth?"Test":null));
Check(Access.Admin(User("verification\\ADMIN"),config),"admin allowlist case insensitive");Check(!Access.Read(User("VERIFICATION\\admin",false),config),"anonymous cannot spoof admin name");Check(Access.Read(User("TEST\\reader"),config)&&!Access.Admin(User("TEST\\reader"),config),"reader cannot write");Check(!Access.Read(User("TEST\\outsider"),config),"unlisted user denied");
var options=ConfigureDb(new DbContextOptionsBuilder<VtbContext>()).Options;
using(var db=new VtbContext(options)){
 Check(db.Model.GetEntityTypes().Count(e=>e.GetViewName()!=null)==4,"four read-only View mappings");
 Check(db.Model.GetEntityTypes().Where(e=>e.GetViewName()!=null).All(e=>e.FindPrimaryKey()==null),"View entities are keyless");
 Check(db.Model.GetEntityTypes().Count(e=>e.GetViewName()==null)==7,"seven table mappings including qualification");
 var doc=db.Model.FindEntityType(typeof(Document))!;Check(doc.GetIndexes().Any(i=>i.IsUnique&&i.GetFilter()=="[is_current] = 1"),"current document filtered unique index");
 var currentProperty=doc.FindProperty(nameof(Document.IsCurrent))!;
 Check(Equals(currentProperty.GetDefaultValue(),false),"document current default is false");
 Check(currentProperty.ValueGenerated==Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAddOrUpdate,"current value read back from database");
 Check(currentProperty.GetBeforeSaveBehavior()==Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore,"INSERT ignores caller-supplied IsCurrent");
 Check(currentProperty.GetAfterSaveBehavior()==Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore,"UPDATE ignores caller-supplied IsCurrent");
 Check(!doc.IsSqlOutputClauseUsed(),"document writes compatible with AFTER INSERT trigger");
 Check(doc.FindProperty(nameof(Document.CreatedAt))!.GetBeforeSaveBehavior()==Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore,"creation timestamp owned by database");
 Check(!CatalogService.Fields("documents").Any(x=>x.Name==nameof(Document.IsCurrent)),"document form binding excludes current flag");
 Check(!new Document().IsCurrent,"new entity does not default current to true");
 Check(db.Model.FindEntityType(typeof(RequirementRule))!.GetForeignKeys().Count()==0,"no invented requirement_rule FK");
 Check(db.Model.FindEntityType(typeof(Department))!.GetForeignKeys().Single().DeleteBehavior==DeleteBehavior.NoAction,"department FK NO ACTION");
  var schemaPath=Argument("--schema");
 if(schemaPath==null)Skip("DDL snapshot comparison (supply --schema <schema.sql>); EF mapping tests still run.");
 else {
 var schema=File.ReadAllText(Path.GetFullPath(schemaPath));
 foreach(System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(schema,@"(?s)CREATE TABLE \[dbo\]\.\[(\w+)\]\((.*?)PRIMARY KEY")) {
  var name=match.Groups[1].Value;
  var expected=System.Text.RegularExpressions.Regex.Matches(match.Groups[2].Value,@"\[(\w+)\]\s+\[\w+\]").Select(x=>x.Groups[1].Value).ToHashSet();

  var entity=db.Model.GetEntityTypes().Single(x=>x.GetTableName()==name&&x.GetViewName()==null);
  var table=Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(name,"dbo");
  Check(expected.SetEquals(entity.GetProperties().Select(x=>x.GetColumnName(table)!)),"all mapped columns match schema: "+name);
 }
 }
 var viewProperty=db.Model.FindEntityType(typeof(DocumentQuality))!.FindProperty(nameof(DocumentQuality.IsKpiScope))!;
 Check(viewProperty.GetColumnName(Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.View("vw_document_quality","dbo"))=="working_status"&&viewProperty.ClrType==typeof(int),"document KPI mapping is working_status / int");
 var catalog=new CatalogService(db);var quality=new QualityService(db,catalog);
 var scopeSql=quality.Scope(new Filter{DivisionId=1,DepartmentId=1,KpiOnly=true,Q="test"}).ToQueryString();
 Check(scopeSql.Contains("DangLamViec")&&scopeSql.Contains("TamHoanHDLD")&&!scopeSql.Contains("is_kpi_scope"),"scope query retains both KPI employment statuses without removed View column");
 Check(QualityService.ConflictingDocuments(db.DocumentQualities).ToQueryString().Contains("EXISTS"),"conflicting-state SQL query translates");
 Check(db.EmployeeQualities.Where(x=>x.EmployeeQualityStatus=="DAY_DU").ToQueryString().Contains("vw_employee_quality"),"quality query uses SQL View");
}
// Isolated in-memory HTTP host. Test authentication exists only in this harness.
// No fake business rows, no database creates, no valid write requests.
var builder=WebApplication.CreateBuilder(new WebApplicationOptions{ContentRootPath=scratch.Root,EnvironmentName="Verification"});
builder.Configuration.Sources.Clear();
builder.Environment.WebRootFileProvider=new PhysicalFileProvider(Path.Combine(repository,"wwwroot"));
builder.Logging.ClearProviders();builder.Configuration.AddConfiguration(config);
builder.WebHost.UseTestServer();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions,ProbeAuth>("Test",_=>{});
builder.Services.AddAuthorization(o=>{var read=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireAssertion(c=>Access.Read(c.User,builder.Configuration)).Build();o.DefaultPolicy=read;o.FallbackPolicy=read;o.AddPolicy("Admin",p=>p.RequireAuthenticatedUser().RequireAssertion(c=>Access.Admin(c.User,builder.Configuration)));});
builder.Services.AddControllersWithViews(o=>{o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());o.Filters.Add<WriteAccessFilter>();o.Filters.Add<DatabaseErrorFilter>();}).AddApplicationPart(typeof(HomeController).Assembly).AddApplicationPart(typeof(ProbeController).Assembly);
builder.Services.AddDbContext<VtbContext>(o=>o.UseSqlServer(translationConnection).AddInterceptors(offline));
builder.Services.AddScoped<CatalogService>();builder.Services.AddScoped<DocumentFileStore>();builder.Services.AddScoped<QualityService>();builder.Services.AddScoped<DashboardService>();
await using var app=builder.Build();app.UseRouting();app.UseAuthentication();app.UseAuthorization();app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");await app.StartAsync();
using var client=app.GetTestClient();
async Task<System.Net.Http.HttpResponseMessage> Send(string user,string url,HttpMethod? method=null){var req=new HttpRequestMessage(method??HttpMethod.Get,url);if(user!="")req.Headers.Add("X-Test-User",user);return await client.SendAsync(req);}
foreach(var url in new[]{"/","/Divisions","/Departments","/Catalog?kind=employees","/Quality/List?kind=documents","/PowerBI"})using(var r=await Send("",url))Check((int)r.StatusCode==401,"anonymous 401 "+url);
using(var r=await Send("VERIFICATION\\admin","/Dashboard")){
 var html=System.Net.WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());
 Check(!html.Contains("Executive Overview")&&!html.Contains("Problem documents")&&!html.Contains("Contract —")&&html.Contains(">Tổng quan</a>")&&html.Contains(">Chất lượng hồ sơ</a>")&&html.Contains(">Hợp đồng lao động</a>"),"Vietnamese-only overview headings and report tabs");
 Check((int)r.StatusCode==200 && html.Contains("executive-overview")&&!html.Contains("<iframe"),"Executive Overview renders without iframe");
 Check(!html.Contains(">Hồ sơ cán bộ</a>") && !html.Contains(">Chất lượng tài liệu</a>") && html.Contains(">Danh sách loại tài liệu</span>"),"simplified sidebar links");
}
using(var r=await Send("TEST\\reader","/Catalog/Delete/1?kind=rules"))Check((int)r.StatusCode==403,"reader cannot delete rule");
using(var r=await Send("VERIFICATION\\admin","/Catalog/Delete/1?kind=rules",HttpMethod.Post))Check((int)r.StatusCode==400,"rule deletion requires CSRF");
using(var r=await Send("VERIFICATION\\admin","/Catalog/Delete/0?kind=rules"))Check((int)r.StatusCode==404,"rule deletion rejects invalid id");
using(var r=await Send("TEST\\outsider","/Home/Setup"))Check((int)r.StatusCode==403,"unlisted 403");
foreach(var url in new[]{"/Divisions/Create","/divisions/create","/departments/edit/1","/Departments/Edit/1","/Catalog/Create?kind=documents","/Catalog/Edit/1?kind=rules","/Catalog/Delete/1?kind=types"})using(var r=await Send("TEST\\reader",url))Check((int)r.StatusCode==403,"reader 403 "+url);
using(var r=await Send("TEST\\reader","/Catalog/Create?kind=employees",HttpMethod.Post))Check((int)r.StatusCode==403,"reader POST forbidden");
using(var r=await Send("VERIFICATION\\admin","/Catalog/Create?kind=employees",HttpMethod.Post))Check((int)r.StatusCode==400,"admin POST without CSRF 400");
foreach(var url in new[]{"/Home/Setup","/Dashboard","/Divisions/Create","/__probe/grid","/__probe/dashboard","/__probe/record","/__probe/profile","/__probe/department"}){
 try{using var r=await Send("VERIFICATION\\admin",url);var html=await r.Content.ReadAsStringAsync();Check((int)r.StatusCode==200&&System.Net.WebUtility.HtmlDecode(html).Contains("Quản lý hồ sơ cán bộ"),"render "+url);if(url=="/__probe/grid")Check(System.Net.WebUtility.HtmlDecode(html).Contains("Chưa có dữ liệu trong danh sách"),"unfiltered empty grid rendered without implying a filter mismatch");}
 catch(Exception ex){Check(false,"render "+url+" "+ex.GetBaseException().Message);}
}
using(var r=await Send("TEST\\reader","/Home/Setup")){var html=await r.Content.ReadAsStringAsync();Check((int)r.StatusCode==200,"reader can view setup");}
using(var r=await Send("VERIFICATION\\admin","/Catalog?kind=not-real"))Check((int)r.StatusCode==404,"unknown catalog 404");
var descriptors=app.Services.GetRequiredService<IActionDescriptorCollectionProvider>().ActionDescriptors.Items.OfType<ControllerActionDescriptor>().ToList();
Check(new[]{"Catalog","Profiles","Quality","PowerBI","Divisions","Departments"}.All(c=>descriptors.Any(a=>a.ControllerName==c)),"all feature controllers registered");
foreach(var url in new[]{"/Catalog?kind=employees","/Catalog?kind=documents&employeeId=TEST0001","/Catalog?kind=types","/Catalog?kind=rules","/Quality/List?kind=documents","/Quality/List?kind=requirements","/Quality/List?kind=contracts","/Quality/List?kind=employees","/Dashboard"}){
 try{using var r=await Send("VERIFICATION\\admin",url);Check((int)r.StatusCode==(url=="/Dashboard"?200:503),"GET route uses simulated unavailable database: "+url);}
 catch(Exception ex){Check(false,"GET "+url+" "+ex.GetBaseException().Message);}
}
foreach(var legacy in new[]{"/", "/Home/Index", "/PowerBI"})using(var r=await Send("VERIFICATION\\admin",legacy))Check((int)r.StatusCode==302 && r.Headers.Location?.ToString()=="/Dashboard","legacy report redirects "+legacy);
using(var r=await Send("VERIFICATION\\admin","/Catalog?kind=documents"))Check((int)r.StatusCode==302 && r.Headers.Location?.ToString().Contains("kind=types")==true,"global document list redirects to document types");
using(var r=await Send("VERIFICATION\\admin","/Dashboard")){var html=System.Net.WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());Check(html.Contains("aria-expanded=\"false\"")&&html.Contains("chevron-right")&&!html.Contains("⌄"),"sidebar accessible chevron");Check(!html.Contains("VTB People")&&!html.Contains("Khóa luận")&&html.Contains("Báo cáo trực quan"),"clean report branding");}
using(var typeContext=new VtbContext(options))Check((await new CatalogService(typeContext).Options("types")).Keys.SequenceEqual(new[]{"DocumentGroup"}),"document type options do not query personnel database");
Check(DashboardService.DaxCount(0)==null && DashboardService.DaxCount(3)==3,"COUNTROWS zero is BLANK, count preserved");
Check(DashboardService.DaxDivide(null,5)==null && DashboardService.DaxDivide(2,0)==null && DashboardService.DaxDivide(2,null)==null && DashboardService.DaxDivide(1,4)==0.25,"DIVIDE preserves BLANK and denominator zero");
Check(DashboardService.DocumentLabel("KHONG_XAC_DINH")==null && DashboardService.DocumentLabel("HOP_LE")=="Hợp lệ","DAX SWITCH unknown status is BLANK");
Check(!Valid(new OverviewFilter{Sort="id; DROP TABLE employee"})&&!Valid(new OverviewFilter{PageSize=1000})&&!Valid(new OverviewFilter{Statuses=["NghiHuu"]}),"dashboard rejects invalid sort size and slicer value");
using(var ctx=new VtbContext(options)){
 var dash=new DashboardService(ctx);var f=new OverviewFilter{Statuses=["DangLamViec"],DivisionIds=[1],DepartmentIds=[2],Search="' OR 1=1 --"};
 var employeeSql=dash.EmployeeMeasureScope(f).ToQueryString();var docSql=dash.DocumentMeasureScope(f).ToQueryString();var tableSql=dash.MissingQuery(f).OrderByDescending(x=>x.Missing).ThenBy(x=>x.EmployeeId).Skip(20).Take(20).ToQueryString();
 Check(employeeSql.Contains("vw_employee_quality")&&employeeSql.Contains("EXISTS"),"employee slicers translate via employee and department relationships");
 Check(docSql.Contains("vw_document_quality")&&docSql.Contains("[working_status] = 1")&&!docSql.Contains("is_kpi_scope")&&!docSql.Contains("DISTINCT"),"document Measure scope translates without DISTINCT");
 Check(!tableSql.Contains("is_kpi_scope")&&tableSql.Contains("DangLamViec")&&tableSql.Contains("TamHoanHDLD"),"missing table keeps KPI employment scope without removed column");
 Check(!ctx.EmployeeQualities.ToQueryString().Contains("is_kpi_scope")&&!ctx.DocumentRequirements.ToQueryString().Contains("has_document"),"full View materialization excludes both removed columns");
 Check(tableSql.Contains("OFFSET")&&tableSql.Contains("@")&&tableSql.Contains("total_documents_missing"),"table query parameters visual filters and server pagination");
 var aggregate=dash.EmployeeMeasureScope(f).GroupBy(x=>1).Select(g=>new{Total=g.LongCount(),Complete=g.LongCount(x=>x.EmployeeQualityStatus=="DAY_DU")}).ToQueryString();Check(aggregate.Contains("COUNT_BIG"),"COUNTROWS aggregates translate to COUNT_BIG");

}
foreach(var url in new[]{"/Dashboard/Data","/Dashboard/Options","/Dashboard/Missing"}){
 using(var r=await Send("",url))Check((int)r.StatusCode==401,"dashboard API anonymous blocked "+url);
 using(var r=await Send("TEST\\outsider",url))Check((int)r.StatusCode==403,"dashboard API outsider blocked "+url);
 using(var r=await Send("TEST\\reader",url)){Check((int)r.StatusCode==(url=="/Dashboard/Missing"?500:503) && r.Content.Headers.ContentType?.MediaType=="application/json","dashboard API reader simulated database failure JSON "+url);}
}
foreach(var url in new[]{"/Dashboard/Data?Statuses=NghiHuu","/Dashboard/Data?DivisionIds=-1","/Dashboard/Missing?Page=0","/Dashboard/Missing?PageSize=10000","/Dashboard/Missing?Sort=invalid"})using(var r=await Send("VERIFICATION\\admin",url))Check((int)r.StatusCode==400,"invalid dashboard query blocked "+url);


foreach(var state in new[]{"HOP_LE","SAP_HET_HAN","DA_HET_HAN","KHONG_HOP_LE","BLANK"}){
 Check(Valid(new DocumentDrillFilter{State=state}),"valid drill state "+state);
 using var r=await Send("VERIFICATION\\admin","/Dashboard/DocumentDetails?state="+state);
 var html=await r.Content.ReadAsStringAsync();Check((int)r.StatusCode==200&&html.Contains("document-drill")&&html.Contains("drill-back"),"render drill "+state);
 using var ctx=new VtbContext(options);var sql=new DashboardService(ctx).DocumentDrillQuery(new DocumentDrillFilter{State=state,DivisionIds=[1],Statuses=["DangLamViec"],Search="' OR 1=1 --"}).OrderBy(x=>x.EmployeeId).Skip(20).Take(20).ToQueryString();
 Check(sql.Contains("DISTINCT")&&sql.Contains("OFFSET")&&sql.Contains("@")&&sql.Contains("[working_status] = 1")&&!sql.Contains("is_kpi_scope"),"drill projected grouping parameterized paging retains source KPI scope "+state);

}
foreach(var path in new[]{"/Dashboard/DocumentDetails?state=HOP_LE","/Dashboard/DocumentDetailsData?state=HOP_LE"}){
 using(var r=await Send("",path))Check((int)r.StatusCode==401,"drill anonymous blocked "+path);
 using(var r=await Send("TEST\\outsider",path))Check((int)r.StatusCode==403,"drill outsider blocked "+path);
}
foreach(var query in new[]{"", "?state=invalid", "?state=HOP_LE&sort=invalid", "?state=HOP_LE&page=0", "?state=HOP_LE&overviewSort=invalid", "?state=HOP_LE&Statuses=NghiHuu"}){
 using var r=await Send("VERIFICATION\\admin","/Dashboard/DocumentDetailsData"+query);Check((int)r.StatusCode==400,"invalid drill filter rejected "+query);
}
using(var r=await Send("TEST\\reader","/Dashboard/DocumentDetailsData?state=HOP_LE"))Check((int)r.StatusCode==503&&r.Content.Headers.ContentType?.MediaType=="application/json","reader drill simulated unavailable database JSON");


foreach(var path in new[]{"/Dashboard/ProblemDocuments","/Dashboard/Contract","/Dashboard/ContractDetails?State=CAN_KY_KET_TIEP"}){
 using(var r=await Send("VERIFICATION\\admin",path)){var html=System.Net.WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());Check((int)r.StatusCode==200&&html.Contains("secondary-report")&&html.Contains("report-table"),"new report renders "+path);if(!path.Contains("Details"))Check(html.Contains("/Dashboard/ProblemDocuments")&&html.Contains("/Dashboard/Contract")&&!html.Contains("Sắp có"),"three working report tabs "+path);}
 using(var r=await Send("",path))Check((int)r.StatusCode==401,"new report anonymous blocked "+path);
}
using(var ctx=new VtbContext(options)){
 var service=new DashboardService(ctx);
 Check(!service.ReportRequirements(new()).ToQueryString().Contains("[dbo].[document]"),"unfiltered requirements preserve missing document rows");
 var scoped=service.ReportRequirements(new ReportFilter{DivisionIds=[1],EmployeeIds=["12345678"],TypeIds=[1]}).ToQueryString();Check(!scoped.Contains("[dbo].[document]")&&scoped.Contains("document_type_id")&&scoped.Contains("@"),"requirement filters retain missing rows using employee/type scope without document join");
 foreach(var kind in new[]{"problem","expiring","contractIssues","expiredContracts","contractDetail"}){
  foreach(var sort in ReportFilter.Columns(kind)){
   var filter=new ReportFilter{Kind=kind,Sort=sort,State=kind=="contractDetail"?"BLANK":null,DivisionIds=[1],Search="' OR 1=1 --"};
   try{var sql=service.ReportOrderQuery(filter).Skip(20).Take(20).ToQueryString();Check(sql.Contains("DISTINCT")&&sql.Contains("OFFSET")&&sql.Contains("@"),"new table SQL translation "+kind+"/"+sort);}catch(Exception ex){Check(false,"new table SQL translation "+kind+"/"+sort+" "+ex.GetBaseException().Message);}
  }
 }
}
Check(DashboardService.RequirementRate(0,0)==0&&DashboardService.RequirementRate(10,3)==0.7,"requirement DIVIDE alternate result and subtraction");
Check(DashboardService.ContractLabel("KHONG_CANH_BAO")=="Không cảnh báo"&&DashboardService.ContractLabel("unknown")==null,"contract SWITCH preserves separate states and BLANK");
foreach(var path in new[]{"/Dashboard/ProblemData","/Dashboard/ContractData","/Dashboard/ReportOptions","/Dashboard/ReportTable?Kind=problem","/Dashboard/ReportTable?Kind=contractDetail&State=CAN_KY_KET_TIEP"}){
 using(var r=await Send("",path))Check((int)r.StatusCode==401,"report API anonymous blocked "+path);
 using(var r=await Send("TEST\\outsider",path))Check((int)r.StatusCode==403,"report API outsider blocked "+path);
 using(var r=await Send("TEST\\reader",path))Check((int)r.StatusCode==503&&r.Content.Headers.ContentType?.MediaType=="application/json","report reader simulated unavailable database JSON "+path);
}
foreach(var path in new[]{"/Dashboard/ReportTable?Kind=contractDetail","/Dashboard/ReportTable?Kind=wrong","/Dashboard/ReportTable?Sort=DROP","/Dashboard/ReportTable?Page=0","/Dashboard/ReportTable?PageSize=1000","/Dashboard/ContractDetails?State=wrong","/Dashboard/ContractDetails"})using(var r=await Send("VERIFICATION\\admin",path))Check((int)r.StatusCode==400,"invalid new report query rejected "+path);

// V5 fixtures are confined to this test executable; never written to VTB2.
var profileFixture=new ProfileModel{Employee=new Employee(),Department=new Department{Division=new Division()},DocumentTypes=new(){[10]=new DocumentType{DocumentName="Gốc",DocumentGroup="Hồ sơ gốc"},[20]=new DocumentType{DocumentName="Hợp đồng",DocumentGroup="Hợp đồng lao động"},[30]=new DocumentType{DocumentName="Quyết định",DocumentGroup="Quyết định / Văn bản khác"},[40]=new DocumentType{DocumentName="Khác",DocumentGroup="Nhóm mới"}},Documents=[new Document{DocumentId=1,DocumentTypeId=10},new Document{DocumentId=2,DocumentTypeId=20},new Document{DocumentId=3,DocumentTypeId=30},new Document{DocumentId=4,DocumentTypeId=40},new Document{DocumentId=5,DocumentTypeId=999}]};
var grouped=ProfilePresentation.Groups(profileFixture);
Check(grouped.Sum(x=>x.Documents.Count)==5&&grouped.SelectMany(x=>x.Documents).Select(x=>x.DocumentId).Distinct().Count()==5,"profile groups preserve every document exactly once");
Check(grouped.Single(x=>x.Source=="Quyết định / Văn bản khác").Title=="Quyết định"&&grouped.Count(x=>x.Unexpected)==2,"profile exact group alias and visible unexpected/unassigned groups");
profileFixture.Documents[0].DocumentTypeId=20;
Check(ProfilePresentation.Groups(profileFixture).Single(x=>x.Source=="Hợp đồng lao động").Documents.Count==2,"profile grouping follows changed document type");
var alerts=new[]{new DocumentQuality{DocumentId=1,QualityStatus="KHONG_HOP_LE",InvalidReason="A"},new DocumentQuality{DocumentId=1,QualityStatus="KHONG_HOP_LE",InvalidReason="A"},new DocumentQuality{DocumentId=1,QualityStatus="KHONG_HOP_LE",InvalidReason="B"},new DocumentQuality{DocumentId=1,QualityStatus="DA_HET_HAN"}};
Check(ProfilePresentation.Warnings(alerts).Count()==3,"profile duplicate warnings removed while distinct reasons and statuses remain");
Check(ProfilePresentation.StateClass("HOP_LE")=="valid"&&ProfilePresentation.StateClass("SAP_HET_HAN")=="expiring"&&ProfilePresentation.StateClass("DA_HET_HAN")=="expired"&&ProfilePresentation.StateClass("KHONG_HOP_LE")=="invalid"&&ProfilePresentation.StateClass("THIEU_HO_SO")=="missing"&&ProfilePresentation.StateClass("new")=="unknown","profile status color mapping");
foreach(var identity in new[]{"VERIFICATION\\admin","TEST\\reader"})using(var response=await Send(identity,"/__probe/profile-v5")){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 var split=html.IndexOf("id=\"historical-documents\"");
 Check(split>0 && html.IndexOf("CURRENT_SECTION_TEST")<split && html.IndexOf("HISTORY_SECTION_TEST")>split,"current and historical documents rendered in separate sections");
 var historyTag=System.Text.RegularExpressions.Regex.Match(html,"<details[^>]*id=\"historical-documents\"[^>]*>").Value;
 Check(historyTag.Length>0 && !historyTag.Contains("open"),"history accordion starts collapsed");
 Check(split>=0 && html.Substring(split).Contains("1 tài liệu"),"history heading includes historical count only");
 Check(!html.Contains("Thêm phiên bản")&&html.Contains("/Catalog/Edit/1?kind=documents")==identity.EndsWith("admin"),"historical edit link follows admin permission without version link");
 Check(response.IsSuccessStatusCode&&html.Contains("01/08/2020")&&!html.Contains("· ·")&&!html.Contains("Nguyên nhân:"),"profile warning render nullable reason and date "+identity);
 Check(html.Contains("profile-document-group")&&html.Contains("1 tài liệu")&&html.Contains("Chi tiết"),"profile grouped document and count render "+identity);
 Check(html.Contains("Sửa tài liệu")==identity.EndsWith("admin"),"profile warning edit follows role "+identity);
}
// Five-choice storage contract supersedes earlier per-rule form policies.
Check(DocumentStorageForms.Options().Select(x=>x.Text).SequenceEqual(new[]{"--","Bản gốc","Bản công chứng","Bản photo","Bản copy"}),"exact five shared options in requested order");
foreach(var old in new string?[]{null,"NULL"," null ","", "   "}){
 Check(DocumentStorageForms.Normalize(old)==null&&DocumentStorageForms.Display(old)=="--","legacy empty alias normalizes/displays --: "+(old??"C# null"));
}
foreach(var value in DocumentStorageForms.Values)Check(Valid(new Document{EmployeeId="TEST0001",DocumentTypeId=7,StorageForm=value}),"allowed storage value: "+value);
foreach(var value in new[]{"Bản chính","--","FORGED"})Check(!Valid(new Document{EmployeeId="TEST0001",DocumentTypeId=7,StorageForm=value}),"reject unsupported raw storage: "+value);
foreach(var query in new[]{"?create=true&nullStorage=true","?nullStorage=true","?literalNull=true","?needsStorage=true","?legacy=true"})using(var response=await Send("VERIFICATION\\admin","/__probe/document-edit"+query)){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 var select=System.Text.RegularExpressions.Regex.Match(html,"<select name=\"StorageForm\"[^>]*>(.*?)</select>",System.Text.RegularExpressions.RegexOptions.Singleline).Value;
 Check(response.IsSuccessStatusCode&&System.Text.RegularExpressions.Regex.Matches(select,"<option ").Count==5,"exact five options in create/edit/legacy/type scenario "+query);
 Check(!select.Contains("required")&&!select.Contains("disabled")&&!html.Contains("document-storage.js"),"storage always enabled, optional and independent of type/rules "+query);
 if(query.Contains("nullStorage")||query.Contains("literalNull"))Check(select.Contains(">--</option>")&&!html.Contains("id=\"storage-form-warning\""),"SQL NULL/literal NULL renders -- without warning "+query);
 if(query.Contains("legacy"))Check(html.Contains("OLD_VALUE")&&html.Contains("ReplaceUnsupportedStorage")&&!select.Contains("OLD_VALUE"),"unsupported old data outside options requires explicit replacement");
}
foreach(var value in DocumentStorageForms.Values.Concat(new[]{"","NULL"," nUlL ","   ","--","Bản chính","FORGED"})){
 using var request=new HttpRequestMessage(HttpMethod.Post,"/__probe/document-validation");request.Headers.Add("X-Test-User","VERIFICATION\\admin");
 request.Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"EmployeeId","TEST0001"},{"DocumentTypeId","7"},{"IssueDate","2024-05-11"},{"StorageForm",value}});
 using var response=await client.SendAsync(request);var body=await response.Content.ReadAsStringAsync();
 var normalized=DocumentStorageForms.Normalize(value);var allowed=DocumentStorageForms.IsAllowed(normalized);
 Check(allowed?response.IsSuccessStatusCode:(int)response.StatusCode==400,"POST normalizes aliases/validates whitelist: "+value);
 if(normalized==null)Check(body.Contains("\"storageForm\":null"),"POST stores C# null, never literal placeholder: "+value);
}
using(var response=await Send("VERIFICATION\\admin","/__probe/document-edit?literalNull=true&readOnly=true")){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());Check(html.Contains("title=\"Không có hình thức lưu trữ\">--</div>"),"detail normalizes legacy NULL display");
}
Check(new DocumentDrillRow{Storage=" NULL "}.StorageDisplay=="--"&&new DocumentDrillRow{Storage="Bản chính"}.StorageDisplay.Contains("cần kiểm tra"),"drill table uses same display normalization");
using(var context=new VtbContext(options)){
 var document=new Document{DocumentId=999,EmployeeId="TEST0001",DocumentTypeId=7,StorageForm="NULL",FilePath="original.pdf"};context.Attach(document);
 document.StorageForm=DocumentStorageForms.Normalize(document.StorageForm);document.IssueDate=new DateOnly(2024,5,12);context.ChangeTracker.DetectChanges();
 Check(context.Entry(document).Property(x=>x.StorageForm).IsModified&&document.StorageForm==null,"tracked legacy literal NULL is marked for update on save, not lost by materialization");
 Check(document.FilePath=="original.pdf"&&!context.Entry(document).Property(x=>x.FilePath).IsModified,"normalization preserves unchanged attachment reference");
}
using(var request=new HttpRequestMessage(HttpMethod.Post,"/__probe/document-validation")){
 request.Headers.Add("X-Test-User","VERIFICATION\\admin");request.Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"EmployeeId","TEST0001"},{"DocumentTypeId","7"},{"IssueDate","2024-05-11"},{"ExpiryDate","2024-05-10"},{"StorageForm"," NULL "}});
 using var response=await client.SendAsync(request);Check((int)response.StatusCode==400&&(await response.Content.ReadAsStringAsync()).Contains("ExpiryDate"),"storage normalization does not clear unrelated ModelState errors");
}
Check(Valid(new Employee{EmployeeId="TEST0001",FullName="Test",DepartmentId=1,PhoneNumber="0123456789",Email="staff@example.test",Hometown="Quê quán"}),"new employee contact values including leading-zero phone accepted");
Check(Valid(new Employee{EmployeeId="TEST0001",FullName="Test",DepartmentId=1}),"nullable employee contacts optional");
Check(!Valid(new Employee{EmployeeId="TEST0001",FullName="Test",DepartmentId=1,Email="invalid-address"}),"invalid email rejected server-side");
Check(!Valid(new Employee{EmployeeId="TEST0001",FullName="Test",DepartmentId=1,PhoneNumber="01234567890"}),"phone matches varchar(10) source DDL");
Check(Valid(new Qualification{EmployeeId="TEST0001",QualificationLevel="Đại học"})&&!Valid(new Qualification{EmployeeId="TEST0001",QualificationLevel=""}),"qualification level required, other fields nullable");
var degrees=new[]{new Qualification{QualificationId=1,GraduationDate=null},new Qualification{QualificationId=2,GraduationDate=new DateOnly(2020,1,1)},new Qualification{QualificationId=3,GraduationDate=new DateOnly(2024,1,1)},new Qualification{QualificationId=4,GraduationDate=new DateOnly(2024,1,1)},new Qualification{QualificationId=5,GraduationDate=null}};
Check(degrees.AsQueryable().ByGraduation().Select(x=>x.QualificationId).SequenceEqual(new[]{4,3,2,5,1}),"qualification order descending dates, null last, descending ID tie breaker");
using(var ctx=new VtbContext(options)){
 var qualification=ctx.Model.FindEntityType(typeof(Qualification))!;
 Check(qualification.GetTableName()=="qualification"&&qualification.FindPrimaryKey()!.Properties.Single().Name=="QualificationId","qualification actual source table name and PK");
 Check(qualification.GetForeignKeys().Single().DeleteBehavior==DeleteBehavior.NoAction&&!qualification.GetIndexes().Any(x=>x.IsUnique),"many degrees per employee and level, FK no cascade");
 Check(qualification.GetProperties().All(x=>x.Name!="IsHighest"),"no highest-degree flag");
 var sql=ctx.Qualifications.Where(x=>x.EmployeeId=="TEST0001").ByGraduation().ToQueryString();Check(sql.Contains("ORDER BY")&&sql.Contains("[graduation_date] DESC")&&sql.Contains("[qualification_id] DESC"),"qualification ordering translates to SQL");
 Check(ctx.Model.FindEntityType(typeof(Employee))!.FindProperty("PhoneNumber")!.GetColumnType()=="varchar(10)","phone mapped as string varchar(10)");
}
foreach(var path in new[]{"/Profiles/Print/TEST0001","/Catalog/Create?kind=qualifications&employeeId=TEST0001"})using(var response=await Send("",path))Check((int)response.StatusCode==401,"new personnel routes require authentication "+path);
foreach(var path in new[]{"/Catalog/Create?kind=qualifications","/Catalog/Edit/1?kind=qualifications","/Catalog/Delete/1?kind=qualifications"})using(var response=await Send("TEST\\reader",path))Check((int)response.StatusCode==403,"reader cannot mutate qualifications "+path);
using(var response=await Send("VERIFICATION\\admin","/Catalog/Delete/1?kind=qualifications",HttpMethod.Post))Check((int)response.StatusCode==400,"qualification delete requires anti-forgery token");
foreach(var empty in new[]{false,true})using(var response=await Send("VERIFICATION\\admin","/__probe/profile-print?empty="+empty)){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 Check(response.IsSuccessStatusCode&&html.Contains("LÝ LỊCH TRÍCH NGANG")&&html.Contains("In / Lưu PDF")&&!html.Contains("navbar-vertical"),"print renders standalone without sidebar, empty="+empty);
 Check(html.Contains("0123456789")&&html.Contains("staff@example.test")&&html.Contains("Quê quán thử"),"print includes contacts, empty="+empty);
 Check(empty?html.Contains("Chưa có thông tin văn bằng."):html.Contains("Bằng A")&&html.Contains("Bằng B")&&html.Contains("Bằng C")&&html.IndexOf("Bằng C")<html.IndexOf("Bằng A"),"print retains all degrees sorted or empty message, empty="+empty);
 Check(html.Contains("NGƯỜI KIỂM TRA")&&html.Contains("print-scope")&&html.Contains("In danh mục hồ sơ")&&html.Contains("Chức danh:"),"independent catalogue print controls and signoff");
 Check(html.Contains("DANH MỤC HỒ SƠ CÁN BỘ")&&html.Contains("Tên hồ sơ")&&!html.Contains("Hồ sơ điện tử"),"print catalogue headings and no electronic column");
 Check(html.Contains("Loại chưa có")&&html.Contains("Hợp đồng mẫu")&&html.Contains("Quyết định mẫu"),"print includes every catalogue type even without documents");
 Check(html.Contains("Chỉ tiêu")&&html.Contains("Kết quả")&&!html.Contains("Các số loại hồ sơ lấy từ View"),"print indicators are a two-column table without old note");
 Check(html.Contains("storage-mark\">✓</td>")&&html.Contains("print-brand")&&html.Contains("CÁN BỘ XÁC NHẬN"),"print storage mark, existing logo and signatures");

}
using(var response=await Send("VERIFICATION\\admin","/__probe/qualification-form")){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());Check(html.Contains("name=\"QualificationLevel\"")&&html.Contains("type=\"date\" name=\"GraduationDate\"")&&!html.Contains("name=\"EmployeeId\""),"qualification edit fields and immutable owner");
}
using(var response=await Send("VERIFICATION\\admin","/__probe/qualification-delete")){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());Check(html.Contains("Xác nhận xóa")&&html.Contains("method=\"post\"")&&html.Contains("__RequestVerificationToken")&&html.Contains("01/01/2024"),"qualification deletion confirmation POST and Vietnamese date");
}
// Attachment checks use temporary files only, never personnel records or live SQL writes.
Check(!CatalogService.Fields("documents").Any(f=>f.Name=="FilePath"),"file reference is not client-bindable");
foreach(var reference in new[]{"../secret.pdf",Path.Combine(Path.GetPathRoot(Path.GetTempPath())!,"secret.pdf"),"https://example.test/a.pdf","",new string('a',32)+".exe"})Check(!DocumentFileStore.IsReference(reference),"unsafe attachment reference rejected");
async Task<bool> AcceptUpload(string name,byte[] data,long? length=null){using var stream=new MemoryStream(data);var upload=new FormFile(stream,0,length??data.Length,"Attachment",name);try{await DocumentFileStore.ValidateAsync(upload);return true;}catch(InvalidDataException){return false;}}
var pdfBytes=System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\n1 0 obj<</Type/Catalog>>endobj\n%%EOF");
var pngBytes=Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=");
Check(await AcceptUpload("document.pdf",pdfBytes),"PDF signature accepted");
Check(await AcceptUpload("image.png",pngBytes),"PNG signature accepted");
Check(await AcceptUpload("image.jpg",new byte[]{255,216,255,224,0,2,255,217}),"JPEG signature accepted");
Check(!await AcceptUpload("script.exe",pdfBytes),"executable extension rejected");
Check(!await AcceptUpload("fake.pdf",pngBytes),"spoofed PDF content rejected");
Check(!await AcceptUpload("empty.pdf",[]),"empty upload rejected");
Check(!await AcceptUpload("large.pdf",pdfBytes,DocumentFileStore.MaxBytes+1),"upload above 10 MB rejected");
using(var scope=app.Services.CreateScope()){
 var store=scope.ServiceProvider.GetRequiredService<DocumentFileStore>();
 var first=await store.StoreAsync(pdfBytes,"first.pdf");var second=await store.StoreAsync(pngBytes,"image.png");
 try{
 foreach(var reference in new[]{first,second,"",new string('f',32)+".pdf"}){
  using var response=await Send("VERIFICATION\\admin","/__probe/file-eye?reference="+reference);
  var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());var exists=store.Exists(reference);
  Check(response.IsSuccessStatusCode&&html.Contains("class=\"document-eye-icon\"")&&!html.Contains("Tải xuống")&&!html.Contains("Tệp tài liệu #"),"list eye has no attachment text or download");
  Check(exists?html.Contains("target=\"_blank\"")&&html.Contains("/DocumentFiles/Open/612")&&!html.Contains(" disabled aria-label="):html.Contains(" disabled aria-label=")&&!html.Contains("/DocumentFiles/Open/612"),"eye enabled only for existing attachment");
 }
 foreach(var details in new[]{true,false}){
  using var response=await Send("VERIFICATION\\admin","/__probe/attachment-display?reference="+first+"&details="+details);
  var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());Check(response.IsSuccessStatusCode&&html.Contains("Tải xuống")==details,"download visible only on document details");
 }
 Check(first!=second&&store.Exists(first)&&store.Exists(second),"unique attachment names never overwrite");Check(!store.Resolve(first)!.Contains("wwwroot"),"attachments stored outside public webroot");Check(File.ReadAllBytes(store.Resolve(first)!).SequenceEqual(pdfBytes),"attachment bytes preserved");Check(store.Resolve("../secret.pdf")==null,"traversal cannot resolve");store.Delete(second);Check(!store.Exists(second)&&store.Exists(first),"deleting replacement leaves original intact");}finally{store.Delete(first);store.Delete(second);}
}
using(var response=await Send("","/DocumentFiles/Open/1"))Check((int)response.StatusCode==401,"anonymous cannot view attachment");
using(var response=await Send("TEST\\outsider","/DocumentFiles/Open/1"))Check((int)response.StatusCode==403,"unauthorized account cannot view attachment");
using(var response=await Send("TEST\\reader","/Catalog/Create?kind=documents",HttpMethod.Post))Check((int)response.StatusCode==403,"reader cannot upload attachment");
using(var response=await Send("VERIFICATION\\admin","/Catalog/Edit/1?kind=documents",HttpMethod.Post))Check((int)response.StatusCode==400,"attachment replacement requires CSRF");
foreach(var gender in new[]{"Nam","Nu",""})using(var r=await Send("VERIFICATION\\admin","/__probe/employee-grid?quality=error&gender="+(gender==""?"none":gender))){
 var html=System.Net.WebUtility.HtmlDecode(await r.Content.ReadAsStringAsync());
 Check(html.Contains(">"+(gender=="Nam"?"Nam":gender=="Nu"?"Nữ":"--")+"</td>"),"plain gender including null "+gender);
 Check(html.Contains("name=\"quality\" value=\"THIEU_HO_SO\"")&&html.Contains("Trạng thái làm việc</label>")&&html.Contains("aria-current=\"true\""),"quality selection and employment filter retained in rendered form "+gender);
 Check(html.Contains("quality=DAY_DU")&&html.Contains("departmentId=3")&&html.Contains("status=TamHoanHDLD")&&html.Contains("page=1"),"quality links preserve scope and reset page "+gender);
}
using(var response=await Send("VERIFICATION\\admin","/__probe/employee-grid")){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 Check(response.IsSuccessStatusCode&&html.Contains("employee-table-scroll")&&html.Contains("employee-actions-cell"),"employee table responsive wrapper and sticky action classes");
 Check(new[]{"Hồ sơ","Chi tiết","Sửa","Xóa"}.All(label=>html.Contains("aria-label=\""+label+"\"")),"all employee text actions retain accessible labels");
 var table=html.Substring(html.IndexOf("<table"));table=table.Substring(0,table.IndexOf("</table>"));
 Check(System.Text.RegularExpressions.Regex.Matches(table,@"<th(?:\s|>)").Count==9&&System.Text.RegularExpressions.Regex.Matches(table,@"<td(?:\s|>)").Count==9,"exactly nine employee headers and cells");
 Check(table.Contains(">TEST0001</span>")&&table.Contains("/Profiles/Details/TEST0001")&&table.Contains("Nguyễn Kiểm Thử</td>")&&!table.Contains("ẨN_")&&!table.Contains("btn-icon")&&!table.Contains("<svg"),"separate employee values and text actions without hidden columns");
 Check(table.Contains("employee-quality-unavailable")&&!table.Contains("employee-quality-dot--unknown")&&table.Contains("aria-label=\"Chưa có kết quả đánh giá\""),"employee dot has accessible explanation and safe unknown fallback");
 Check(table.Contains("Trạng thái làm việc</th>")&&table.Contains("employee-employment--DangLamViec")&&table.Contains(">Nam</td>"),"employment badge retained and gender rendered as plain text");
 Check(!html.Contains("Cần bổ sung/theo dõi")&&!html.Contains("Chưa xác định")&&html.Contains("Đầy đủ")&&html.Contains("Thiếu hồ sơ"),"employee legend has only two states");
 Check(html.IndexOf("id=\"sidebar-toggle\"")>html.IndexOf("<aside")&&html.IndexOf("id=\"sidebar-toggle\"")<html.IndexOf("</aside>"),"desktop panel control is inside sidebar");
 Check(html.Contains("sidebar-toggle")&&html.Contains("organization-submenu")&&html.Contains("sidebar-backdrop"),"single sidebar toggle, submenu and mobile backdrop render");
}
var listEmployee=new Employee{EmployeeId="TEST0001",FullName="Nguyễn Kiểm Thử",DepartmentId=12};
var departmentLabels=new Dictionary<int,string>{{12,"Phòng phân tích dữ liệu"}};
Check(CatalogService.EmployeeListValue(listEmployee,"EmployeeId","TEST0001 — Nguyễn Kiểm Thử",departmentLabels)=="TEST0001","employee list code never uses dropdown label");
Check(CatalogService.EmployeeListValue(listEmployee,"FullName","TEST0001 — Nguyễn Kiểm Thử",departmentLabels)=="Nguyễn Kiểm Thử","employee list name has no repeated code");
Check(CatalogService.EmployeeListValue(listEmployee,"DepartmentId","Khối A / Phòng phân tích dữ liệu",departmentLabels)=="Phòng phân tích dữ liệu","department cell contains only department name");
var contractProfile=new ProfileModel{Employee=new Employee{EmployeeId="TEST0001"},Contract=new LaborContractCheck{EmployeeId="TEST0001",LaborContractStatus="HOP_LE",ExistHdxdth1=1,HdthuviecExpired=1,Hdxdth1Expired=1,ExistHdkxdth=1},DocumentTypes=new(){[1]=new DocumentType{DocumentCode="HDTHUVIEC",DocumentGroup="Hợp đồng lao động"},[2]=new DocumentType{DocumentCode="HDXDTH1",DocumentGroup="Hợp đồng lao động"},[3]=new DocumentType{DocumentCode="CCCD",DocumentGroup="Hồ sơ gốc"},[4]=new DocumentType{DocumentCode="HDKXDTH",DocumentGroup="Hợp đồng lao động"}},Documents=[new Document{DocumentId=1,EmployeeId="TEST0001",DocumentTypeId=1,IsCurrent=true},new Document{DocumentId=2,EmployeeId="TEST0001",DocumentTypeId=2,IsCurrent=true},new Document{DocumentId=3,EmployeeId="TEST0001",DocumentTypeId=3},new Document{DocumentId=4,EmployeeId="TEST0001",DocumentTypeId=4,IsCurrent=true}],Warnings=[new DocumentQuality{DocumentId=1,QualityStatus="DA_HET_HAN"},new DocumentQuality{DocumentId=2,QualityStatus="DA_HET_HAN"},new DocumentQuality{DocumentId=2,QualityStatus="KHONG_HOP_LE"},new DocumentQuality{DocumentId=3,QualityStatus="DA_HET_HAN"},new DocumentQuality{DocumentId=4,QualityStatus="SAP_HET_HAN"}]};
Check(ProfilePresentation.HasValidCurrentContract(contractProfile),"profile validity uses employee contract View result only");
Check(ProfilePresentation.ProfileWarnings(contractProfile).Count()==3,"superseded trial and first fixed-term excluded even when current within own type");
Check(ProfilePresentation.ProfileWarnings(contractProfile).Any(q=>q.DocumentId==2&&q.QualityStatus=="KHONG_HOP_LE")&&ProfilePresentation.ProfileWarnings(contractProfile).Any(q=>q.DocumentId==3)&&ProfilePresentation.ProfileWarnings(contractProfile).Any(q=>q.QualityStatus=="SAP_HET_HAN"),"invalid, non-contract expired, and expiring warnings remain");
Check(contractProfile.Warnings.Count==5&&contractProfile.Documents.Count==4,"source quality rows and history remain unchanged");
contractProfile.Contract.LaborContractStatus="CAN_KY_KET_TIEP";Check(ProfilePresentation.ProfileWarnings(contractProfile).Count()==5,"non-valid contract status preserves expired alerts");
contractProfile.Contract.LaborContractStatus="HOP_LE";contractProfile.Contract.EmployeeId="OTHER001";Check(ProfilePresentation.ProfileWarnings(contractProfile).Count()==5,"other employee contract result cannot hide warnings");
contractProfile.Contract.EmployeeId="TEST0001";contractProfile.Contract.ExistHdkxdth=0;contractProfile.Contract.ValidHdxdth1=1;
Check(ProfilePresentation.ProfileWarnings(contractProfile).Any(q=>q.DocumentId==2&&q.QualityStatus=="DA_HET_HAN"),"expired current contract without successor remains even if aggregate says valid");
Check(!ProfilePresentation.ProfileWarnings(contractProfile).Any(q=>q.DocumentId==1),"valid first fixed-term supersedes trial");
contractProfile.Contract.ValidHdxdth1=0;contractProfile.Contract.ValidHdxdth2=1;Check(!ProfilePresentation.ProfileWarnings(contractProfile).Any(q=>q.DocumentId==2&&q.QualityStatus=="DA_HET_HAN"),"valid second fixed-term supersedes first");
contractProfile.Contract.ValidHdxdth2=0;contractProfile.Contract.ValidHdxdth1=1;contractProfile.Documents[1].IsCurrent=false;contractProfile.Documents.Add(new Document{DocumentId=5,EmployeeId="TEST0001",DocumentTypeId=2,IsCurrent=true});
Check(!ProfilePresentation.ProfileWarnings(contractProfile).Any(q=>q.DocumentId==2&&q.QualityStatus=="DA_HET_HAN"),"same-type historical revision needs View valid flag and current replacement");
contractProfile.Contract.ValidHdxdth1=0;Check(ProfilePresentation.ProfileWarnings(contractProfile).Any(q=>q.DocumentId==2&&q.QualityStatus=="DA_HET_HAN"),"is_current false alone never suppresses expiration");
contractProfile.Contract=null;Check(ProfilePresentation.ProfileWarnings(contractProfile).Count()==5,"missing View result keeps all alerts on next evaluation");
Check(ProfilePresentation.EmploymentColor("DangLamViec")=="green"&&ProfilePresentation.EmploymentColor("TamHoanHDLD")=="yellow"&&ProfilePresentation.EmploymentColor("ChamDutHDLD")=="secondary","employment badge colors");
Check(ProfilePresentation.EmploymentLabel("ChamDutHDLD")=="Chấm dứt HĐLĐ","employment display label without storage change");
await VerifyCancellation(options);
// Display ordering fixtures only; no writes to VTB2.
{
 var types=new[]{new DocumentType{DocumentTypeId=90},new DocumentType{DocumentTypeId=2},new DocumentType{DocumentTypeId=7},new DocumentType{DocumentTypeId=1}}.AsQueryable();
 var rules=new[]{new RequirementRule{RequirementId=20,DocumentTypeId=2},new RequirementRule{RequirementId=3,DocumentTypeId=90,IsActive=false},new RequirementRule{RequirementId=30,DocumentTypeId=90},new RequirementRule{RequirementId=25,DocumentTypeId=7}}.AsQueryable();
 Check(types.InRequirementOrder(rules).Select(t=>t.DocumentTypeId).SequenceEqual(new[]{90,2,7,1}),"type order follows first unfiltered rule; duplicate removed; unruled retained last");
 Check(types.Where(t=>t.DocumentTypeId!=90).InRequirementOrder(rules).Select(t=>t.DocumentTypeId).SequenceEqual(new[]{2,7,1}),"filtered types preserve canonical relative order");
 var docs=new[]{new Document{DocumentId=1,DocumentTypeId=90,IsCurrent=true,IssueDate=new(2020,1,1)},new Document{DocumentId=2,DocumentTypeId=90,IssueDate=new(2025,1,1)},new Document{DocumentId=3,DocumentTypeId=90,IssueDate=new(2025,1,1)},new Document{DocumentId=4,DocumentTypeId=2,IsCurrent=true},new Document{DocumentId=5,DocumentTypeId=90,IssueDate=new(2025,1,1),CreatedAt=new(2025,2,1)}}.AsQueryable();
 Check(docs.InRequirementOrder(types,rules).Select(d=>d.DocumentId).SequenceEqual(new[]{1,5,3,2,4}),"current and history adjacent by type; issue date, creation and ID tie breakers");
 using var ctx=new VtbContext(options);
 var typeSql=ctx.DocumentTypes.InRequirementOrder(ctx).Select(t=>new{t.DocumentTypeId,t.DocumentName}).ToQueryString();
 var docSql=ctx.Documents.Where(d=>d.EmployeeId=="TEST0001").InRequirementOrder(ctx).Skip(20).Take(20).ToQueryString();
 Check(typeSql.Contains("MIN(")&&typeSql.Contains("ORDER BY"),"canonical dropdown ordering translates to SQL");
 Check(docSql.Contains("[is_current] DESC")&&docSql.Contains("[issue_date] DESC")&&docSql.Contains("[created_at] DESC")&&docSql.Contains("[document_id] DESC")&&docSql.Contains("OFFSET"),"document ordering translates before SQL pagination");
}
{
 using var ctx=new VtbContext(options);var service=new DashboardService(ctx);
 foreach(var filter in new[]{new ReportFilter(),new ReportFilter{DivisionIds=[1],DepartmentIds=[2],EmployeeIds=["TEST0001"],TypeIds=[3],Groups=["Hồ sơ gốc"]}}){
  var sql=service.ProblemOrganizationQuery(filter).ToQueryString();
  Check(sql.Contains("COUNT_BIG")&&sql.Contains("GROUP BY")&&sql.Contains("division_id")&&sql.Contains("department_id"),"problem hierarchy aggregates source counts by organization IDs");
  Check(sql.Contains("vw_employee_document_requirement"),"hierarchy reuses requirement View and filter context");
 }
 Check(DashboardService.RequirementRate(100,20)==.8&&DashboardService.RequirementRate(0,0)==0,"organization rate uses total counts and existing zero denominator semantics");
}
{
 var types=new[]{new DocumentType{DocumentTypeId=10}}.AsQueryable();
 var rules=new[]{new RequirementRule{RequirementId=1,DocumentTypeId=10},new RequirementRule{RequirementId=2,DocumentTypeId=10}}.AsQueryable();
 var source=new[]{new Document{DocumentId=901,DocumentTypeId=10,IsCurrent=true},new Document{DocumentId=902,DocumentTypeId=10},new Document{DocumentId=903,DocumentTypeId=20},new Document{DocumentId=904,DocumentTypeId=999}}.AsQueryable();
 var ordered=source.InRequirementOrder(types,rules).ToList();
 Check(ordered.Count==4&&ordered.Select(d=>d.DocumentId).Distinct().Count()==4,"ordering never drops orphan type or no-rule history and never duplicates from multiple rules");
 using var ctx=new VtbContext(options);var sql=ctx.Documents.InRequirementOrder(ctx).ToQueryString();
 Check(!sql.Contains("INNER JOIN")&&!sql.Contains("[dbo].[document_type]"),"document ordering does not require matching catalog row");
}
foreach(var kind in new[]{"mixed","current","history","empty"}){
 using var response=await Send("VERIFICATION\\admin","/__probe/history-cases?kind="+kind);
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 Check(response.IsSuccessStatusCode&&html.Contains("id=\"historical-documents\""),"history section always rendered: "+kind);
 var count=System.Text.RegularExpressions.Regex.Matches(html,"data-document-id=").Count;
 var expected=kind=="mixed"?5:kind=="current"?1:kind=="history"?4:0;
 Check(count==expected,"all source documents rendered exactly once: "+kind);
 if(kind is "current" or "empty")Check(html.Contains("Chưa có tài liệu lịch sử"),"empty history message: "+kind);
 else Check(html.Contains("Khác")&&html.Contains("/Catalog/Edit/905?kind=documents"),"unassigned history visible with edit permission: "+kind);
}
{
 using var ctx=new VtbContext(options);var sql=new DashboardService(ctx).ContractOrganizationQuery(new ReportFilter{DivisionIds=[1],DepartmentIds=[2],EmployeeIds=["TEST0001"]}).ToQueryString();
 Check(sql.Contains("COUNT(DISTINCT")&&sql.Contains("GROUP BY")&&sql.Contains("division_id")&&sql.Contains("department_id"),"contract organization counts unique employees by organization IDs");
 Check(sql.Contains("CAN_KY_KET_TIEP")&&sql.Contains("THIEU_HDXDTH1")&&sql.Contains("@"),"contract organization keeps issue statuses and parameterized filters");
}
{
 ProfileModel DotFixture()=>new(){Employee=new Employee{EmployeeId="TEST0001"},Quality=new EmployeeQuality{EmployeeId="TEST0001",EmploymentStatus="DangLamViec",EmployeeQualityStatus="DAY_DU",TotalRequiredDocuments=1},Contract=new LaborContractCheck{EmployeeId="TEST0001",LaborContractStatus="HOP_LE",ExistHdkxdth=1},DocumentTypes=new(){[1]=new DocumentType{DocumentTypeId=1,DocumentGroup="Hồ sơ gốc"},[2]=new DocumentType{DocumentTypeId=2,DocumentGroup="Hợp đồng lao động",DocumentCode="HDXDTH1"}},Documents=[new Document{DocumentId=1,EmployeeId="TEST0001",DocumentTypeId=1,IsCurrent=true}],DocumentQualityRows=[new DocumentQuality{EmployeeId="TEST0001",DocumentId=1,QualityStatus="HOP_LE"}]};
 var model=DotFixture();Check(EmployeeQualityIndicators.Evaluate(model,0).State=="success","View DAY_DU maps to green");
 Check(EmployeeQualityIndicators.Evaluate(model,3).State=="success","display does not recompute View assessment from supplied missing count");
 foreach(var issue in new[]{"Thiếu hồ sơ","Tài liệu không hợp lệ","Tài liệu đã hết hạn","Cần ký tiếp HĐLĐ","Thiếu HĐXĐTH1"}){
  model=DotFixture();model.Quality!.EmployeeQualityStatus="THIEU_HO_SO";model.Quality.QualityIssue=issue;
  Check(EmployeeQualityIndicators.Evaluate(model,0) is {State:"error",Label:"Thiếu hồ sơ",Tooltip:var tip}&&tip.Contains(issue),"View issue retained: "+issue);
 }
 foreach(var state in new[]{"KHONG_HOP_LE","DA_HET_HAN","SAP_HET_HAN","KHONG_XAC_DINH"}){
  model=DotFixture();model.DocumentQualityRows[0].QualityStatus=state;model.Warnings=model.DocumentQualityRows.ToList();
  Check(EmployeeQualityIndicators.Evaluate(model,2).State=="success","document evidence alone does not override employee View: "+state);
 }
 foreach(var state in new[]{"CAN_KY_KET_TIEP","THIEU_HDXDTH1","KHONG_CANH_BAO"}){
  model=DotFixture();model.Contract!.LaborContractStatus=state;
  Check(EmployeeQualityIndicators.Evaluate(model,0).State=="success","contract evidence does not recompute employee assessment: "+state);
 }
 model=DotFixture();model.Documents.Add(new Document{DocumentId=2,IsCurrent=false});model.Warnings.Add(new DocumentQuality{DocumentId=2,QualityStatus="DA_HET_HAN"});
 Check(EmployeeQualityIndicators.Evaluate(model,0).State=="success","historical expired evidence does not override DAY_DU");
 model=DotFixture();model.Quality!.EmploymentStatus="TamHoanHDLD";Check(EmployeeQualityIndicators.Evaluate(model,0).State=="success","suspended employee uses View assessment");
 foreach(var state in new[]{"DAY_DU","THIEU_HO_SO"}){
  model=DotFixture();model.Quality!.EmploymentStatus="NghiHuu";model.Quality.EmployeeQualityStatus=state;
  Check(EmployeeQualityIndicators.Evaluate(model,0).State==(state=="DAY_DU"?"success":"error"),"retired employee uses retirement requirement assessment: "+state);
  model.Quality.EmploymentStatus="ChamDutHDLD";
  Check(EmployeeQualityIndicators.Evaluate(model,0) is {State:"unknown",Label:"Không áp dụng",NotApplicable:true},"terminated employee not applicable: "+state);
 }
 model=DotFixture();model.Quality=null;Check(EmployeeQualityIndicators.Evaluate(model,0).State=="unknown","missing employee assessment unavailable");
 model=DotFixture();model.Quality!.EmployeeQualityStatus="UNKNOWN";Check(EmployeeQualityIndicators.Evaluate(model,0).State=="unknown","unknown employee assessment unavailable");
 model=DotFixture();model.DocumentQualityRows.Clear();Check(EmployeeQualityIndicators.Evaluate(model,0).State=="success","missing document evidence does not erase employee View assessment");
 model=DotFixture();model.Quality!.EmployeeQualityStatus="THIEU_HO_SO";model.Quality.QualityIssue="Tài liệu không hợp lệ";
 var before=EmployeeQualityIndicators.Evaluate(model,0);model.Warnings=[new DocumentQuality{DocumentId=1,QualityStatus="KHONG_HOP_LE"},new DocumentQuality{DocumentId=1,QualityStatus="KHONG_HOP_LE"}];
 Check(EmployeeQualityIndicators.Evaluate(model,0)==before,"duplicate evidence does not multiply View issue text");
}
{
 using var ctx=new VtbContext(options);var service=new DashboardService(ctx);
 var missingSql=service.MissingQuery(new()).ToQueryString();Check(missingSql.Contains("THIEU_HO_SO")&&missingSql.Contains("employee_quality_status")&&missingSql.Contains("DangLamViec")&&missingSql.Contains("TamHoanHDLD")&&!missingSql.Contains("vw_labor_contract_check"),"followup uses View assessment and Rule 9 rather than recomputing contract issues");
 var problemSql=service.ReportTableQuery(new ReportFilter{Kind="problem"}).ToQueryString();Check(problemSql.Contains("KHONG_HOP_LE")&&problemSql.Contains("DA_HET_HAN")&&problemSql.Contains("[document_id]"),"problem table has explicit issue states and document identity");
 var expiringSql=service.ReportOrderQuery(new ReportFilter{Kind="expiring"}).ToQueryString();Check(expiringSql.Contains("SAP_HET_HAN")&&expiringSql.Contains("ORDER BY"),"expiring table retains warning state and ordered pagination");
}
// New requirement drill counts applicable employee/type pairs, not quality failures.
{
 var source=new[]{
  new DocumentRequirement{EmployeeId="TEST0001",DocumentTypeId=1,EmploymentStatus="DangLamViec",RequirementStatus="THIEU_HO_SO"},
  new DocumentRequirement{EmployeeId="TEST0001",DocumentTypeId=1,EmploymentStatus="DangLamViec",RequirementStatus="THIEU_HO_SO"},
  new DocumentRequirement{EmployeeId="TEST0001",DocumentTypeId=2,EmploymentStatus="DangLamViec",RequirementStatus="THIEU_HO_SO"},
  new DocumentRequirement{EmployeeId="TEST0001",DocumentTypeId=2,EmploymentStatus="DangLamViec",RequirementStatus="DA_CO"},
  new DocumentRequirement{EmployeeId="TEST0002",DocumentTypeId=1,EmploymentStatus="TamHoanHDLD",RequirementStatus="THIEU_HO_SO"},
  new DocumentRequirement{EmployeeId="TEST0003",DocumentTypeId=1,EmploymentStatus="NghiHuu",RequirementStatus="THIEU_HO_SO"},
  new DocumentRequirement{EmployeeId="TEST0004",DocumentTypeId=1,EmploymentStatus="ChamDutHDLD",RequirementStatus="THIEU_HO_SO"}
 }.AsQueryable();
 var pairs=DashboardService.RequirementPairs(source).ToList();
 Check(pairs.Count==3&&pairs.Sum(p=>p.Missing)==2,"requirement drill deduplicates employee/type and limits Rule 9 scope");
 Check(pairs.Single(p=>p.EmployeeId=="TEST0001"&&p.DocumentTypeId==2).Missing==0,"existing current record counts as present despite another missing source row");
 Check(pairs.Any(p=>p.EmployeeId=="TEST0002")&&!pairs.Any(p=>p.EmployeeId is "TEST0003" or "TEST0004"),"suspended included, retired/terminated excluded from requirement drill");
 Check(new RequirementChartPoint{Total=0,Missing=0}.Rate==null&&new RequirementChartPoint{Total=10,Missing=3}.Rate==.7,"current drill rate returns not applicable for no requirements and weighted ratio otherwise");
 using var context=new VtbContext(options);var dashboard=new DashboardService(context);
 foreach(var statuses in new[]{Array.Empty<string>(),new[]{"DangLamViec"},new[]{"TamHoanHDLD"},new[]{"NghiHuu"}}){
  var sql=dashboard.DocumentDrillQuery(new DocumentDrillFilter{State="HOP_LE",Statuses=statuses}).ToQueryString();
  Check(sql.Contains("[working_status] = 1")&&sql.Contains("HOP_LE"),"direct document drill cannot bypass source working_status: "+string.Join(",",statuses));
 }
 Check(DashboardService.RequirementPairs(dashboard.ReportRequirements(new ReportFilter{TypeIds=[1],DivisionIds=[2]})).ToQueryString().Contains("GROUP BY"),"requirement drill pair aggregation translates with type/organization scope");
}
using(var response=await Send("VERIFICATION\\admin","/Dashboard/ProblemDocuments")){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 Check(new[]{"data-rate-mode=\"all\"","data-rate-mode=\"division\"","data-rate-mode=\"detail\"","id=\"requirement-division\"","id=\"requirement-detail\""}.All(html.Contains),"current requirement chart modes, division picker and detail dialog render");
 Check(!html.Contains("Phạm vi KPI và biểu đồ:"),"removed problem dashboard note does not return");
}
foreach(var path in new[]{"/__probe/grid","/__probe/grid?q=test"}){
 using var response=await Send("VERIFICATION\\admin",path);var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 Check(response.IsSuccessStatusCode&&html.Contains(path.Contains('?')?"Không có kết quả phù hợp":"Chưa có dữ liệu trong danh sách"),"distinct filtered/unfiltered empty state: "+path);
}
using(var response=await Send("TEST\\reader","/__probe/employee-grid")){
 var html=System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
 Check(html.Contains("aria-label=\"Hồ sơ\"")&&html.Contains("aria-label=\"Chi tiết\"")&&!html.Contains("aria-label=\"Sửa\"")&&!html.Contains("aria-label=\"Xóa\""),"current employee grid hides write links from reader");
}
Check(offline.Blocked>0,"HTTP data routes exercised fail-closed database guard; no real SQL connection");
Console.WriteLine($"RESULT: {passed} passed, {failures.Count} failed, {skipped} skipped. HTTP uses TestServer; SQL translation only; no live database or browser tested.");Environment.ExitCode=failures.Count==0?0:1;
public class ProbeAuth(IOptionsMonitor<AuthenticationSchemeOptions> o,ILoggerFactory l,UrlEncoder e):AuthenticationHandler<AuthenticationSchemeOptions>(o,l,e)
{
 protected override Task<AuthenticateResult> HandleAuthenticateAsync(){var name=Request.Headers["X-Test-User"].ToString();return Task.FromResult(string.IsNullOrEmpty(name)?AuthenticateResult.NoResult():AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,name)},"Test")),"Test")));}
}
[Route("__probe")]
public class ProbeController:Controller
{
  [HttpGet("profile")]public IActionResult Profile()=>View("~/Views/Profiles/Details.cshtml",new ProfileModel{Employee=new Employee(),Department=new Department{Division=new Division()}});
 [HttpGet("employee-grid")]public IActionResult EmployeeGrid(string? gender="Nam",string? quality=null){RouteData.Values["controller"]="Catalog";RouteData.Values["action"]="Index";return View("~/Views/Shared/Grid.cshtml",new GridModel{Kind="employees",Title="Danh sách cán bộ",Filter=new Filter{Quality=quality,Q="test",DivisionId=2,DepartmentId=3,Status="TamHoanHDLD"},Filters=["Status"],Headers=CatalogService.Fields("employees").Where(f=>f.Name is not "DateOfBirth" and not "IsManagerial").Select(f=>f.Label).ToArray(),Rows=[new GridRow{Cells=["TEST0001","Nguyễn Kiểm Thử","0123456789","staff@example.test","ẨN_QUÊ_QUÁN",gender=="none"?null:gender,"Phòng phân tích dữ liệu","Chuyên viên thử nghiệm","DangLamViec","01/01/2020","ẨN_NGÀY_NGHỈ"],Links=[new("Hồ sơ","Profiles","Details","TEST0001"),new("Chi tiết","Catalog","Details","TEST0001","employees"),new("Sửa","Catalog","Edit","TEST0001","employees"),new("Xóa","Catalog","Delete","TEST0001","employees")]}]});}
 [HttpGet("file-eye")]public IActionResult FileEye(string? reference)=>View("~/Views/Shared/_DocumentFileEye.cshtml",new Document{DocumentId=612,FilePath=reference});
 [HttpGet("attachment-display")]public IActionResult AttachmentDisplay(string? reference,bool details){RouteData.Values["action"]=details?"Details":"Edit";return View("~/Views/Shared/_DocumentAttachment.cshtml",new Document{DocumentId=612,FilePath=reference});}
 [HttpGet("history-cases")]public IActionResult HistoryCases(string kind){
 var docs=new List<Document>{new(){DocumentId=901,DocumentTypeId=10,IsCurrent=true},new(){DocumentId=902,DocumentTypeId=10},new(){DocumentId=903,DocumentTypeId=20},new(){DocumentId=904,DocumentTypeId=20},new(){DocumentId=905,DocumentTypeId=999}};
 if(kind=="current")docs=docs.Where(d=>d.IsCurrent).ToList();if(kind=="history")docs=docs.Where(d=>!d.IsCurrent).ToList();if(kind=="empty")docs.Clear();
 return View("~/Views/Profiles/_DocumentGroups.cshtml",new ProfileModel{Employee=new Employee(),Documents=docs,DocumentTypes=new(){[10]=new DocumentType{DocumentName="Hợp đồng thử việc",DocumentGroup="Hợp đồng lao động"},[20]=new DocumentType{DocumentName="Hợp đồng xác định thời hạn lần 1",DocumentGroup="Hợp đồng lao động"}}});
 }
 [HttpGet("profile-v5")]public IActionResult ProfileV5()=>View("~/Views/Profiles/Details.cshtml",new ProfileModel{Employee=new Employee(),Department=new Department{Division=new Division()},DocumentTypes=new(){[10]=new DocumentType{DocumentName="Hợp đồng thử việc",DocumentGroup="Hợp đồng lao động"}},Documents=[new Document{DocumentId=2,DocumentTypeId=10,IsCurrent=true,SpecificName="CURRENT_SECTION_TEST"},new Document{DocumentId=1,DocumentTypeId=10,SpecificName="HISTORY_SECTION_TEST"}],Warnings=[new DocumentQuality{DocumentId=1,DocumentName="Hợp đồng thử việc",QualityStatus="DA_HET_HAN",ExpiryDate=new DateOnly(2020,8,1)}]});
 [HttpGet("department")]public IActionResult Department()=>View("~/Views/Departments/Form.cshtml",new DepartmentForm());
 [HttpGet("grid")]public IActionResult Grid(string? q=null)=>View("~/Views/Shared/Grid.cshtml",new GridModel{Title="Kiểm tra trạng thái trống",Filter=new Filter{Q=q},Headers=["Mã","Tên"]});
 [HttpGet("dashboard")]public IActionResult Dashboard()=>View("~/Views/Shared/Dashboard.cshtml",new DashboardModel());
 // Test-only endpoint, no persistence; excluded from the website assembly.
 [HttpPost("document-validation"),IgnoreAntiforgeryToken]public IActionResult DocumentValidation([FromForm]Document value)=>ModelState.IsValid?Ok(new{value.StorageForm}):BadRequest(ModelState.Keys.Where(k=>ModelState[k]!.Errors.Count>0).ToArray());
 [HttpGet("document-edit")]public IActionResult DocumentEdit(bool create=false,bool legacy=false,bool needsStorage=false,bool nullStorage=false,bool literalNull=false,bool readOnly=false)=>View("~/Views/Shared/Record.cshtml",new EditRecord{Kind="documents",Title="Tài liệu",ReadOnly=readOnly,Editing=!create,Id=create?null:"91",Entity=new Document{DocumentId=91,EmployeeId="TEST0001",DocumentTypeId=needsStorage?8:7,IssueDate=new DateOnly(2024,5,10),IsCurrent=true,StorageForm=nullStorage?null:literalNull?"NULL":legacy?"OLD_VALUE":"Bản công chứng",FilePath="files/original.pdf"},Fields=CatalogService.Fields("documents"),Options=new(){["StorageForm"]=DocumentStorageForms.Options(),["DocumentTypeId"]=[new("Loại hiện tại","7"),new("Loại khác","8")]}});
 [HttpGet("profile-print")]public IActionResult ProfilePrint(bool empty=false){var degrees=new List<Qualification>{new(){QualificationId=1,QualificationLevel="Đại học",DegreeName="Bằng A",GraduationDate=new DateOnly(2020,1,1)},new(){QualificationId=2,QualificationLevel="Đại học",DegreeName="Bằng B",GraduationDate=null},new(){QualificationId=3,QualificationLevel="Thạc sĩ",DegreeName="Bằng C",GraduationDate=new DateOnly(2024,1,1)}};return View("~/Views/Profiles/Print.cshtml",new ProfileModel{Employee=new Employee{EmployeeId="TEST0001",FullName="Kiểm thử bản in",PhoneNumber="0123456789",Email="staff@example.test",Hometown="Quê quán thử"},Department=new Department{Division=new Division()},Qualifications=empty?[]:degrees.AsQueryable().ByGraduation().ToList(),DocumentTypes=new(){[1]=new DocumentType{DocumentTypeId=1,DocumentName="Hồ sơ mẫu",DocumentGroup="Hồ sơ gốc"},[2]=new DocumentType{DocumentTypeId=2,DocumentName="Loại chưa có",DocumentGroup="Hồ sơ gốc"},[3]=new DocumentType{DocumentTypeId=3,DocumentName="Hợp đồng mẫu",DocumentGroup="Hợp đồng lao động"},[4]=new DocumentType{DocumentTypeId=4,DocumentName="Quyết định mẫu",DocumentGroup="Quyết định / Văn bản khác"}},Documents=[new Document{DocumentId=1,DocumentTypeId=1,StorageForm="Bản gốc",IsCurrent=true},new Document{DocumentId=2,DocumentTypeId=1,StorageForm=null},new Document{DocumentId=3,DocumentTypeId=1,StorageForm="Bản copy"}]});}
 [HttpGet("qualification-form"),HttpGet("qualification-delete")]public IActionResult QualificationForm()=>View("~/Views/Shared/Record.cshtml",new EditRecord{Kind="qualifications",Title="Trình độ chuyên môn",Editing=true,ReadOnly=Request.Path.Value!.EndsWith("delete"),Deleting=Request.Path.Value!.EndsWith("delete"),Id="9",Entity=new Qualification{QualificationId=9,EmployeeId="TEST0001",QualificationLevel="Đại học",GraduationDate=new DateOnly(2024,1,1)},Fields=CatalogService.Fields("qualifications")});
 [HttpGet("record")]public IActionResult Record()=>View("~/Views/Shared/Record.cshtml",new EditRecord{Kind="types",Title="Kiểm tra biểu mẫu",Entity=new DocumentType(),Fields=CatalogService.Fields("types")});
}








// This interceptor always throws BEFORE the provider opens a connection (sync and async).
public sealed class OfflineDatabase : DbConnectionInterceptor
{
 public int Blocked {get;private set;}
 private InvalidOperationException Unavailable(){Blocked++;return new InvalidOperationException("No database provider connection permitted in Verification (simulated unavailable database).");}
 public override InterceptionResult ConnectionOpening(DbConnection connection,ConnectionEventData eventData,InterceptionResult result)=>throw Unavailable();
 public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection,ConnectionEventData eventData,InterceptionResult result,CancellationToken cancellationToken=default)=>throw Unavailable();
}
public sealed class VerificationScratch : IDisposable
{
 public string Root{get;}=Path.Combine(Path.GetTempPath(),"vtb-verification-"+Guid.NewGuid().ToString("N"));
 public VerificationScratch()=>Directory.CreateDirectory(Root);
 public void Dispose(){try{Directory.Delete(Root,true);}catch(IOException){Console.Error.WriteLine("Temporary Verification directory could not be removed.");}catch(UnauthorizedAccessException){Console.Error.WriteLine("Temporary Verification directory could not be removed.");}}
}
