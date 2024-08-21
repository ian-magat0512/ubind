DECLARE @Schema NVARCHAR(128);
DECLARE @TableIndex NVARCHAR(3);
DECLARE @CreateStage6Table NVARCHAR(MAX);

SET @Schema = '{0}'
SET @TableIndex = '{1}'
IF @TableIndex != ''
	BEGIN
		SET @TableIndex = '_' + @TableIndex
	END

SET @CreateStage6Table = N'
    CREATE TABLE [' + @Schema + '].[Stage6' + @TableIndex + ']
        (
            [Gnaf_Pid] [varchar](16) NULL,
            [Flat_Number] [varchar](8) NULL,
            [Number_First] [varchar](8) NULL,
            [Number_Last] [varchar](8) NULL,
            [Street_Name] [varchar](50) NULL,
            [Street_Type] [varchar](16) NULL,
            [Locality_Name] [varchar](50) NULL,
            [State] [varchar](4) NULL,
            [Postcode] [varchar](8) NULL,
            [Elev] [varchar](16) NULL,
            [Flood_Depth_20] [varchar](8) NULL,
            [Flood_Depth_50] [varchar](8) NULL,
            [Flood_Depth_100] [varchar](8) NULL,
            [Flood_Depth_Extreme] [varchar](8) NULL,
            [Flood_Ari_Gl] [varchar](8) NULL,
            [Flood_Ari_Gl1M] [varchar](8) NULL,
            [Flood_Ari_Gl2M] [varchar](8) NULL,
            [Notes_Id] [varchar](4) NULL,
            [Level_Nfid_Id] [varchar](4) NULL,
            [Level_Fez_Id] [varchar](4) NULL,
            [Flood_Code] [varchar](8) NULL,
            [Floor_Height] [varchar](8) NULL,
            [Latitude] [varchar](16) NULL,
            [Longitude] [varchar](16) NULL,
        ) ;
    ';

EXEC sp_executesql @CreateStage6Table;