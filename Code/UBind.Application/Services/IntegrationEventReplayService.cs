// <copyright file="IntegrationEventReplayService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using UBind.Application.Export;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Processing;

    /// <summary>
    /// Service for replaying integration events.
    /// </summary>
    public class IntegrationEventReplayService : IIntegrationEventReplayService
    {
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IJobClient backgroundJobClient;
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationEventReplayService"/> class.
        /// The Application Operation Service.
        /// </summary>
        /// <param name="quoteAggregateRepository">The user aggregate repository.</param>
        /// <param name="releaseQueryService">Service for getting the current product release.</param>
        /// <param name="backgroundJobClient">Background job client.</param>
        public IntegrationEventReplayService(
            IQuoteAggregateRepository quoteAggregateRepository,
            IReleaseQueryService releaseQueryService,
            IJobClient backgroundJobClient,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.releaseQueryService = releaseQueryService;
            this.backgroundJobClient = backgroundJobClient;
        }

        /// <inheritdoc/>
        public async Task<List<string>> ReplayIntegrationEvents(Guid tenantId, Guid policyId, ApplicationEventType eventType, int sequenceNumber)
        {
            var quoteAggregate = await this.quoteAggregateRepository.GetByIdAtSequenceNumber(tenantId, policyId, sequenceNumber);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, policyId.ToString(), "policyId");
            var quote = quoteAggregate.GetQuoteBySequenceNumber(sequenceNumber);
            quote = EntityHelper.ThrowIfNotFound(quote, sequenceNumber.ToString(), "sequenceNumber");
            var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                quoteAggregate.TenantId,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quote.ProductReleaseId);
            var currentRelease = this.releaseQueryService.GetRelease(releaseContext);
            var customer = quote.LatestCustomerDetails?.Data;
            var customerMaskedName = customer != null
                ? customer.FullName != null ? customer.FullName.ToMaskedName() : string.Empty
                : string.Empty;
            var customerMaskedEmail = customer != null
                ? customer.Email != null ? PersonInformationHelper.GetMaskedEmail(customer.Email) : string.Empty
                : string.Empty;
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(quoteAggregate.TenantId);
            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(quoteAggregate.TenantId, quoteAggregate.ProductId);
            var integrationConfigurationModel = currentRelease.IntegrationsConfigurationModel;
            if (integrationConfigurationModel == null)
            {
                throw new ErrorException(Errors.Integrations.NoIntegrationsConfiguration(productAlias));
            }

            List<string> jobIds = new List<string>();
            foreach (var exporter in integrationConfigurationModel.GetExportersForEvent(eventType))
            {
                var jobId = this.backgroundJobClient.Enqueue<IntegrationEventHandler>(
                    s => s.Handle(
                        quoteAggregate.TenantId,
                        quoteAggregate.CreatedTimestamp.ToIso8601DateTimeInAet(),
                        Guid.NewGuid(),
                        eventType,
                        exporter.Id,
                        quote.Id,
                        sequenceNumber,
                        quoteAggregate.ProductId,
                        productAlias,
                        tenantAlias,
                        quoteAggregate.Environment,
                        null,
                        customerMaskedName,
                        customerMaskedEmail,
                        true),
                    quoteAggregate.TenantId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment);
                jobIds.Add(jobId);
            }

            return jobIds;
        }

        /// <inheritdoc/>
        public async Task<string> ReplayIntegrationEvent(
            Guid tenantId, Guid policyId, ApplicationEventType eventType, int sequenceNumber, string integrationId)
        {
            var quoteAggregate = await this.quoteAggregateRepository.GetByIdAtSequenceNumber(tenantId, policyId, sequenceNumber);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, "ID", policyId.ToString(), "QuoteAggregate");
            var quote = quoteAggregate.GetQuoteBySequenceNumber(sequenceNumber);
            quote = EntityHelper.ThrowIfNotFound(quote, "SequenceNumber", sequenceNumber.ToString(), "Quote");
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(quoteAggregate.TenantId);
            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(
                quoteAggregate.TenantId, quoteAggregate.ProductId);
            var customer = quote.LatestCustomerDetails?.Data;
            var customerMaskedName = customer != null
                ? customer.FullName != null ? customer.FullName.ToMaskedName() : string.Empty
                : string.Empty;
            var customerMaskedEmail = customer != null
                ? customer.Email != null ? PersonInformationHelper.GetMaskedEmail(customer.Email) : string.Empty
                : string.Empty;
            var jobId = this.backgroundJobClient.Enqueue<IntegrationEventHandler>(
                (s) => s.Handle(
                    quoteAggregate.TenantId,
                    quoteAggregate.CreatedTimestamp.ToIso8601DateTimeInAet(),
                    Guid.NewGuid(),
                    eventType,
                    integrationId,
                    quote.Id,
                    sequenceNumber,
                    quoteAggregate.ProductId,
                    productAlias,
                    tenantAlias,
                    quoteAggregate.Environment,
                    null,
                    customerMaskedName,
                    customerMaskedEmail,
                    true),
                quoteAggregate.TenantId,
                quoteAggregate.ProductId,
                quoteAggregate.Environment);
            return jobId;
        }
    }
}
