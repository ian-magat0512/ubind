// <copyright file="GetVehicleGearTypesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.RedBook;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class GetVehicleGearTypesQueryHandler : IQueryHandler<GetVehicleGearTypesQuery, IEnumerable<string>>
{
    private readonly IRedBookRepository redBookRepository;

    public GetVehicleGearTypesQueryHandler(IRedBookRepository redBookRepository)
    {
        this.redBookRepository = redBookRepository;
    }

    public async Task<IEnumerable<string>> Handle(GetVehicleGearTypesQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await this.redBookRepository
            .GetVehicleGearTypeByMakeCodeFamilyCodeYearBadgeAndBodyStyle(
                request.MakeCode,
                request.FamilyCode,
                request.Year,
                request.Badge,
                request.BodyStyle);
    }
}
