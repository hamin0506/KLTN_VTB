using Microsoft.AspNetCore.Mvc.Rendering;
using Vtb.Hr.Web.Data;
namespace Vtb.Hr.Web.Models;
public class Filter
{
 public string? Q{get;set;} public int? DivisionId{get;set;}public int? DepartmentId{get;set;}public string? EmployeeId{get;set;}public int? DocumentTypeId{get;set;}
 public string? Status{get;set;} public string? Group{get;set;}public string? Current{get;set;}public string? Term{get;set;} public bool KpiOnly{get;set;}public int Page{get;set;}=1;
 public Dictionary<string,string> Routes(string? kind=null)=>new(){["q"]=Q??"",["divisionId"]=DivisionId?.ToString()??"",["departmentId"]=DepartmentId?.ToString()??"",["employeeId"]=EmployeeId??"",["documentTypeId"]=DocumentTypeId?.ToString()??"",["status"]=Status??"",["group"]=Group??"",["current"]=Current??"",["term"]=Term??"",["kpiOnly"]=KpiOnly.ToString(),["kind"]=kind??""};
}
public record Field(string Name,string Label,string Type="text",bool Required=false,int? Max=null);
public record RowLink(string Label,string Controller,string Action,string? Id=null,string? Kind=null,string? EmployeeId=null,int? DocumentTypeId=null);
public class GridRow {public List<string?> Cells{get;set;}=[];public List<RowLink> Links{get;set;}=[];}
public class GridModel
{
 public string Title{get;set;}=""; public string? Note{get;set;}public string? Kind{get;set;}
 public string[] Headers{get;set;}=[];public List<GridRow> Rows{get;set;}=[];public Filter Filter{get;set;}=new();public int Total{get;set;}public int Pages=>Math.Max(1,(int)Math.Ceiling(Total/20d));
 public List<string> Filters{get;set;}=[];public Dictionary<string,List<SelectListItem>> Options{get;set;}=[];public RowLink? Create{get;set;}
}
public class EditRecord
{
 public string? OriginalStorageForm{get;set;}
 public string Kind{get;set;}="";public string Title{get;set;}="";public string? Id{get;set;}public bool Editing{get;set;}public bool ReadOnly{get;set;}public bool Deleting{get;set;}

 public object Entity{get;set;}=null!;public List<Field> Fields{get;set;}=[];public Dictionary<string,List<SelectListItem>> Options{get;set;}=[];
 public string? Value(string name)=>Entity.GetType().GetProperty(name)?.GetValue(Entity) switch{DateOnly d=>d.ToString("yyyy-MM-dd"),bool b=>b?"true":"false",var v=>v?.ToString()};
}
public class DashboardModel
{
 public Filter Filter{get;set;}=new();public Dictionary<string,List<SelectListItem>> Options{get;set;}=[];
 public int Employees{get;set;}public int Documents{get;set;}public int Complete{get;set;}public int Missing{get;set;}public int Expired{get;set;}public int Expiring{get;set;}public int Invalid{get;set;}public int Valid{get;set;}public int QualityRows{get;set;}public int QualityDocuments{get;set;}public int Conflicts{get;set;}public List<EmployeeQuality> Alerts{get;set;}=[];
}
public class ProfileModel
{
 public List<int> OrderedDocumentTypeIds{get;set;}=[];
 public List<Qualification> Qualifications{get;set;}=[];

 public Employee Employee{get;set;}=null!;public Department Department{get;set;}=null!;public EmployeeQuality? Quality{get;set;}
 public List<Document> Documents{get;set;}=[];public List<DocumentRequirement> Missing{get;set;}=[];public List<DocumentQuality> Warnings{get;set;}=[];public LaborContractCheck? Contract{get;set;}
 public Dictionary<int,string> TypeNames{get;set;}=[];
 public Dictionary<int,DocumentType> DocumentTypes{get;set;}=[];
 public List<DocumentQuality> DocumentQualityRows{get;set;}=[];
}
public record ProfileDocumentSection(ProfileModel Profile, bool IsCurrent);
