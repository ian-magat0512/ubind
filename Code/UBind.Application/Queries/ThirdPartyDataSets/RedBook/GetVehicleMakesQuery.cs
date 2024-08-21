// <copyright file="GetVehicleMakesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    public class GetVehicleMakesQuery : IQuery<IEnumerable<VehicleMake>>
    {
        public GetVehicleMakesQuery(int? year = null)
        {
            this.Year = year;
        }

        public int? Year { get; }
    }
}
