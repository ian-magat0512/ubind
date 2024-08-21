// <copyright file="GetVehicleYearsQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;

    public class GetVehicleYearsQuery : IQuery<IReadOnlyList<int>>
    {
        public GetVehicleYearsQuery(string makeCode, string familyCode)
        {
            this.FamilyCode = familyCode;
            this.MakeCode = makeCode;
        }

        public string FamilyCode { get; }

        public string MakeCode { get; }
    }
}