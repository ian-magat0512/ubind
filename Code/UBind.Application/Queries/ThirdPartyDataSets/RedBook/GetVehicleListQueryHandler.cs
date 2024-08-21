// <copyright file="GetVehicleListQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Queries.ThirdPartyDataSets.RedBook;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets.RedBook;

    public class GetVehicleListQueryHandler : IQueryHandler<GetVehicleListQuery, IReadOnlyList<Vehicle>>
    {
        private readonly IRedBookRepository redBookRepository;

        public GetVehicleListQueryHandler(IRedBookRepository redBookRepository)
        {
            this.redBookRepository = redBookRepository;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Vehicle>> Handle(GetVehicleListQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (await this.redBookRepository.GetVehicleListAsync(
                request.MakeCode, request.FamilyCode, request.Year, request.Badge, request.BodyStyle, request.GearType)).ToList();
        }
    }
}
