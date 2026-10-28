using Microsoft.EntityFrameworkCore;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
namespace Vtb.Hr.Web.Services;
public partial class DashboardService
{
 public static string DocumentState(string status)=>status.ToUpperInvariant() switch{"HOP_LE"=>"HOP_LE","SAP_HET_HAN"=>"SAP_HET_HAN","DA_HET_HAN"=>"DA_HET_HAN","KHONG_HOP_LE"=>"KHONG_HOP_LE",_=>"BLANK"};
 public IQueryable<DocumentDrillRow> DocumentDrillQuery(DocumentDrillFilter f){
  var employees=EmployeeScope(f.Scope());
  var docs=db.DocumentQualities.AsNoTracking();
  // Drillthrough filters the calculated SWITCH column, not the source Measure's CALCULATE scope.
  docs=f.State=="BLANK"?docs.Where(x=>x.QualityStatus!="HOP_LE"&&x.QualityStatus!="SAP_HET_HAN"&&x.QualityStatus!="DA_HET_HAN"&&x.QualityStatus!="KHONG_HOP_LE"):docs.Where(x=>x.QualityStatus==f.State);
  var q=from d in docs join e in employees on d.EmployeeId equals e.EmployeeId join dep in db.Departments on e.DepartmentId equals dep.DepartmentId
   select new DocumentDrillRow{EmployeeId=d.EmployeeId,FullName=d.FullName,Department=dep.DepartmentName,Code=d.DocumentCode,Document=d.DocumentName,IssueDate=d.IssueDate,ExpiryDate=d.ExpiryDate,Days=d.ExpiryDays,RequiredStorage=d.RequiredStorageForm,Storage=d.StorageForm,Reason=d.InvalidReason};
  // PBIR table has only Column projections (no document_id/Measure). Group identical visible tuples.
  q=q.Distinct();
  if(!string.IsNullOrWhiteSpace(f.Search)){var term=f.Search.Trim();q=q.Where(x=>x.EmployeeId.Contains(term)||x.FullName.Contains(term)||x.Code.Contains(term)||x.Document.Contains(term));}return q;
 }
 public async Task<DocumentDrillPage> DocumentDetails(DocumentDrillFilter f,CancellationToken ct){
  var q=DocumentDrillQuery(f);var total=await q.CountAsync(ct);var page=Math.Min(f.Page,Math.Max(1,(int)Math.Ceiling(total/(double)f.PageSize)));
  IOrderedQueryable<DocumentDrillRow> sorted=f.Sort switch{
   "name"=>f.Desc?q.OrderByDescending(x=>x.FullName):q.OrderBy(x=>x.FullName),"department"=>f.Desc?q.OrderByDescending(x=>x.Department):q.OrderBy(x=>x.Department),"code"=>f.Desc?q.OrderByDescending(x=>x.Code):q.OrderBy(x=>x.Code),"document"=>f.Desc?q.OrderByDescending(x=>x.Document):q.OrderBy(x=>x.Document),
   "issueDate"=>f.Desc?q.OrderByDescending(x=>x.IssueDate):q.OrderBy(x=>x.IssueDate),"expiryDate"=>f.Desc?q.OrderByDescending(x=>x.ExpiryDate):q.OrderBy(x=>x.ExpiryDate),"days"=>f.Desc?q.OrderByDescending(x=>x.Days):q.OrderBy(x=>x.Days),"requiredStorage"=>f.Desc?q.OrderByDescending(x=>x.RequiredStorage):q.OrderBy(x=>x.RequiredStorage),"storage"=>f.Desc?q.OrderByDescending(x=>x.Storage):q.OrderBy(x=>x.Storage),"reason"=>f.Desc?q.OrderByDescending(x=>x.Reason):q.OrderBy(x=>x.Reason),_=>f.Desc?q.OrderByDescending(x=>x.EmployeeId):q.OrderBy(x=>x.EmployeeId)};
  return new(await sorted.ThenBy(x=>x.EmployeeId).ThenBy(x=>x.Code).ThenBy(x=>x.IssueDate).ThenBy(x=>x.ExpiryDate).ThenBy(x=>x.Document).ThenBy(x=>x.Department).ThenBy(x=>x.FullName).ThenBy(x=>x.Days).ThenBy(x=>x.RequiredStorage).ThenBy(x=>x.Storage).ThenBy(x=>x.Reason).Skip((page-1)*f.PageSize).Take(f.PageSize).ToListAsync(ct),total,page,f.PageSize);
 }
}
