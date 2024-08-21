DECLARE @TableIndex NVARCHAR(MAX);
DECLARE @Columns NVARCHAR(MAX);
DECLARE @SqlQuery NVARCHAR(MAX);
DECLARE @DropViewQuery NVARCHAR(MAX);
DECLARE @Schema NVARCHAR(MAX);

SET @Schema = '{0}'
SET @TableIndex = '{1}'

-- Create a table to store the list of tables
CREATE TABLE #TableList
(
    TableName NVARCHAR(MAX)
);

-- Insert the fixed list of tables into the temporary table
IF @TableIndex = ''
	BEGIN
		-- Insert the fixed list of tables into the temporary table
		INSERT INTO #TableList (TableName)
		SELECT t.name
		FROM sys.tables t
		INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
		WHERE s.name = @Schema and t.name NOT LIKE '%[_][0-9][0-9]';
	END
ELSE
	BEGIN
		-- Insert the fixed list of tables into the temporary table
		INSERT INTO #TableList (TableName)
		SELECT LEFT(t.name, LEN(t.name) - 3) AS name
		FROM sys.tables t
		INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
		WHERE s.name = @Schema and t.name LIKE '%[_]' + @TableIndex;
	END

-- Create a cursor to loop through the list of table names
DECLARE tableCursor CURSOR FOR
SELECT TableName
FROM #TableList;

-- Declare variables to store the current table name
DECLARE @CurrentTableName NVARCHAR(MAX);

-- Open the cursor
OPEN tableCursor;

-- Fetch the first record
FETCH NEXT FROM tableCursor INTO @CurrentTableName;

-- Start the loop
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Drop the view if it exists
    SET @DropViewQuery = N'DROP VIEW IF EXISTS [GlassGuide].[' + @CurrentTableName + '_View]';
    EXEC sp_executesql @DropViewQuery;

    -- Get the columns for the current table and format them with brackets
    SET @Columns = (
        SELECT STRING_AGG('[' + name + ']', ', ') AS COLUMN_NAME
        FROM sys.columns
        WHERE object_id = OBJECT_ID(@Schema + '.' + @CurrentTableName + '_' + @TableIndex)
    );

    -- Create the view
    SET @SqlQuery =  N'
        CREATE VIEW [GlassGuide].[' + @CurrentTableName + '_View]
        WITH SCHEMABINDING
        AS
        SELECT ' + @Columns + ' FROM [' + @Schema + '].[' + @CurrentTableName + '_' + @TableIndex + '];';
    EXEC sp_executesql @SqlQuery;

    -- Fetch the next record
    FETCH NEXT FROM tableCursor INTO @CurrentTableName;
END

-- Close and deallocate the cursor
CLOSE tableCursor;
DEALLOCATE tableCursor;

-- Clean up the temporary table
DROP TABLE #TableList;