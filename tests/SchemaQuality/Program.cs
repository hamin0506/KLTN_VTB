using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Vtb.Hr.Web.Data;
using Vtb.Hr.Web.Models;
using Vtb.Hr.Web.Services;
int passed=0;
void Check(bool ok,string name){if(!ok)throw new Exception(name);passed++;Console.WriteLine("PASS "+name);}
foreach(var pair in new[]{("DangLamViec","Đang làm việc"),("TamHoanHDLD","Tạm hoãn HĐLĐ"),("ChamDutHDLD","Chấm dứt HĐLĐ"),("NghiHuu","Nghỉ hưu")}){
 Check(Labels.Text(pair.Item1)==pair.Item2&&ProfilePresentation.EmploymentLabel(pair.Item1)==pair.Item2,"shared employment label "+pair.Item1);
 var option=CatalogService.Choices(pair.Item1).Single();Check(option.Value==pair.Item1&&option.Text==pair.Item2,"form/filter raw value preserved "+pair.Item1);
}
using var db=new VtbContext(new DbContextOptionsBuilder<VtbContext>().UseSqlServer("Server=localhost;Database=SchemaTest;Integrated Security=True;TrustServerCertificate=True").Options);
void Columns<T>(string view,string expected){var e=db.Model.FindEntityType(typeof(T))!;var id=StoreObjectIdentifier.View(view,"dbo");Check(e.GetProperties().Select(p=>p.GetColumnName(id)).ToHashSet().SetEquals(expected.Split(' ')),view+" exact columns");}
Columns<EmployeeQuality>("vw_employee_quality","employee_id full_name department_id position_name is_managerial employment_status join_date leave_date total_required_documents total_documents_present total_documents_missing missing_document_codes labor_contract_status quality_issue employee_quality_status");
Columns<DocumentQuality>("vw_document_quality","document_id employee_id full_name document_code document_name document_group issue_date expiry_date expiry_days storage_form required_storage_form requirement_level warning_threshold_days is_current invalid_reason quality_status working_status");
Columns<DocumentRequirement>("vw_employee_document_requirement","employee_id full_name position_name department_id department_name is_managerial employment_status requirement_id document_type_id document_code document_name document_group requirement_level required_storage_form warning_threshold_days requirement_reason document_id issue_date expiry_date storage_form file_path requirement_status");
Columns<LaborContractCheck>("vw_labor_contract_check","employee_id full_name department_id department_name employment_status valid_hdthuviec hdthuviec_expired exist_hdthuviec valid_hdxdth1 hdxdth1_expired exist_hdxdth1 valid_hdxdth2 hdxdth2_expired exist_hdxdth2 exist_hdkxdth labor_contract_status");
var reasons=new[]{"Thiếu tài liệu","Tài liệu bắt buộc đã hết hạn","Tài liệu bắt buộc không hợp lệ","Thiếu hợp đồng HDXDTH1","Cần ký kết hợp đồng tiếp theo","Thiếu tài liệu; Tài liệu bắt buộc không hợp lệ; Cần ký kết hợp đồng tiếp theo"};
var people=Enumerable.Range(1,8).Select(i=>new Employee{EmployeeId=i.ToString("D8"),DepartmentId=1,EmploymentStatus="DangLamViec"}).ToArray();
var quality=people.Select((e,i)=>new EmployeeQuality{EmployeeId=e.EmployeeId,DepartmentId=1,EmploymentStatus=e.EmploymentStatus,EmployeeQualityStatus=i<6?"THIEU_HO_SO":"DAY_DU",QualityIssue=i<6?reasons[i]:"Không có cảnh báo",TotalDocumentsMissing=i==0||i==5?2:0}).ToArray();
var rows=DashboardService.FollowupQuery(people.AsQueryable(),quality.AsQueryable(),Array.Empty<LaborContractCheck>().AsQueryable(),new[]{new Department{DepartmentId=1,DepartmentName="Test"}}.AsQueryable()).ToList();
Check(rows.Count==6&&rows.Select(x=>x.EmployeeId).Distinct().Count()==6,"six issue cases once; complete excluded");
for(int i=0;i<6;i++){Check(rows[i].Issue==reasons[i],"all reasons preserved "+i);Check(rows[i].Missing==quality[i].TotalDocumentsMissing,"missing count preserved "+i);Check(EmployeeQualityIndicators.FromQuality(quality[i]).State=="error","red from View "+i);}
Check(EmployeeQualityIndicators.FromQuality(quality[6]).State=="success","complete without invented contract conditions");
quality[7].EmploymentStatus="ChamDutHDLD";quality[7].QualityIssue="Không áp dụng đánh giá hồ sơ bắt buộc";
var ended=EmployeeQualityIndicators.FromQuality(quality[7]);Check(ended.NotApplicable&&ended.Label=="Không áp dụng"&&ended.State=="unknown"&&ended.Tooltip.Contains(quality[7].QualityIssue),"terminated non-applicable, SQL status preserved");
Check(EmployeeQualityIndicators.FromQuality(null).State=="unknown","missing View row not complete");
Check(EmployeeQualityIndicators.Evaluate(new ProfileModel{Quality=quality[6],Warnings=[new DocumentQuality{QualityStatus="KHONG_HOP_LE"}]},99).State=="success","document warning cannot override employee View");
Check(new Filter{Quality="DAY_DU"}.Quality=="DAY_DU"&&new Filter{Quality="error"}.Quality=="THIEU_HO_SO"&&new Filter{Quality="KHONG_HOP_LE"}.Quality==null,"two filters; old bookmarks normalized");
var service=new DashboardService(db);var f=new OverviewFilter{Statuses=["DangLamViec","TamHoanHDLD"],DivisionIds=[1],DepartmentIds=[1]};
var dq=service.DocumentMeasureScope(f).ToQueryString();Check(dq.Contains("working_status")&&!dq.Contains("is_kpi_scope"),"actual exported scope column in SQL");
var sql=service.MissingQuery(f).ToQueryString();Check(sql.Contains("quality_issue")&&sql.Contains("employee_quality_status")&&sql.Contains("THIEU_HO_SO"),"followup translates with View reasons/status");
Check(sql.Contains("DangLamViec")&&sql.Contains("TamHoanHDLD")&&sql.Contains("division_id")&&sql.Contains("department_id"),"employee/organization scope retained");
Check(!sql.Contains("total_documents_expired")&&!sql.Contains("total_documents_invalid")&&!sql.Contains("is_current_employee"),"no removed employee columns");
// In-memory SQL-result fixtures only: these verify presentation, not execution of SQL CASE rules.
foreach(var sample in new[]{
 ("probation expired, no process warning","DangLamViec","DAY_DU","KHONG_CANH_BAO","success"),
 ("fixed1 expired with successor","DangLamViec","DAY_DU","HOP_LE","success"),
 ("fixed2 needs successor","DangLamViec","THIEU_HO_SO","CAN_KY_KET_TIEP","error"),
 ("ordinary missing","DangLamViec","THIEU_HO_SO","HOP_LE","error"),
 ("mandatory expired","DangLamViec","THIEU_HO_SO","HOP_LE","error"),
 ("indefinite with no warning","DangLamViec","DAY_DU","KHONG_CANH_BAO","success"),
 ("suspended","TamHoanHDLD","THIEU_HO_SO","CAN_KY_KET_TIEP","error"),
 ("terminated","ChamDutHDLD","DAY_DU","KHONG_CANH_BAO","unknown"),
 ("retired retirement requirement","NghiHuu","THIEU_HO_SO","KHONG_CANH_BAO","error")}){
 var result=new EmployeeQuality{EmploymentStatus=sample.Item2,EmployeeQualityStatus=sample.Item3,LaborContractStatus=sample.Item4};
 Check(EmployeeQualityIndicators.Evaluate(new ProfileModel{Quality=result,Warnings=[new DocumentQuality{DocumentCode="HDTHUVIEC",QualityStatus="DA_HET_HAN"}]},0).State==sample.Item5,sample.Item1);
}
var requirementSql=service.ReportRequirements(new ReportFilter{DivisionIds=[1],TypeIds=[1]}).ToQueryString();
Check(requirementSql.Contains("document_type_id")&&requirementSql.Contains("employee_id")&&!requirementSql.Contains("FROM [dbo].[document]"),"missing requirement survives employee/type filtering without document existence");
Check(service.EmployeeMeasureScope(new OverviewFilter()).ToQueryString().Contains("TamHoanHDLD"),"current personnel KPI scope explicit");
Check(EmployeeQualityIndicators.FromQuality(new EmployeeQuality{EmployeeQualityStatus="DAY_DU",EmploymentStatus="ChamDutHDLD"}).Label=="Không áp dụng","terminated empty issue still non-applicable");
// Rule 9 fixtures: storage is independent of assessment and personnel state.
var archiveEmployee=new Employee{EmployeeId="TEST9001",EmploymentStatus="DangLamViec"};
var archiveDocs=new List<Document>{new(){DocumentId=901,EmployeeId="TEST9001",DocumentTypeId=901,IsCurrent=true,IssueDate=new(2024,1,1)},new(){DocumentId=902,EmployeeId="TEST9001",DocumentTypeId=901,IsCurrent=false,IssueDate=new(2023,1,1)},new(){DocumentId=903,EmployeeId="TEST9001",DocumentTypeId=902,IsCurrent=true,IssueDate=new(2025,1,1)}};
var archive=new ProfileModel{Employee=archiveEmployee,Documents=archiveDocs,DocumentTypes=new(){[901]=new(){DocumentTypeId=901,DocumentCode="HDXDTH1",DocumentName="Hợp đồng kiểm thử",DocumentGroup="Hợp đồng lao động"},[902]=new(){DocumentTypeId=902,DocumentCode="HDKXDTH",DocumentName="Hợp đồng khác",DocumentGroup="Hợp đồng lao động"}},Warnings=[new(){DocumentId=901,QualityStatus="DA_HET_HAN"}]};
foreach(var state in new[]{"DangLamViec","TamHoanHDLD","NghiHuu","ChamDutHDLD"}){
 archiveEmployee.EmploymentStatus=state;
 var documents=ProfilePresentation.Groups(archive).SelectMany(g=>g.Documents).ToList();
 Check(documents.Count==3&&documents.Select(x=>x.DocumentId).Distinct().Count()==3,"all archived contracts retained "+state);
 Check(documents.Count(d=>d.IsCurrent)==2&&documents.Count(d=>!d.IsCurrent)==1,"current/history unchanged "+state);
 Check(archive.DocumentQualityRows.Count==0&&documents.Count==3,"quality row not required "+state);
 Check(ProfilePresentation.ProfileWarnings(archive).Count()==(state is "DangLamViec" or "TamHoanHDLD"?1:0),"Rule 9 warning scope "+state);
}
db.Attach(archiveEmployee);foreach(var doc in archiveDocs)db.Attach(doc);
archiveEmployee.EmploymentStatus="NghiHuu";db.ChangeTracker.DetectChanges();
Check(db.Entry(archiveEmployee).State==EntityState.Modified&&archiveDocs.All(d=>db.Entry(d).State==EntityState.Unchanged),"EF employee status edit does not mark documents modified/deleted");
var storageSql=db.Documents.Where(d=>d.EmployeeId=="TEST9001").InRequirementOrder(db).ToQueryString();
Check(!storageSql.Contains("vw_document_quality")&&!storageSql.Contains("employment_status")&&storageSql.Contains("FROM [dbo].[document]"),"storage query has no quality join or personnel exclusion");
var kpiSql=service.KpiEmployeeIds().ToQueryString();
Check(kpiSql.Contains("DangLamViec")&&kpiSql.Contains("TamHoanHDLD")&&!kpiSql.Contains("NghiHuu")&&!kpiSql.Contains("ChamDutHDLD"),"KPI active/suspended scope retained");
foreach(var kind in new[]{"problem","expiring","expiredContracts"})Check(service.ReportTableQuery(new ReportFilter{Kind=kind}).ToQueryString().Contains("[working_status] = 1"),"warning table working scope "+kind);
var contractSql=service.ReportContracts(new ReportFilter()).ToQueryString();Check(contractSql.Contains("DangLamViec")&&contractSql.Contains("TamHoanHDLD"),"contract warning/drill scope active only");
// Query translation only: ToQueryString never opens the SQL connection.
foreach(var statuses in new[]{Array.Empty<string>(),new[]{"DangLamViec","TamHoanHDLD"},new[]{"DangLamViec"},new[]{"TamHoanHDLD"}}){
 foreach(var state in new[]{"HOP_LE","SAP_HET_HAN","DA_HET_HAN","KHONG_HOP_LE","BLANK"}){
  var drillSql=service.DocumentDrillQuery(new DocumentDrillFilter{State=state,Statuses=statuses,DivisionIds=[1],DepartmentIds=[1],Search="TEST"}).ToQueryString();
  Check(drillSql.Contains("[working_status] = 1")&&drillSql.Contains("quality_status")&&drillSql.Contains("division_id")&&drillSql.Contains("department_id")&&drillSql.Contains("TEST"),"drill scope/state/organization/search retained "+state+"/"+string.Join(',',statuses));
  Check(statuses.All(status=>drillSql.Contains(status)),"selected employment filter retained");
 }
}
RequirementRule Rule(int id,DateOnly start,DateOnly? end,bool active=true,int type=1)=>new(){RequirementId=id,DocumentTypeId=type,EffectiveFrom=start,EffectiveTo=end,IsActive=active};
bool Conflict(RequirementRule oldRule,RequirementRule candidate,bool editing=false)=>CatalogService.OverlappingRules(new[]{oldRule}.AsQueryable(),candidate,editing).Any();
var oldRule=Rule(1,new(2025,1,1),new(2025,12,31));
var nextRule=Rule(2,new(2026,1,1),null);
Check(!Conflict(oldRule,nextRule),"31 December then 1 January permitted");
Check(Conflict(Rule(1,new(2025,1,1),new(2026,1,1)),nextRule),"shared end/start date overlaps");
Check(Conflict(Rule(1,new(2025,1,1),null),nextRule),"open old interval blocks new active version");
Check(!Conflict(oldRule,oldRule,true),"edit excludes own rule ID");
Check(Conflict(oldRule,oldRule,false),"create does not exclude a supplied ID");
Check(!Conflict(oldRule,Rule(2,new(2025,6,1),null,false)),"inactive candidate allowed");
Check(Conflict(oldRule,Rule(2,new(2025,6,1),null,true)),"reactivation of overlapping version blocked");
Check(!Conflict(Rule(1,new(2025,1,1),null,false),nextRule),"inactive existing version ignored");
Check(!Conflict(oldRule,Rule(2,new(2025,6,1),null,true,2)),"different type allowed");
Check(!Conflict(nextRule,oldRule),"earlier disjoint version permitted");
Check(Conflict(oldRule,Rule(2,new(2025,6,1),new(2025,6,1))),"one-day interval within old version overlaps");
Check(Conflict(Rule(1,new(2025,1,1),null),Rule(2,new(2026,1,1),null)),"two open intervals overlap");
foreach(var candidate in new[]{nextRule,oldRule}){
 var overlapSql=CatalogService.OverlappingRules(db.RequirementRules,candidate,true).ToQueryString();
 Check(overlapSql.Contains("effective_from")&&overlapSql.Contains("effective_to")&&overlapSql.Contains("is_active")&&overlapSql.Contains("requirement_id"),"overlap predicate translates for nullable/dated end");
}
// Duplicate rules/current rows cannot inflate employee/type requirements.
DocumentRequirement Req(string id,int type,string status,string employment="DangLamViec")=>new(){EmployeeId=id,DocumentTypeId=type,RequirementStatus=status,EmploymentStatus=employment};
var normalized=DashboardService.RequirementPairs(new[]{Req("A",1,"THIEU_HO_SO"),Req("A",1,"THIEU_HO_SO"),Req("A",2,"DA_CO"),Req("A",2,"THIEU_HO_SO"),Req("B",1,"THIEU_HO_SO","TamHoanHDLD"),Req("C",1,"THIEU_HO_SO","NghiHuu"),Req("D",1,"THIEU_HO_SO","ChamDutHDLD")}.AsQueryable()).ToList();
Check(normalized.Count==3&&normalized.Sum(x=>x.Missing)==2,"pair normalization: duplicate rules excluded, retired excluded, present quality-independent");
Check(normalized.Single(x=>x.EmployeeId=="A"&&x.DocumentTypeId==2).Missing==0,"existing current document defeats missing duplicate requirement");
var weightedRows=Enumerable.Range(1,100).Select(i=>new RequirementDrillRow{EmployeeId=i.ToString(),DocumentTypeId=1,DivisionId=1,DivisionName="Khối",DepartmentId=i==1?11:12,DepartmentName=i==1?"Phòng A":"Phòng B",Missing=i==1?1:0}).AsQueryable();
var aggregate=DashboardService.RequirementChartQuery(weightedRows,"divisions").Single();
Check(aggregate.Total==100&&aggregate.Missing==1&&aggregate.Employees==1&&aggregate.Rate==0.99,"division rate 99%, not mean department rate 50%");
Check(new RequirementChartPoint().Rate==null,"zero requirements is not applicable");
var sameEmployee=new[]{new RequirementDrillRow{EmployeeId="A",DivisionId=1,DocumentTypeId=1,Missing=1},new RequirementDrillRow{EmployeeId="A",DivisionId=1,DocumentTypeId=2,Missing=1}}.AsQueryable();
var units=DashboardService.RequirementChartQuery(sameEmployee,"divisions").Single();Check(units.Missing==2&&units.Employees==1,"two missing requirements but one employee");
foreach(var level in new[]{"types","divisions","departments"}){
 var filter=new RequirementDrillFilter{Level=level,DrillDivisionId=1,DrillDepartmentId=11,DrillTypeId=2,DivisionIds=[1],DepartmentIds=[11],EmployeeIds=["TEST0001"],TypeIds=[2],Groups=["Hồ sơ gốc"],Search="TEST"};
 var query=service.RequirementDrillQuery(filter);
 var chartSql=DashboardService.RequirementChartQuery(query,level).ToQueryString();
 Check(chartSql.Contains("THIEU_HO_SO")&&chartSql.Contains("DangLamViec")&&chartSql.Contains("TamHoanHDLD")&&chartSql.Contains("GROUP BY")&&chartSql.Contains("document_type_id")&&chartSql.Contains("division_id")&&chartSql.Contains("department_id"),"requirement chart translates with Rule9 and ID scope "+level);
 var peopleSql=service.RequirementPeopleQuery(filter).OrderBy(x=>x.EmployeeId).Skip(0).Take(20).ToQueryString();
 Check(peopleSql.Contains("OFFSET")&&peopleSql.Contains("TEST")&&!peopleSql.Contains("vw_employee_quality"),"people SQL paginates and searches actual missing pairs "+level);
}
var allDepartments=DashboardService.RequirementChartQuery(weightedRows,"departments").Single(x=>x.Id==11);
var detailDepartment=DashboardService.RequirementChartQuery(weightedRows.Where(x=>x.DivisionId==1),"departments").Single(x=>x.Id==11);
Check(allDepartments.Rate==detailDepartment.Rate&&allDepartments.Total==detailDepartment.Total&&allDepartments.Missing==detailDepartment.Missing&&allDepartments.Employees==detailDepartment.Employees,"all departments and selected division have identical department measures");
var allSql=service.RequirementPeopleQuery(new RequirementDrillFilter{Level="departments",DrillDepartmentId=11}).ToQueryString();
Check(allSql.Contains("department_id")&&allSql.Contains("THIEU_HO_SO")&&allSql.Contains("DangLamViec")&&allSql.Contains("TamHoanHDLD"),"all-department detail without division retains exact department, missing and Rule9 scope");
var typedDrill=service.DocumentDrillQuery(new DocumentDrillFilter{State="HOP_LE",DocumentTypeId=12}).ToQueryString();
Check(typedDrill.Contains("document_type_id")&&typedDrill.Contains("[working_status] = 1"),"document type and personnel scope retained for colored segment");
var issueDrill=service.ReportTableQuery(new ReportFilter{Kind="contractDetail",IssuesOnly=true,IssueDepartmentId=11}).ToQueryString();
Check(issueDrill.Contains("CAN_KY_KET_TIEP")&&issueDrill.Contains("THIEU_HDXDTH1")&&issueDrill.Contains("department_id")&&issueDrill.Contains("TamHoanHDLD"),"contract department drill uses current contract issues and Rule9");
Console.WriteLine($"{passed} passed; no live database connection or writes.");
