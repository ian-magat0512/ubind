// <copyright file="GetVehicleByGlassCodeQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// Represents the command for obtaining the vehicle details by glass code.
/// </summary>
public class GetVehicleByGlassCodeQuery : IQuery<VehicleDetails>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleByGlassCodeQuery"/> class.
    /// </summary>
    /// <param name="glassCode">The glass code.</param>
    public GetVehicleByGlassCodeQuery(string glassCode)
    {
        this.GlassCode = glassCode;
    }

    /// <summary>
    /// Gets the glass code.
    /// </summary>
    public string GlassCode { get; }
}