DECLARE @Schema NVARCHAR(MAX);
DECLARE @TableIndex NVARCHAR(MAX);
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
		WHERE name = 'Index1_GG_Vehicle' + @TableIndex
			AND object_id = OBJECT_ID('' + @Schema + '.GG_Vehicle' + @TableIndex + '')
		)
BEGIN

SET @CreateIndex = N'
	CREATE NONCLUSTERED INDEX [Index1_GG_Vehicle' + @TableIndex + '] ON [' + @Schema + '].[GG_Vehicle' + @TableIndex + '] (
		[GlassCode] ASC
		,[Nvic] ASC
		,[Year] ASC
		,[MakeCode] ASC
		,[FamilyCode] ASC
	) WITH (
		SORT_IN_TEMPDB = OFF
		,DROP_EXISTING = OFF
		,ONLINE = OFF
	) ON [PRIMARY];
';
EXEC sp_executesql @CreateIndex;

END