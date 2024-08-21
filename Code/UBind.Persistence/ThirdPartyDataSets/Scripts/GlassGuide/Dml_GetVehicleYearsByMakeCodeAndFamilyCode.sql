SELECT [Year]
FROM [GlassGuide].[GG_Year_View]
WHERE [MakeCode] = @MakeCode AND [FamilyCode] = @FamilyCode
GROUP BY [Year]
ORDER BY [Year] DESC