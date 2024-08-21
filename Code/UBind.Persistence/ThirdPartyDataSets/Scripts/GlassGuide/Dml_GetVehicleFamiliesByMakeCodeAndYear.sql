SELECT
	[MakeCode],
	[FamilyCode],
	[FamilyDescription],
	[VehicleTypeCode],
	[StartYear],
	[LatestYear]
FROM [GlassGuide].[GG_Family_View]
WHERE [MakeCode] = @MakeCode
	AND @Year BETWEEN [StartYear] AND [LatestYear]
ORDER BY [FamilyDescription]