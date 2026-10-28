using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Services;
namespace Vtb.Hr.Web.Controllers;
public class ProfilesController(VtbContext db):Controller
{
 public IActionResult Index()=>RedirectToAction("Index","Catalog",new{kind="employees"});
 public async Task<IActionResult> Details(string id){var model=await Load(id);return model==null?NotFound():View(model);}
 public async Task<IActionResult> Print(string id){var model=await Load(id);return model==null?NotFound():View(model);}
 private async Task<ProfileModel?> Load(string id)
 {
  var employee=await db.Employees.AsNoTracking().SingleOrDefaultAsync(x=>x.EmployeeId==id);if(employee==null)return null;
  var orderedTypes=await db.DocumentTypes.AsNoTracking().InRequirementOrder(db).ToListAsync();
  var types=orderedTypes.ToDictionary(x=>x.DocumentTypeId);
  var qualityRows=await db.DocumentQualities.AsNoTracking().Where(x=>x.EmployeeId==id).ToListAsync();
  return new ProfileModel{OrderedDocumentTypeIds=orderedTypes.Select(x=>x.DocumentTypeId).ToList(),Qualifications=await db.Qualifications.AsNoTracking().Where(x=>x.EmployeeId==id).ByGraduation().ToListAsync(),Employee=employee,Department=await db.Departments.AsNoTracking().Include(x=>x.Division).SingleAsync(x=>x.DepartmentId==employee.DepartmentId),
   Quality=await db.EmployeeQualities.SingleOrDefaultAsync(x=>x.EmployeeId==id),Contract=await db.LaborContractChecks.AsNoTracking().SingleOrDefaultAsync(x=>x.EmployeeId==id),
   Documents=await db.Documents.AsNoTracking().Where(x=>x.EmployeeId==id).InRequirementOrder(db).ToListAsync(),
   Missing=await db.DocumentRequirements.Where(x=>x.EmployeeId==id&&x.RequirementStatus=="THIEU_HO_SO").ToListAsync(),Warnings=qualityRows.Where(x=>x.QualityStatus!="HOP_LE").ToList(),DocumentQualityRows=qualityRows,DocumentTypes=types,
   TypeNames=types.ToDictionary(x=>x.Key,x=>x.Value.DocumentName)};
 }
}