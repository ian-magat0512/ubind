IF EXISTS(SELECT * FROM sys.indexes WHERE name='AK_AggregateIdTenantIdAggregateTypeIndex' AND object_id = OBJECT_ID('EventRecordWithGuidIds'))
BEGIN
    DROP index [dbo].[EventRecordWithGuidIds].[AK_AggregateIdTenantIdAggregateTypeIndex];
END;


DECLARE @createIndex NVARCHAR(1000) = 'CREATE NONCLUSTERED INDEX [AK_AggregateIdTenantIdAggregateTypeIndex] ON [dbo].[EventRecordWithGuidIds]
(
	[TenantId],[AggregateId],[AggregateType]
)';

EXEC(@createIndex);

IF EXISTS(SELECT * FROM sys.indexes WHERE name='AK_TenantIdAggregateTypeIndex' AND object_id = OBJECT_ID('EventRecordWithGuidIds'))
BEGIN
    DROP index [dbo].[EventRecordWithGuidIds].[AK_TenantIdAggregateTypeIndex];
END;


DECLARE @createIndex2 NVARCHAR(1000) = 'CREATE NONCLUSTERED INDEX [AK_TenantIdAggregateTypeIndex] ON [dbo].[EventRecordWithGuidIds]
(
	[TenantId],[AggregateType]
)';

EXEC(@createIndex2);
