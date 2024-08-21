// <copyright file="VehicleDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ThirdPartyDataSets.GlassGuide;

using Newtonsoft.Json;

/// <summary>
/// Represents the vehicle entity class.
/// </summary>
[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class VehicleDetails
{
    /// <summary>
    /// Gets or sets the vehicle's glass code.
    /// </summary>
    public string? GlassCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's unique National Vehicle Identifier Code.
    /// </summary>
    public string? Nvic { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's description from concatenated series, variant, body, transmission, and engine size values.
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
    /// Gets or sets the vehicle's type code (e.g. PV).
    /// </summary>
    public string? VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's type description (e.g. Passenger Vehicle).
    /// </summary>
    public string? VehicleTypeDescription { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's family code.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's family description.
    /// </summary>
    public int? Month { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's series.
    /// </summary>
    public string? Series { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's variant.
    /// </summary>
    public string? Variant { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's engine type.
    /// </summary>
    public string? EngineType { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's engine volume.
    /// </summary>
    public int? EngineVolume { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's engine size.
    /// </summary>
    public string? EngineSize { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's transmission.
    /// </summary>
    public string? Transmission { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's cylinders.
    /// </summary>
    public string? Cylinders { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's low trade value.
    /// </summary>
    public int? LowTradeValue { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's high trade value.
    /// </summary>
    public int? HighTradeValue { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's dealer used value.
    /// </summary>
    public int? DealerUsedValue { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's below average value.
    /// </summary>
    public int? BelowAverageValue { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's average value.
    /// </summary>
    public int? AverageValue { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's above average value.
    /// </summary>
    public int? AboveAverageValue { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's new price.
    /// </summary>
    public int? NewPrice { get; set; }

    /// <summary>
    /// Gets or sets the vehicle's used price.
    /// </summary>
    public int? UsedPrice { get; set; }
}