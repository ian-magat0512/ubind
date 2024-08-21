// <copyright file="RedBookVehicleTypesController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers.ThirdPartyDataSets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Commands.ThirdPartyDataSets;
    using UBind.Application.DataDownloader;
    using UBind.Application.Queries.ThirdPartyDataSets;
    using UBind.Application.Queries.ThirdPartyDataSets.RedBook;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.RedBookUpdaterJob;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// The controller for handling vehicle-related queries and requests.
    /// </summary>
    [Route("/api/v1/redbook")]
    public class RedBookVehicleTypesController : Controller
    {
        /// <summary>
        /// The mediator service.
        /// </summary>
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedBookVehicleTypesController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator service.</param>
        public RedBookVehicleTypesController(ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Create or Update RedBook updater job.
        /// </summary>
        /// <param name="isForceUpdate">To force to overwrite the DataSet.</param>
        /// <returns>Action result ok/error.</returns>
        [HttpPost("car/update-job")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(UpdaterJobStatusResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateUpdateJob(bool isForceUpdate = false)
        {
            var updaterJobManifest = new UpdaterJobManifest(DataDownloaderProtocol.Ftp, isForceUpdate);

            var createUpdateJobStatus = await this.mediator.Send(
                new CreateUpdaterJobCommand(typeof(UpdaterJobStateMachine), updaterJobManifest));
            return this.Ok(createUpdateJobStatus);
        }

        /// <summary>
        /// Retrieves the list of updater jobs.
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
        /// Get the updater job status.
        /// </summary>
        /// <param name="updateJobId">The updater job id.</param>
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
        /// Get the collection of vehicle makes by vehicle makes year.
        /// </summary>
        /// <param name="year">The vehicle makes year.</param>
        /// <returns>Returns the list of vehicle makes.</returns>
        [HttpGet("car/make")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(VehicleMakesResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicleMakes(string year = "")
        {
            var yearValue = this.ParseYearGroupToIntegerOrThrow(year);
            var vehicleMakes = await this.mediator.Send(new GetVehicleMakesQuery(yearValue));
            var result = new VehicleMakesResponseModel
            {
                Makes = vehicleMakes,
            };

            return this.Ok(await Task.FromResult(result));
        }

        /// <summary>
        /// Get the collection of vehicle families by make code and vehicle makes year group.
        /// </summary>
        /// <param name="make">The vehicle make code.</param>
        /// /// <param name="makeCode">Obsolete: please use 'make' parameter instead.</param>
        /// <param name="year">The makes year.</param>
        /// <returns>Returns the list of vehicle families.</returns>
        [HttpGet("car/family")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(VehicleFamilyResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicleFamilies([FromQuery][Required] string make, string makeCode = "", string year = "")
        {
            var makeValue = make ?? makeCode;
            var yearValue = this.ParseYearGroupToIntegerOrThrow(year);
            this.ValidateRequiredParameters(
                new Dictionary<string, object?>()
                { { "make", makeValue }, });
            var vehicleFamilies = await this.mediator.Send(new GetVehicleFamiliesQuery(makeValue, yearValue));
            var result = new VehicleFamilyResponseModel
            {
                Families = vehicleFamilies,
            };

            return this.Ok(await Task.FromResult(result));
        }

        /// <summary>
        /// Get the collection of vehicle years by make code and family code.
        /// </summary>
        /// <param name="make">The vehicle make code.</param>
        /// <param name="family">The family code.</param>
        /// <returns>Returns the list of vehicle families.</returns>
        [HttpGet("car/year")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(VehicleYearResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicleYears([FromQuery][Required] string make, string family = "")
        {
            this.ValidateRequiredParameters(
                new Dictionary<string, object?>()
                {
                    { "make", make },
                });
            var vehicleFamilies = await this.mediator.Send(new GetVehicleYearsQuery(make, family));
            var result = new VehicleYearResponseModel
            {
                Years = vehicleFamilies,
            };

            return this.Ok(await Task.FromResult(result));
        }

        /// <summary>
        /// Get the collection of vehicle badges by make code, family code, year, body and transmission.
        /// </summary>
        /// <param name="make">The vehicle make code.</param>
        /// <param name="family">The vehicle family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <param name="body">The vehicle body style.</param>
        /// <param name="transmission">The vehicle transmission type.</param>
        /// <returns>Returns the list of vehicle families.</returns>
        [HttpGet("car/badge")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(VehicleBadgeResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicleBadges(
            [FromQuery][Required] string make,
            [FromQuery][Required] string family,
            [FromQuery][Required] string year,
            [FromQuery] string body = "",
            [FromQuery] string transmission = "")
        {
            var yearValue = this.ParseYearGroupToIntegerOrThrow(year) ?? 0;
            this.ValidateRequiredParameters(
                new Dictionary<string, object?>()
                {
                    { "make", make },
                    { "family", family },
                    { "year", yearValue },
                });
            var vehicleBadges = await this.mediator.Send(new GetVehicleBadgesQuery(make, family, yearValue, body, transmission));
            var result = new VehicleBadgeResponseModel
            {
                Badges = vehicleBadges,
            };
            return this.Ok(await Task.FromResult(result));
        }

        /// <summary>
        /// Get the collection of vehicle body styles by make code, family code, year, badge and transmission.
        /// </summary>
        /// <param name="make">The vehicle make code.</param>
        /// <param name="family">The vehicle family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <param name="badge">The vehicle badge name.</param>
        /// <param name="transmission">The vehicle transmission type.</param>
        /// <returns>Returns the list of vehicle families.</returns>
        [HttpGet("car/body-style")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(VehicleBodyStyleResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicleBodyStyles(
            [FromQuery][Required] string make,
            [FromQuery][Required] string family,
            [FromQuery][Required] string year,
            [FromQuery] string badge = "",
            [FromQuery] string transmission = "")
        {
            var yearValue = this.ParseYearGroupToIntegerOrThrow(year) ?? 0;
            this.ValidateRequiredParameters(
                new Dictionary<string, object?>()
                {
                    { "make", make },
                    { "family", family },
                    { "year", yearValue },
                });
            var vehicleBodyStyles = await this.mediator.Send(new GetVehicleBodyStylesQuery(
                make, family, yearValue, badge, transmission));
            var result = new VehicleBodyStyleResponseModel
            {
                BodyStyles = vehicleBodyStyles
            };

            return this.Ok(await Task.FromResult(result));
        }

        /// <summary>
        /// Get the collection of vehicle transmission types by make code, family code, year, badge and body style.
        /// </summary>
        /// <param name="make">The vehicle make code.</param>
        /// <param name="family">The vehicle family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <param name="badge">The vehicle badge name.</param>
        /// <param name="body">The vehicle body style.</param>
        /// <returns>Returns the list of vehicle families.</returns>
        [HttpGet("car/transmission-type")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(VehicleGearTypeResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicleGearTransmissionTypes(
            [FromQuery][Required] string make,
            [FromQuery][Required] string family,
            [FromQuery][Required] string year,
            [FromQuery] string badge = "",
            [FromQuery] string body = "")
        {
            var yearValue = this.ParseYearGroupToIntegerOrThrow(year) ?? 0;
            this.ValidateRequiredParameters(
                new Dictionary<string, object?>()
                {
                    { "make", make },
                    { "family", family },
                    { "year", yearValue },
                });
            var vehicleGearTypes = await this.mediator.Send(new GetVehicleGearTypesQuery(
                make, family, yearValue, badge, body));
            var result = new VehicleGearTypeResponseModel
            {
                TransmissionTypes = vehicleGearTypes
            };
            return this.Ok(await Task.FromResult(result));
        }

        /// <summary>
        /// Get the collection of vehicles by make code , family code and year.
        /// </summary>
        /// <param name="make">The vehicle make code.</param>
        /// <param name="family">The vehicle family code.</param>
        /// <param name="year">The vehicle year.</param>
        /// <returns>Returns the list of vehicles.</returns>
        [HttpGet("car")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(VehicleResponseModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicleList(
            [FromQuery][Required] string make,
            [FromQuery][Required] string family,
            [FromQuery][Required] string year,
            [FromQuery] string badge = "",
            [FromQuery] string body = "",
            [FromQuery] string transmission = "")
        {
            var yearValue = this.ParseYearGroupToIntegerOrThrow(year) ?? 0;
            this.ValidateRequiredParameters(
                new Dictionary<string, object?>()
                {
                    { "make", make },
                    { "family", family },
                    { "year", yearValue },
                });
            var vehicleList = await this.mediator.Send(new GetVehicleListQuery(
                make, family, yearValue, badge, transmission, body));
            var result = new VehicleResponseModel
            {
                Vehicles = vehicleList,
            };
            return this.Ok(await Task.FromResult(result));
        }

        /// <summary>
        /// Get the vehicle details by vehicle key.
        /// </summary>
        /// <param name="vehicleKey">The vehicle key.</param>
        /// <returns>Returns the vehicle details.</returns>
        [HttpGet("car/{vehicleKey}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ProducesResponseType(typeof(Domain.ThirdPartyDataSets.RedBook.VehicleDetails), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicleByVehicleKey([Required] string vehicleKey)
        {
            this.ValidateRequiredParameters(
                new Dictionary<string, object?>()
                { { "vehicleKey", vehicleKey }, });
            return this.Ok(await this.mediator.Send(new GetVehicleByVehicleKeyQuery(vehicleKey)));
        }

        private int? ParseYearGroupToIntegerOrThrow(string year)
        {
            if (string.IsNullOrEmpty(year))
            {
                return null;
            }

            if (!int.TryParse(year, out int result))
            {
                throw new ErrorException(Domain.Errors.ThirdPartyDataSets.RedBook.YearGroupNotFound(year));
            }
            return result;
        }

        private void ValidateRequiredParameters(IDictionary<string, object?> requiredParameters)
        {
            List<string> missingParameters = new List<string>();

            if (requiredParameters == null)
            {
                return;
            }

            foreach (KeyValuePair<string, object?> item in requiredParameters)
            {
                if (item.Value == null)
                {
                    missingParameters.Add(item.Key);
                    continue;
                }

                // Check for default values
                if ((item.Value is int && (int)item.Value == 0) ||
                    (item.Value is string && string.IsNullOrWhiteSpace((string)item.Value)))
                {
                    missingParameters.Add(item.Key);
                    continue;
                }
            }

            if (missingParameters.Any())
            {
                throw new ErrorException(Domain.Errors.ThirdPartyDataSets.RedBook
                    .RequiredParametersMissing(missingParameters.ToArray()));
            }
        }
    }
}
