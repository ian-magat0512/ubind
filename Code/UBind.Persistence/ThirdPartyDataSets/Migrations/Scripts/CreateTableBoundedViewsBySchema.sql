DECLARE @Schema NVARCHAR(128)
DECLARE @TableName NVARCHAR(128)
DECLARE @ViewSchemaAndName NVARCHAR(128)
DECLARE @Columns NVARCHAR(MAX);
DECLARE @SqlQuery NVARCHAR(MAX);

-- Loop through these currently available Schemas
DECLARE schemaCursor CURSOR FOR
	SELECT SchamaName = name
	FROM sys.schemas
	WHERE name IN ('RedBook', 'Nfid', 'Gnaf')
	
OPEN schemaCursor

FETCH NEXT FROM schemaCursor INTO @Schema

WHILE @@FETCH_STATUS = 0
BEGIN

	-- Create view for each table in the schema
	DECLARE tableCursor CURSOR FOR
		SELECT ViewName = name
		FROM sys.tables
		WHERE schema_id = SCHEMA_ID(@Schema)

	OPEN tableCursor

	FETCH NEXT FROM tableCursor INTO @TableName
	WHILE @@FETCH_STATUS = 0 
	BEGIN
		-- Make sure to remove the table index prefix if the table has any
		SET @ViewSchemaAndName = @Schema + '.'
						+ IIF(@TableName LIKE '%[_][0-9][0-9]', LEFT(@TableName, LEN(@TableName) - 3), @TableName)
						+ '_View'

		-- Get the comma separated column names of the current table
		SELECT @Columns = STRING_AGG('[' + name + ']', ', ')
		FROM sys.columns
		WHERE object_id = OBJECT_ID(@Schema + '.' + @TableName)

		-- Create the view if it doesn't exist
		IF OBJECT_ID(@ViewSchemaAndName) IS NULL
		BEGIN
			SET @SqlQuery =  N'
					CREATE VIEW ' + @ViewSchemaAndName + '
					WITH SCHEMABINDING
					AS
					SELECT ' + @Columns + '
					FROM ' + @Schema + '.' + @TableName + ''
			EXEC sp_executesql @SqlQuery;
		END

		FETCH NEXT FROM tableCursor INTO @TableName
	END

	CLOSE tableCursor
	DEALLOCATE tableCursor

	FETCH NEXT FROM schemaCursor INTO @Schema
END

CLOSE schemaCursor
DEALLOCATE schemaCursor