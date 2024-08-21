// <copyright file="VehicleMake.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ThirdPartyDataSets.GlassGuide;

using Newtonsoft.Json;

/// <summary>
/// Represents the vehicle makes entity class.
/// </summary>
public class VehicleMake
{
    /// <summary>
    /// Gets or sets the manufacturer's code.
    /// </summary>
    public string? MakeCode { get; set; }

    /// <summary>
    /// Gets or sets the manufacturer's name.
    /// </summary>
    [JsonProperty("description")]
    public string? MakeDescription { get; set; }

    /// <summary>
    /// Gets or sets the first year that this manufacturer produced a car.
    /// </summary>
    public int StartYear { get; set; }

    /// <summary>
    /// Gets or sets the latest year that this manufacturer produced a car.
    /// </summary>
    public int LatestYear { get; set; }
}