using System.ComponentModel.DataAnnotations;
namespace Vtb.Hr.Web.Models;
public class ReportFilter:IValidatableObject {
 public int[] DivisionIds{get;set;}=[];public int[] DepartmentIds{get;set;}=[];public string[] EmployeeIds{get;set;}=[];public int[] TypeIds{get;set;}=[];public string[] Groups{get;set;}=[];
 [StringLength(100)]public string? Search{get;set;} [StringLength(100)]public string? OptionSearch{get;set;}
 [Range(1,1000000)]public int Page{get;set;}=1;[Range(10,100)]public int PageSize{get;set;}=20;
 public string Kind{get;set;}="problem";public string Sort{get;set;}="";public bool Desc{get;set;}public string? State{get;set;}
 public static string[] Columns(string kind)=>kind switch{
 "problem"=>["Id","Name","Document","Group","IssueDate","ExpiryDate","Days","Status","Reason"],
 "expiring"=>["Id","Name","Code","Document","ExpiryDate","Days"],
 "expiredContracts"=>["Id","Name","Code","Document","IssueDate","ExpiryDate","Days","Status"],
 "contractIssues" or "contractDetail"=>["Id","Name","Department","Employment","Status"],_=>[]};
 public OverviewFilter Scope()=>new(){DivisionIds=DivisionIds,DepartmentIds=DepartmentIds};
 public bool HasScope=>DivisionIds.Length+DepartmentIds.Length+EmployeeIds.Length+TypeIds.Length+Groups.Length>0;
 public IEnumerable<ValidationResult> Validate(ValidationContext c){
  foreach(var e in Scope().Validate(c))yield return e;
  if(EmployeeIds.Length>200||EmployeeIds.Any(x=>x.Length!=8)||TypeIds.Length>500||TypeIds.Any(x=>x<1)||Groups.Any(x=>x!="Hồ sơ gốc"&&x!="Quyết định / Văn bản khác"))yield return new("Bộ lọc không hợp lệ.");
  if(!new[]{"problem","expiring","contractIssues","expiredContracts","contractDetail"}.Contains(Kind))yield return new("Bảng không hợp lệ.");
  if(Sort!=""&&!Columns(Kind).Contains(Sort))yield return new("Sắp xếp không hợp lệ.");
  if(State!=null&&!new[]{"HOP_LE","THIEU_HDXDTH1","CAN_KY_KET_TIEP","KHONG_CANH_BAO","BLANK"}.Contains(State))yield return new("Trạng thái hợp đồng không hợp lệ.");
  if(Kind=="contractDetail"&&State==null)yield return new("Chọn trạng thái hợp đồng.");
 }
}
public class ReportRow {
 public string Id{get;set;}="";public string Name{get;set;}="";public string? Code{get;set;}public string? Document{get;set;}public string? Group{get;set;}public DateOnly? IssueDate{get;set;}public DateOnly? ExpiryDate{get;set;}public int? Days{get;set;}public string? Status{get;set;}public string? Reason{get;set;}public string? Department{get;set;}public string? Employment{get;set;}
}
public record ReportTable(List<ReportRow> Rows,int Total,int Page,int PageSize);
public record ReportPoint(string Name,double? Value,string? Code=null);
public record ReportData(Dictionary<string,double?> Kpis,List<ReportPoint> First,List<ReportPoint> Second,List<ReportOrganizationRow>? Organization=null,List<ContractOrganizationRow>? ContractOrganization=null);
public record ReportOption(string Id,string Name);

public class ReportOrganizationRow { public int DivisionId{get;set;} public string DivisionName{get;set;}=""; public int DepartmentId{get;set;} public string DepartmentName{get;set;}=""; public long Total{get;set;} public long Missing{get;set;} }

public class ContractOrganizationRow { public int DivisionId{get;set;} public string DivisionName{get;set;}=""; public int DepartmentId{get;set;} public string DepartmentName{get;set;}=""; public int Count{get;set;} }
