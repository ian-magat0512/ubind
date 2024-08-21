// <copyright file="GetVehicleFamiliesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    public class GetVehicleFamiliesQueryHandler : IQueryHandler<GetVehicleFamiliesQuery, IEnumerable<VehicleFamily>>
    {
        private readonly IRedBookRepository redBookRepository;

        public GetVehicleFamiliesQueryHandler(IRedBookRepository redBookRepository)
        {
            this.redBookRepository = redBookRepository;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VehicleFamily>> Handle(GetVehicleFamiliesQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!request.Year.HasValue)
            {
                return await this.redBookRepository.GetVehicleFamiliesByMakeCodeAsync(request.MakeCode);
            }

            return await this.redBookRepository.GetVehicleFamiliesByMakeCodeAndYearGroupAsync(request.MakeCode, request.Year.Value);
        }
    }
}
