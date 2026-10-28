-- READ ONLY. Run against VTB2 in SSMS or sqlcmd. No writes or schema changes.
SET NOCOUNT ON;
DECLARE @division_id int = NULL, @department_id int = NULL, @kpi_only bit = 0;
-- Use the same scope as the website filters. Set the three variables above as needed.
WITH scope AS (
 SELECT e.employee_id FROM dbo.employee e JOIN dbo.department d ON d.department_id=e.department_id
 WHERE (@division_id IS NULL OR d.division_id=@division_id)
 AND (@department_id IS NULL OR e.department_id=@department_id)
 AND (@kpi_only=0 OR e.employment_status IN ('DangLamViec','TamHoanHDLD'))
)
SELECT
 (SELECT COUNT(*) FROM scope) AS employees,
 (SELECT COUNT(*) FROM dbo.document d WHERE d.employee_id IN(SELECT employee_id FROM scope)) AS documents_including_history,
 (SELECT COUNT(*) FROM dbo.vw_employee_quality q WHERE q.employee_id IN(SELECT employee_id FROM scope) AND q.employee_quality_status='DAY_DU') AS complete_employees,
 (SELECT COUNT(*) FROM dbo.vw_employee_quality q WHERE q.employee_id IN(SELECT employee_id FROM scope) AND q.employee_quality_status='THIEU_HO_SO') AS missing_employees,
 (SELECT COUNT(DISTINCT q.document_id) FROM dbo.vw_document_quality q WHERE q.employee_id IN(SELECT employee_id FROM scope) AND q.quality_status='DA_HET_HAN') AS expired_documents,
 (SELECT COUNT(DISTINCT q.document_id) FROM dbo.vw_document_quality q WHERE q.employee_id IN(SELECT employee_id FROM scope) AND q.quality_status='SAP_HET_HAN') AS expiring_documents,
 (SELECT COUNT(DISTINCT q.document_id) FROM dbo.vw_document_quality q WHERE q.employee_id IN(SELECT employee_id FROM scope) AND q.quality_status='KHONG_HOP_LE') AS invalid_documents;
SELECT 'vw_document_quality' AS source,COUNT(*) AS row_count FROM dbo.vw_document_quality
UNION ALL SELECT 'vw_employee_document_requirement',COUNT(*) FROM dbo.vw_employee_document_requirement
UNION ALL SELECT 'vw_labor_contract_check',COUNT(*) FROM dbo.vw_labor_contract_check
UNION ALL SELECT 'vw_employee_quality',COUNT(*) FROM dbo.vw_employee_quality;
SELECT quality_status, COUNT(*) AS view_rows, COUNT(DISTINCT document_id) AS distinct_documents FROM dbo.vw_document_quality GROUP BY quality_status;
SELECT COUNT(*) AS documents_with_conflicting_states FROM (SELECT document_id FROM dbo.vw_document_quality GROUP BY document_id HAVING COUNT(DISTINCT quality_status)>1) x;
SELECT document_type_id, COUNT(*) AS rule_count FROM dbo.requirement_rule GROUP BY document_type_id HAVING COUNT(*)>1;
SELECT COUNT(*) AS orphan_rules FROM dbo.requirement_rule r LEFT JOIN dbo.document_type t ON r.document_type_id=t.document_type_id WHERE t.document_type_id IS NULL;