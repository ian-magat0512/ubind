SELECT
    DISTINCT Vehicles.BodyStyleDescription as Description
FROM
    [RedBook].[VEVehicle_View] Vehicles
WHERE

	-- Required Parameters
    Vehicles.MakeCode = @MakeCode
    AND Vehicles.FamilyCode = @FamilyCode
    AND Vehicles.YearGroup = @Year

	-- Optional Parameters
	AND ( 
			(@Badge = '' AND Vehicles.VehicleKey IS NOT NULL) 
			OR (@Badge <> '' AND  Vehicles.BadgeDescription = @Badge ) 
		)

	AND (   
			(@GearType = '' AND Vehicles.VehicleKey IS NOT NULL)
			OR (@GearType <> '' AND Vehicles.GearTypeDescription = @GearType )
		)
ORDER BY
    Vehicles.BodyStyleDescription