SELECT MakeCode
	,Description
	,StartYear
	,LatestYear
FROM RedBook.VEMake_View
WHERE @Year BETWEEN StartYear
		AND LatestYear
ORDER BY Description
