DECLARE @Schema NVARCHAR(MAX);
DECLARE @TableIndex NVARCHAR(MAX);
DECLARE @CreateVEFamilyTable NVARCHAR(MAX);
DECLARE @CreateVEMakeTable NVARCHAR(MAX);
DECLARE @CreateVEVehicleTable NVARCHAR(MAX);
DECLARE @CreateVEYearTable NVARCHAR(MAX);

SET @Schema = '{0}'
SET @TableIndex = '{1}'
IF @TableIndex != ''
BEGIN
	SET @TableIndex = '_' + @TableIndex
END
SET @CreateVEFamilyTable = N'
	CREATE TABLE [' + @Schema + '].[VEFamily' + @TableIndex + '](
		[MakeCode] [varchar](50) NULL,
		[FamilyCode] [varchar](50) NULL,
		[VehicleTypeCode] [varchar](50) NULL,
		[Description] [varchar](50) NULL,
		[StartYear] [int] NULL,
		[LatestYear] [int] NULL
	) ;
';
EXEC sp_executesql @CreateVEFamilyTable;

SET @CreateVEMakeTable = N'
	CREATE TABLE [' + @Schema + '].[VEMake' + @TableIndex + '](
		[MakeCode] [varchar](50) NULL,
		[Description] [varchar](50) NULL,
		[StartYear] [int] NULL,
		[LatestYear] [int] NULL
	) ;
';
EXEC sp_executesql @CreateVEMakeTable;

SET @CreateVEVehicleTable = N'
	CREATE TABLE [' + @Schema + '].[VEVehicle'  + @TableIndex + '](
		[VehicleKey] [varchar](100) NOT NULL,
		[MakeCode] [varchar](100) NULL,
		[FamilyCode] [varchar](100) NULL,
		[VehicleTypeCode] [varchar](100) NULL,
		[YearGroup] int  NULL,
		[MonthGroup] int   NULL,
		[SequenceNum] int NULL,
		[Description] [varchar](200) NULL,
		[CurrentRelease] [varchar](200) NULL,
		[ImportFlag] [varchar](200) NULL,
		[LimitedEdition] [varchar](200) NULL,
		[Series] [varchar](200) NULL,
		[SeriesModelYear] [varchar](200) NULL,
		[BadgeDescription] [varchar](200) NULL,
		[BadgeSecondaryDescription] [varchar](200) NULL,
		[BodyStyleDescription] [varchar](200) NULL,
		[BodyConfigDescription] [varchar](200) NULL,
		[WheelBaseConfig] [varchar](200) NULL,
		[Roofline] [varchar](200) NULL,
		[ExtraIdentification] [varchar](200) NULL,
		[DriveDescription] [varchar](200) NULL,
		[GearTypeDescription] [varchar](200) NULL,
		[GearLocationDescription] [varchar](200) NULL,
		[GearNum] [varchar](200) NULL,
		[DoorNum] [varchar](200) NULL,
		[EngineSize] [varchar](200) NULL,
		[EngineDescription] [varchar](200) NULL,
		[Cylinders] [varchar](200) NULL,
		[FuelTypeDescription] [varchar](200) NULL,
		[InductionDescription] [varchar](200) NULL,
		[OptionCategory] [varchar](200) NULL,
		[TradeMin] [varchar](200) NULL,
		[TradeMax] [varchar](200) NULL,
		[PrivateMin] [varchar](200) NULL,
		[PrivateMax] [varchar](200) NULL,
		[IsPPlateApproved] [varchar](5) NULL,
		[VFactsClass] [varchar](100) NULL,
		[VFactsSegment] [varchar](100) NULL,
		[VFactsPrice] [varchar](100) NULL,
		[HighPoweredVehicle] [varchar](100) NULL
	) ;
';
EXEC sp_executesql @CreateVEVehicleTable;

SET @CreateVEYearTable = N'
	CREATE TABLE [' + @Schema + '].[VEYear' + @TableIndex + '](
		[MakeCode] [varchar](50) NULL,
		[FamilyCode] [varchar](50) NULL,
		[VehicleTypeCode] [varchar](50) NULL,
		[YearGroup] [int] NULL,
		[MonthGroup] [int] NULL,
		[Description] [varchar](50) NULL
	)  ;
';
EXEC sp_executesql @CreateVEYearTable;