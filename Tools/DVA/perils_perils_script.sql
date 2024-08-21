-- Please change 'PERILS_CSV_DIR' with the directory path location of perils data
-- And update the effectivity date ('[EffectiveDate]') for when the new rates are to be used.

--- CREATE TEMP TABLE
CREATE TABLE #PerilsDump
(
	GnafPid VARCHAR(25),
	PostCode NVARCHAR(MAX),
	IcaZone NVARCHAR(MAX),
	StormRate FLOAT,
	CycloneRate FLOAT,
	FireRate FLOAT,
	FloodRate FLOAT,
	QuakeRate FLOAT,
	TotalRate FLOAT
)
GO 

-- INSERT DATE FROM PERILS TO TEMP TABLE
BULK INSERT #PerilsDump
FROM 'PERILS_CSV_DIR\DSHI_Perils_Rates.csv' 
WITH (FIRSTROW = 2, FIELDTERMINATOR = ',', ROWTERMINATOR = '\n')
GO 

-- ALTER TABLE TO INCLUDE EFFECTIVE DATE TIME
ALTER TABLE #PerilsDump ADD EffectiveDate DATETIME NULL 
GO 

-- UPDATE TEMP TABLE WITH VALUE FOR EFFECTIVITY DATE
UPDATE #PerilsDump
SET [EffectiveDate] = '2022-01-01'
WHERE [EffectiveDate] IS NULL
GO 

-- TRANSFER ALL DATA TO PERILS TABLE
INSERT INTO Perils (GnafPid, PostCode, IcaZone, StormRate, CycloneRate, FireRate, FloodRate, QuakeRate, TotalRate, EffectiveDate)
SELECT GnafPid, PostCode, IcaZone, StormRate, CycloneRate, FireRate, FloodRate, QuakeRate, TotalRate, EffectiveDate FROM #PerilsDump

-- CLEANUP
DROP Table IF EXISTS tempdb..#PerilsDump