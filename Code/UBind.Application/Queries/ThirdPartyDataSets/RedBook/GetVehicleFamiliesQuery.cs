// <copyright file="GetVehicleFamiliesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    public class GetVehicleFamiliesQuery : IQuery<IEnumerable<VehicleFamily>>
    {
        public GetVehicleFamiliesQuery(string makeCode, int? year)
        {
            this.Year = year;
            this.MakeCode = makeCode;
        }

        public int? Year { get; }

        public string MakeCode { get; }
    }
}