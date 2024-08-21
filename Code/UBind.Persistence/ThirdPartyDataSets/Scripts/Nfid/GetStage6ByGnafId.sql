SELECT [Gnaf_Pid] AS GnafAddressId
      , [Elev] AS Elevation
      , [Flood_Depth_20] AS FloodDepth20
      , [Flood_Depth_50] AS FloodDepth50
      , [Flood_Depth_100] AS FloodDepth100
      , [Flood_Depth_Extreme] AS FloodDepthExtreme
      , [Flood_Ari_Gl] AS FloodAriGl
      , [Flood_Ari_Gl1M] AS FloodAriGl1M
      , [Flood_Ari_Gl2M] AS FloodAriGl2M
      , [Notes_Id] AS NotesId
      , [Level_Nfid_Id] AS LevelNfidId
      , [Level_Fez_Id] AS LevelFezId
      , [Flood_Code] AS floodCode
      , [Floor_Height] AS floodCode
      , [Latitude] AS latitude
      , [Longitude] AS longitude
FROM [Nfid].[Stage6_View]
WHERE [Gnaf_Pid] = @GnafId