using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Services;
namespace Vtb.Hr.Web.Controllers;
[Authorize]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class DashboardController(DashboardService service,ILogger<DashboardController> logger):Controller
{
 [HttpGet]public IActionResult Index()=>View();
 [HttpGet]public Task<IActionResult> Data([FromQuery]OverviewFilter filter,CancellationToken ct)=>Read(filter,()=>service.Overview(filter,ct));
 [HttpGet]public Task<IActionResult> Options([FromQuery]OverviewFilter filter,CancellationToken ct)=>Read(filter,()=>service.Options(filter,ct));
 [HttpGet]public Task<IActionResult> Missing([FromQuery]OverviewFilter filter,CancellationToken ct)=>Read(filter,()=>service.Missing(filter,ct));
  [HttpGet]public IActionResult DocumentDetails([FromQuery]DocumentDrillFilter filter){if(!ModelState.IsValid)return BadRequest("Trạng thái hoặc bộ lọc không hợp lệ.");return View(filter);}
 [HttpGet]public Task<IActionResult> DocumentDetailsData([FromQuery]DocumentDrillFilter filter,CancellationToken ct)=>Read(filter.Scope(),()=>service.DocumentDetails(filter,ct));
 [HttpGet]public IActionResult ProblemDocuments()=>View();
 [HttpGet]public IActionResult Contract()=>View();
 [HttpGet]public IActionResult ContractDetails([FromQuery]ReportFilter filter){if(!ModelState.IsValid||filter.State==null)return BadRequest("Chọn trạng thái hợp đồng hợp lệ.");return View(filter);}
 [HttpGet]public Task<IActionResult> ProblemData([FromQuery]ReportFilter filter,CancellationToken ct)=>Read(filter.Scope(),()=>service.ProblemData(filter,ct));
 [HttpGet]public Task<IActionResult> ContractData([FromQuery]ReportFilter filter,CancellationToken ct)=>Read(filter.Scope(),()=>service.ContractData(filter,ct));
 [HttpGet]public Task<IActionResult> ReportOptions([FromQuery]ReportFilter filter,CancellationToken ct)=>Read(filter.Scope(),()=>service.ReportOptions(filter,ct));
 [HttpGet]public Task<IActionResult> ReportTable([FromQuery]ReportFilter filter,CancellationToken ct)=>Read(filter.Scope(),()=>service.ReportRows(filter,ct));
 private async Task<IActionResult> Read<T>(OverviewFilter filter,Func<Task<T>> query){
  if(!ModelState.IsValid)return BadRequest(new{message="Bộ lọc hoặc phân trang không hợp lệ."});
  try{return Json(await query());}
  catch(OperationCanceledException) when(HttpContext.RequestAborted.IsCancellationRequested){
   // A newer filter request/navigation can abort the old database read. Do not treat it as a database outage.
   logger.LogDebug("Dashboard request canceled by client: {TraceId}",HttpContext.TraceIdentifier);
   return StatusCode(499);
  }
  catch(SqlException e){logger.LogError(e,"Executive Overview SQL read failed");return StatusCode(503,new{message="Không thể đọc dữ liệu VTB2. Kiểm tra kết nối SQL Server và quyền truy cập.",traceId=HttpContext.TraceIdentifier});}
  catch(InvalidOperationException e) when(e.Message.Contains("No database provider")){return StatusCode(503,new{message="Chưa cấu hình kết nối database."});}
 }
}


