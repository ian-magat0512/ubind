// <copyright file="GetVehicleBadgesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook;

using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ThirdPartyDataSets.RedBook;

public class GetVehicleBadgesQuery : IQuery<IEnumerable<VehicleBadge>>
{
    public GetVehicleBadgesQuery(string makeCode, string familyCode, int year, string? bodyStyle, string? gearType)
    {
        this.MakeCode = makeCode;
        this.FamilyCode = familyCode;
        this.Year = year;
        this.BodyStyle = bodyStyle;
        this.GearType = gearType;
    }

    public int Year { get; }

    public string MakeCode { get; }

    public string FamilyCode { get; }

    public string? BodyStyle { get; }

    public string? GearType { get; }
}
