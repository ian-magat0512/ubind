// <copyright file="GetVehicleListQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// Represents the command for obtaining the collection of vehicles by vehicle make code, family code and year.
/// </summary>
public class GetVehicleListQuery : IQuery<IReadOnlyList<Vehicle>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleListQuery" /> class.
    /// </summary>
    /// <param name="makeCode">The vehicle make code.</param>
    /// <param name="familyCode">The vehicle family code.</param>
    /// <param name="year">The vehicle year.</param>
    public GetVehicleListQuery(string makeCode, string familyCode, int year)
    {
        this.MakeCode = makeCode;
        this.FamilyCode = familyCode;
        this.Year = year;
    }

    /// <summary>
    /// Gets the vehicle year group.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Gets the make code.
    /// </summary>
    public string MakeCode { get; }

    /// <summary>
    /// Gets the vehicle family code.
    /// </summary>
    public string FamilyCode { get; }
}