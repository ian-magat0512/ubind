IF EXISTS(SELECT *FROM sys.indexes WHERE name='IX_PolicyReadModels_CreationDate' AND object_id = OBJECT_ID('PolicyReadModels'))
BEGIN
    DROP index [dbo].[PolicyReadModels].[IX_PolicyReadModels_CreationDate];
END;

DECLARE @online VARCHAR(3) = CASE
	WHEN (CAST(SERVERPROPERTY ('edition') AS NVARCHAR(128)) LIKE 'Enterprise Edition%') THEN 'ON'
	ELSE 'OFF'
END;

DECLARE @createIndex NVARCHAR(1000) = 'CREATE NONCLUSTERED INDEX [IX_PolicyReadModels_CreationDate] ON [dbo].[PolicyReadModels]
(
	[CreationTimeInTicksSinceEpoch] DESC
)
INCLUDE(
[Id],
[TenantId],
[ProductId],
[IsTestData],
[QuoteId],
[OwnerUserId],
[CustomerId],
[CustomerFullName],
[PolicyIssueTimeInTicksSinceEpoch],
[PolicyNumber],
[InceptionDateAsDateTime],
[ExpiryDateAsDateTime],
[InceptionTimeAsTicksSinceEpoch],
[ExpiryTimeAsTicksSinceEpoch],
[CancellationEffectiveTimeInTicksSinceEpoch],
[SerializedCalculationResult],
[LastUpdatedTicksSinceEpoch]) 
WHERE ([PolicyNumber] IS NOT NULL)
WITH (ONLINE = ' + @online + ')';

EXEC(@createIndex);
