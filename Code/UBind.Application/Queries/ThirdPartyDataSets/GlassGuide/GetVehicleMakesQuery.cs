// <copyright file="GetVehicleMakesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// Represents the command for obtaining the collection of vehicle makes by vehicle year group.
/// </summary>
public class GetVehicleMakesQuery : IQuery<IEnumerable<VehicleMake>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleMakesQuery" /> class.
    /// </summary>
    /// <param name="year">The vehicle makes year.</param>
    public GetVehicleMakesQuery(int? year = null)
    {
        this.Year = year;
    }

    /// <summary>
    /// Gets the vehicle year group.
    /// </summary>
    public int? Year { get; }
}