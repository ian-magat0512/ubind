PRINT N'-- SETTING FUNCTIONS --';

IF OBJECT_ID('SP_UpdateTenantNewId') IS NOT NULL 
	BEGIN
		DROP PROCEDURE SP_UpdateTenantNewId
	END

IF OBJECT_ID('SP_UpdateProductNewId') IS NOT NULL 
	BEGIN
		DROP PROCEDURE SP_UpdateProductNewId
	END

IF OBJECT_ID('SP_UpdateProductNewId') IS NULL
BEGIN
	DECLARE @sql3 NVARCHAR (3500);
	SET @sql3 = 
			'Create PROCEDURE SP_UpdateProductNewId 
			@tableName varchar(255),
			@pageSize int,
			@tmpTenantId varchar(255),
			@tmpProductId varchar(255),
			@tmpProductNewId UNIQUEIDENTIFIER
	AS
	BEGIN
		declare @sql nvarchar (1500);
		declare @productIdCol varchar(250) = IIF(COL_LENGTH(''dbo.''+@tableName, ''Product_Id'') IS NOT NULL, ''Product_Id'', ''ProductId'');
		declare @tenantIdCol varchar(250) = IIF(COL_LENGTH(''dbo.''+@tableName, ''Tenant_Id'') IS NOT NULL, ''Tenant_Id'', ''TenantId'');

		-- Set the Product New ID
		IF @tmpProductId IS NOT NULL AND COL_LENGTH(''dbo.''+@tableName,''ProductNewId'') IS NOT NULL
			begin
				PRINT N''--''+@tableName+'' was assigned with Product New ID # ''+CAST(@tmpProductNewId AS varchar(255));
				set @sql = N''UPDATE TOP (''+CAST(@pageSize AS varchar(255))+'') ''+@tableName+'' SET ProductNewId = @tmpProductNewId WHERE ''+@productIdCol+'' = @tmpProductId and ''+@tenantIdCol+'' = @tmpTenantId and (ProductNewId is null or ProductNewId = ''''00000000-0000-0000-0000-000000000000'''')'';

				exec sp_executesql 
					@sql,
					N''@tmpProductNewId UNIQUEIDENTIFIER, @tmpProductId varchar(255), @tmpTenantId varchar(255)'',
					@tmpProductNewId,
					@tmpProductId,
					@tmpTenantId;
			end
	END'
	EXEC sp_executesql @sql3;
END

-- This is a dynamic update --
IF OBJECT_ID('SP_UpdateTenantNewId') IS NULL
BEGIN
	DECLARE @sql4 NVARCHAR (3500);
	SET @sql4 = 
			'Create PROCEDURE SP_UpdateTenantNewId 
			@tableName varchar(255),
			@pageSize int,
			@tmpTenantId varchar(255),
			@tmpTenantNewId UNIQUEIDENTIFIER
	AS
	BEGIN
		declare @sql nvarchar (1500);
		declare @tenantIdCol varchar(250) = IIF(COL_LENGTH(''dbo.''+@tableName, ''Tenant_Id'') IS NOT NULL, ''Tenant_Id'', ''TenantId'');

		-- Set the Tenant New ID
		IF @tmpTenantId IS NOT NULL AND COL_LENGTH(''dbo.''+@tableName, ''TenantNewId'') IS NOT NULL
			begin
				PRINT N''-- ''+@tableName+'' was assigned Tenant New ID # ''+CAST(@tmpTenantNewId AS varchar(255));
				set @sql = N''UPDATE TOP (''+CAST(@pageSize AS varchar(255))+'') ''+@tableName+'' SET TenantNewId = @tmpTenantNewId WHERE ''+@tenantIdCol+'' = @tmpTenantId and (TenantNewId is null or TenantNewId = ''''00000000-0000-0000-0000-000000000000'''')'';

				exec sp_executesql 
					@sql,
					N''@tmpTenantNewId UNIQUEIDENTIFIER, @tmpTenantId varchar(255)'',
					@tmpTenantNewId,
					@tmpTenantId;
			end
	END'
	EXEC sp_executesql @sql4;
END