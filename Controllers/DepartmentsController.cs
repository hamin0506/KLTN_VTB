using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Controllers;
public class DepartmentsController(VtbContext db, IConfiguration config, ILogger<DepartmentsController> logger) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (string.IsNullOrWhiteSpace(config.GetConnectionString("VTB2")))
            context.Result = RedirectToAction("Setup", "Home");
    }
    public async Task<IActionResult> Index(string? q, int page = 1, int? divisionId = null)
    {
        try
        {
            var query = db.Departments.AsNoTracking().Include(x => x.Division).AsQueryable();
            if (divisionId.HasValue) query = query.Where(x => x.DivisionId == divisionId);
            q = q?.Trim();
            if (!string.IsNullOrEmpty(q)) query = query.Where(x => x.DepartmentCode.Contains(q) || x.DepartmentName.Contains(q) || x.Division.DivisionName.Contains(q));
            var total = await query.CountAsync();
            page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / 10d)));
            return View(new DepartmentList { DivisionId = divisionId, Divisions = await db.Divisions.AsNoTracking().OrderBy(x => x.DivisionName).Select(x => new SelectListItem(x.DivisionName, x.DivisionId.ToString())).ToListAsync(), Items = await query.OrderBy(x => x.DepartmentCode).Skip((page - 1) * 10).Take(10).ToListAsync(), Q = q, Page = page, Total = total });
        }
        catch (SqlException ex) { return ConnectionError(ex); }
    }
    public Task<IActionResult> Create() => Form(new DepartmentForm());
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var item = await db.Departments.FindAsync(id);
            return item == null ? NotFound() : await Form(new DepartmentForm { DepartmentId = item.DepartmentId, DivisionId = item.DivisionId, DepartmentCode = item.DepartmentCode, DepartmentName = item.DepartmentName });
        }
        catch (SqlException ex) { return ConnectionError(ex); }
    }
    [HttpPost] public Task<IActionResult> Create(DepartmentForm model) { model.DepartmentId = 0; return Save(model, false); }
    [HttpPost] public Task<IActionResult> Edit(int id, DepartmentForm model) => id != model.DepartmentId ? Task.FromResult<IActionResult>(BadRequest()) : Save(model, true);
    private async Task<IActionResult> Save(DepartmentForm model, bool edit)
    {
        model.DepartmentCode = model.DepartmentCode?.Trim() ?? "";
        model.DepartmentName = model.DepartmentName?.Trim() ?? "";
        if (!ModelState.IsValid) return await Form(model);
        try
        {
            if (!await db.Divisions.AnyAsync(x => x.DivisionId == model.DivisionId)) ModelState.AddModelError(nameof(model.DivisionId), "Đơn vị không còn tồn tại. Vui lòng chọn lại.");
            if (await db.Departments.AnyAsync(x => x.DepartmentId != model.DepartmentId && x.DepartmentCode == model.DepartmentCode)) ModelState.AddModelError(nameof(model.DepartmentCode), "Mã phòng ban đã tồn tại.");
            if (!ModelState.IsValid) return await Form(model);
            var item = edit ? await db.Departments.FindAsync(model.DepartmentId) : new Department();
            if (item == null) return NotFound();
            item.DivisionId = model.DivisionId;
            item.DepartmentCode = model.DepartmentCode;
            item.DepartmentName = model.DepartmentName;
            if (!edit) db.Add(item);
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã lưu phòng ban thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException) { ModelState.AddModelError("", "Bản ghi đã thay đổi hoặc bị xóa. Vui lòng tải lại."); }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Department save failed");
            ModelState.AddModelError("", ex.InnerException is SqlException sql ? sql.Number switch {
                2601 or 2627 => "Mã phòng ban đã tồn tại.",
                547 => "Đơn vị trực thuộc không còn tồn tại. Vui lòng chọn lại.",
                _ => "Không thể lưu phòng ban. Kiểm tra kết nối và quyền truy cập."
            } : "Không thể lưu phòng ban. Vui lòng thử lại.");
        }
        catch (SqlException ex) { return ConnectionError(ex); }
        return await Form(model);
    }
    private async Task<IActionResult> Form(DepartmentForm model)
    {
        try
        {
            model.Divisions = await db.Divisions.AsNoTracking().OrderBy(x => x.DivisionName).Select(x => new SelectListItem { Value = x.DivisionId.ToString(), Text = x.DivisionCode + " — " + x.DivisionName }).ToListAsync();
            return View("Form", model);
        }
        catch (SqlException ex) { return ConnectionError(ex); }
    }
    public Task<IActionResult> Details(int id) => Read(id, "Details");
    public Task<IActionResult> Delete(int id) => Read(id, "Delete");
    private async Task<IActionResult> Read(int id, string view)
    {
        try { var item = await db.Departments.AsNoTracking().Include(x => x.Division).SingleOrDefaultAsync(x => x.DepartmentId == id); return item == null ? NotFound() : View(view, item); }
        catch (SqlException ex) { return ConnectionError(ex); }
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var item = await db.Departments.FindAsync(id);
            if (item == null) return NotFound();
            db.Remove(item);
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa phòng ban.";
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Department delete failed");
            TempData["Error"] = ex.InnerException is SqlException { Number: 547 } ? "Không thể xóa: phòng ban đang có cán bộ trực thuộc." : "Không thể xóa phòng ban. Vui lòng tải lại và thử lại.";
        }
        catch (SqlException ex) { return ConnectionError(ex); }
        return RedirectToAction(nameof(Index));
    }
    private IActionResult ConnectionError(Exception ex)
    {
        logger.LogError(ex, "Department database unavailable");
        TempData["Error"] = "Chưa truy cập được VTB2. Kiểm tra kết nối SQL Server.";
        return RedirectToAction("Setup", "Home");
    }
}