-- Executive Overview: SELECT only. Run in VTB2 via SSMS under the authorized account.
-- Defaults match no explicit slicer selection. Set parameters for single-selection comparisons.
DECLARE @DivisionId int = NULL, @DepartmentId int = NULL;
DECLARE @EmploymentStatus varchar(30) = NULL;
WITH scope AS (
 SELECT e.employee_id FROM dbo.employee e JOIN dbo.department d ON d.department_id=e.department_id
 WHERE (@DivisionId IS NULL OR d.division_id=@DivisionId)
 AND (@DepartmentId IS NULL OR EXISTS(SELECT 1 FROM dbo.department selected WHERE selected.department_id=@DepartmentId AND selected.division_id=d.division_id AND selected.department_name=d.department_name))
 AND (@EmploymentStatus IS NULL OR e.employment_status=@EmploymentStatus)
), eq AS (
 SELECT q.* FROM dbo.vw_employee_quality q WHERE q.employee_id IN (SELECT employee_id FROM scope)
 AND q.employment_status IN ('DangLamViec','TamHoanHDLD')
), dq AS (
 SELECT q.* FROM dbo.vw_document_quality q WHERE q.employee_id IN (SELECT employee_id FROM scope) AND q.is_kpi_scope=1
), a AS (
 SELECT NULLIF(COUNT_BIG(*),0) total_employees,
 NULLIF(COUNT_BIG(CASE WHEN employee_quality_status='DAY_DU' THEN 1 END),0) complete_employees,
 NULLIF(COUNT_BIG(CASE WHEN employee_quality_status='THIEU_HO_SO' THEN 1 END),0) missing_employees FROM eq
), b AS (
 SELECT NULLIF(COUNT_BIG(*),0) total_documents,
 NULLIF(COUNT_BIG(CASE WHEN quality_status='SAP_HET_HAN' THEN 1 END),0) expiring_documents,
 NULLIF(COUNT_BIG(CASE WHEN quality_status='DA_HET_HAN' THEN 1 END),0) expired_documents,
 NULLIF(COUNT_BIG(CASE WHEN quality_status='KHONG_HOP_LE' THEN 1 END),0) invalid_documents FROM dq
)
SELECT a.*,CAST(complete_employees AS float)/NULLIF(total_employees,0) completeness_rate,b.* FROM a CROSS JOIN b;
