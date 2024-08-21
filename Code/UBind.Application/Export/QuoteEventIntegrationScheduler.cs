// <copyright file="QuoteEventIntegrationScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Processing;
    using UBind.Domain.Product;

    /// <inheritdoc/>
    public class QuoteEventIntegrationScheduler : IQuoteEventIntegrationScheduler
    {
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IJobClient backgroundJobClient;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEventIntegrationScheduler"/> class.
        /// </summary>
        /// <param name="releaseQueryService">Service for providing the current product release.</param>
        /// <param name="backgroundJobClient">A client for queing integration jobs.</param>
        public QuoteEventIntegrationScheduler(
            IReleaseQueryService releaseQueryService,
            IJobClient backgroundJobClient,
            IClock clock)
        {
            this.releaseQueryService = releaseQueryService;
            this.backgroundJobClient = backgroundJobClient;
            this.clock = clock;
        }

        public void Dispatch(
            QuoteAggregate aggregate,
            IEvent<QuoteAggregate, Guid> quoteEvent,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            var quote = aggregate.GetQuoteBySequenceNumber(sequenceNumber);
            if (quoteEvent.GetType() == typeof(QuoteAggregate.QuoteStateChangedEvent)
                && quote != null)
            {
                if (quote.IsExpired(this.clock.Now()))
                {
                    // we don't trigger integration jobs for expired quotes. They can be handled by automations.
                    return;
                }
            }

            var applicationEventTypes = QuoteEventTypeMap.Map(quoteEvent);
            if (aggregate.IsBeingReplayed)
            {
                // skip this process if was dispatched because of replay all events, just to recreate read models from orphaned aggregates.
                return;
            }

            Guid? productReleaseId = quote != null
                ? quote.ProductReleaseId
                : this.releaseQueryService.GetDefaultProductReleaseIdOrThrow(
                    aggregate.TenantId,
                    aggregate.ProductId,
                    aggregate.Environment);

            var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                aggregate.TenantId,
                aggregate.ProductId,
                aggregate.Environment,
                productReleaseId);
            if (this.HasExporters(releaseContext, applicationEventTypes))
            {
                // don't trigger the integration hangfire job until the aggregate has been saved:
                EventHandler handler = null;
                aggregate.SavedChanges += handler = (object instance, EventArgs args) =>
                {
                    this.backgroundJobClient.Enqueue<IntegrationEventHandler>(
                        s => s.HandleApplicationEvent(
                            aggregate.TenantId,
                            aggregate.Id,
                            applicationEventTypes,
                            sequenceNumber,
                            null),
                        aggregate.ProductContext);
                };
            }
        }

        private bool HasExporters(ReleaseContext releaseContext, List<ApplicationEventType> applicationEventTypes)
        {
            var currentRelease = this.releaseQueryService.GetRelease(releaseContext);
            var integrationConfigurationModel = currentRelease.IntegrationsConfigurationModel;
            if (integrationConfigurationModel == null)
            {
                return false;
            }

            foreach (var applicationEventType in applicationEventTypes)
            {
                if (integrationConfigurationModel.GetExportersForEvent(applicationEventType).Any())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
