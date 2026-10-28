using Microsoft.AspNetCore.Mvc;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Services;
namespace Vtb.Hr.Web.Controllers;
public class QualityController(QualityService service):Controller
{
 public async Task<IActionResult> Index(Filter f)=>View("Dashboard",await service.Dashboard(f));
 public async Task<IActionResult> List(string kind,Filter f)=>kind is "documents" or "requirements" or "contracts" or "employees"?View("Grid",await service.List(kind,f)):NotFound();
}