// <copyright file="GetVehicleByGlassCodeQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

/// <summary>
/// This class is for handling the commands for obtaining vehicle details by glass code.
/// </summary>
public class GetVehicleByGlassCodeQueryHandler : IQueryHandler<GetVehicleByGlassCodeQuery, VehicleDetails>
{
    private readonly IGlassGuideRepository glassGuideRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetVehicleByGlassCodeQueryHandler"/> class.
    /// </summary>
    /// <param name="glassGuideRepository">The Glass's Guide repository.</param>
    public GetVehicleByGlassCodeQueryHandler(IGlassGuideRepository glassGuideRepository)
    {
        this.glassGuideRepository = glassGuideRepository;
    }

    /// <inheritdoc/>
    public async Task<VehicleDetails> Handle(GetVehicleByGlassCodeQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var vehicleResult = await this.glassGuideRepository.GetVehicleByVehicleKeyAsync(query.GlassCode);

        if (vehicleResult == null)
        {
            throw new ErrorException(Errors.ThirdPartyDataSets.GlassGuide.GlassCodeInvalid(query.GlassCode));
        }

        return vehicleResult;
    }
}