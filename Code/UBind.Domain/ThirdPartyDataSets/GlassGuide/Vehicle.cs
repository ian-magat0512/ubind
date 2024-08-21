// <copyright file="Vehicle.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ThirdPartyDataSets.GlassGuide;

using Newtonsoft.Json;

/// <summary>
/// Represents the vehicle entity class in summary.
/// </summary>
public class Vehicle
{
    /// <summary>
    /// Gets or sets the vehicle's glass code.
    /// </summary>
    [JsonProperty("vehicleKey")]
    public string? GlassCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's description.
    /// </summary>
    public string? VehicleDescription { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's manufacturer code.
    /// </summary>
    public string? MakeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's manufacturer description.
    /// </summary>
    public string? MakeDescription { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's family code.
    /// </summary>
    public string? FamilyCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's family description.
    /// </summary>
    public string? FamilyDescription { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's year of first production.
    /// </summary>
    public int Year { get; set; }
}
