SELECT
    DISTINCT Vehicles.VehicleKey,
    Vehicles.Description AS VehicleDescription,
    Vehicles.MakeCode,
    VehicleMake.Description AS MakeDescription,
    Vehicles.FamilyCode,
    VehicleFamily.Description AS FamilyDescription,
    Vehicles.YearGroup,
    Vehicles.MonthGroup
FROM
    [RedBook].[VEVehicle_View] AS Vehicles
    INNER JOIN [RedBook].[VEMake_View] AS VehicleMake ON Vehicles.MakeCode = VehicleMake.MakeCode
    INNER JOIN [RedBook].[VEFamily_View] AS VehicleFamily ON Vehicles.FamilyCode = VehicleFamily.FamilyCode
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
	AND (   
			(@BodyStyle = '' AND Vehicles.VehicleKey IS NOT NULL) 
			OR (@BodyStyle <> '' AND  Vehicles.BodyStyleDescription = @BodyStyle ) 
		)

ORDER BY
    Vehicles.Description