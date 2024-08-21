IF EXISTS(SELECT *FROM sys.indexes WHERE name='IX_LoginAttemptResults_TenantId_EmailAddress_OrganisationId' AND object_id = OBJECT_ID('LoginAttemptResults'))
BEGIN
    DROP index [dbo].LoginAttemptResults.IX_LoginAttemptResults_TenantId_EmailAddress_OrganisationId;
END;

DECLARE @online VARCHAR(3) = CASE
	WHEN (CAST(SERVERPROPERTY ('edition') AS NVARCHAR(128)) LIKE 'Enterprise Edition%') THEN 'ON'
	ELSE 'OFF'
END;

DECLARE @createIndex NVARCHAR(1000) = 'CREATE NONCLUSTERED INDEX [IX_LoginAttemptResults_TenantId_EmailAddress_OrganisationId] ON [dbo].[LoginAttemptResults]
(
	[TenantId], [OrganisationId], [EmailAddress] DESC
)
INCLUDE(
[Id],
[Succeeded],
[Error],
[ClientIpAddress],
[CreationTimeInTicksSinceEpoch])
WHERE ([EmailAddress] IS NOT NULL)
WITH (ONLINE = ' + @online + ')';

EXEC(@createIndex);
