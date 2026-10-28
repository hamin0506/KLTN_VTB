-- Read-only diagnosis. Select VTB2 in SSMS; no UPDATE/DDL or personnel names.
SELECT c.name,TYPE_NAME(c.system_type_id) AS data_type,c.max_length,c.is_nullable
FROM sys.columns c WHERE c.object_id=OBJECT_ID('dbo.document') AND c.name='storage_form';
SELECT name,definition,is_disabled FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID('dbo.document');
SELECT d.document_id,t.document_code,
 CASE WHEN d.storage_form IS NULL THEN 'SQL_NULL'
 WHEN UPPER(LTRIM(RTRIM(d.storage_form)))=N'NULL' THEN 'LITERAL_NULL'
 WHEN LEN(LTRIM(RTRIM(d.storage_form)))=0 THEN 'EMPTY_TEXT' ELSE 'OTHER_VALUE' END AS storage_kind,
 d.storage_form,DATALENGTH(d.storage_form) AS storage_bytes,d.is_current
FROM dbo.document d JOIN dbo.document_type t ON t.document_type_id=d.document_type_id
WHERE t.document_code='HDXDTH1';
SELECT t.document_code,r.requirement_id,r.required_storage_form,r.is_active,r.effective_from,r.effective_to
FROM dbo.document_type t LEFT JOIN dbo.requirement_rule r ON r.document_type_id=t.document_type_id
WHERE t.document_code='HDXDTH1';
