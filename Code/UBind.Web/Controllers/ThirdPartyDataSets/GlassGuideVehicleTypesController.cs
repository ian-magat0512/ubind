// <copyright file="GlassGuideVehicleTypesController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers.ThirdPartyDataSets;

using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UBind.Application.Commands.ThirdPartyDataSets;
using UBind.Application.DataDownloader;
using UBind.Application.Queries.ThirdPartyDataSets;
using UBind.Application.Queries.ThirdPartyDataSets.GlassGuide;
using UBind.Application.ThirdPartyDataSets;
using UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;
using UBind.Application.ThirdPartyDataSets.ViewModel;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Permissions;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;
using UBind.Web.Filters;
using UBind.Web.Mapping.ThirdPartyDataSets.GlassGuide;
using UBind.Web.ResourceModels;

/// <summary>
/// The controller for handling Glass's Guide vehicle-related queries and requests.
/// </summary>
[Route("/api/v1/glass-guide")]
public class GlassGuideVehicleTypesController : Controller
{
    /// <summary>
    /// The mediator service.
    /// </summary>
    private readonly ICqrsMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlassGuideVehicleTypesController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator service.</param>
    public GlassGuideVehicleTypesController(ICqrsMediator mediator)
    {
        this.mediator = mediator;
    }

