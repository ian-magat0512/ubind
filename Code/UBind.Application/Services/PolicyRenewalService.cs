// <copyright file="PolicyRenewalService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System.Threading.Tasks;
    using Humanizer;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Services;

    /// <inheritdoc/>
    public class PolicyRenewalService : IPolicyRenewalService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IInvoiceNumberRepository invoiceNumberRepository;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;

        public PolicyRenewalService(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IQuoteWorkflowProvider quoteWorkflowProvider,
            ICachingResolver cachingResolver,
            IQuoteAggregateRepository quoteAggregateRepository,
            IInvoiceNumberRepository invoiceNumberRepository,
            IProductConfigurationProvider productConfigurationProvider,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
        {
            this.cachingResolver = cachingResolver;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteWorkflowProvider = quoteWorkflowProvider;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.invoiceNumberRepository = invoiceNumberRepository;
            this.productConfigurationProvider = productConfigurationProvider;
            this.timeOfDayScheme = timeOfDayScheme;
        }

        public async Task<QuoteAggregate> RenewPolicyWithQuote(
            Guid tenantId,
            Guid quoteAggregateId,
            CalculationResult calculationResult,
            FormData finalFormData,
            DateTimeZone timeZone,
            ReleaseContext releaseContext)
        {
            QuoteAggregate? quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, quoteAggregateId);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
            var latestQuote = quoteAggregate.GetLatestQuote();
            latestQuote = EntityHelper.ThrowIfNotFound(latestQuote, quoteAggregateId, "quote");
            await this.EnsureReferenceNumbersAvailableOrThrow(latestQuote, releaseContext);

            var customerData = quoteAggregate.Quotes
                .Reverse()
                .Select(q => q.LatestCustomerDetails?.Data)
                .FirstOrDefault(data => data != null);
            customerData = EntityHelper.ThrowIfNotFound(customerData, "customerData");
            await this.RenewPolicy(
                releaseContext,
                finalFormData,
                calculationResult,
                timeZone,
                quoteAggregate,
                latestQuote.Id,
                latestQuote.QuoteNumber,
                customerData);
            await this.ProgressQuoteState(
                quoteAggregate, QuoteAction.Policy, releaseContext);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quoteAggregate;
        }

        public async Task<QuoteAggregate> RenewPolicyWithoutQuote(
            Guid tenantId,
            Guid quoteAggregateId,
            IPolicyReadModelDetails policy,
            CalculationResult calculationResult,
            FormData finalFormData,
            DateTimeZone timeZone,
            ReleaseContext releaseContext)
        {
            var customerData = new PersonalDetails(
                policy.TenantId,
                new PersonCommonProperties
                {
                    Id = policy.CustomerId.GetValueOrDefault(),
                    FullName = policy.CustomerFullName,
                    OrganisationId = policy.OrganisationId,
                });

            QuoteAggregate? quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, quoteAggregateId);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quoteAggregate");
            var latestQuote = quoteAggregate.GetLatestQuote();
            latestQuote = EntityHelper.ThrowIfNotFound(latestQuote, quoteAggregateId, "quote");
            await this.EnsureReferenceNumbersAvailableOrThrow(latestQuote, releaseContext);
            await this.RenewPolicy(
                releaseContext,
                finalFormData,
                calculationResult,
                timeZone,
                quoteAggregate,
                null,
                null,
                customerData);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quoteAggregate;
        }

        private async Task RenewPolicy(
            ReleaseContext releaseContext,
            FormData finalFormData,
            CalculationResult calculationResult,
            DateTimeZone timeZone,
            QuoteAggregate quoteAggregate,
            Guid? quoteId,
            string? quoteNumber,
            IPersonalDetails customerData)
        {
            var timestamp = this.clock.Now();
            var calcResult = new QuoteDataUpdate<CalculationResult>(Guid.NewGuid(), calculationResult, timestamp);

            var quoteDataSnapshot = this.GetQuoteDataSnapshot(finalFormData, calcResult, customerData);

            var calculationData = new CachingJObjectWrapper(calcResult.Data.Json);

            var configuration = await this.productConfigurationProvider.GetProductConfiguration(releaseContext, WebFormAppType.Quote);
            var quoteDataRetriever = new StandardQuoteDataRetriever(configuration, finalFormData, calculationData);

            var expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy expiry date")));

            var effectiveDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.EffectiveDate)
                ?? quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy adjustment effective date")));

            var expiryDateTime = expiryDate.At(this.timeOfDayScheme.GetEndTime());
            var expiryTimestamp = expiryDateTime.InZoneLeniently(timeZone).ToInstant();
            var effectiveDateTime = effectiveDate.At(this.timeOfDayScheme.GetEndTime());
            var effectiveTimestamp = effectiveDateTime.InZoneLeniently(timeZone).ToInstant();

            var parentPolicy = quoteAggregate.Policy!;
            if (effectiveDateTime != parentPolicy.ExpiryDateTime
                && effectiveDateTime.Date.Equals(parentPolicy.ExpiryDateTime.GetValueOrDefault().Date))
            {
                // if the renewal date is correct but the time is different, let's just adjust the
                // time to match the policy expiry time.
                effectiveDateTime = parentPolicy.ExpiryDateTime.GetValueOrDefault();
                effectiveTimestamp = parentPolicy.ExpiryTimestamp.GetValueOrDefault();
            }

            bool isMutual = await this.IsMutualTenant(releaseContext.TenantId);
            RenewalQuote.ValidateRenewalCanProceed(
                effectiveDateTime,
                effectiveTimestamp,
                expiryDateTime,
                expiryTimestamp,
                parentPolicy,
                isMutual);

            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var @event = new QuoteAggregate.PolicyRenewedEvent(
                quoteAggregate.TenantId,
                quoteAggregate.Id,
                quoteId,
                quoteNumber,
                effectiveDateTime,
                effectiveTimestamp,
                expiryDateTime,
                expiryTimestamp,
                quoteDataSnapshot,
                performingUserId,
                timestamp,
                null);

            quoteAggregate.ApplyNewEvent(@event);
        }

        private QuoteDataSnapshot GetQuoteDataSnapshot(FormData finalFormData, QuoteDataUpdate<CalculationResult> calcResult, IPersonalDetails customerData)
        {
            var timestamp = this.clock.Now();
            var data = new QuoteDataUpdate<FormData>(Guid.NewGuid(), finalFormData, timestamp);

            var customerDetails = new QuoteDataUpdate<IPersonalDetails>(Guid.NewGuid(), customerData, timestamp);
            var quoteDataSnapshot = new QuoteDataSnapshot(data, calcResult, customerDetails);
            return quoteDataSnapshot;
        }

        private async Task<bool> IsMutualTenant(Guid tenantId)
        {
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
            return TenantHelper.IsMutual(tenantAlias);
        }

        private async Task ProgressQuoteState(
            QuoteAggregate quoteAggregate,
            QuoteAction quoteActions,
            ReleaseContext releaseContext)
        {
            var quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            var quoteStatus = QuoteStatus.Complete.Humanize();
            var resultingState = quoteWorkflow.GetResultingState(quoteActions, quoteStatus);
            var stateChangeEvent = new QuoteAggregate.QuoteStateChangedEvent(
                quoteAggregate.TenantId,
                quoteAggregate.Id,
                quoteAggregate.GetLatestQuote().Id,
                quoteActions,
                this.httpContextPropertiesResolver.PerformingUserId,
                quoteStatus,
                resultingState,
                this.clock.Now());
            quoteAggregate.ApplyNewEvent(stateChangeEvent);
        }

        private async Task EnsureReferenceNumbersAvailableOrThrow(Domain.Aggregates.Quote.Quote quote, ReleaseContext releaseContext)
        {
            var quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            var options = quote.GetBindingOptions(quoteWorkflow);
            if (options.HasFlag(BindOptions.TransactionRecord) || options.HasFlag(BindOptions.PolicyAndTransactionRecord))
            {
                var availableNumbers = this.invoiceNumberRepository.GetAvailableForProduct(
                    releaseContext.TenantId,
                    releaseContext.ProductId,
                    releaseContext.Environment);
                if (!availableNumbers.Any())
                {
                    var product = await this.cachingResolver.GetProductOrNull(
                        new GuidOrAlias(releaseContext.TenantId), new GuidOrAlias(releaseContext.ProductId));
                    throw new ErrorException(Domain.Errors.Automation.RenewPolicyAction.InvoiceNumberNotAvailable(
                        product?.Details?.Name, releaseContext.Environment));
                }
            }
        }
    }
}
