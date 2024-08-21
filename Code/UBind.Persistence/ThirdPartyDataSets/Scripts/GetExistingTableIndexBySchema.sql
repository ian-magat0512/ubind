-- Set the @Schema value using c# string format
-- This will check for the existence of the latest table with the
-- given schema and will return a two-character numeric suffix
DECLARE @Schema NVARCHAR(128) = '{0}'
DECLARE @TableName NVARCHAR(128)

SELECT TOP 1 
	@TableName = name
FROM sys.tables
WHERE schema_id = SCHEMA_ID(@Schema)
ORDER BY create_date DESC

IF @TableName LIKE '%[_][0-9][0-9]'
BEGIN
    -- Extract the two-character numeric suffix
    SELECT Suffix = RIGHT(@TableName, 2)
END
ELSE
BEGIN
    -- No suffix or no matching table, so no suffix to retrieve
    SELECT Suffix = NULL
END
