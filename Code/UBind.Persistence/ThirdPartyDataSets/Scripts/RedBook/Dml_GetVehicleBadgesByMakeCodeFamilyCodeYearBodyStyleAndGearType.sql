SELECT
	DISTINCT Vehicles.BadgeDescription as Description,
	ISNULL(Vehicles.BadgeSecondaryDescription, '') as SecondaryDescription,
	Vehicles.MakeCode,
	Vehicles.FamilyCode,
	Vehicles.VehicleTypeCode
FROM
    [RedBook].[VEVehicle_View] Vehicles
WHERE

	-- Required Parameters
    Vehicles.MakeCode = @MakeCode
    AND Vehicles.FamilyCode = @FamilyCode
    AND Vehicles.YearGroup = @Year

	-- Optional Parameters
	AND ( 
			(@BodyStyle = '' AND Vehicles.VehicleKey IS NOT NULL) 
			OR (@BodyStyle <> '' AND Vehicles.BodyStyleDescription = @BodyStyle ) 
		)

	AND (   
			(@GearType = '' AND Vehicles.VehicleKey IS NOT NULL)
			OR (@GearType <> '' AND Vehicles.GearTypeDescription = @GearType )
		)
ORDER BY
    Vehicles.BadgeDescription