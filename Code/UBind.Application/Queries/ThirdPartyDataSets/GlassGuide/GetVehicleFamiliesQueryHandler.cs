// <copyright file="GetVehicleFamiliesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// This class is for handling the commands for obtaining the collection of vehicle makes by the vehicle year group.
/// </summary>
public class GetVehicleFamiliesQueryHandler : IQueryHandler<GetVehicleFamiliesQuery, IEnumerable<VehicleFamily>>
{
    private readonly IGlassGuideRepository glassGuideRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleFamiliesQueryHandler "/> class.
    /// </summary>
    /// <param name="glassGuideRepository">The Glass's Guide repository.</param>
    public GetVehicleFamiliesQueryHandler(IGlassGuideRepository glassGuideRepository)
    {
        this.glassGuideRepository = glassGuideRepository;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<VehicleFamily>> Handle(GetVehicleFamiliesQuery query, CancellationToken cancellationToken)
    {
        if (query.Year.HasValue && (query.Year.Value < 1900 || query.Year.Value > DateTime.Now.Year))
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.YearGroupInvalid(query.Year.Value));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = query.Year.HasValue
            ? await this.glassGuideRepository.GetVehicleFamiliesByMakeCodeAndYearGroupAsync(query.MakeCode, query.Year.Value)
            : await this.glassGuideRepository.GetVehicleFamiliesByMakeCodeAsync(query.MakeCode);
        return await Task.FromResult(result);
    }
}