// <copyright file="GetVehicleYearsQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

/// <summary>
/// This class is for handling the commands for obtaining the collection of vehicle years by vehicle make code and vehicle family code.
/// </summary>
public class GetVehicleYearsQueryHandler : IQueryHandler<GetVehicleYearsQuery, IReadOnlyList<int>>
{
    private readonly IGlassGuideRepository glassGuideRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleYearsQueryHandler "/> class.
    /// </summary>
    /// <param name="glassGuideRepository">The Glass's Guide repository.</param>
    public GetVehicleYearsQueryHandler(IGlassGuideRepository glassGuideRepository)
    {
        this.glassGuideRepository = glassGuideRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<int>> Handle(GetVehicleYearsQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return string.IsNullOrEmpty(query.FamilyCode)
            ? (await this.glassGuideRepository.GetVehicleYearsByMakeCodeAsync(query.MakeCode)).ToList()
            : (await this.glassGuideRepository.GetVehicleYearsByMakeCodeAndFamilyCodeAsync(query.MakeCode, query.FamilyCode)).ToList();
    }
}