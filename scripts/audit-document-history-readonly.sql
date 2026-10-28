-- Read-only audit for ALL employees. No document content, names or file paths returned.
SET NOCOUNT ON;
SELECT DB_NAME() AS database_name, @@SERVERNAME AS server_name;
SELECT e.employee_id, COUNT_BIG(d.document_id) AS total_documents,
 SUM(CONVERT(bigint,CASE WHEN d.is_current=1 THEN 1 ELSE 0 END)) AS current_documents,
 SUM(CONVERT(bigint,CASE WHEN d.is_current=0 THEN 1 ELSE 0 END)) AS historical_documents,
 SUM(CONVERT(bigint,CASE WHEN d.document_id IS NOT NULL AND d.is_current IS NULL THEN 1 ELSE 0 END)) AS unclassified_documents,
 SUM(CONVERT(bigint,CASE WHEN d.document_id IS NOT NULL AND t.document_type_id IS NULL THEN 1 ELSE 0 END)) AS missing_catalog_documents
FROM dbo.employee e
LEFT JOIN dbo.document d ON d.employee_id=e.employee_id
LEFT JOIN dbo.document_type t ON t.document_type_id=d.document_type_id
GROUP BY e.employee_id ORDER BY e.employee_id;
SELECT d.document_id,d.employee_id,d.document_type_id,d.issue_date,d.created_at,d.is_current
FROM dbo.document d ORDER BY d.employee_id,d.document_type_id,d.is_current DESC,d.issue_date DESC,d.created_at DESC,d.document_id DESC;
