// <copyright file="VehicleSegmentType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

public enum VehicleSegmentType
{
    /// <summary>
    /// Passenger Vehicles (up to 10 years old)
    /// </summary>
    Pvg,

    /// <summary>
    /// Older Cars (passenger vehicles older than 10 years)
    /// </summary>
    Ocg,

    /// <summary>
    /// Light Commercial Vehicles
    /// </summary>
    Cvg,

    /// <summary>
    /// Older Light Commercial (essentially light commercial vehicles older than 10 years)
    /// </summary>
    Olc,

    /// <summary>
    /// Unknown segment type
    /// </summary>
    Unknown,
}
