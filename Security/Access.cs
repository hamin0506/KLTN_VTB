using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Vtb.Hr.Web.Security;
public static class Access
{
    public static bool Admin(ClaimsPrincipal user, IConfiguration config) => Member(user, config, "Administrators");
    public static bool Read(ClaimsPrincipal user, IConfiguration config) => Admin(user,config) || Member(user,config,"Readers");
    private static bool Member(ClaimsPrincipal user,IConfiguration config,string role) => user.Identity?.IsAuthenticated == true && (config.GetSection("Access:"+role).Get<string[]>() ?? []).Contains(user.Identity.Name ?? "",StringComparer.OrdinalIgnoreCase);
}
public class WriteAccessFilter(IAuthorizationService auth) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if(context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()) return;
        var action=(context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)?.ActionName;
        if((!HttpMethods.IsGet(context.HttpContext.Request.Method) && !HttpMethods.IsHead(context.HttpContext.Request.Method)) || action is "Create" or "Edit" or "Delete")
            if(!(await auth.AuthorizeAsync(context.HttpContext.User,"Admin")).Succeeded) context.Result=new ForbidResult();
    }
}
