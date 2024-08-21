// <copyright file="RawColumnsVehicle.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob.ColumnMaps;

/// <summary>
/// Represents the 0-based "common" column mapping of Glass's Guide U12 and N12 data component files.
/// </summary>
public enum RawColumnsVehicle
{
    Code = 0,
    Nvic,
    Year,
    Mth,
    Make,
    Family,
    Variant,
    Series,
    Style,
    Engine,
    Cc,
    Size,
    Transmission,
    Cyl,
}
