SELECT
    GG_Vehicle_View.GlassCode,
    GG_Vehicle_View.MakeCode,
    GG_Vehicle_View.MakeDescription,
    GG_Vehicle_View.FamilyCode,
    GG_Vehicle_View.FamilyDescription,
    GG_Vehicle_View.Year,
    TRIM(CONCAT(GG_Vehicle_View.Series, ' ', GG_Vehicle_View.Variant, ' ', GG_Vehicle_View.Body, ' ', GG_Vehicle_View.Transmission, ' ', GG_Vehicle_View.EngineSize)) AS VehicleDescription
FROM
    GlassGuide.GG_Vehicle_View
WHERE
    GG_Vehicle_View.MakeCode = @MakeCode
    AND GG_Vehicle_View.FamilyCode = @FamilyCode
    AND GG_Vehicle_View.Year = @Year
ORDER BY
    VehicleDescription