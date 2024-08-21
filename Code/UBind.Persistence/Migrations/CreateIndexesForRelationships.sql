IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'AK_RelationshipTenantFromEntityIndex'
			AND object_id = OBJECT_ID('Relationships')
		)
BEGIN
	CREATE NONCLUSTERED INDEX AK_RelationshipTenantFromEntityIndex ON [Relationships] (
		[TenantId] ASC
		,[FromEntityId] ASC )
END

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'AK_RelationshipTenantToEntityIndex'
			AND object_id = OBJECT_ID('Relationships')
		)
BEGIN
	CREATE NONCLUSTERED INDEX AK_RelationshipTenantToEntityIndex ON [Relationships] (
		[TenantId] ASC
		,[ToEntityId] ASC )
END

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'AK_RelationshipTenantFromTypeIndex'
			AND object_id = OBJECT_ID('Relationships')
		)
BEGIN
	CREATE NONCLUSTERED INDEX AK_RelationshipTenantFromTypeIndex ON [Relationships] (
		[TenantId] ASC
		, [Type] ASC
		, [FromEntityType] ASC
		, [FromEntityId] ASC )
END

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'AK_RelationshipTenantToTypeIndex'
			AND object_id = OBJECT_ID('Relationships')
		)
BEGIN
	CREATE NONCLUSTERED INDEX AK_RelationshipTenantToTypeIndex ON [Relationships] (
		[TenantId] ASC
		, [Type] ASC
		, [ToEntityType] ASC
		, [ToEntityId] ASC )
END