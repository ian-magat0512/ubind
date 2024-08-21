SELECT
    Vehicle.VehicleKey,
    Vehicle.Description AS vehicleDescription,
    Vehicle.MakeCode,
    Make.Description AS makeDescription,
    Vehicle.FamilyCode,
    Family.Description AS familyDescription,
    Vehicle.VehicleTypeCode,
    Vehicle.YearGroup,
    Vehicle.MonthGroup,
    Vehicle.SequenceNum AS sequenceNumber,
    CASE
        Vehicle.CurrentRelease
        WHEN 'F' THEN CAST(0 AS BIT)
        ELSE CAST(1 AS BIT)
    END AS CurrentRelease,
    CASE
        Vehicle.LimitedEdition
        WHEN 'F' THEN CAST(0 AS BIT)
        ELSE CAST(1 AS BIT)
    END AS LimitedEdition,
    Vehicle.Series AS seriesPublic,
    Vehicle.seriesModelYear AS seriesModelYear,
    Vehicle.BadgeDescription,
    Vehicle.BadgeSecondaryDescription,
    Vehicle.BodyStyleDescription,
    Vehicle.BodyConfigDescription,
    Vehicle.WheelBaseConfig,
    Vehicle.Roofline,
    Vehicle.ExtraIdentification,
    Vehicle.DriveDescription,
    Vehicle.GearTypeDescription,
    Vehicle.GearLocationDescription,
    Vehicle.GearNum,
    Vehicle.DoorNum,
    Vehicle.EngineSize,
    Vehicle.EngineDescription,
    Vehicle.Cylinders,
    Vehicle.FuelTypeDescription,
    Vehicle.InductionDescription,
    Vehicle.OptionCategory,
    Vehicle.tradeMin,
    Vehicle.tradeMax,
    Vehicle.privateMin,
    Vehicle.privateMax,
    Vehicle.VFactsClass,
    Vehicle.VFactsSegment,
    Vehicle.VFactsPrice,
    Vehicle.HighPoweredVehicle,
    CASE
        Vehicle.IsPPlateApproved
        WHEN 'F' THEN CAST(0 AS BIT)
        ELSE CAST(1 AS BIT)
    END AS IsPPlateApproved
FROM
    RedBook.VEVehicle_View Vehicle
    LEFT OUTER JOIN RedBook.VEFamily_View Family ON Vehicle.FamilyCode = Family.FamilyCode
    LEFT OUTER JOIN RedBook.VEMake_View Make ON Vehicle.MakeCode = Make.MakeCode
WHERE
    (Vehicle.VehicleKey = @VehicleKey)