IF EXISTS(SELECT * FROM sys.indexes WHERE name='AK_AggregateIdTenantIdAggregateTypeIndex' AND object_id = OBJECT_ID('EventRecordWithGuidIds'))
BEGIN
    DROP index [dbo].[EventRecordWithGuidIds].[AK_AggregateIdTenantIdAggregateTypeIndex];
END;

IF EXISTS(SELECT * FROM sys.indexes WHERE name='AK_TenantIdAggregateTypeIndex' AND object_id = OBJECT_ID('EventRecordWithGuidIds'))
BEGIN
    DROP index [dbo].[EventRecordWithGuidIds].[AK_TenantIdAggregateTypeIndex];
END;
