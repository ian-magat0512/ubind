// <copyright file="GetVehicleYearsQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Represents the command for obtaining the collection of vehicle years by vehicle make code and vehicle family code.
/// </summary>
public class GetVehicleYearsQuery : IQuery<IReadOnlyList<int>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleYearsQuery" /> class.
    /// </summary>
    /// <param name="makeCode">The vehicle make code.</param>
    /// <param name="familyCode">The vehicle family code.</param>
    public GetVehicleYearsQuery(string makeCode, string familyCode)
    {
        this.FamilyCode = familyCode;
        this.MakeCode = makeCode;
    }

    /// <summary>
    /// Gets the vehicle family code.
    /// </summary>
    public string FamilyCode { get; }

    /// <summary>
    /// Gets the vehicle make code.
    /// </summary>
    public string MakeCode { get; }
}