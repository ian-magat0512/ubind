// <copyright file="GetVehicleMakesQueryHandler.cs" company="uBind">
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

    public class GetVehicleMakesQueryHandler : IQueryHandler<GetVehicleMakesQuery, IEnumerable<VehicleMake>>
    {
        private readonly IRedBookRepository redBookRepository;

        public GetVehicleMakesQueryHandler(IRedBookRepository redBookRepository)
        {
            this.redBookRepository = redBookRepository;
        }

        public async Task<IEnumerable<VehicleMake>> Handle(GetVehicleMakesQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!request.Year.HasValue)
            {
                return await this.redBookRepository.GetVehicleMakesAsync();
            }

            return await this.redBookRepository.GetVehicleMakesByYearGroupAsync(request.Year.Value);
        }
    }
}
