IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'AK_RelationshipTenantFromEntityIndex'
			AND object_id = OBJECT_ID('Relationships')
		)
BEGIN
	DROP index [dbo].[Relationships].[AK_RelationshipTenantFromEntityIndex];
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'AK_RelationshipTenantToEntityIndex'
			AND object_id = OBJECT_ID('Relationships')
		)
BEGIN
	DROP index [dbo].[Relationships].[AK_RelationshipTenantToEntityIndex];
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'AK_RelationshipTenantFromTypeIndex'
			AND object_id = OBJECT_ID('Relationships')
		)
BEGIN
	DROP index [dbo].[Relationships].[AK_RelationshipTenantFromTypeIndex];
END

IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'AK_RelationshipTenantToTypeIndex'
			AND object_id = OBJECT_ID('Relationships')
		)
BEGIN
	DROP index [dbo].[Relationships].[AK_RelationshipTenantToTypeIndex];
END