using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vtb.Hr.Web.Services;
namespace Vtb.Hr.Web.Controllers;
public class HomeController:Controller
{
 public IActionResult Index()=>RedirectToAction("Index","Dashboard");
 public IActionResult Setup()=>View();
 [AllowAnonymous]public IActionResult Error(){Response.StatusCode=500;return View();}
 public IActionResult Module(string id)=>id switch{
 "employees" or "profiles"=>RedirectToAction("Index","Catalog",new{kind="employees"}),"documents"=>RedirectToAction("Index","Catalog",new{kind="types"}),"quality"=>RedirectToAction("Index","Catalog",new{kind="rules"}),"powerbi"=>RedirectToAction("Index"),_=>NotFound()};
}

