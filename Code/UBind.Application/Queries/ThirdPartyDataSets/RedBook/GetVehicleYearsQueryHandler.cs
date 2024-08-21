// <copyright file="GetVehicleYearsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class GetVehicleYearsQueryHandler : IQueryHandler<GetVehicleYearsQuery, IReadOnlyList<int>>
    {
        private readonly IRedBookRepository redBookRepository;

        public GetVehicleYearsQueryHandler(IRedBookRepository redBookRepository)
        {
            this.redBookRepository = redBookRepository;
        }

        public async Task<IReadOnlyList<int>> Handle(GetVehicleYearsQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(request.FamilyCode))
            {
                return (await this.redBookRepository.GetVehicleYearsByMakeCodeAsync(request.MakeCode)).ToList();
            }

            return (await this.redBookRepository.GetVehicleYearsByMakeCodeAndFamilyCodeAsync(request.MakeCode, request.FamilyCode)).ToList();
        }
    }
}
