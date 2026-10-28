using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Vtb.Hr.Web.Models;
public class DepartmentForm
{
    public int DepartmentId { get; set; }
    [Display(Name = "Đơn vị trực thuộc"), Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn đơn vị trực thuộc.")]
    public int DivisionId { get; set; }
    [Display(Name = "Mã phòng ban"), Required(ErrorMessage = "Vui lòng nhập mã phòng ban."), StringLength(20, ErrorMessage = "Mã tối đa 20 ký tự."), RegularExpression(@"[\x21-\x7E]+", ErrorMessage = "Mã dùng ký tự ASCII, không có khoảng trắng.")]
    public string DepartmentCode { get; set; } = "";
    [Display(Name = "Tên phòng ban"), Required(ErrorMessage = "Vui lòng nhập tên phòng ban."), StringLength(200, ErrorMessage = "Tên tối đa 200 ký tự.")]
    public string DepartmentName { get; set; } = "";
    [ValidateNever] public List<SelectListItem> Divisions { get; set; } = [];
}
public class DepartmentList
{
    public int? DivisionId { get; set; }
    public List<SelectListItem> Divisions { get; set; } = [];
    public List<Data.Department> Items { get; set; } = [];
    public string? Q { get; set; }
    public int Page { get; set; }
    public int Total { get; set; }
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / 10d));
}