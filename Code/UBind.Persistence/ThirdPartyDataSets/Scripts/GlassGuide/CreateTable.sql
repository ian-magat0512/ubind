DECLARE @Schema NVARCHAR(MAX);
DECLARE @TableIndex NVARCHAR(MAX);
DECLARE @CreateVehicleTable NVARCHAR(MAX);
DECLARE @CreateMakeTable NVARCHAR(MAX);
DECLARE @CreateFamilyTable NVARCHAR(MAX);
DECLARE @CreateYearTable NVARCHAR(MAX);

SET @Schema = '{0}'
SET @TableIndex = '{1}'
IF @TableIndex != ''
BEGIN
	SET @TableIndex = '_' + @TableIndex
END

SET @CreateVehicleTable = N'
	CREATE TABLE [' + @Schema + '].[GG_Vehicle'  + @TableIndex + '](
		[GlassCode] [varchar](50) NOT NULL,
		[Nvic] [varchar](50) NOT NULL,
		[DataCode] [varchar](50) NOT NULL,
		[VehicleTypeCode] [varchar](50) NOT NULL,
		[VehicleTypeDescription] [varchar](100) NOT NULL,
		[Year] int NULL,
		[Month] int NULL,
		[MakeCode] [varchar](50) NULL,
		[MakeDescription] [varchar](100) NULL,
		[FamilyCode] [varchar](50) NULL,
		[FamilyDescription] [varchar](100) NULL,
		[Variant] [varchar](100) NULL,
		[Series] [varchar](100) NULL,
		[Body] [varchar](100) NULL,
		[EngineType] [varchar](100) NULL,
		[EngineVolume] int NULL,
		[EngineSize] [varchar](50) NULL,
		[Transmission] [varchar](100) NULL,
		[Cylinders] [varchar](50) NULL,
		[LowTradeValue] int NULL,
		[HighTradeValue] int NULL,
		[DealerUsedValue] int NULL,
		[BelowAverageValue] int NULL,
		[AverageValue] int NULL,
		[AboveAverageValue] int NULL,
		[NewPrice] int NULL,
		[UsedPrice] int NULL,
	) ;
';
EXEC sp_executesql @CreateVehicleTable;

SET @CreateMakeTable = N'
	CREATE TABLE [' + @Schema + '].[GG_Make'  + @TableIndex + '](
		[MakeCode] [varchar](50) NULL,
		[MakeDescription] [varchar](100) NULL,
		[StartYear] [int] NULL,
		[LatestYear] [int] NULL,
	) ;
';
EXEC sp_executesql @CreateMakeTable;

SET @CreateFamilyTable = N'
	CREATE TABLE [' + @Schema + '].[GG_Family'  + @TableIndex + '](
		[MakeCode] [varchar](50) NULL,
		[FamilyCode] [varchar](50) NULL,
		[FamilyDescription] [varchar](100) NULL,
		[VehicleTypeCode] [varchar](50) NULL,
		[StartYear] [int] NULL,
		[LatestYear] [int] NULL,
	) ;
';
EXEC sp_executesql @CreateFamilyTable;

SET @CreateYearTable = N'
	CREATE TABLE [' + @Schema + '].[GG_Year'  + @TableIndex + '](
		[MakeCode] [varchar](50) NULL,
		[FamilyCode] [varchar](50) NULL,
		[Year] [int] NULL,
	) ;
';
EXEC sp_executesql @CreateYearTable;