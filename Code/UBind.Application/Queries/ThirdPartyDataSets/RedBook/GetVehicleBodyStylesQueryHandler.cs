// <copyright file="GetVehicleBodyStylesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook;

using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class GetVehicleBodyStylesQueryHandler : IQueryHandler<GetVehicleBodyStylesQuery, IEnumerable<string>>
{
    private readonly IRedBookRepository redBookRepository;

    public GetVehicleBodyStylesQueryHandler(IRedBookRepository redBookRepository)
    {
        this.redBookRepository = redBookRepository;
    }

    public async Task<IEnumerable<string>> Handle(GetVehicleBodyStylesQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await this.redBookRepository
            .GetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType(
                request.MakeCode,
                request.FamilyCode,
                request.Year,
                request.Badge,
                request.GearType);
    }
}
