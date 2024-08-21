// <copyright file="VehicleFamily.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ThirdPartyDataSets.GlassGuide;

using Newtonsoft.Json;

/// <summary>
/// Represents the vehicle family entity class.
/// </summary>
public class VehicleFamily
{
    /// <summary>
    /// Gets or sets the manufacturer code of the vehicle family.
    /// </summary>
    public string? MakeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle family code.
    /// </summary>
    public string? FamilyCode { get; set; }

    /// <summary>
    /// Gets or sets the type of vehicle (e.g. PV).
    /// </summary>
    public string? VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the family name in full.
    /// </summary>
    [JsonProperty("description")]
    public string? FamilyDescription { get; set; }

    /// <summary>
    /// Gets or sets the year that this family started.
    /// </summary>
    public int StartYear { get; set; }

    /// <summary>
    /// Gets or sets the latest year that this family ran to.
    /// </summary>
    public int LatestYear { get; set; }
}