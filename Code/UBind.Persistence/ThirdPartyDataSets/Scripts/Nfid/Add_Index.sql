DECLARE @Schema NVARCHAR(128);
DECLARE @TableIndex NVARCHAR(3);
DECLARE @CreateIndex NVARCHAR(MAX);

SET @Schema = '{0}'
SET @TableIndex = '{1}'
IF @TableIndex != ''
	BEGIN
		SET @TableIndex = '_' + @TableIndex
	END

IF NOT EXISTS (
    SELECT *
    FROM sys.indexes
    WHERE name = 'Index1_Stage6' + @TableIndex
        AND object_id = OBJECT_ID(@Schema + 'Nfid.Stage6' + @TableIndex))
BEGIN
	
SET @CreateIndex = N'
    CREATE UNIQUE CLUSTERED INDEX Index1_Stage6' + @TableIndex + ' ON [' + @Schema + '].[Stage6' + @TableIndex + '] (
        [Gnaf_Pid] ASC)
    WITH (
        SORT_IN_TEMPDB = OFF
        ,DROP_EXISTING = OFF
        ,ONLINE = OFF
    ) ON [PRIMARY];
';
EXEC sp_executesql @CreateIndex;

END
