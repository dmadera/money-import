DECLARE @sql AS varchar(max) = '';
DECLARE @ColumnName AS varchar(100) = 'ID'
DECLARE @ColumnValue AS varchar(100) = 'A9BB3865-9292-4A80-82B3-56A8DB2054CA'
DECLARE @ResultQuery AS varchar(max) =  'SELECT ''@TableName'' AS TableName ' + 
                                        '       ,@ColumnName ' +
                                        'FROM @TableName ' +
                                        'WHERE @ColumnName = ''' + @ColumnValue + '''';
SET @ResultQuery = REPLACE(@ResultQuery, '@ColumnName', QUOTENAME(@ColumnName));

WITH AllTables AS (
    SELECT SCHEMA_NAME(Tables.schema_id) AS SchemaName
          ,Tables.name AS TableName
          ,Columns.name AS ColumnName
    FROM sys.tables AS Tables
        INNER JOIN sys.columns AS Columns 
            ON Tables.object_id = Columns.object_id
    WHERE Columns.name = @ColumnName
)
SELECT @sql = @sql + ' UNION ALL ' +
       REPLACE(@ResultQuery, '@TableName', QUOTENAME(TableName)) + CHAR(13)
FROM AllTables

SET @sql = STUFF(@sql, 1, LEN(' UNION ALL'), '');

--PRINT @sql;

SET @sql = 
'WITH AllTables AS ( ' +
   @sql + 
') ' +
'SELECT DISTINCT TableName ' +
'FROM AllTables ';

EXEC (@sql)