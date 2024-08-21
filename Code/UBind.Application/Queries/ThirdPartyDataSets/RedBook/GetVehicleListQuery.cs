// <copyright file="GetVehicleListQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    public class GetVehicleListQuery : IQuery<IReadOnlyList<Vehicle>>
    {
        public GetVehicleListQuery(string makeCode, string familyCode, int year, string? badge, string? gearType, string? bodyStyle)
        {
            this.MakeCode = makeCode;
            this.FamilyCode = familyCode;
            this.Year = year;
            this.Badge = badge;
            this.GearType = gearType;
            this.BodyStyle = bodyStyle;
        }

        public int Year { get; }

        public string MakeCode { get; }

        public string FamilyCode { get; }

        public string? Badge { get; }

        public string? BodyStyle { get; }

        public string? GearType { get; }
    }
}