    /// <summary>
    /// Create Update Job for the Glass's Guide "Car" dataset.
    /// </summary>
    /// <param name="isForceUpdate">if false, the downloaded dataset will be compared to the previously downloaded one,
    ///  and an import will only be performed if the checksum is different,  true, the downloaded dataset will be imported
    ///  regardless of whether the same dataset has been imported previously.</param>
    /// <returns>Action result ok/error.</returns>
    [HttpPost("car/update-job")]
    [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
    [MustHavePermission(Permission.ManageBackgroundJobs)]
    [ProducesResponseType(typeof(UpdaterJobStatusResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUpdateJob(bool isForceUpdate = false)
    {
        var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Sftp, isForceUpdate);
        var createUpdateJobStatus = await this.mediator.Send(
            new CreateUpdaterJobCommand(typeof(UpdaterJobStateMachine), updaterJobManifest));
        return this.Ok(createUpdateJobStatus);
    }

    /// <summary>
    /// List Update Jobs for the Glass's Guide "Car" dataset.
    /// </summary>
    /// <returns>Action result ok/error.</returns>
    [HttpGet("car/update-job")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHavePermission(Permission.ManageBackgroundJobs)]
    [ProducesResponseType(typeof(IEnumerable<JobStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListUpdaterJobs()
    {
        var listOfjobStatus = await this.mediator.Send(
            new GetUpdaterJobsStatusesQuery(typeof(UpdaterJobStateMachine)));
        return this.Ok(listOfjobStatus);
    }

    /// <summary>
    /// Lookup a specific Update Job for the Glass's Guide "Car" dataset.
    /// </summary>
    /// <param name="updateJobId">the UUID of the update job.</param>
    /// <returns>An updater jobs status.</returns>
    [HttpGet("car/update-job/{updateJobId}")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHavePermission(Permission.ManageBackgroundJobs)]
    [ProducesResponseType(typeof(IJobStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpdaterJobsStatus(Guid updateJobId)
    {
        var jobStatus = await this.mediator.Send(new GetUpdaterJobStatusQuery(typeof(UpdaterJobStateMachine), updateJobId));
        return this.Ok(jobStatus);
    }

    /// <summary>
    /// Cancel the Update Job (ie set it's status to Cancelled).
    /// </summary>
    /// <param name="updateJobId">the UUID of the update job.</param>
    /// <returns>An updater jobs status.</returns>
    [HttpDelete("car/update-job/{updateJobId}")]
    [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
    [MustHavePermission(Permission.ManageBackgroundJobs)]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PatchUpdaterJobRequestVm([FromRoute] Guid updateJobId)
    {
        var jobStatus = await this.mediator.Send(
            new CancelUpdaterJobCommand(typeof(UpdaterJobStateMachine), updateJobId));
        return this.Ok(jobStatus);
    }

    /// <summary>
    /// List Vehicle Makes from the Glass's Guide "Car" dataset.
    /// </summary>
    /// <param name="year">The list of makes returned must only include those produced during the specified year.</param>
    /// <returns>A list of vehicle "makes" from the Glass's Guide "Car" dataset, filtered by the arguments used.</returns>
    [HttpGet("car/make")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
    [ProducesResponseType(typeof(ResourceModels.ThirdPartyDataSets.GlassGuide.VehicleMakesResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlassGuideVehicleMakes([ModelBinder(BinderType = typeof(OptionalYearModelBinder))] int? year)
    {
        var vehicleMakes = await this.mediator.Send(new Application.Queries.ThirdPartyDataSets.GlassGuide.GetVehicleMakesQuery(year));
        vehicleMakes.ForEach(make => make.MakeDescription = make.MakeDescription?.ToGlassGuideVehicleDescription());
        var result = new ResourceModels.ThirdPartyDataSets.GlassGuide.VehicleMakesResponseModel
        {
            Makes = vehicleMakes,
        };

        return this.Ok(await Task.FromResult(result));
    }

    /// <summary>
    /// List Vehicle Families from the Glass's Guide "Car" dataset.
    /// </summary>
    /// <param name="make">The list of families returned must only include the families of the specified make.</param>
    /// <param name="year">The list of families returned must only include those produced during the specified year.</param>
    /// <returns>A list of vehicle "families" from the Glass's Guide "Car" dataset, filtered by the arguments used.</returns>
    [HttpGet("car/family")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
    [ProducesResponseType(typeof(ResourceModels.ThirdPartyDataSets.GlassGuide.VehicleFamilyResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlassGuideVehicleFamilies(
        [FromQuery][Required] string make,
        [ModelBinder(BinderType = typeof(OptionalYearModelBinder))] int? year)
    {
        if (string.IsNullOrEmpty(make))
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.MakeCodeParameterMissing());
        }

        var vehicleFamilies = await this.mediator.Send(new Application.Queries.ThirdPartyDataSets.GlassGuide.GetVehicleFamiliesQuery(make, year));
        vehicleFamilies.ForEach(family => family.FamilyDescription = family.FamilyDescription?.ToGlassGuideVehicleDescription());
        var result = new ResourceModels.ThirdPartyDataSets.GlassGuide.VehicleFamilyResponseModel
        {
            Families = vehicleFamilies,
        };

        return this.Ok(await Task.FromResult(result));
    }

    /// <summary>
    /// List Vehicle Years from the Glass's Guide "Car" dataset.
    /// </summary>
    /// <param name="make">The list of years returned must only include the years during which the specified make was produced.</param>
    /// <param name="family">The list of years returned must only include the years during which the specified family (of the specified make) was produced.</param>
    /// <returns>A list of vehicle "years" from the Glass's Guide "Car" dataset, filtered by the arguments used.</returns>
    [HttpGet("car/year")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
    [ProducesResponseType(typeof(VehicleYearResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlassGuideVehicleYears([FromQuery][Required] string make, string family = "")
    {
        if (string.IsNullOrEmpty(make))
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.MakeCodeParameterMissing());
        }

        var vehicleYears = await this.mediator.Send(new Application.Queries.ThirdPartyDataSets.GlassGuide.GetVehicleYearsQuery(make, family));
        var result = new VehicleYearResponseModel
        {
            Years = vehicleYears,
        };

        return this.Ok(await Task.FromResult(result));
    }

    /// <summary>
    /// List Unique Vehicles from the Glass's Guide "Car" dataset.
    /// </summary>
    /// <param name="make">The list of vehicles returned must only include the those of the specified make.</param>
    /// <param name="family">The list of vehicles returned must only include the those of the specified family.</param>
    /// <param name="year">The list of vehicles returned must only include those produced during the specified year.</param>
    /// <returns>A list of vehicle "vehicles" from the Glass's Guide "Car" dataset, filtered by the arguments used.</returns>
    [HttpGet("car")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
    [ProducesResponseType(typeof(ResourceModels.ThirdPartyDataSets.GlassGuide.VehicleResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlassGuideVehicleList(
        [FromQuery][Required] string make,
        [FromQuery][Required] string family,
        [FromQuery][Required][ModelBinder(BinderType = typeof(RequiredYearModelBinder))] int? year)
    {
        if (this.HttpContext.Request.Path.ToString().EndsWith("/"))
        {
            // added below to fix wrong controller routing (i.e. "car/" is routed here instead of car/{glassCode})
            return await this.GetGlassGuideVehicleByGlassCode(string.Empty);
        }

        if (string.IsNullOrEmpty(make))
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.MakeCodeParameterMissing());
        }

        if (string.IsNullOrEmpty(family))
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.FamilyCodeParameterMissing());
        }

        if (year == null)
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.YearParameterMissing());
        }

        var vehicleList = await this.mediator.Send(new Application.Queries.ThirdPartyDataSets.GlassGuide.GetVehicleListQuery(make, family, year.Value));
        vehicleList.ForEach(vehicle =>
        {
            vehicle.VehicleDescription = vehicle.VehicleDescription?.ToGlassGuideVehicleDescription();
            vehicle.MakeDescription = vehicle.MakeDescription?.ToGlassGuideVehicleDescription();
            vehicle.FamilyDescription = vehicle.FamilyDescription?.ToGlassGuideVehicleDescription();
        });
        var result = new ResourceModels.ThirdPartyDataSets.GlassGuide.VehicleResponseModel
        {
            Vehicles = vehicleList,
        };

        return this.Ok(await Task.FromResult(result));
    }

    /// <summary>
    /// Lookup a Unique Vehicle from the Glass's Guide "Car" dataset.
    /// </summary>
    /// <param name="glassCode">The key identifying the unique vehicle type in the Glass's Guide "Car" dataset.</param>
    /// <returns>The full details of a vehicle from the Glass's Guide "Car" dataset.</returns>
    [HttpGet("car/{glassCode}")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
    [ProducesResponseType(typeof(VehicleDetails), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlassGuideVehicleByGlassCode([Required] string glassCode)
    {
        if (string.IsNullOrEmpty(glassCode))
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.GlassCodeParameterMissing());
        }

        var vehicleDetails = await this.mediator.Send(new GetVehicleByGlassCodeQuery(glassCode));
        vehicleDetails.VehicleDescription = vehicleDetails.VehicleDescription?.ToGlassGuideVehicleDescription().Trim();
        vehicleDetails.MakeDescription = vehicleDetails.MakeDescription?.ToGlassGuideVehicleDescription();
        vehicleDetails.FamilyDescription = vehicleDetails.FamilyDescription?.ToGlassGuideVehicleDescription();
        vehicleDetails.Series = vehicleDetails.Series?.ToGlassGuideVehicleDescription();
        vehicleDetails.Variant = vehicleDetails.Variant?.ToGlassGuideVehicleDescription();
        vehicleDetails.Body = vehicleDetails.Body?.ToGlassGuideVehicleDescription();
        vehicleDetails.EngineType = vehicleDetails.EngineType?.ToGlassGuideVehicleDescription();
        vehicleDetails.Transmission = vehicleDetails.Transmission?.ToGlassGuideVehicleDescription();
        return this.Ok(vehicleDetails);
    }
}
