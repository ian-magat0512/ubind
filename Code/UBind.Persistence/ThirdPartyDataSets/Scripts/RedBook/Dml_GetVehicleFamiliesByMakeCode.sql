SELECT [MakeCode]
	,[FamilyCode]
	,[VehicleTypeCode]
	,[Description]
	,[StartYear]
	,[LatestYear]
FROM [RedBook].[VEFamily_View]
WHERE [MakeCode] = @MakeCode
ORDER BY Description