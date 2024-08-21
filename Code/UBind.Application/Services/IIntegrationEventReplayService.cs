// <copyright file="IIntegrationEventReplayService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// Service for re-triggering quote event integrations.
    /// </summary>
    public interface IIntegrationEventReplayService
    {
        /// <summary>
        /// Re-trigger any configured integrations for a given event on a given policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="sequenceNumber">The sequence number of the event within its aggregate.</param>
        /// <returns>A list of the hangfire Job IDs.</returns>
        Task<List<string>> ReplayIntegrationEvents(Guid tenantId, Guid policyId, ApplicationEventType eventType, int sequenceNumber);

        /// <summary>
        /// Re-trigger a specific integration for a given event on a given policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="eventType">Te type of the event.</param>
        /// <param name="sequenceNumber">The sequence number of the event within its aggregate.</param>
        /// <param name="integrationId">The ID of the integration to re-trigger.</param>
        /// <returns>The hangfire Job ID.</returns>
        Task<string> ReplayIntegrationEvent(
            Guid tenantId, Guid policyId, ApplicationEventType eventType, int sequenceNumber, string integrationId);
    }
}
