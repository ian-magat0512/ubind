// <copyright file="IntegrationEventReplayController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Permissions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for manually replaying events which would normally occur as part of a web form fill or workflow.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/replay/")]
    public class IntegrationEventReplayController : PortalBaseController
    {
        private readonly IIntegrationEventReplayService applicationOperationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationEventReplayController"/> class.
        /// Replay Controller.
        /// </summary>
        /// <param name="applicationOperationService">Application operation service.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public IntegrationEventReplayController(
            IIntegrationEventReplayService applicationOperationService, ICachingResolver cachingResolver)
            : base(cachingResolver)
        {
            this.applicationOperationService = applicationOperationService;
        }

        /// <summary>
        /// Re-emits a specified event to the integrations system, in order to re-trigger any configured integrations.
        /// </summary>
        /// <remarks>Requires authentication as a uBind admin (with permission to replay integration events).</remarks>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="sequenceNumber">The sequence number of the event within its aggregate.</param>
        /// <param name="tenant">The tenant ID or alias (only required if you are logged in as a master tenant user).</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [Route("policy/{policyId}/event/{eventType}/sequence/{sequenceNumber}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ReplayIntegrationEvents)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RetriggerAllIntegrationsForEvent(
            Guid policyId, ApplicationEventType eventType, int sequenceNumber, string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "replay integration events",
                 "your user account doesn't have access to the master tenancy");
            List<string> jobIds = await this.applicationOperationService.ReplayIntegrationEvents(
                tenantId, policyId, eventType, sequenceNumber);
            return this.Ok($"The following hangfire Job IDs were created: {string.Join(", ", jobIds)}");
        }

        /// <summary>
        /// Re-emits a specified event to the integrations system, in order to re-trigger a particular integration.
        /// </summary>
        /// <remarks>Requires authentication as a uBind admin (with permission to replay integration events).</remarks>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="sequenceNumber">The sequence number of the event within its aggregate.</param>
        /// <param name="integrationId">The ID of the integration to re-trigger.</param>
        /// <param name="tenant">The tenant ID or alias (only required if you are logged in as a master tenant user).</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [Route("policy/{policyId}/event/{eventType}/sequence/{sequenceNumber}/integration/{integrationId}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.ReplayIntegrationEvents)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RetriggerGivenIntegrationForGivenEvent(
            Guid policyId, ApplicationEventType eventType, int sequenceNumber, string integrationId, string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(
                 tenant,
                 "replay integration events",
                 "your user account doesn't have access to the master tenancy");
            var jobId = await this.applicationOperationService.ReplayIntegrationEvent(
                tenantId, policyId, eventType, sequenceNumber, integrationId);
            return this.Ok($"The following hangfire Job ID was created: {jobId}");
        }
    }
}
