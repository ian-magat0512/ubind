IF EXISTS(SELECT *FROM sys.indexes WHERE name='IX_PolicyReadModels_LastModifiedDate_CreatedDate' AND object_id = OBJECT_ID('PolicyReadModels'))
BEGIN
    DROP index [dbo].[PolicyReadModels].IX_PolicyReadModels_LastModifiedDate_CreatedDate;
END;

DECLARE @online VARCHAR(3) = CASE
    WHEN (CAST(SERVERPROPERTY ('edition') AS NVARCHAR(128)) LIKE 'Enterprise Edition%') THEN 'ON'
    ELSE 'OFF'
END;

DECLARE @createIndex NVARCHAR(1000) = 'CREATE NONCLUSTERED INDEX [IX_PolicyReadModels_LastModifiedDate_CreatedDate] ON [dbo].[PolicyReadModels]
(
    [CreatedTicksSinceEpoch] DESC,
    [LastModifiedTicksSinceEpoch] ASC
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
[IssuedTicksSinceEpoch],
[PolicyNumber],
[InceptionTicksSinceEpoch],
[ExpiryTicksSinceEpoch],
[CancellationEffectiveTicksSinceEpoch],
[SerializedCalculationResult],
[LastModifiedByUserTicksSinceEpoch],
[PolicyState],
[Environment]) 
WHERE ([PolicyNumber] IS NOT NULL AND [InceptionTicksSinceEpoch] > 0)
WITH (ONLINE = ' + @online + ')';

EXEC(@createIndex);
