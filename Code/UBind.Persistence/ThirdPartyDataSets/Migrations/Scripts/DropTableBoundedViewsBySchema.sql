DECLARE @Schema NVARCHAR(128)
DECLARE @View NVARCHAR(128)
DECLARE @SqlQuery NVARCHAR(MAX);

-- Loop through these three Schemas
DECLARE schemaCursor CURSOR FOR
	SELECT SchamaName = name
	FROM sys.schemas
	WHERE name IN ('RedBook', 'Nfid', 'Gnaf')
	
OPEN schemaCursor

FETCH NEXT FROM schemaCursor INTO @Schema

WHILE @@FETCH_STATUS = 0
BEGIN

	DECLARE viewCursor CURSOR FOR
		SELECT name
		FROM sys.views
		WHERE schema_id = SCHEMA_ID(@Schema)

	OPEN viewCursor

	FETCH NEXT FROM viewCursor INTO @View
	WHILE @@FETCH_STATUS = 0 
	BEGIN
		SET @SqlQuery =  N'DROP VIEW IF EXISTS ' + @Schema + '.' + @View + ''
		EXEC sp_executesql @SqlQuery;

		FETCH NEXT FROM viewCursor INTO @View
	END

	CLOSE viewCursor
	DEALLOCATE viewCursor


	FETCH NEXT FROM schemaCursor INTO @Schema
END

CLOSE schemaCursor
DEALLOCATE schemaCursor
