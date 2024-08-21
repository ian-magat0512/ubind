// <copyright file="GetVehicleListQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// This class is for handling the commands for obtaining the collection of vehicles by vehicle make code, family code and year.
/// </summary>
public class GetVehicleListQueryHandler : IQueryHandler<GetVehicleListQuery, IReadOnlyList<Vehicle>>
{
    private readonly IGlassGuideRepository glassGuideRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleListQueryHandler"/> class.
    /// </summary>
    /// <param name="glassGuideRepository">The Glass's Guide repository.</param>
    public GetVehicleListQueryHandler(IGlassGuideRepository glassGuideRepository)
    {
        this.glassGuideRepository = glassGuideRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Vehicle>> Handle(GetVehicleListQuery query, CancellationToken cancellationToken)
    {
        if (query.Year < 1900 || query.Year > DateTime.Now.Year)
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.YearGroupInvalid(query.Year));
        }

        cancellationToken.ThrowIfCancellationRequested();
        return (await this.glassGuideRepository.GetVehicleListByMakeCodeFamilyCodeAndYearAsync(query.MakeCode, query.FamilyCode, query.Year)).ToList();
    }
}