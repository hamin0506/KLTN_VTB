using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Vtb.Hr.Web.Data;
// Based on the explicit dbo.qualification DDL in D:/KLTN/VTB2.sql; live schema pending verification.
[Table("qualification",Schema="dbo")]
public class Qualification
{
 [Key]public int QualificationId{get;set;}
 [Required(ErrorMessage="Chọn cán bộ."),StringLength(8,ErrorMessage="Mã cán bộ tối đa 8 ký tự."),Display(Name="Cán bộ"),Column(TypeName="char(8)")]public string EmployeeId{get;set;}="";
 [Required(ErrorMessage="Nhập trình độ."),StringLength(50,ErrorMessage="Trình độ tối đa 50 ký tự.")][Display(Name="Trình độ")]public string QualificationLevel{get;set;}="";
 [StringLength(200,ErrorMessage="Tên văn bằng tối đa 200 ký tự.")][Display(Name="Tên văn bằng")]public string? DegreeName{get;set;}
 [StringLength(200,ErrorMessage="Chuyên ngành tối đa 200 ký tự.")][Display(Name="Chuyên ngành")]public string? Major{get;set;}
 [StringLength(255,ErrorMessage="Cơ sở đào tạo tối đa 255 ký tự.")][Display(Name="Cơ sở đào tạo")]public string? Institution{get;set;}
 [DataType(DataType.Date),Column(TypeName="date")][Display(Name="Ngày tốt nghiệp")]public DateOnly? GraduationDate{get;set;}
 [StringLength(500,ErrorMessage="Ghi chú tối đa 500 ký tự.")][Display(Name="Ghi chú")]public string? Notes{get;set;}
 [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]public Employee? Employee{get;set;}
}
