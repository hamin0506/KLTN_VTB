using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Controllers;
public class DivisionsController(VtbContext db, IConfiguration config, ILogger<DivisionsController> logger) : Controller
{
 public override void OnActionExecuting(ActionExecutingContext c) { if(string.IsNullOrWhiteSpace(config.GetConnectionString("VTB2"))) c.Result=RedirectToAction("Setup","Home"); }
 public async Task<IActionResult> Index(string? q,int page=1) {
  try { var query=db.Divisions.AsNoTracking(); q=q?.Trim(); if(!string.IsNullOrEmpty(q)) query=query.Where(x=>x.DivisionCode.Contains(q)||x.DivisionName.Contains(q));
   var total=await query.CountAsync(); page=Math.Clamp(page,1,Math.Max(1,(int)Math.Ceiling(total/10d)));
   return View(new DivisionList{Items=await query.OrderBy(x=>x.DivisionCode).Skip((page-1)*10).Take(10).ToListAsync(),Q=q,Page=page,Total=total});
  } catch(SqlException ex){return ConnectionError(ex);}
 }
 public IActionResult Create()=>View("Form",new DivisionForm());
 public async Task<IActionResult> Edit(int id) { try {var e=await db.Divisions.FindAsync(id); return e==null?NotFound():View("Form",new DivisionForm{DivisionId=e.DivisionId,DivisionCode=e.DivisionCode,DivisionName=e.DivisionName});} catch(SqlException ex){return ConnectionError(ex);} }
 [HttpPost] public Task<IActionResult> Create(DivisionForm model)=>Save(model,false);
 [HttpPost] public async Task<IActionResult> Edit(int id,DivisionForm model){if(id!=model.DivisionId)return BadRequest();return await Save(model,true);}
 private async Task<IActionResult> Save(DivisionForm m,bool edit) {
  m.DivisionCode=m.DivisionCode?.Trim()??"";m.DivisionName=m.DivisionName?.Trim()??"";
  if(!edit)m.DivisionId=0;
  if(!ModelState.IsValid)return View("Form",m);
  try {
   if(await db.Divisions.AnyAsync(x=>x.DivisionId!=m.DivisionId&&x.DivisionCode==m.DivisionCode))ModelState.AddModelError("DivisionCode","Mã đơn vị đã tồn tại.");
   if(await db.Divisions.AnyAsync(x=>x.DivisionId!=m.DivisionId&&x.DivisionName==m.DivisionName))ModelState.AddModelError("DivisionName","Tên đơn vị đã tồn tại.");
   if(!ModelState.IsValid)return View("Form",m);
   var e=edit?await db.Divisions.FindAsync(m.DivisionId):new Division(); if(e==null)return NotFound();
   e.DivisionCode=m.DivisionCode;e.DivisionName=m.DivisionName;if(!edit)db.Add(e);
   await db.SaveChangesAsync();TempData["Success"]="Đã lưu đơn vị thành công.";return RedirectToAction(nameof(Index));
  } catch(DbUpdateConcurrencyException){ModelState.AddModelError("","Bản ghi đã thay đổi hoặc bị xóa. Vui lòng tải lại.");}
  catch(DbUpdateException ex){logger.LogWarning(ex,"Division save failed");ModelState.AddModelError("",ex.InnerException is SqlException {Number:2601 or 2627}?"Mã hoặc tên đã tồn tại. Vui lòng kiểm tra lại.":"Không thể lưu dữ liệu. Kiểm tra kết nối và quyền truy cập SQL Server.");}
  catch(SqlException ex){return ConnectionError(ex);}return View("Form",m);
 }
 public async Task<IActionResult> Details(int id) {try{var e=await db.Divisions.AsNoTracking().SingleOrDefaultAsync(x=>x.DivisionId==id);return e==null?NotFound():View(e);}catch(SqlException ex){return ConnectionError(ex);}}
 public async Task<IActionResult> Delete(int id) {try{var e=await db.Divisions.FindAsync(id);return e==null?NotFound():View(e);}catch(SqlException ex){return ConnectionError(ex);}}
 [HttpPost,ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id) {
  try {var e=await db.Divisions.FindAsync(id);if(e==null)return NotFound();db.Remove(e);await db.SaveChangesAsync();TempData["Success"]="Đã xóa đơn vị.";return RedirectToAction(nameof(Index));}
  catch(DbUpdateException ex){logger.LogWarning(ex,"Division delete failed");TempData["Error"]=ex.InnerException is SqlException {Number:547}?"Không thể xóa: đơn vị đang có phòng ban trực thuộc.":"Không thể xóa bản ghi. Vui lòng tải lại và thử lại.";return RedirectToAction(nameof(Index));}
  catch(SqlException ex){return ConnectionError(ex);}
 }
 private IActionResult ConnectionError(Exception ex){logger.LogError(ex,"Database unavailable");TempData["Error"]="Chưa truy cập được VTB2. Kiểm tra tên server, xác thực và quyền database.";return RedirectToAction("Setup","Home");}
}