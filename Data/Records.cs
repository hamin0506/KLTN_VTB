using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Vtb.Hr.Web.Data;
[Table("employee",Schema="dbo")]
public class Employee : IValidatableObject
{
 [Key,Column("employee_id",TypeName="char(8)"),Display(Name="Mã cán bộ"),Required(ErrorMessage="Nhập mã cán bộ."),RegularExpression(@"[\x21-\x7E]{8}",ErrorMessage="Mã cán bộ gồm đúng 8 ký tự ASCII, không có khoảng trắng.")]
 public string EmployeeId {get;set;}="";
 [Required(ErrorMessage="Nhập họ tên."),StringLength(50,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Họ và tên")]public string FullName {get;set;}="";
 [DataType(DataType.Date),Display(Name="Ngày sinh")]public DateOnly? DateOfBirth {get;set;}
 [Required(ErrorMessage="Vui lòng nhập hoặc chọn giá trị."),RegularExpression("Nam|Nu",ErrorMessage="Chọn Nam hoặc Nữ."),Column(TypeName="varchar(10)"),Display(Name="Giới tính")]public string Gender {get;set;}="Nam";
 [Range(1,int.MaxValue,ErrorMessage="Chọn phòng ban."),Display(Name="Phòng ban")]public int DepartmentId {get;set;}
 [StringLength(200,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Chức danh")]public string? PositionName {get;set;}
 [Display(Name="Cán bộ quản lý")]public bool IsManagerial {get;set;}
 [Required(ErrorMessage="Vui lòng nhập hoặc chọn giá trị."),RegularExpression("DangLamViec|TamHoanHDLD|ChamDutHDLD|NghiHuu",ErrorMessage="Chọn trạng thái làm việc hợp lệ."),Column(TypeName="varchar(30)"),Display(Name="Trạng thái làm việc")]public string EmploymentStatus {get;set;}="DangLamViec";
 [DataType(DataType.Date),Display(Name="Ngày vào làm")]public DateOnly JoinDate {get;set;}=DateOnly.FromDateTime(DateTime.Today);
 [DataType(DataType.Date),Display(Name="Ngày nghỉ")]public DateOnly? LeaveDate {get;set;}
 [StringLength(10,ErrorMessage="Số điện thoại tối đa 10 ký tự."),Column(TypeName="varchar(10)"),Display(Name="Số điện thoại")]public string? PhoneNumber{get;set;}
 [StringLength(254,ErrorMessage="Email tối đa 254 ký tự."),EmailAddress(ErrorMessage="Email không đúng định dạng."),Column(TypeName="varchar(254)"),Display(Name="Email")]public string? Email{get;set;}
 [StringLength(255,ErrorMessage="Quê quán tối đa 255 ký tự."),Display(Name="Quê quán")]public string? Hometown{get;set;}
 [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]public ICollection<Qualification> Qualifications{get;set;}=new List<Qualification>();
 public IEnumerable<ValidationResult> Validate(ValidationContext c){if(LeaveDate<JoinDate)yield return new("Ngày nghỉ không được trước ngày vào làm.",[nameof(LeaveDate)]);}
}
[Table("document",Schema="dbo")]
public class Document : IValidatableObject
{
 [Key]public int DocumentId {get;set;}
 [Required(ErrorMessage="Vui lòng nhập hoặc chọn giá trị."),Column(TypeName="char(8)"),Display(Name="Cán bộ")]public string EmployeeId {get;set;}="";
 [Range(1,int.MaxValue,ErrorMessage="Chọn loại tài liệu."),Display(Name="Loại tài liệu")]public int DocumentTypeId {get;set;}
 [StringLength(300,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Tên cụ thể")]public string? SpecificName {get;set;}
 [DataType(DataType.Date),Display(Name="Ngày cấp")]public DateOnly IssueDate {get;set;}=DateOnly.FromDateTime(DateTime.Today);
 [DataType(DataType.Date),Display(Name="Ngày hết hạn")]public DateOnly? ExpiryDate {get;set;}
 [Microsoft.AspNetCore.Mvc.ModelBinder(BinderType=typeof(Vtb.Hr.Web.Models.StorageFormModelBinder)),StringLength(50,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Hình thức lưu trữ")]public string? StorageForm {get;set;}
 [StringLength(500,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Tệp đính kèm")]public string? FilePath {get;set;}
 [Display(Name="Tài liệu hiện hành")]public bool IsCurrent {get;set;}
 public DateTime CreatedAt {get;set;}
 public DateTime UpdatedAt {get;set;}
 public IEnumerable<ValidationResult> Validate(ValidationContext c){
  if(ExpiryDate<IssueDate)yield return new("Ngày hết hạn không được trước ngày cấp.",[nameof(ExpiryDate)]);
  if(!Vtb.Hr.Web.Models.DocumentStorageForms.IsAllowed(StorageForm))yield return new(Vtb.Hr.Web.Models.DocumentStorageForms.InvalidMessage(StorageForm),[nameof(StorageForm)]);
 }
}
[Table("document_type",Schema="dbo")]
public class DocumentType
{
 [Key]public int DocumentTypeId {get;set;}
 [Required(ErrorMessage="Nhập mã loại tài liệu."),StringLength(50,ErrorMessage="Tối đa {1} ký tự."),Column(TypeName="varchar(50)"),RegularExpression(@"[\x21-\x7E]+",ErrorMessage="Mã dùng ký tự ASCII không khoảng trắng."),Display(Name="Mã loại tài liệu")]public string DocumentCode {get;set;}="";
 [Required(ErrorMessage="Nhập tên loại tài liệu."),StringLength(200,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Tên loại tài liệu")]public string DocumentName {get;set;}="";
 [Required(ErrorMessage="Vui lòng nhập hoặc chọn giá trị."),RegularExpression("Hồ sơ gốc|Hợp đồng lao động|Quyết định / Văn bản khác",ErrorMessage="Chọn nhóm tài liệu hợp lệ."),StringLength(100,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Nhóm tài liệu")]public string DocumentGroup {get;set;}="Hồ sơ gốc";
 [StringLength(200,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Mô tả")]public string? Description {get;set;}
}
[Table("requirement_rule",Schema="dbo")]
public class RequirementRule : IValidatableObject
{
 [Key]public int RequirementId {get;set;}
 [Range(1,int.MaxValue,ErrorMessage="Chọn loại tài liệu."),Display(Name="Loại tài liệu")]public int DocumentTypeId {get;set;}
 [Required(ErrorMessage="Vui lòng nhập hoặc chọn giá trị."),RegularExpression("BAT_BUOC|TUY_CHON|CO_DIEU_KIEN",ErrorMessage="Chọn mức độ yêu cầu hợp lệ."),Column(TypeName="varchar(30)"),Display(Name="Mức độ yêu cầu")]public string RequirementLevel {get;set;}="BAT_BUOC";
 [StringLength(50,ErrorMessage="Tối đa {1} ký tự."),Display(Name="Hình thức lưu trữ yêu cầu")]public string? RequiredStorageForm {get;set;}
 [Range(0,int.MaxValue,ErrorMessage="Ngưỡng cảnh báo phải từ 0 trở lên."),Display(Name="Ngưỡng cảnh báo (ngày)")]public int? WarningThresholdDays {get;set;}
 [DataType(DataType.Date),Display(Name="Hiệu lực từ")]public DateOnly EffectiveFrom {get;set;}=DateOnly.FromDateTime(DateTime.Today);
 [DataType(DataType.Date),Display(Name="Hiệu lực đến")]public DateOnly? EffectiveTo {get;set;}
 [Display(Name="Đang áp dụng")]public bool IsActive {get;set;}=true;
 public IEnumerable<ValidationResult> Validate(ValidationContext c){if(EffectiveTo<EffectiveFrom)yield return new("Ngày kết thúc không được trước ngày bắt đầu.",[nameof(EffectiveTo)]);}
}
