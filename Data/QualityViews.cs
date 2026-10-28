namespace Vtb.Hr.Web.Data;
public class DocumentQuality
{
 public int DocumentId{get;set;} public string EmployeeId{get;set;}="";public string FullName{get;set;}="";
 public string DocumentCode{get;set;}="";public string DocumentName{get;set;}="";public string DocumentGroup{get;set;}="";
 public DateOnly IssueDate{get;set;} public DateOnly? ExpiryDate{get;set;} public int? ExpiryDays{get;set;}
 public string? StorageForm{get;set;} public string? RequiredStorageForm{get;set;}public string? RequirementLevel{get;set;}public int? WarningThresholdDays{get;set;}
 public bool IsCurrent{get;set;} public string? InvalidReason{get;set;}public string QualityStatus{get;set;}="";public int IsKpiScope{get;set;}
}
public class EmployeeQuality
{
 public string EmployeeId{get;set;}="";public string FullName{get;set;}="";public int DepartmentId{get;set;}public string? PositionName{get;set;}
 public bool IsManagerial{get;set;}public string EmploymentStatus{get;set;}="";public DateOnly JoinDate{get;set;}public DateOnly? LeaveDate{get;set;}
  public int TotalRequiredDocuments{get;set;}public int TotalDocumentsPresent{get;set;}public int TotalDocumentsMissing{get;set;}
 public string MissingDocumentCodes{get;set;}="";public string? LaborContractStatus{get;set;}public string QualityIssue{get;set;}="";public string EmployeeQualityStatus{get;set;}="";
}
public class DocumentRequirement
{
 public string EmployeeId{get;set;}="";public string FullName{get;set;}="";public string? PositionName{get;set;}public int DepartmentId{get;set;}public string DepartmentName{get;set;}="";
 public bool IsManagerial{get;set;}public string EmploymentStatus{get;set;}="";public int RequirementId{get;set;}public int DocumentTypeId{get;set;}
 public string DocumentCode{get;set;}="";public string DocumentName{get;set;}="";public string DocumentGroup{get;set;}="";public string RequirementLevel{get;set;}="";
 public string? RequiredStorageForm{get;set;}public int? WarningThresholdDays{get;set;}public string? RequirementReason{get;set;}public int? DocumentId{get;set;}
 public DateOnly? IssueDate{get;set;}public DateOnly? ExpiryDate{get;set;}public string? StorageForm{get;set;}public string? FilePath{get;set;}
 public string RequirementStatus{get;set;}="";
}
public class LaborContractCheck
{
 public string EmployeeId{get;set;}="";public string FullName{get;set;}="";public int DepartmentId{get;set;}public string DepartmentName{get;set;}="";public string EmploymentStatus{get;set;}="";
 public int? ValidHdthuviec{get;set;}public int? HdthuviecExpired{get;set;}public int? ExistHdthuviec{get;set;}
 public int? ValidHdxdth1{get;set;}public int? Hdxdth1Expired{get;set;}public int? ExistHdxdth1{get;set;}
 public int? ValidHdxdth2{get;set;}public int? Hdxdth2Expired{get;set;}public int? ExistHdxdth2{get;set;}
 public int? ExistHdkxdth{get;set;}public string LaborContractStatus{get;set;}="";
}