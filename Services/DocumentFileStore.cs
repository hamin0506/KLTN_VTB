using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
namespace Vtb.Hr.Web.Services;

// Only server-generated references can resolve to files. Never resolve legacy paths supplied by users.
public sealed class DocumentFileStore(IWebHostEnvironment environment, ILogger<DocumentFileStore> logger)
{
 public const long MaxBytes=10*1024*1024;
 private string Root=>Path.Combine(environment.ContentRootPath,"App_Data","DocumentFiles");
 private static readonly Regex ReferencePattern=new(@"^[a-f0-9]{32}\.(pdf|jpg|jpeg|png)$",RegexOptions.CultureInvariant);
 public static bool IsReference(string? value)=>value!=null&&ReferencePattern.IsMatch(value);
 public static string ContentType(string reference)=>Path.GetExtension(reference) switch{".pdf"=>"application/pdf",".png"=>"image/png",_=>"image/jpeg"};
 public string? Resolve(string? reference)=>IsReference(reference)?Path.Combine(Root,reference!):null;
 public bool Exists(string? reference)=>Resolve(reference) is {} path&&File.Exists(path);
 public static async Task<byte[]> ValidateAsync(IFormFile upload,CancellationToken ct=default)
 {
  if(upload.Length<=0||upload.Length>MaxBytes)throw new InvalidDataException("Tệp phải có nội dung và không vượt quá 10 MB.");
  var extension=Path.GetExtension(upload.FileName).ToLowerInvariant();
  if(extension is not (".pdf" or ".jpg" or ".jpeg" or ".png"))throw new InvalidDataException("Chỉ chấp nhận tệp PDF, JPG, JPEG hoặc PNG.");
  await using var source=upload.OpenReadStream();using var buffer=new MemoryStream();var chunk=new byte[81920];int count;
  while((count=await source.ReadAsync(chunk,ct))>0){if(buffer.Length+count>MaxBytes)throw new InvalidDataException("Tệp không được vượt quá 10 MB.");await buffer.WriteAsync(chunk.AsMemory(0,count),ct);}
  var bytes=buffer.ToArray();
  var valid=extension switch{
   ".pdf"=>bytes.AsSpan().StartsWith("%PDF-"u8)&&System.Text.Encoding.ASCII.GetString(bytes,Math.Max(0,bytes.Length-2048),Math.Min(2048,bytes.Length)).Contains("%%EOF"),
   ".png"=>bytes.AsSpan().StartsWith(new byte[]{137,80,78,71,13,10,26,10})&&bytes.Length>=33&&bytes.AsSpan(12,4).SequenceEqual("IHDR"u8)&&bytes.AsSpan(Math.Max(0,bytes.Length-8),4).SequenceEqual("IEND"u8),
   _=>bytes.Length>=4&&bytes[0]==255&&bytes[1]==216&&bytes[2]==255&&bytes[^2]==255&&bytes[^1]==217};
  if(!valid)throw new InvalidDataException("Nội dung tệp không khớp định dạng đã chọn hoặc tệp bị hỏng.");
  return bytes;
 }
 public async Task<string> StoreAsync(byte[] bytes,string fileName,CancellationToken ct=default)
 {
  var ext=Path.GetExtension(fileName).ToLowerInvariant();
  if(ext is not (".pdf" or ".jpg" or ".jpeg" or ".png"))throw new InvalidDataException("Định dạng không hợp lệ.");
  Directory.CreateDirectory(Root);var reference=Guid.NewGuid().ToString("N")+ext;var path=Resolve(reference)!;var created=false;
  try{await using var output=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.None,81920,true);created=true;await output.WriteAsync(bytes,ct);}
  catch{if(created)Delete(reference);throw;}
  return reference;
 }
 public void Delete(string? reference){try{if(Resolve(reference) is {} path)File.Delete(path);}catch(IOException){logger.LogWarning("Attachment cleanup deferred due to file access.");}catch(UnauthorizedAccessException){logger.LogWarning("Attachment cleanup requires storage permission.");}}
 public async Task CleanupUnusedAsync(string? reference,VtbContext db)
 {
  if(!IsReference(reference))return;
  try{if(!await db.Documents.AsNoTracking().AnyAsync(d=>d.FilePath==reference))Delete(reference);}
  catch{logger.LogWarning("Attachment cleanup deferred because references could not be verified.");}
 }
}
