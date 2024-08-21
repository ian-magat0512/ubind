SELECT [MakeCode]
	,[FamilyCode]
	,[VehicleTypeCode]
	,[Description]
	,[StartYear]
	,[LatestYear]
FROM [RedBook].[VEFamily_View]
WHERE [MakeCode] = @MakeCode
	AND @Year BETWEEN StartYear
		AND LatestYear
ORDER BY Description