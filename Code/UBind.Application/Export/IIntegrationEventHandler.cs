// <copyright file="IIntegrationEventHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Threading.Tasks;
    using Hangfire.Server;
    using UBind.Domain;

    /// <summary>
    /// Handles integration events by passing them to relevant integrators.
    /// </summary>
    public interface IIntegrationEventHandler
    {
        /// <summary>
        /// For handling quote events.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="integrationEventId">The integration event ID.</param>
        /// <param name="eventType">The quote event type.</param>
        /// <param name="integrationId">The ID of the integration that should be triggered.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="sequenceNumber">The sequence number of the event within its aggregate.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="tenantAlias">The Alias of the tenant.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="performContext">The hangfire context.</param>
        /// <param name="customerName">The masked customer name.</param>
        /// <param name="customerEmail">The masked customer email.</param>
        /// <param name="isRetriggering">A value indicating whether the event is being re-triggered by the replay function. Default value is false.</param>
        /// <returns>A task that can be awaited.</returns>
        Task Handle(
            Guid tenantId,
            string timestamp,
            Guid integrationEventId,
            ApplicationEventType eventType,
            string integrationId,
            Guid quoteId,
            int sequenceNumber,
            Guid productId,
            string productAlias,
            string tenantAlias,
            DeploymentEnvironment environment,
            PerformContext performContext,
            string customerName,
            string customerEmail,
            bool isRetriggering = false);
    }
}
