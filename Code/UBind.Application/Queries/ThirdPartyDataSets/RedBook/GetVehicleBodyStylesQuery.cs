// <copyright file="GetVehicleBodyStylesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook;

using UBind.Domain.Patterns.Cqrs;

public class GetVehicleBodyStylesQuery : IQuery<IEnumerable<string>>
{
    public GetVehicleBodyStylesQuery(string makeCode, string familyCode, int year, string? badge, string? gearType)
    {
        this.MakeCode = makeCode;
        this.FamilyCode = familyCode;
        this.Year = year;
        this.Badge = badge;
        this.GearType = gearType;
    }

    public int Year { get; }

    public string MakeCode { get; }

    public string FamilyCode { get; }

    public string? Badge { get; }

    public string? GearType { get; }
}
