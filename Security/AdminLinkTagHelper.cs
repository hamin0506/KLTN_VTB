using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
namespace Vtb.Hr.Web.Security;
[HtmlTargetElement("a",Attributes="asp-action")]
public class AdminLinkTagHelper(IConfiguration config):TagHelper
{
 [ViewContext,HtmlAttributeNotBound]public ViewContext ViewContext{get;set;}=null!;
 public override void Process(TagHelperContext context,TagHelperOutput output){var action=context.AllAttributes["asp-action"]?.Value?.ToString();if(action is "Create" or "Edit" or "Delete"&&!Access.Admin(ViewContext.HttpContext.User,config))output.SuppressOutput();}
}