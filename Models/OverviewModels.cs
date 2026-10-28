using System.ComponentModel.DataAnnotations;
namespace Vtb.Hr.Web.Models;
public class OverviewFilter : IValidatableObject
{
 public int[] DivisionIds {get;set;}=[];
 public int[] DepartmentIds {get;set;}=[];
 public string[] Statuses {get;set;}=[];
 [StringLength(100)] public string? Search {get;set;}
 [Range(1,1000000)] public int Page {get;set;}=1;
 [Range(10,100)] public int PageSize {get;set;}=20;
 public string Sort {get;set;}="missing";
 public bool Desc {get;set;}=true;
 public static readonly string[] AllowedStatuses=["DangLamViec","TamHoanHDLD"];
 public static readonly string[] AllowedSorts=["id","name","department","required","present","missing","issue","codes"];
 public IEnumerable<ValidationResult> Validate(ValidationContext context){
  if(DivisionIds.Length>200||DepartmentIds.Length>500||DivisionIds.Any(x=>x<=0)||DepartmentIds.Any(x=>x<=0))yield return new("Bộ lọc tổ chức không hợp lệ.");
  if(Statuses.Length>2||Statuses.Any(x=>!AllowedStatuses.Contains(x)))yield return new("Trạng thái không thuộc bộ lọc Tổng quan.");
  if(!AllowedSorts.Contains(Sort))yield return new("Cột sắp xếp không hợp lệ.");
 }
}
public record OverviewKpis(long? TotalEmployees,long? CompleteEmployees,double? CompletenessRate,long? MissingEmployees,long? ExpiringDocuments,long? ExpiredDocuments,long? InvalidDocuments,long? TotalDocuments);
public record ChartPoint(string Category,string? Series,long Value,string? Parent=null,string? Code=null,int? DivisionId=null,int? DepartmentId=null,string? Label=null);
public class MissingEmployeeRow {
 public string EmployeeId {get;set;}=""; public string FullName {get;set;}="";public string DepartmentName {get;set;}="";
 public int Required {get;set;}public int Present {get;set;}public int Missing {get;set;}public string? Issue {get;set;}public string? Codes {get;set;}
}
public record MissingEmployeePage(List<MissingEmployeeRow> Rows,int Total,int Page,int PageSize);
public record OrganizationOption(int Id,string Name,int? DivisionId=null);
public record OverviewOptions(List<OrganizationOption> Divisions,List<OrganizationOption> Departments);
public record OverviewResult(OverviewKpis Kpis,List<ChartPoint> EmployeesByDepartment,List<ChartPoint> DocumentDistribution,List<ChartPoint> DocumentsByType,DateTimeOffset ReadAt);

