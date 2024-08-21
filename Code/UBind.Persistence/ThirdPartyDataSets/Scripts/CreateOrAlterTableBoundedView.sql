-- Set the schema and table index values using c# string format
DECLARE @Schema NVARCHAR(128) = '{0}';
DECLARE @TableIndex NVARCHAR(2) = '{1}';
DECLARE @Columns NVARCHAR(MAX);
DECLARE @SqlQuery NVARCHAR(MAX);

-- Create a table to store the list of tables
CREATE TABLE #TableList
(
    TableName NVARCHAR(128)
);

IF @TableIndex = ''
	BEGIN
		-- Get tables without indexes
		INSERT INTO #TableList (TableName)
		SELECT t.name
		FROM sys.tables t
		WHERE t.schema_id = SCHEMA_ID(@Schema) and t.name NOT LIKE '%[_][0-9][0-9]';
	END
ELSE
	BEGIN
		-- Get tables based on the schema and index
		INSERT INTO #TableList (TableName)
		SELECT t.name
		FROM sys.tables t
		WHERE t.schema_id = SCHEMA_ID(@Schema) and t.name LIKE '%[_]' + @TableIndex;
	END


-- Declare variables to store the current table name
DECLARE @CurrentTableName NVARCHAR(128)
DECLARE @NewTableSchemaAndName NVARCHAR(128)
DECLARE @ViewSchemaAndName NVARCHAR(128)
DECLARE @DropAndCreateView BIT

-- Create a cursor to loop through the list of table names
DECLARE tableCursor CURSOR FOR
SELECT TableName
FROM #TableList;

OPEN tableCursor;

FETCH NEXT FROM tableCursor INTO @CurrentTableName;

WHILE @@FETCH_STATUS = 0
BEGIN
	-- Remove prefixes for view name
	SET @ViewSchemaAndName = CASE WHEN @TableIndex = ''
								THEN  N'[' + @Schema + '].[' + @CurrentTableName + '_View]'
								ELSE N'[' + @Schema + '].[' + LEFT(@CurrentTableName, LEN(@CurrentTableName) - 3) + '_View]'
							END
	SET @NewTableSchemaAndName = N'[' + @Schema + '].[' + @CurrentTableName + ']'
	SET @DropAndCreateView = 0

    -- Get the columns for the current table and format them with brackets
    SET @Columns = (
        SELECT STRING_AGG('[' + name + ']', ', ') AS COLUMN_NAME
        FROM sys.columns
        WHERE object_id = OBJECT_ID(@NewTableSchemaAndName)
    );
	
    -- Validate if view does not exists yet
	IF OBJECT_ID(@ViewSchemaAndName) IS NULL
	BEGIN
		SET @DropAndCreateView = 1
	END
	ELSE
	BEGIN
		BEGIN TRY
			-- Upddate the view
			SET @SqlQuery =  N'
				ALTER VIEW  ' + @ViewSchemaAndName + '
				WITH SCHEMABINDING
				AS
				SELECT ' + @Columns + ' 
				FROM ' + @NewTableSchemaAndName + ''
			EXEC sp_executesql @SqlQuery;
		END TRY  
		BEGIN CATCH  
			SET @DropAndCreateView = 1
		END CATCH
	END

	IF @DropAndCreateView = 1
	BEGIN
		SET @SqlQuery = N'DROP VIEW IF EXISTS ' + @ViewSchemaAndName
		EXEC sp_executesql @SqlQuery

		SET @SqlQuery =  N'
				CREATE VIEW ' + @ViewSchemaAndName + '
				WITH SCHEMABINDING
				AS
				SELECT ' + @Columns + ' 
				FROM ' + @NewTableSchemaAndName + ''
		EXEC sp_executesql @SqlQuery; 
	END

    FETCH NEXT FROM tableCursor INTO @CurrentTableName;
END

CLOSE tableCursor;
DEALLOCATE tableCursor;

-- Clean up the temporary table
DROP TABLE #TableList;