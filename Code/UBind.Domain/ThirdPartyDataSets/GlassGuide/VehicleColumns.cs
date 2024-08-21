// <copyright file="VehicleColumns.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// Represents the 0-based column mapping of Glass's Guide's Vehicle table.
/// </summary>
public enum VehicleColumns
{
    GlassCode = 0,
    Nvic,
    DataCode,
    VehicleTypeCode,
    VehicleTypeDescription,
    Year,
    Month,
    MakeCode,
    MakeDescription,
    FamilyCode,
    FamilyDescription,
    Variant,
    Series,
    Body,
    EngineType,
    EngineVolume,
    EngineSize,
    Transmission,
    Cylinders,
    LowTradeValue,
    HighTradeValue,
    DealerUsedValue,
    BelowAverageValue,
    AverageValue,
    AboveAverageValue,
    NewPrice,
    UsedPrice,
}
