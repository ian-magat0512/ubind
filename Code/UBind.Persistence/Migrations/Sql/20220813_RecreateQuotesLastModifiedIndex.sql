IF EXISTS(SELECT *FROM sys.indexes WHERE name='IX_Quotes_PolicyReadModels_LastUpdated' AND object_id = OBJECT_ID('Quotes'))
BEGIN
    DROP index [dbo].[Quotes].[IX_Quotes_PolicyReadModels_LastUpdated];
END;

DECLARE @online VARCHAR(3) = CASE
	WHEN (CAST(SERVERPROPERTY ('edition') AS NVARCHAR(128)) LIKE 'Enterprise Edition%') THEN 'ON'
	ELSE 'OFF'
END;

DECLARE @createIndex NVARCHAR(1000) = 'CREATE NONCLUSTERED INDEX [IX_Quotes_PolicyReadModels_LastUpdated] ON [dbo].[Quotes]
(
	[LastModifiedTicksSinceEpoch] ASC
)
INCLUDE([PolicyId],
[Type],
[QuoteState],
[LatestFormData],
[QuoteNumber],
[CreatedTicksSinceEpoch],
[LastModifiedByUserTicksSinceEpoch],
[ExpiryTicksSinceEpoch],
[IsDiscarded],
[IsTestData]) 
WHERE ([QuoteNumber] IS NOT NULL)
WITH (ONLINE = ' + @online + ')';

EXEC(@createIndex);
