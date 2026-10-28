using Microsoft.AspNetCore.Mvc.Rendering;
namespace Vtb.Hr.Web.Models;
public static class DocumentStorageForms
{
 public const string EmptyLabel="--";
 public const string EmptyDescription="Không có hình thức lưu trữ";
 public static IReadOnlyList<string> Values {get;}=Array.AsReadOnly(new[]{"Bản gốc","Bản công chứng","Bản photo","Bản copy"});
 public static string? Normalize(string? value)=>string.IsNullOrWhiteSpace(value)||string.Equals(value.Trim(),"NULL",StringComparison.OrdinalIgnoreCase)?null:value;
 public static bool IsAllowed(string? value)=>value is null||Values.Contains(value,StringComparer.Ordinal);
 public static string InvalidMessage(string? value)=>"Hình thức lưu trữ cần kiểm tra. Chọn --, Bản gốc, Bản công chứng, Bản photo hoặc Bản copy.";
 public static string Display(string? value){var normalized=Normalize(value);return normalized is null?EmptyLabel:IsAllowed(normalized)?normalized:normalized+" (cần kiểm tra)";}
 public static List<SelectListItem> Options()=>new[]{new SelectListItem(EmptyLabel,"")}.Concat(Values.Select(value=>new SelectListItem(value,value))).ToList();
}
