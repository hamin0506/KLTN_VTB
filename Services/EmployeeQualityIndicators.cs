using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;

// SQL owns the employee assessment. Document statuses are not recomputed here.
public static class EmployeeQualityIndicators
{
 public static async Task<Dictionary<string,EmployeeQualityIndicator>> Matching(VtbContext db,IQueryable<Employee> scope,string state,CancellationToken ct=default)
 {
  var status=state switch {"success" or "DAY_DU"=>"DAY_DU","error" or "THIEU_HO_SO"=>"THIEU_HO_SO",_=>throw new ArgumentException("Chọn trạng thái chất lượng hợp lệ.",nameof(state))};
  var ids=scope.Select(e=>e.EmployeeId);
  var rows=await db.EmployeeQualities.AsNoTracking().Where(q=>ids.Contains(q.EmployeeId)&&q.EmployeeQualityStatus==status).ToListAsync(ct);
  return rows.ToDictionary(q=>q.EmployeeId,FromQuality);
 }
 public static async Task<Dictionary<string,EmployeeQualityIndicator>> Load(VtbContext db,IEnumerable<string> employeeIds,CancellationToken ct=default)
 {
  var ids=employeeIds.Distinct().ToArray();if(ids.Length==0)return [];
  var rows=await db.EmployeeQualities.AsNoTracking().Where(q=>ids.Contains(q.EmployeeId)).ToListAsync(ct);
  return rows.ToDictionary(q=>q.EmployeeId,FromQuality);
 }
 public static EmployeeQualityIndicator FromQuality(EmployeeQuality? q)
 {
  if(q==null||q.EmployeeQualityStatus is not ("DAY_DU" or "THIEU_HO_SO"))return EmployeeQualityIndicator.Unknown;
  if(q.EmploymentStatus=="ChamDutHDLD")return new("unknown",string.IsNullOrWhiteSpace(q.QualityIssue)?"Không áp dụng đánh giá hồ sơ bắt buộc":q.QualityIssue){Label="Không áp dụng",NotApplicable=true};
  var label=Labels.Text(q.EmployeeQualityStatus);
  var issue=q.QualityIssue;
  // A non-applicable assessment must not be presented as proof of compliant documents.
  if(q.EmploymentStatus=="ChamDutHDLD"&&string.IsNullOrWhiteSpace(issue))issue="Không áp dụng đánh giá hồ sơ bắt buộc";
  return new(q.EmployeeQualityStatus=="DAY_DU"?"success":"error",string.IsNullOrWhiteSpace(issue)?label:label+" — "+issue)
  {Label=label,NotApplicable=q.EmploymentStatus=="ChamDutHDLD"};
 }
 public static EmployeeQualityIndicator Evaluate(ProfileModel model,int missingTypes)=>FromQuality(model.Quality);
}
