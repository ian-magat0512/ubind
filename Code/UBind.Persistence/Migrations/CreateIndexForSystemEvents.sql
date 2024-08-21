DECLARE @online VARCHAR(3) = CASE
	WHEN (CAST(SERVERPROPERTY ('edition') AS NVARCHAR(128)) LIKE 'Enterprise Edition%') THEN 'ON'
	ELSE 'OFF'
END;

IF NOT EXISTS (
	SELECT *
	FROM sys.indexes
	WHERE name = 'IX_SystemEvents_ExpiryTicksSinceEpoch_EventType'
		AND object_id = OBJECT_ID('dbo.SystemEvents')
)
BEGIN
	DECLARE @createIndex NVARCHAR(1000) =
		'CREATE NONCLUSTERED INDEX IX_SystemEvents_ExpiryTicksSinceEpoch_EventType ON dbo.SystemEvents (ExpiryTicksSinceEpoch, EventType)
		WITH (ONLINE = ' + @online + ')';
	EXEC(@createIndex);
END
