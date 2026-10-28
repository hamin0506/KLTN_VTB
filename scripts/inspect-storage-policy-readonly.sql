-- SELECT only. Run on VTB2 to verify live schema/View and storage requirements.
SELECT c.name,TYPE_NAME(c.system_type_id) AS data_type,c.is_nullable FROM sys.columns c WHERE c.object_id=OBJECT_ID('dbo.document') AND c.name='storage_form';
SELECT name,definition,is_disabled FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID('dbo.document');
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.vw_document_quality')) AS document_quality_definition;
SELECT t.document_code,t.document_name,t.document_group,r.requirement_id,r.required_storage_form,
CASE WHEN r.requirement_id IS NULL THEN 'NO_EFFECTIVE_RULE' WHEN r.required_storage_form IS NULL THEN 'OPTIONAL' ELSE 'REQUIRED' END AS storage_policy,
r.effective_from,r.effective_to
FROM dbo.document_type t LEFT JOIN dbo.requirement_rule r ON r.document_type_id=t.document_type_id AND r.is_active=1
AND r.effective_from<=CAST(GETDATE() AS date) AND(r.effective_to IS NULL OR r.effective_to>=CAST(GETDATE() AS date))
ORDER BY t.document_group,t.document_code,r.requirement_id;
SELECT DISTINCT storage_form FROM dbo.document;
SELECT DISTINCT required_storage_form FROM dbo.requirement_rule;
