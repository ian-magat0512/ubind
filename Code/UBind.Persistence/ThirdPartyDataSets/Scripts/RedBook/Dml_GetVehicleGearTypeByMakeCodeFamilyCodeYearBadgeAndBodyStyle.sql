SELECT
   DISTINCT Vehicles.GearTypeDescription as Description
FROM
    [RedBook].[VEVehicle_View] AS Vehicles
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
			(@BodyStyle = '' AND Vehicles.VehicleKey IS NOT NULL) 
			OR (@BodyStyle <> '' AND  Vehicles.BodyStyleDescription = @BodyStyle ) 
		)
ORDER BY
    Vehicles.GearTypeDescription