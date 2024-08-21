SELECT
	[MakeCode],
	[MakeDescription],
	[StartYear],
	[LatestYear]
FROM [GlassGuide].[GG_Make_View]
WHERE @Year BETWEEN [StartYear] AND [LatestYear]
ORDER BY [MakeDescription]