using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;
public record ProfileDocumentGroup(string? Source,string Title,bool Unexpected,List<Document> Documents);
public static class ProfilePresentation
{
 // Presentation-only filtering: no SQL quality row or dashboard measure is changed.
 // This is the same employee-level result displayed on the profile, not a second validity rule.
 public static bool HasValidCurrentContract(ProfileModel model)=>
  model.Contract?.EmployeeId==model.Employee.EmployeeId&&model.Contract.LaborContractStatus=="HOP_LE";
 private static bool IsSupersededContract(ProfileModel model,Document document)
 {
  if(document.EmployeeId!=model.Employee.EmployeeId||!model.DocumentTypes.TryGetValue(document.DocumentTypeId,out var type)||type.DocumentGroup!="Hợp đồng lao động")return false;
  var c=model.Contract!;
  // is_current is scoped to employee + document type, not the entire contract career.
  // Follow the successor codes and evidence exposed by vw_labor_contract_check.
  var laterStage=type.DocumentCode switch{
   "HDTHUVIEC"=>c.ExistHdxdth1==1&&(c.ValidHdxdth1==1||c.ValidHdxdth2==1||c.ExistHdkxdth==1),
   "HDXDTH1"=>c.ValidHdxdth2==1||c.ExistHdkxdth==1,
   "HDXDTH2"=>c.ExistHdkxdth==1,
   _=>false};
  if(laterStage)return true;
  // An older revision of the SAME type also needs a current replacement and a valid flag from the View.
  var validSameType=type.DocumentCode switch{"HDTHUVIEC"=>c.ValidHdthuviec==1,"HDXDTH1"=>c.ValidHdxdth1==1,"HDXDTH2"=>c.ValidHdxdth2==1,_=>false};
  return !document.IsCurrent&&validSameType&&model.Documents.Any(d=>d.DocumentId!=document.DocumentId&&d.EmployeeId==document.EmployeeId&&d.DocumentTypeId==document.DocumentTypeId&&d.IsCurrent);
 }
 public static IEnumerable<DocumentQuality> ProfileWarnings(ProfileModel model)
 {
  var valid=HasValidCurrentContract(model);
  return Warnings(model.Warnings.Where(q=>!(valid&&q.QualityStatus=="DA_HET_HAN"&&
   model.Documents.Any(d=>d.DocumentId==q.DocumentId&&IsSupersededContract(model,d)))));
 }
 public static string EmploymentColor(string? status)=>status switch{"DangLamViec"=>"green","TamHoanHDLD"=>"yellow",_=>"secondary"};
 public static string EmploymentLabel(string? status)=>status switch{"ChamDutHDLD"=>"Đã nghỉ việc","DangLamViec"=>"Đang làm việc","TamHoanHDLD"=>"Tạm hoãn HĐLĐ","NghiHuu"=>"Nghỉ hưu",_=>"Trạng thái khác"};
 public static string Text(string? value)=>string.IsNullOrWhiteSpace(value)?"--":value;
 public static int CurrentCount(ProfileModel model,string state){var ids=model.Documents.Where(d=>d.IsCurrent).Select(d=>d.DocumentId).ToHashSet();return model.DocumentQualityRows.Where(q=>ids.Contains(q.DocumentId)&&q.QualityStatus==state).Select(q=>q.DocumentId).Distinct().Count();}

 public static string StateClass(string? state)=>state switch {"HOP_LE" or "DAY_DU"=>"valid","SAP_HET_HAN"=>"expiring","DA_HET_HAN"=>"expired","KHONG_HOP_LE"=>"invalid","THIEU_HO_SO"=>"missing",_=>"unknown"};
 public static string StateLabel(string? state)=>string.IsNullOrWhiteSpace(state)||state=="KHONG_XAC_DINH"?"Chưa xác định":Labels.Text(state);
 // Display deduplication only: never reduce SQL measure rows or drop distinct reasons.
 public static IEnumerable<DocumentQuality> Warnings(IEnumerable<DocumentQuality> rows)=>rows.DistinctBy(x=>new{x.DocumentId,x.QualityStatus,Reason=string.IsNullOrWhiteSpace(x.InvalidReason)?null:x.InvalidReason.Trim(),x.ExpiryDate,x.ExpiryDays});
 public static List<ProfileDocumentGroup> Groups(ProfileModel model){
  string? Key(string? value)=>string.IsNullOrWhiteSpace(value)?null:value;
  var groups=model.DocumentTypes.Values.Select(x=>Key(x.DocumentGroup)).Concat(model.Documents.Select(x=>model.DocumentTypes.TryGetValue(x.DocumentTypeId,out var type)?Key(type.DocumentGroup):null)).Distinct().ToList();
  int Order(string? name)=>name switch{"Hồ sơ gốc"=>0,"Hợp đồng lao động"=>1,"Quyết định / Văn bản khác" or "Quyết định"=>2,_=>3};
  return groups.OrderBy(Order).ThenBy(x=>x,StringComparer.CurrentCulture).Select(name=>new ProfileDocumentGroup(name,Order(name)==3?(name==null?"Khác (chưa có nhóm)":"Khác — "+name):name=="Quyết định / Văn bản khác"?"Quyết định":name??"Khác",Order(name)==3,model.Documents.Where(x=>(model.DocumentTypes.TryGetValue(x.DocumentTypeId,out var type)?Key(type.DocumentGroup):null)==name).ToList())).ToList();
 }
}
