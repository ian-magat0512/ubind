DECLARE @Schema NVARCHAR(MAX);
DECLARE @TableIndex NVARCHAR(MAX);
DECLARE @CreateIndex NVARCHAR(MAX);

SET @Schema = '{0}'
SET @TableIndex = '{1}'

IF @TableIndex != ''
BEGIN
	SET @TableIndex = '_' + @TableIndex
END

IF NOT EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'Index1_VEVehicle' + @TableIndex
			AND object_id = OBJECT_ID('' + @Schema + '.VEVehicle' + @TableIndex + '')
		)
BEGIN

SET @CreateIndex = N'
CREATE NONCLUSTERED INDEX [Index1_CommonCarQuery_VEVehicle_' + @TableIndex + '] ON [' + @Schema + '].[VEVehicle' + @TableIndex + ']
(
	[MakeCode] ASC,
	[YearGroup] ASC,
	[FamilyCode] ASC,
	[Description] ASC,
	[VehicleKey] ASC
)

CREATE NONCLUSTERED INDEX [Index2_BadgeQuery_VEVehicle_' + @TableIndex + '] ON [' + @Schema + '].[VEVehicle' + @TableIndex + ']
(
	[MakeCode] ASC,
	[YearGroup] ASC,
	[FamilyCode] ASC,
	[BadgeDescription] ASC
)

CREATE NONCLUSTERED INDEX [Index3_GearTypeQuery_VEVehicle_' + @TableIndex + '] ON [' + @Schema + '].[VEVehicle' + @TableIndex + ']
(
	[MakeCode] ASC,
	[YearGroup] ASC,
	[FamilyCode] ASC,
	[GearTypeDescription] ASC
)

CREATE NONCLUSTERED INDEX [Index4_BodyStyleQuery_VEVehicle_' + @TableIndex + '] ON [' + @Schema + '].[VEVehicle' + @TableIndex + ']
(
	[MakeCode] ASC,
	[YearGroup] ASC,
	[FamilyCode] ASC,
	[BodyStyleDescription] ASC
)
';
EXEC sp_executesql @CreateIndex;

END