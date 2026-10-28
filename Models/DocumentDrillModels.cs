using System.ComponentModel.DataAnnotations;
namespace Vtb.Hr.Web.Models;
public class DocumentDrillFilter : IValidatableObject
{
 [Required]public string State {get;set;}="";
 public int[] DivisionIds {get;set;}=[];public int[] DepartmentIds {get;set;}=[];public string[] Statuses {get;set;}=[];
 [StringLength(100)]public string? Search {get;set;}
 [Range(1,1000000)]public int Page {get;set;}=1;
 [Range(10,100)]public int PageSize {get;set;}=20;
 public string Sort {get;set;}="id";public bool Desc {get;set;}
 // Return context is distinct from the detail table's own search/sort/page.
 [StringLength(100)]public string? OverviewSearch {get;set;}
 [Range(1,1000000)]public int OverviewPage {get;set;}=1;
 [Range(10,100)]public int OverviewPageSize {get;set;}=20;
 public string OverviewSort {get;set;}="missing";public bool OverviewDesc {get;set;}=true;
 public OverviewFilter Scope()=>new(){DivisionIds=DivisionIds,DepartmentIds=DepartmentIds,Statuses=Statuses};
 public IEnumerable<ValidationResult> Validate(ValidationContext context){
  foreach(var error in Scope().Validate(context))yield return error;
  if(!new[]{"HOP_LE","SAP_HET_HAN","DA_HET_HAN","KHONG_HOP_LE","BLANK"}.Contains(State))yield return new("Trạng thái chất lượng tài liệu không hợp lệ.");
  if(!new[]{"id","name","department","code","document","issueDate","expiryDate","days","requiredStorage","storage","reason"}.Contains(Sort))yield return new("Cột sắp xếp không hợp lệ.");
  if(!OverviewFilter.AllowedSorts.Contains(OverviewSort))yield return new("Ngữ cảnh tổng quan không hợp lệ.");
 }
}
public class DocumentDrillRow {
 public string StorageDisplay=>DocumentStorageForms.Display(Storage);
 public string EmployeeId {get;set;}="";public string FullName {get;set;}="";public string Department {get;set;}="";
 public string Code {get;set;}="";public string Document {get;set;}="";public DateOnly IssueDate {get;set;}public DateOnly? ExpiryDate {get;set;}public int? Days {get;set;}
 public string? RequiredStorage {get;set;}public string? Storage {get;set;}public string? Reason {get;set;}
}
public record DocumentDrillPage(List<DocumentDrillRow> Rows,int Total,int Page,int PageSize);
