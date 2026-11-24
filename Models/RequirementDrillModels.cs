using System.ComponentModel.DataAnnotations;
namespace Vtb.Hr.Web.Models;
public class RequirementDrillFilter : ReportFilter {
 [Range(1,int.MaxValue)] public int? DrillDivisionId{get;set;}
 [Range(1,int.MaxValue)] public int? DrillDepartmentId{get;set;}
 [Range(1,int.MaxValue)] public int? DrillTypeId{get;set;}
 [RegularExpression("^(types|divisions|departments)$")] public string Level{get;set;}="divisions";
 [RegularExpression("^(id|name|department|missing)$")] public string DetailSort{get;set;}="missing";
}
public class RequirementPair {public string EmployeeId{get;set;}="";public int DocumentTypeId{get;set;}public int Missing{get;set;}}
public class RequirementDrillRow {
 public string EmployeeId{get;set;}="";public string FullName{get;set;}="";public int DivisionId{get;set;}public string DivisionName{get;set;}="";public int DepartmentId{get;set;}public string DepartmentName{get;set;}="";public int DocumentTypeId{get;set;}public string DocumentName{get;set;}="";public int Missing{get;set;}
}
public class RequirementChartPoint {
 public int Id{get;set;}public string Name{get;set;}="";public long Total{get;set;}public long Missing{get;set;}public int Employees{get;set;}
 public double? Rate=>Total==0?null:(double)(Total-Missing)/Total;
}
public class RequirementPerson {
 public string EmployeeId{get;set;}="";public string FullName{get;set;}="";public string Department{get;set;}="";public int Missing{get;set;}public List<string> Documents{get;set;}=[];
}
public record RequirementPeoplePage(List<RequirementPerson> Rows,int Total,long Missing,int Page,int PageSize);
