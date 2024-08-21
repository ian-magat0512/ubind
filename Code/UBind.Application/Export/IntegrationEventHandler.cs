// <copyright file="IntegrationEventHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Threading.Tasks;
    using Hangfire.Console;
    using Hangfire.Server;
    using StackExchange.Profiling;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    /// <inheritdoc/>
    public class IntegrationEventHandler : IIntegrationEventHandler
    {
        private readonly IIntegrationConfigurationProvider configurationProvider;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly ICachingResolver cachingResolver;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationEventHandler"/> class.
        /// </summary>
        /// <param name="productConfigurationProvider">Product configuration provider.</param>
        /// <param name="configurationProvider">Provides the integration configuration.</param>
        /// <param name="quoteAggregateResolverService">The service to resolve the quote aggregate for a given quote ID.</param>
        public IntegrationEventHandler(
            IProductConfigurationProvider productConfigurationProvider,
            IIntegrationConfigurationProvider configurationProvider,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            ICachingResolver cachingResolver,
            IReleaseQueryService releaseQueryService,
            IQuoteAggregateRepository quoteAggregateRepository)
        {
            this.configurationProvider = configurationProvider;
            this.productConfigurationProvider = productConfigurationProvider;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.cachingResolver = cachingResolver;
            this.releaseQueryService = releaseQueryService;
            this.quoteAggregateRepository = quoteAggregateRepository;
        }

        /// <inheritdoc/>
        [DisplayName("Integration Event | TENANT: {9}, PRODUCT: {8}, ENVIRONMENT: {10}, EVENT: '{3}', SEQUENCE: {6}, INTEGRATION: {4}, QUOTE ID: {5}, CUSTOMER NAME: {12}, EMAIL: {13}, TIMESTAMP: {1}")]
        public async Task Handle(
            Guid tenantId,
            string timestamp,
            Guid integrationEventId,
            ApplicationEventType eventType,
            string integrationId,
            Guid quoteId,
            int eventSequenceNumber,
            Guid productId,
            string productAlias,
            string tenantAlias,
            DeploymentEnvironment environment,
            PerformContext? context,
            string? customerName,
            string? customerEmail,
            bool isRetriggering = false)
        {
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
            if (quoteAggregate == null)
            {
                throw new NotFoundException(Errors.General.NotFound("quote", quoteId));
            }

            var quote = quoteAggregate.GetQuoteBySequenceNumber(eventSequenceNumber);
            var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                tenantId,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId);

            await this.Handle(
                releaseContext,
                quoteAggregate,
                integrationEventId,
                eventType,
                integrationId,
                eventSequenceNumber,
                productAlias,
                tenantAlias,
                customerName,
                customerEmail,
                context);
        }

        public async Task HandleApplicationEvent(
            Guid tenantId,
            Guid quoteAggregateId,
            List<ApplicationEventType> applicationEventTypes,
            int eventSequenceNumber,
            PerformContext context)
        {
            // TEMPORARY - wait before processing the event to ensure that the quote aggregate has been persisted
            // This is not the correct solution, but it's quick fix. Full fix will be in UB-10052.
            await Task.Delay(2000);

            using (MiniProfiler.Current.Step($"QuoteEventIntegrationScheduler HandleApplicationEvent"))
            {
                var aggregate = this.quoteAggregateRepository.GetById(tenantId, quoteAggregateId);
                var quote = aggregate.GetQuoteBySequenceNumber(eventSequenceNumber);
                if (quote == null)
                {
                    // if the quote aggregate doesn't have a quote, then it might have directly just been created
                    // with a policy. This is not supported for integrations.
                    return;
                }

                var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                    tenantId,
                    aggregate.ProductId,
                    aggregate.Environment,
                    quote.ProductReleaseId);
                var currentRelease = this.releaseQueryService.GetRelease(releaseContext);
                var integrationConfigurationModel = currentRelease.IntegrationsConfigurationModel;
                if (integrationConfigurationModel == null)
                {
                    return;
                }

                var customer = quote.LatestCustomerDetails?.Data;
                var customerMaskedName = customer?.FullName?.ToMaskedName() ?? string.Empty;
                var customerMaskedEmail = customer?.Email != null ? PersonInformationHelper.GetMaskedEmail(customer.Email) : string.Empty;
                var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(aggregate.TenantId);
                var productAlias = this.cachingResolver.GetProductAliasOrThrow(aggregate.TenantId, aggregate.ProductId);

                foreach (var applicationEventType in applicationEventTypes)
                {
                    foreach (var exporter in integrationConfigurationModel?.GetExportersForEvent(applicationEventType))
                    {
                        using (MiniProfiler.Current.Step($"Process integration for event \"{applicationEventType}\""))
                        {
                            await this.Handle(
                                releaseContext,
                                aggregate,
                                Guid.NewGuid(),
                                applicationEventType,
                                exporter.Id,
                                eventSequenceNumber,
                                productAlias,
                                tenantAlias,
                                customerMaskedName,
                                customerMaskedEmail,
                                context);
                        }
                    }
                }
            }
        }

        private async Task Handle(
            ReleaseContext releaseContext,
            QuoteAggregate quoteAggregate,
            Guid integrationEventId,
            ApplicationEventType eventType,
            string integrationId,
            int eventSequenceNumber,
            string productAlias,
            string tenantAlias,
            string? customerName,
            string? customerEmail,
            PerformContext context,
            bool isRetriggering = false)
        {
            var jobId = context.BackgroundJob.Id;
            var quote = quoteAggregate.GetQuoteBySequenceNumber(eventSequenceNumber);
            if (quote == null)
            {
                // if the quote aggregate doesn't have a quote, then it might have directly just been created
                // with a policy. This is not supported for integrations.
                return;
            }

            var timestamp = quoteAggregate.CreatedTimestamp.ToIso8601DateTimeInAet();
            var tenantId = quoteAggregate.TenantId;
            var productId = quoteAggregate.ProductId;
            var environment = quoteAggregate.Environment;
            StringBuilder logInfo = new StringBuilder();
            logInfo.AppendLine($"TIMESTAMP: {timestamp}");
            logInfo.AppendLine($"TENANT ID: {tenantId}");
            logInfo.AppendLine($"PRODUCT ID: {productId}");
            logInfo.AppendLine($"TENANT: {tenantAlias}");
            logInfo.AppendLine($"PRODUCT: {productAlias}");
            logInfo.AppendLine($"ENVIRONMENT: {environment}");
            logInfo.AppendLine($"EVENT: '{eventType}'");
            logInfo.AppendLine($"SEQUENCE: {eventSequenceNumber}");
            logInfo.AppendLine($"INTEGRATION: {integrationId}");
            logInfo.AppendLine($"QUOTE AGGREGATE ID: {quoteAggregate.Id}");
            logInfo.AppendLine($"QUOTE ID: {quote.Id}");

            if (quoteAggregate.Policy?.PolicyId != default)
            {
                logInfo.AppendLine($"POLICY ID: {quoteAggregate.Policy.PolicyId}");
            }

            logInfo.AppendLine($"CUSTOMER NAME: {customerName}");
            logInfo.AppendLine($"EMAIL: {customerEmail}");
            context.WriteLine(logInfo);

            var productConfig = await this.productConfigurationProvider.GetProductConfiguration(
                releaseContext,
                WebFormAppType.Quote);

            var config = await this.configurationProvider.GetIntegrationConfigurationAsync(
                releaseContext,
                productConfig);

            await config.ExecuteIntegration(
                integrationId,
                new ApplicationEvent(
                    integrationEventId,
                    eventType,
                    quoteAggregate,
                    quote.Id,
                    eventSequenceNumber,
                    jobId,
                    releaseContext.ProductReleaseId,
                    isRetriggering));
        }
    }
}
