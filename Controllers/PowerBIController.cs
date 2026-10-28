using Microsoft.AspNetCore.Mvc;
namespace Vtb.Hr.Web.Controllers;
public class PowerBIController:Controller
{
 public IActionResult Index()=>RedirectToAction("Index","Dashboard");
}
