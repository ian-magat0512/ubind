// <copyright file="GetVehicleGearTypesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook;

using UBind.Domain.Patterns.Cqrs;

public class GetVehicleGearTypesQuery : IQuery<IEnumerable<string>>
{
    public GetVehicleGearTypesQuery(string makeCode, string familyCode, int year, string? badge, string? bodyStyle)
    {
        this.MakeCode = makeCode;
        this.FamilyCode = familyCode;
        this.Year = year;
        this.Badge = badge;
        this.BodyStyle = bodyStyle;
    }

    public int Year { get; }

    public string MakeCode { get; }

    public string FamilyCode { get; }

    public string? Badge { get; }

    public string? BodyStyle { get; }
}
