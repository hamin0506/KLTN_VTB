using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Services;
namespace Vtb.Hr.Web.Controllers;
[Authorize]
public class DocumentFilesController(VtbContext db,DocumentFileStore files):Controller
{
 [HttpGet]
 public async Task<IActionResult> Open(int id,bool download=false)
 {
  var document=await db.Documents.AsNoTracking().SingleOrDefaultAsync(d=>d.DocumentId==id,HttpContext.RequestAborted);
  if(document==null)return NotFound();
  var path=files.Resolve(document.FilePath);
  if(path==null||!System.IO.File.Exists(path))return Missing();
  FileStream stream;
  try{stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read|FileShare.Delete);}
  catch(IOException){return Missing();}catch(UnauthorizedAccessException){return Missing();}
  Response.Headers.CacheControl="no-store";Response.Headers["X-Content-Type-Options"]="nosniff";
  Response.Headers["Content-Security-Policy"]="sandbox; default-src 'none'; frame-ancestors 'self'";
  return new FileStreamResult(stream,DocumentFileStore.ContentType(document.FilePath!)){EnableRangeProcessing=true,FileDownloadName=download?$"tai-lieu-{id}{Path.GetExtension(document.FilePath)}":null};
 }
 private ContentResult Missing()=>new(){StatusCode=404,ContentType="text/plain; charset=utf-8",Content="Không tìm thấy tệp đính kèm"};
}
