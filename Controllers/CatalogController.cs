using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Services;
namespace Vtb.Hr.Web.Controllers;
public class CatalogController(VtbContext db,CatalogService service,ILogger<CatalogController> logger,DocumentFileStore files):Controller
{
 public async Task<IActionResult> Index(string kind,Filter f)=>kind switch{
  "employees"=>View("Grid",await service.List<Employee>(kind,f)),"qualifications"=>View("Grid",await service.List<Qualification>(kind,f)),"documents" when string.IsNullOrWhiteSpace(f.EmployeeId)=>RedirectToAction(nameof(Index),new{kind="types"}),"documents"=>View("Grid",await service.List<Document>(kind,f)),"types"=>View("Grid",await service.List<DocumentType>(kind,f)),"rules"=>View("Grid",await service.List<RequirementRule>(kind,f)),_=>NotFound()};
 [Authorize(Policy="Admin")]
 public Task<IActionResult> Create(string kind,string? employeeId,int? documentTypeId)=>Render(kind,null,false,false,employeeId,documentTypeId);
 [Authorize(Policy="Admin")]
 public Task<IActionResult> Edit(string kind,string id)=>Render(kind,id,false,false);
 public Task<IActionResult> Details(string kind,string id)=>Render(kind,id,true,false);
 [Authorize(Policy="Admin")]
 public Task<IActionResult> Delete(string kind,string id)=>Render(kind,id,true,true);
 private async Task<IActionResult> Render(string kind,string? id,bool readOnly,bool deleting,string? employeeId=null,int? documentTypeId=null)
 {
  if(!CatalogService.Known(kind))return NotFound();object? entity=id==null?kind switch{"employees"=>new Employee(),"qualifications"=>new Qualification{EmployeeId=employeeId??""},"documents"=>new Document{EmployeeId=employeeId??"",DocumentTypeId=documentTypeId??0},"types"=>new DocumentType(),_=>new RequirementRule()}:await Find(kind,id);
  if(entity==null)return NotFound();
  return await Form(kind,entity,id!=null,readOnly,deleting);
 }
 private async Task<object?> Find(string kind,string id){if(kind=="employees")return await db.Employees.FindAsync(id);if(!int.TryParse(id,out var key)||key<=0)return null;return kind switch{"documents"=>await db.Documents.FindAsync(key),"qualifications"=>await db.Qualifications.FindAsync(key),"types"=>await db.DocumentTypes.FindAsync(key),"rules"=>await db.RequirementRules.FindAsync(key),_=>null};}
 private async Task<IActionResult> Form(string kind,object entity,bool edit,bool read=false,bool delete=false)=>View("Record",new EditRecord{Kind=kind,Title=CatalogService.Title(kind),Id=edit?CatalogService.Id(entity):null,Entity=entity,Editing=edit,ReadOnly=read,Deleting=delete,Fields=CatalogService.Fields(kind),Options=await service.Options(kind),OriginalStorageForm=entity is Document doc?db.Entry(doc).Property(x=>x.StorageForm).OriginalValue:null});
 [HttpPost,Authorize(Policy="Admin"),RequestSizeLimit(12*1024*1024),RequestFormLimits(MultipartBodyLengthLimit=12*1024*1024),ActionName("Create")] public Task<IActionResult> CreatePost(string kind)=>Dispatch(kind,null);
 [HttpPost,Authorize(Policy="Admin"),RequestSizeLimit(12*1024*1024),RequestFormLimits(MultipartBodyLengthLimit=12*1024*1024),ActionName("Edit")] public Task<IActionResult> EditPost(string kind,string id)=>Dispatch(kind,id);
 private Task<IActionResult> Dispatch(string kind,string? id)=>kind switch{"employees"=>Save<Employee>(kind,id),"qualifications"=>Save<Qualification>(kind,id),"documents"=>Save<Document>(kind,id),"types"=>Save<DocumentType>(kind,id),"rules"=>Save<RequirementRule>(kind,id),_=>Task.FromResult<IActionResult>(NotFound())};
 private async Task<IActionResult> Save<T>(string kind,string? id) where T:class,new()
 {
  var entity=id==null?new T():(T?)await Find(kind,id);if(entity==null)return NotFound();var fields=CatalogService.Fields(kind).Select(x=>x.Name).ToHashSet();if(id!=null&&kind=="employees")fields.Remove("EmployeeId");
  // Metadata corrections update this row; ownership and the DB-managed current flag stay immutable.
  if(id!=null&&(entity is Document||entity is Qualification)){fields.Remove("EmployeeId");}
  var originalStorage=entity is Document before?before.StorageForm:null;
  if(entity is Document normalizing)normalizing.StorageForm=DocumentStorageForms.Normalize(normalizing.StorageForm);
  await TryUpdateModelAsync(entity,"",p=>fields.Contains(p.PropertyName??""));
  if(id!=null&&entity is Document&&!DocumentStorageForms.IsAllowed(DocumentStorageForms.Normalize(originalStorage))&&Request.Form["ReplaceUnsupportedStorage"]!="true")
   ModelState.AddModelError("StorageForm","Giá trị cũ cần kiểm tra. Hãy chọn hình thức phù hợp và xác nhận thay thế giá trị cũ.");
  // Normalize allowed text fields and revalidate after normalization.
  foreach(var p in typeof(T).GetProperties().Where(p=>p.PropertyType==typeof(string)&&fields.Contains(p.Name)&&p.Name!="StorageForm"&&Request.HasFormContentType&&Request.Form.ContainsKey(p.Name)))p.SetValue(entity,((string?)p.GetValue(entity))?.Trim());
  if(entity is Document){
   foreach(var field in new[]{"IssueDate","ExpiryDate","DocumentTypeId"}){
    if(ModelState.TryGetValue(field,out var state)&&state.Errors.Count>0&&(field=="DocumentTypeId"?!int.TryParse(state.AttemptedValue,out _):!DateOnly.TryParse(state.AttemptedValue,out _))){
     state.Errors.Clear();ModelState.AddModelError(field,field=="DocumentTypeId"?"Vui lòng chọn loại tài liệu hợp lệ.":"Vui lòng nhập ngày hợp lệ.");
    }
   }
  }
  byte[]? attachmentBytes=null;IFormFile? attachment=null;
  if(entity is Document){
   var uploads=Request.Form.Files;
   if(uploads.Count>1||uploads.Any(f=>f.Name!="Attachment"))ModelState.AddModelError("Attachment","Mỗi tài liệu chỉ được đính kèm một tệp.");
   attachment=uploads.GetFile("Attachment");
   if(attachment!=null)try{attachmentBytes=await DocumentFileStore.ValidateAsync(attachment,HttpContext.RequestAborted);}catch(InvalidDataException ex){ModelState.AddModelError("Attachment",ex.Message);}
  }
  if(!ModelState.IsValid)return await Form(kind,entity,id!=null);
  var removeAttachment=id!=null&&entity is Document&&Request.Form["RemoveAttachment"]=="true"&&attachment==null;
  var oldFile=(entity as Document)?.FilePath;string? newFile=null;var commitAttempted=false;
  // TryUpdateModelAsync has already validated binding and the entity. Never erase ModelState errors.
  try{
   await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
   foreach(var error in await service.Validate(entity,id!=null))ModelState.AddModelError(error.Field,error.Error);
   if(!ModelState.IsValid){await tx.RollbackAsync();await tx.DisposeAsync();return await Form(kind,entity,id!=null);}
   if(entity is Document withFile&&attachmentBytes!=null){if(id!=null)oldFile=await db.Documents.AsNoTracking().Where(d=>d.DocumentId==withFile.DocumentId).Select(d=>d.FilePath).SingleAsync();newFile=await files.StoreAsync(attachmentBytes,attachment!.FileName,HttpContext.RequestAborted);withFile.FilePath=newFile;}
   if(entity is Document removing&&removeAttachment){
    var currentFile=await db.Documents.AsNoTracking().Where(d=>d.DocumentId==removing.DocumentId).Select(d=>d.FilePath).SingleAsync();
    if(currentFile!=oldFile){await tx.RollbackAsync();await tx.DisposeAsync();ModelState.AddModelError("Attachment","Tệp đã được thay đổi bởi một thao tác khác. Hãy tải lại trang trước khi xóa.");return await Form(kind,entity,true);}
    removing.FilePath=null;
   }
   if(entity is Document document){document.UpdatedAt=await db.Database.SqlQuery<DateTime>($"SELECT SYSDATETIME() AS [Value]").SingleAsync();}
   if(id==null)db.Add(entity);await db.SaveChangesAsync();commitAttempted=true;await tx.CommitAsync();await tx.DisposeAsync();if(newFile!=null||removeAttachment)await files.CleanupUnusedAsync(oldFile,db);TempData["Success"]="Đã lưu dữ liệu thành công.";return entity is Qualification degree?RedirectToAction("Details","Profiles",new{id=degree.EmployeeId}):entity is Document saved?RedirectToAction("Details","Profiles",new{id=saved.EmployeeId}):RedirectToAction(nameof(Index),new{kind});
  }catch(DbUpdateConcurrencyException){ModelState.AddModelError("","Bản ghi đã bị thay đổi hoặc xóa. Vui lòng tải lại.");}
  catch(DbUpdateException ex){logger.LogWarning(ex,"Save {Kind} failed",kind);if(entity is Document&&ex.InnerException is SqlException {Number:2601 or 2627})ModelState.AddModelError("DocumentTypeId","Cán bộ đã có tài liệu hiện hành thuộc loại này. Vui lòng chọn lại hoặc tải lại hồ sơ.");else ModelState.AddModelError("",ex.InnerException is SqlException {Number:2601 or 2627}?"Dữ liệu bị trùng mã hoặc tên.":"Không thể lưu do ràng buộc hoặc quyền truy cập. Kiểm tra dữ liệu và thử lại.");}
  catch(SqlException ex) when(ex.Number==1205){ModelState.AddModelError("","Dữ liệu đang được cập nhật đồng thời. Vui lòng thử lại.");}
  catch(IOException){ModelState.AddModelError("Attachment","Không thể lưu tệp. Vui lòng thử lại hoặc kiểm tra quyền thư mục lưu trữ.");}
  catch(UnauthorizedAccessException){ModelState.AddModelError("Attachment","Máy chủ chưa có quyền lưu tệp đính kèm.");}
  finally{
   // Commit may have succeeded even if SQL acknowledgement was lost: verify references before deleting.
   if(newFile!=null){if(!commitAttempted)files.Delete(newFile);else await files.CleanupUnusedAsync(newFile,db);}
  }
  if(entity is Document failedDocument)failedDocument.FilePath=oldFile;
  return await Form(kind,entity,id!=null);
 }
 [HttpPost,Authorize(Policy="Admin"),ActionName("Delete")]
 public async Task<IActionResult> DeletePost(string kind,string id)
 {
  if(!CatalogService.Known(kind))return NotFound();
  try{await using var tx=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);var entity=await Find(kind,id);if(entity==null)return NotFound();
   if(entity is DocumentType t&&await db.RequirementRules.AnyAsync(x=>x.DocumentTypeId==t.DocumentTypeId)){TempData["Error"]="Không thể xóa loại tài liệu đang có quy tắc hồ sơ.";return RedirectToAction(nameof(Index),new{kind});}
   db.Remove(entity);await db.SaveChangesAsync();await tx.CommitAsync();TempData["Success"]="Đã xóa bản ghi.";if(entity is Qualification degree)return RedirectToAction("Details","Profiles",new{id=degree.EmployeeId});if(entity is Document removed){await tx.DisposeAsync();await files.CleanupUnusedAsync(removed.FilePath,db);return RedirectToAction("Details","Profiles",new{id=removed.EmployeeId});}
  }catch(DbUpdateException ex){logger.LogWarning(ex,"Delete {Kind} failed",kind);TempData["Error"]="Không thể xóa: bản ghi còn được tham chiếu, đã thay đổi hoặc bạn chưa có quyền ghi dữ liệu.";}
  catch(SqlException ex) when(ex.Number==1205){TempData["Error"]="Có cập nhật đồng thời. Vui lòng thử lại.";}
  return RedirectToAction(nameof(Index),new{kind});
 }
}



