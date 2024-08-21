// <copyright file="GetVehicleFamiliesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// Represents the command for obtaining the collection of vehicle families by make code and vehicle makes year group.
/// </summary>
public class GetVehicleFamiliesQuery : IQuery<IEnumerable<VehicleFamily>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleFamiliesQuery" /> class.
    /// </summary>
    /// <param name="makeCode">The vehicle make code.</param>
    /// <param name="year">The vehicle makes year group.</param>
    public GetVehicleFamiliesQuery(string makeCode, int? year)
    {
        this.Year = year;
        this.MakeCode = makeCode;
    }

    /// <summary>
    /// Gets the vehicle year group.
    /// </summary>
    public int? Year { get; }

    /// <summary>
    /// Gets the make code.
    /// </summary>
    public string MakeCode { get; }
}