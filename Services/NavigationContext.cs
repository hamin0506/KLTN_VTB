using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
namespace Vtb.Hr.Web.Services;

// Return navigation may target only known read pages, never an external URL or write action.
public static class NavigationContext
{
 public static string? Safe(string? value){
  if(string.IsNullOrEmpty(value)||value.Length>4096||value.Any(char.IsControl)||value.Contains('\\'))return null;
  var path=value.Split('?', '#')[0];
  return Regex.IsMatch(path,@"^/(?:Catalog(?:/Index)?|Quality(?:/Index|/List)?|Divisions(?:/Index)?|Departments(?:/Index)?|Dashboard(?:/Index|/ProblemDocuments|/Contract|/ContractDetails|/DocumentDetails)?|Profiles/Details(?:/[A-Za-z0-9]{8})?)/?$",RegexOptions.IgnoreCase)?value:null;
 }
 // Organization edit forms return only to their own list, retaining the original query verbatim.
 public static string? OrganizationReturn(HttpRequest request,string controller){
  if(controller is not ("Departments" or "Divisions"))return null;
  var target=Return(request);if(target==null)return null;
  var path=target.Split('?', '#')[0].TrimEnd('/');
  return path.Equals("/"+controller,StringComparison.OrdinalIgnoreCase)||path.Equals("/"+controller+"/Index",StringComparison.OrdinalIgnoreCase)?target:null;
 }
 public static string? Current(HttpRequest request)=>Safe(request.PathBase+request.Path+request.QueryString);
 public static string? Return(HttpRequest request)=>Safe(request.HasFormContentType?request.Form["returnUrl"].ToString():request.Query["returnUrl"].ToString());
}
