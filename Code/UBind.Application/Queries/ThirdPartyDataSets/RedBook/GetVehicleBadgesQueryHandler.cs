// <copyright file="GetVehicleBadgesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.ThirdPartyDataSets.RedBook;

public class GetVehicleBadgesQueryHandler : IQueryHandler<GetVehicleBadgesQuery, IEnumerable<VehicleBadge>>
{
    private readonly IRedBookRepository redBookRepository;

    public GetVehicleBadgesQueryHandler(IRedBookRepository redBookRepository)
    {
        this.redBookRepository = redBookRepository;
    }

    public async Task<IEnumerable<VehicleBadge>> Handle(
        GetVehicleBadgesQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await this.redBookRepository
            .GetVehicleBadgesByMakeCodeFamilyCodeYearBodyStyleAndGearType(
                request.MakeCode,
                request.FamilyCode,
                request.Year,
                request.BodyStyle,
                request.GearType);
    }
}
