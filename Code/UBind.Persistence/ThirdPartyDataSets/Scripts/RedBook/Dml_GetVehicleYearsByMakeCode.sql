SELECT YearGroup
FROM [RedBook].[VEYear_View]
WHERE [MakeCode] = @MakeCode
GROUP BY YearGroup
ORDER BY YearGroup DESC