using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;

// Display-only classification; never changes SQL evaluation results or dashboard measures.
public static class EmployeeQualityIndicators
{
 // Reuse the exact dot evaluator for the complete filtered scope. Only IDs are loaded up front;
 // quality data is bounded to 256 employees per batch (six queries per batch, never per row).
 public static async Task<Dictionary<string,EmployeeQualityIndicator>> Matching(VtbContext db,IQueryable<Employee> scope,string state,CancellationToken ct=default)
 {
  if(state is not ("success" or "error"))throw new ArgumentException("Bộ lọc chất lượng không hợp lệ",nameof(state));
  var ids=await scope.Select(x=>x.EmployeeId).Distinct().ToListAsync(ct);
  var result=new Dictionary<string,EmployeeQualityIndicator>();
  foreach(var batch in ids.Chunk(256)){
   var evaluated=await Load(db,batch,ct);
   foreach(var pair in evaluated)if(pair.Value.State==state)result.Add(pair.Key,pair.Value);
  }
  return result;
 }
 public static async Task<Dictionary<string,EmployeeQualityIndicator>> Load(VtbContext db,IEnumerable<string> employeeIds,CancellationToken ct=default)
 {
  var ids=employeeIds.Distinct().ToArray();if(ids.Length==0)return [];
  var docs=await db.Documents.AsNoTracking().Where(d=>ids.Contains(d.EmployeeId)&&d.IsCurrent)
   .Select(d=>new Document{DocumentId=d.DocumentId,EmployeeId=d.EmployeeId,DocumentTypeId=d.DocumentTypeId,IsCurrent=d.IsCurrent}).ToListAsync(ct);
  var typeIds=docs.Select(d=>d.DocumentTypeId).Distinct().ToArray();
  var types=await db.DocumentTypes.AsNoTracking().Where(t=>typeIds.Contains(t.DocumentTypeId)).ToDictionaryAsync(t=>t.DocumentTypeId,ct);
  var quality=await db.DocumentQualities.AsNoTracking().Where(q=>ids.Contains(q.EmployeeId)).Select(q=>new DocumentQuality{EmployeeId=q.EmployeeId,DocumentId=q.DocumentId,QualityStatus=q.QualityStatus}).ToListAsync(ct);
  var employees=await db.EmployeeQualities.AsNoTracking().Where(e=>ids.Contains(e.EmployeeId)).Select(e=>new EmployeeQuality{EmployeeId=e.EmployeeId,EmploymentStatus=e.EmploymentStatus,EmployeeQualityStatus=e.EmployeeQualityStatus,TotalRequiredDocuments=e.TotalRequiredDocuments,TotalDocumentsMissing=e.TotalDocumentsMissing}).ToListAsync(ct);
  var contracts=await db.LaborContractChecks.AsNoTracking().Where(c=>ids.Contains(c.EmployeeId)).ToListAsync(ct);
  var missing=await db.DocumentRequirements.AsNoTracking().Where(r=>ids.Contains(r.EmployeeId)&&r.RequirementStatus=="THIEU_HO_SO").Select(r=>new{r.EmployeeId,r.DocumentTypeId}).Distinct().ToListAsync(ct);
  var byDocument=docs.ToLookup(d=>d.EmployeeId);var byQuality=quality.ToLookup(q=>q.EmployeeId);var byEmployee=employees.ToLookup(e=>e.EmployeeId);var byContract=contracts.ToLookup(c=>c.EmployeeId);var byMissing=missing.ToLookup(r=>r.EmployeeId);
  return ids.ToDictionary(id=>id,id=>{
   var eq=byEmployee[id].ToList();var cs=byContract[id].ToList();
   var model=new ProfileModel{Employee=new Employee{EmployeeId=id},Documents=byDocument[id].ToList(),DocumentTypes=types,Quality=eq.Count==1?eq[0]:null,Contract=cs.Count==1?cs[0]:null,DocumentQualityRows=byQuality[id].ToList(),Warnings=byQuality[id].Where(q=>q.QualityStatus!="HOP_LE").ToList()};
   return Evaluate(model,byMissing[id].Count());
  });
 }
 public static EmployeeQualityIndicator Evaluate(ProfileModel model,int missingTypes)
 {
  var current=model.Documents.Where(d=>d.EmployeeId==model.Employee.EmployeeId&&d.IsCurrent).ToList();var ids=current.Select(d=>d.DocumentId).ToHashSet();
  // Reuse the existing successor-contract exclusion, including per-type current contracts.
  var warnings=ProfilePresentation.ProfileWarnings(model).Where(q=>ids.Contains(q.DocumentId)).ToList();
  int Count(string status)=>warnings.Where(q=>q.QualityStatus==status).Select(q=>q.DocumentId).Distinct().Count();
  var invalid=Count("KHONG_HOP_LE");var expired=Count("DA_HET_HAN");
  var reasons=new List<string>();if(invalid>0)reasons.Add($"Có {invalid} tài liệu hiện hành không hợp lệ");if(expired>0)reasons.Add($"Có {expired} tài liệu hiện hành đã hết hạn cần xử lý");
  if(missingTypes>0)reasons.Add($"Thiếu {missingTypes} loại tài liệu bắt buộc");else if(model.Quality?.TotalDocumentsMissing>0)reasons.Add($"Thiếu {model.Quality.TotalDocumentsMissing} loại tài liệu bắt buộc");
  // Same employee scope as vw_employee_quality; no invented severity for unknown states.
  var active=model.Quality?.EmploymentStatus is "DangLamViec" or "TamHoanHDLD";
  if(active&&model.Contract?.LaborContractStatus=="THIEU_HDXDTH1")reasons.Add("Thiếu HĐXĐTH1");
  if(active&&model.Contract?.LaborContractStatus=="CAN_KY_KET_TIEP")reasons.Add("Cần ký tiếp HĐLĐ");
  if(reasons.Count==0&&model.Quality?.EmployeeQualityStatus=="THIEU_HO_SO")reasons.Add("Hồ sơ cần bổ sung hoặc xử lý theo kết quả đánh giá");
  if(reasons.Count>0)return new("error",string.Join("; ",reasons.Distinct()));
  var known=new[]{"HOP_LE","SAP_HET_HAN","DA_HET_HAN","KHONG_HOP_LE"};
  var covered=current.Count>0&&current.All(d=>model.DocumentTypes.ContainsKey(d.DocumentTypeId)&&model.DocumentQualityRows.Any(q=>q.DocumentId==d.DocumentId))&&model.DocumentQualityRows.Where(q=>ids.Contains(q.DocumentId)).All(q=>known.Contains(q.QualityStatus));
  // KHONG_CANH_BAO alone is not proof of a valid contract (existing profile rule).
  if(covered&&model.Quality is {EmployeeQualityStatus:"DAY_DU",TotalDocumentsMissing:0,TotalRequiredDocuments:>0}&&ProfilePresentation.HasValidCurrentContract(model))return new("success","Hồ sơ đạt yêu cầu");
  return EmployeeQualityIndicator.Unknown;
 }
}
