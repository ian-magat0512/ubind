-- Set the schema and table index values using c# string format
DECLARE @Schema NVARCHAR(128) = '{0}';
DECLARE @TableIndex NVARCHAR(2)  = '{1}';

-- Create a temporary to store the list of tables
CREATE TABLE #TableList
(
    TableName NVARCHAR(128)
);

-- Include tables without indexes to clean up the old data.
-- Todo: This block can be removed in the future once we have tables with index prefix for GNAF, NFID, and RedBook
INSERT INTO #TableList (TableName)
SELECT t.name
FROM sys.tables t
WHERE t.schema_id = SCHEMA_ID(@Schema) and t.name NOT LIKE '%[_][0-9][0-9]';

-- Get tables based on the schema and index
INSERT INTO #TableList (TableName)
SELECT t.name
FROM sys.tables t
WHERE t.schema_id = SCHEMA_ID(@Schema) and t.name LIKE '%[_]' + @TableIndex;

DECLARE @TableName NVARCHAR(128);
DECLARE @ForeignKeyName NVARCHAR(128);
DECLARE @ForeignKeyTable NVARCHAR(128);
DECLARE @DropSql NVARCHAR(MAX);

-- Loop through the table names in #TableList and drop each table
DECLARE table_cursor CURSOR FOR
	SELECT TableName FROM #TableList;

OPEN table_cursor;

FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Drop foreign key constraints
    DECLARE ForeignKeyCursor CURSOR FOR
    SELECT name, FkTableName = OBJECT_NAME(parent_object_id)
    FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID(@Schema + '.' + @TableName) 
        OR referenced_object_id = OBJECT_ID(@Schema + '.' + @TableName)

    OPEN ForeignKeyCursor
    FETCH NEXT FROM ForeignKeyCursor INTO @ForeignKeyName, @ForeignKeyTable

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @DropSql = CONCAT('ALTER TABLE ', @Schema, '.', @ForeignKeyTable, ' DROP CONSTRAINT ', @ForeignKeyName)
        EXEC sp_executesql @DropSql
        FETCH NEXT FROM ForeignKeyCursor INTO @ForeignKeyName, @ForeignKeyTable
    END

    CLOSE ForeignKeyCursor;
    DEALLOCATE ForeignKeyCursor;

	-- Drop the table
    SET @DropSql = CONCAT('DROP TABLE IF EXISTS ', @Schema, '.', @TableName, '')
    EXEC sp_executesql @DropSql;

    FETCH NEXT FROM table_cursor INTO @TableName;
END;

CLOSE table_cursor;
DEALLOCATE table_cursor;

-- Drop the temporary table #TableList
DROP TABLE #TableList;