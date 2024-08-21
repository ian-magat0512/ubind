SELECT [Year]
FROM [GlassGuide].[GG_Year_View]
WHERE [MakeCode] = @MakeCode
GROUP BY [Year]
ORDER BY [Year] DESC