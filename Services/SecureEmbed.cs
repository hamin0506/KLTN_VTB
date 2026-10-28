using Microsoft.AspNetCore.WebUtilities;
namespace Vtb.Hr.Web.Services;
public static class SecureEmbed
{
 public static string? Validate(string? raw)
 {
  if(!Uri.TryCreate(raw,UriKind.Absolute,out var u)||u.Scheme!="https"||!u.Host.Equals("app.powerbi.com",StringComparison.OrdinalIgnoreCase)||u.Port!=443||u.AbsolutePath!="/reportEmbed"||!string.IsNullOrEmpty(u.UserInfo)||!string.IsNullOrEmpty(u.Fragment))return null;
  var q=QueryHelpers.ParseQuery(u.Query);if(!q.TryGetValue("reportId",out var report)||report.Count!=1||!Guid.TryParse(report[0],out _))return null;
  return u.AbsoluteUri;
 }
}