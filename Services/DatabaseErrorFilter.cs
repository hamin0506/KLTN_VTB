using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
namespace Vtb.Hr.Web.Services;
public class DatabaseErrorFilter(ILogger<DatabaseErrorFilter> logger):IExceptionFilter
{
 public void OnException(ExceptionContext c) {
  if(c.Exception is SqlException || c.Exception is InvalidOperationException && c.Exception.Message.Contains("No database provider")) {
   logger.LogError(c.Exception,"Database read failed; trace {TraceId}",c.HttpContext.TraceIdentifier);
   c.Result=new ViewResult{ViewName="DatabaseUnavailable",StatusCode=503};c.ExceptionHandled=true;
  }
 }
}