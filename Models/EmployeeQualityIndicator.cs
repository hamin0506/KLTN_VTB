namespace Vtb.Hr.Web.Models;
public record EmployeeQualityIndicator(string State,string Tooltip)
{
 public static EmployeeQualityIndicator Unknown=>new("unknown","Chưa có kết quả đánh giá");
}
