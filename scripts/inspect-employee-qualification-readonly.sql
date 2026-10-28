-- Read-only schema inventory. Select VTB2 in SSMS. No DDL/DML.
SELECT DB_NAME() AS database_name;
SELECT s.name AS schema_name,t.name AS table_name,c.column_id,c.name AS column_name,ty.name AS data_type,c.max_length,c.precision,c.scale,c.is_nullable,c.is_identity,dc.definition AS default_definition
FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.object_id=c.default_object_id
WHERE s.name='dbo' AND t.name IN('employee','qualification','employee_qualification') ORDER BY t.name,c.column_id;
SELECT OBJECT_NAME(i.object_id) AS table_name,i.name,i.is_primary_key,i.is_unique,i.filter_definition,c.name AS column_name,ic.key_ordinal
FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE i.object_id IN(OBJECT_ID('dbo.employee'),OBJECT_ID('dbo.qualification'),OBJECT_ID('dbo.employee_qualification')) ORDER BY table_name,i.name,ic.key_ordinal;
SELECT OBJECT_NAME(f.parent_object_id) AS table_name,f.name,c.name AS column_name,OBJECT_NAME(f.referenced_object_id) AS referenced_table,rc.name AS referenced_column,f.delete_referential_action_desc,f.update_referential_action_desc
FROM sys.foreign_keys f JOIN sys.foreign_key_columns fc ON fc.constraint_object_id=f.object_id JOIN sys.columns c ON c.object_id=fc.parent_object_id AND c.column_id=fc.parent_column_id JOIN sys.columns rc ON rc.object_id=fc.referenced_object_id AND rc.column_id=fc.referenced_column_id
WHERE f.parent_object_id IN(OBJECT_ID('dbo.employee'),OBJECT_ID('dbo.qualification'),OBJECT_ID('dbo.employee_qualification'));
SELECT OBJECT_NAME(parent_object_id) AS table_name,name,definition,is_disabled FROM sys.check_constraints
WHERE parent_object_id IN(OBJECT_ID('dbo.employee'),OBJECT_ID('dbo.qualification'),OBJECT_ID('dbo.employee_qualification'));
