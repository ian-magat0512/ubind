SELECT
	[MakeCode],
	[FamilyCode],
	[FamilyDescription],
	[VehicleTypeCode],
	[StartYear],
	[LatestYear]
FROM [GlassGuide].[GG_Family_View]
WHERE [MakeCode] = @MakeCode
ORDER BY [FamilyDescription]