// <copyright file="GetVehicleByVehicleKeyQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    public class GetVehicleByVehicleKeyQuery : IQuery<VehicleDetails>
    {
        public GetVehicleByVehicleKeyQuery(string vehicleKey)
        {
            this.VehicleKey = vehicleKey;
        }

        public string VehicleKey { get; }
    }
}
