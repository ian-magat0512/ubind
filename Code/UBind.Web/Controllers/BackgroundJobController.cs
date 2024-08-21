// <copyright file="BackgroundJobController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Provides for utlities and maintenance operations on aggregates of the system.
    /// </summary>
    [Route("/api/v1/background-job")]
    public class BackgroundJobController : Controller
    {
        private readonly IBackgroundJobService backgroundJobService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJobController"/> class.
        /// </summary>
        /// <param name="backgroundJobService">The hangfire service.</param>
        public BackgroundJobController(
            IBackgroundJobService backgroundJobService)
        {
            this.backgroundJobService = backgroundJobService;
        }

        /// <summary>
        /// Mark Job as Acknowledged.
        /// </summary>
        /// <param name="backgroundJobModel">The HangFire Job Model.</param>
        /// <returns>Text confirming that Job ID has been Acknowledge.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [Route("acknowledge")]
        [Produces("text/plain")]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult Acknowledge([FromBody] BackGroundJobModel backgroundJobModel)
        {
            this.backgroundJobService.Acknowledge(
                this.User.GetTenantId(),
                backgroundJobModel.BackgroundJobId.ToString(),
                this.User.GetId().Value,
                backgroundJobModel.TicketId,
                backgroundJobModel.AcknowledgementMessage);
            return this.Ok($"Hangfire Job ID {backgroundJobModel.BackgroundJobId} has been acknowledged.");
        }

        /// <summary>
        /// Get list of Failed jobs regardless of whether it was marked as acknowledged or not.
        /// </summary>
        /// <param name="backgroundJobQueryOptionsModel">The Background job query options model.</param>
        /// <returns>Text indicating the number of failed jobs.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ViewBackgroundJobs, Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(List<BackgroundJobDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFailedJobs(BackgroundJobQueryOptionsModel backgroundJobQueryOptionsModel)
        {
            List<BackgroundJobDto> backgroundJobDto = await this.backgroundJobService.GetFailedAndRetryingJobs(
                this.User.GetTenantId(),
                backgroundJobQueryOptionsModel.FilterEnvironment,
                backgroundJobQueryOptionsModel.FilterTenant,
                backgroundJobQueryOptionsModel.FilterProduct,
                backgroundJobQueryOptionsModel.Environment.ToString(),
                backgroundJobQueryOptionsModel.TenantAlias,
                backgroundJobQueryOptionsModel.ProductAlias,
                backgroundJobQueryOptionsModel.IsAcknowledged);
            return this.Ok(backgroundJobDto);
        }

        /// <summary>
        /// Counts the number of failed hangfire jobs.
        /// </summary>
        /// <param name="backgroundJobQueryOptionsModel">The background job query options model.</param>
        /// <returns>Text indicating the number of failed jobs.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 120)]
        [ValidateModel]
        [Route("count")]
        [Produces("text/plain")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public async Task<IActionResult> GetJobsCount([FromQuery] BackgroundJobQueryOptionsModel backgroundJobQueryOptionsModel)
        {
            var numberOfAcknowledgedFailedJobs = 0;
            var numberOfNotAcknowledgedFailedJobs = 0;

            if (backgroundJobQueryOptionsModel.IsAcknowledged == true)
            {
                numberOfAcknowledgedFailedJobs = await this.GetFailedJobsCount(backgroundJobQueryOptionsModel, true);
            }
            else if (backgroundJobQueryOptionsModel.IsAcknowledged == false)
            {
                numberOfNotAcknowledgedFailedJobs = await this.GetFailedJobsCount(backgroundJobQueryOptionsModel, false);
            }
            else
            {
                numberOfAcknowledgedFailedJobs = await this.GetFailedJobsCount(backgroundJobQueryOptionsModel, true);
                numberOfNotAcknowledgedFailedJobs = await this.GetFailedJobsCount(backgroundJobQueryOptionsModel, false);
            }

            var numberOfFailedJobs = numberOfAcknowledgedFailedJobs + numberOfNotAcknowledgedFailedJobs;
            var hasBothAcknowlegedAndNotAcknowledgedFailedJobs = numberOfAcknowledgedFailedJobs > 0 && numberOfNotAcknowledgedFailedJobs > 0;
            var hasOnlyAcknowledgeFailedJobs = numberOfAcknowledgedFailedJobs > 0 && numberOfNotAcknowledgedFailedJobs == 0;
            var hasOnlyNotAcknowledgeFailedJobs = numberOfNotAcknowledgedFailedJobs > 0 && numberOfAcknowledgedFailedJobs == 0;

            var message = hasBothAcknowlegedAndNotAcknowledgedFailedJobs ?
                          $"FAIL There are {numberOfFailedJobs} failed jobs, of which {numberOfAcknowledgedFailedJobs} have been acknowledged and {numberOfNotAcknowledgedFailedJobs} have not been acknowledged." :
                          hasOnlyAcknowledgeFailedJobs ? $"OK There are {numberOfFailedJobs} failed jobs which have all been acknowledged." :
                          hasOnlyNotAcknowledgeFailedJobs ? $"FAIL There are {numberOfFailedJobs} failed jobs, none of which have been acknowledged." :
                          "OK There are no failed jobs";

            return this.Ok(message);
        }

        /// <summary>
        /// Create a failing background job for testing purposes.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <param name="productAlias">The product alias.</param>
        /// <returns>Text indicating the job is created.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [Produces("text/plain")]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateFailedJobs(string environment, string tenantAlias, string productAlias)
        {
            var jobId = await this.backgroundJobService.CreateTestFailedJob(
                this.User.GetTenantId(),
                environment,
                tenantAlias,
                productAlias);
            return this.Ok($"Test Failed jobs with Id {jobId} has been created.");
        }

        /// <summary>
        /// Expires a background job.
        /// </summary>
        /// <param name="jobId">The ID of the job to be expired.</param>
        /// <returns>A confirmation message stating expiry is set when successful.</returns>
        [HttpPost]
        [Route("{jobId}/expire")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [Produces("text/plain")]
        [MustHavePermission(Permission.ManageBackgroundJobs)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult ExpireJobs(string jobId)
        {
            this.backgroundJobService.ExpireJob(jobId);
            return this.Ok($"Background job {jobId} is expired.");
        }

        private async Task<int> GetFailedJobsCount(BackgroundJobQueryOptionsModel backgroundJobQueryOptionsModel, bool? isAcknowledged = null)
        {
            return await this.backgroundJobService.GetFailedAndRetryingJobsCount(
            backgroundJobQueryOptionsModel.FilterEnvironment,
            backgroundJobQueryOptionsModel.FilterTenant,
            backgroundJobQueryOptionsModel.FilterProduct,
            backgroundJobQueryOptionsModel.Environment.ToString(),
            backgroundJobQueryOptionsModel.TenantAlias,
            backgroundJobQueryOptionsModel.ProductAlias,
            isAcknowledged);
        }
    }
}
