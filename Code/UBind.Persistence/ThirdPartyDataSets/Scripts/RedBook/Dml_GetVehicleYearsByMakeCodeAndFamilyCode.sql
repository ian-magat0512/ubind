SELECT YearGroup
FROM [RedBook].[VEYear_View]
WHERE [MakeCode] = @MakeCode and
       FamilyCode = @FamilyCode
GROUP BY YearGroup
ORDER BY YearGroup DESC
