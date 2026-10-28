-- Read-only: run in SSMS on VTB2. No document/personnel rows are modified.
SELECT c.name, TYPE_NAME(c.system_type_id) AS data_type, c.max_length, c.is_nullable
FROM sys.columns c WHERE c.object_id=OBJECT_ID(N'dbo.document') AND c.name=N'storage_form';
SELECT name, definition, is_disabled FROM sys.check_constraints
WHERE parent_object_id=OBJECT_ID(N'dbo.document');
SELECT DISTINCT storage_form FROM dbo.document ORDER BY storage_form;
SELECT DISTINCT required_storage_form FROM dbo.requirement_rule ORDER BY required_storage_form;
