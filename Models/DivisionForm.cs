using System.ComponentModel.DataAnnotations;
namespace Vtb.Hr.Web.Models;
public class DivisionForm {
 public int DivisionId {get;set;}
 [Display(Name="Mã đơn vị"), Required(ErrorMessage="Vui lòng nhập mã đơn vị."), StringLength(20, ErrorMessage="Mã tối đa 20 ký tự."), RegularExpression(@"[\x21-\x7E]+", ErrorMessage="Mã dùng ký tự ASCII, không có khoảng trắng.")]
 public string DivisionCode {get;set;} = "";
 [Display(Name="Tên đơn vị"), Required(ErrorMessage="Vui lòng nhập tên đơn vị."), StringLength(200,ErrorMessage="Tên tối đa 200 ký tự.")]
 public string DivisionName {get;set;} = "";
}
public class DivisionList { public List<Data.Division> Items {get;set;}=[]; public string? Q {get;set;} public int Page {get;set;} public int Total {get;set;} public int Pages => Math.Max(1,(int)Math.Ceiling(Total/10d)); }