// <copyright file="ValidateBindPolicyCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.CustomPipelines.BindPolicy
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Handler for the bind policy command that verifies that the operation can continue.
    /// </summary>
    /// <typeparam name="TRequest">The request of type <see cref="BindPolicyCommand"/>.</typeparam>
    /// <typeparam name="TResponse">The response of type <see cref="Unit"/>.</typeparam>
    public class ValidateBindPolicyCommandHandler<TRequest, TResponse>
        : IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>
    {
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly IQuoteAggregateResolverService quoteResolverService;
        private readonly ICachingResolver cachingResolver;
        private readonly IInvoiceNumberRepository invoiceNumberRepository;
        private readonly ICreditNoteNumberRepository creditNoteNumberRepository;
        private readonly IPolicyNumberRepository policyNumberRepository;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;
        private readonly IPolicyService policyService;
        private readonly IClock clock;
        private readonly IAggregateLockingService aggregateLockingService;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;

        public ValidateBindPolicyCommandHandler(
            IProductFeatureSettingService productFeature,
            IQuoteWorkflowProvider quoteWorkflowProvider,
            IQuoteAggregateResolverService quoteAggregateResolver,
            ICachingResolver cachingResolver,
            IInvoiceNumberRepository invoiceNumberRepo,
            ICreditNoteNumberRepository creditNoteNumberRepo,
            IPolicyNumberRepository policyNumberRepo,
            IProductConfigurationProvider productConfigurationProvider,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            IClock clock,
            IPolicyService policyService,
            IAggregateLockingService aggregateLockingService,
            IQuoteAggregateRepository quoteAggregateRepository)
        {
            this.productFeatureSettingService = productFeature;
            this.quoteWorkflowProvider = quoteWorkflowProvider;
            this.quoteResolverService = quoteAggregateResolver;
            this.cachingResolver = cachingResolver;
            this.invoiceNumberRepository = invoiceNumberRepo;
            this.creditNoteNumberRepository = creditNoteNumberRepo;
            this.policyNumberRepository = policyNumberRepo;
            this.productConfigurationProvider = productConfigurationProvider;
            this.timeOfDayScheme = timeOfDayScheme;
            this.clock = clock;
            this.policyService = policyService;
            this.aggregateLockingService = aggregateLockingService;
            this.quoteAggregateRepository = quoteAggregateRepository;
        }

        public async Task<ValueTuple<NewQuoteReadModel, PolicyReadModel>> Handle(BindPolicyCommand command, RequestHandlerDelegate<ValueTuple<NewQuoteReadModel, PolicyReadModel>> next, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!command.QuoteId.HasValue)
            {
                return await next();
            }

            var quoteAggregateId = this.quoteResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId.Value);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.ReleaseContext.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.ReleaseContext.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                if (command.Environment.HasValue)
                {
                    if (quoteAggregate.Environment != command.Environment.Value)
                    {
                        throw new ErrorException(Errors.Policy.Issuance.EnvironmentMismatch(
                            command.QuoteId.Value, quoteAggregate.Environment, command.Environment.Value));
                    }
                }

                EntityHelper.ThrowIfNotFound(quoteAggregate, command.QuoteId.Value, "Quote");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId.Value);
                var productConfiguration = await this.productConfigurationProvider.GetProductConfiguration(
                    command.ReleaseContext, WebFormAppType.Quote);
                quote.ThrowIfGivenCalculationIdNotMatchingTheLatest(command.BindRequirements?.CalculationResultId);

                if (quote.Type == QuoteType.Renewal)
                {
                    var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(command.ReleaseContext.TenantId);
                    var isMutual = TenantHelper.IsMutual(tenantAlias);
                    this.ThrowIfInvalidPolicyPeriodStartDate(
                        quoteAggregate, quote, productConfiguration, command.BindRequirements, isMutual);
                    if (quoteAggregate.Policy == null)
                    {
                        throw new ErrorException(Errors.General.Unexpected($"Policy for aggregate with ID not found \"{quoteAggregate.Id}\"."));
                    }

                    this.ThrowIfRenewalIsNotAllowedAtTheCurrentTime(quoteAggregate.Policy);
                }

                if (!string.IsNullOrEmpty(command.PolicyNumber))
                {
                    await this.policyService.ThrowIfPolicyNumberInUse(
                        command.ReleaseContext.TenantId,
                        quoteAggregate.ProductId,
                        quoteAggregate.Environment,
                        command.PolicyNumber);
                }
                else
                {
                    await this.ThrowIfNoPolicyNumbersAvailable(
                        command.ReleaseContext,
                        quoteAggregate,
                        command.QuoteId.Value);
                }
                var now = this.clock.Now();
                if (quote.IsExpired(now))
                {
                    throw new ErrorException(Errors.Policy.Issuance.InvalidStateDetected(quote.QuoteStatus));
                }

                if (command.AllowBindingForIncompleteQuotes)
                {
                    if (quote.QuoteStatus.Equals(StandardQuoteStates.Nascent, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ErrorException(Errors.Operations.Bind.NotPermittedForQuoteState(quote.QuoteStatus));
                    }
                }
                else
                {
                    IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(command.ReleaseContext);
                    if (!quote.IsBindable(quoteWorkflow))
                    {
                        throw new ErrorException(Errors.Operations.Bind.NotPermittedForQuoteState(quote.QuoteStatus));
                    }
                }

                quote.ThrowIfGivenCalculationInvalidatesApprovedQuote(productConfiguration.FormDataSchema);
                var calculationResult = quote.LatestCalculationResult.Data;
                if (!calculationResult.IsBindable)
                {
                    throw new ErrorException(Errors.Operations.Bind.InvalidCalculationStateDetected(
                        calculationResult.CalculationResultState, CalculationResult.BindableState));
                }

                command.QuoteAggregate = quoteAggregate;
                return await next();
            }
        }

        private void ThrowIfInvalidPolicyPeriodStartDate(
            QuoteAggregate quoteAggregate, Quote quote, IProductConfiguration productConfiguration, BindRequirementDto? requirements, bool isMutual)
        {
            var calculationResult = quote.LatestCalculationResult.Data;
            var formData = quote.LatestFormData.Data;
            if (formData == default)
            {
                if (requirements == null)
                {
                    throw new ErrorException(Errors.Operations.Bind.CalculationIdNotProvided());
                }
                throw new ErrorException(
                    Errors.Operations.Bind.FormDataReferencedByCalculationIsNotFound(requirements.CalculationResultId));
            }

            var quoteDataRetriever = new StandardQuoteDataRetriever(productConfiguration, formData, calculationResult);

            var expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy expiry date")));
            var effectiveDateMaybe = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.EffectiveDate);
            if (!effectiveDateMaybe.HasValue)
            {
                // let's see if they gave an inception date instead
                effectiveDateMaybe = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate)
                    .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy adjustment effective date")));
            }

            var effectiveDate = effectiveDateMaybe.Value;
            LocalDateTime expiryDateTime = expiryDate.At(this.timeOfDayScheme.GetEndTime());
            Instant expiryTimestamp = expiryDateTime.InZoneLeniently(quote.TimeZone).ToInstant();
            LocalDateTime effectiveDateTime = effectiveDate.At(this.timeOfDayScheme.GetEndTime());
            Instant effectiveTimestamp = effectiveDateTime.InZoneLeniently(quote.TimeZone).ToInstant();

            if (quoteAggregate.Policy == null)
            {
                throw new ErrorException(Errors.General.Unexpected($"Policy for aggregate with ID not found \"{quoteAggregate.Id}\"."));
            }

            RenewalQuote.ValidateRenewalCanProceed(
                effectiveDateTime,
                effectiveTimestamp,
                expiryDateTime,
                expiryTimestamp,
                quoteAggregate.Policy,
                isMutual);
        }

        private async Task ThrowIfNoPolicyNumbersAvailable(
            ReleaseContext releaseContext,
            QuoteAggregate quoteAggregate,
            Guid quoteId)
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            var options = quote.GetBindingOptions(quoteWorkflow);
            if (options.HasFlag(BindOptions.Policy))
            {
                if (!this.policyNumberRepository.GetAvailableForProduct(
                    quoteAggregate.TenantId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment).Any())
                {
                    throw new ReferenceNumberUnavailableException(Errors.NumberPool.NoneAvailable(quoteAggregate.TenantId.ToString(), quoteAggregate.ProductId.ToString(), "policy"));
                }
            }

            if (options.HasFlag(BindOptions.TransactionRecord))
            {
                var calculationResult = quote.LatestCalculationResult.Data;
                if (calculationResult.PayablePrice.TotalPayable > 0)
                {
                    if (!this.invoiceNumberRepository.GetAvailableForProduct(
                        quoteAggregate.TenantId,
                        quoteAggregate.ProductId,
                        quoteAggregate.Environment).Any())
                    {
                        throw new ReferenceNumberUnavailableException(Errors.NumberPool.NoneAvailable(quoteAggregate.TenantId.ToString(), quoteAggregate.ProductId.ToString(), "invoice"));
                    }
                }
                else if (calculationResult.PayablePrice.TotalPayable < 0)
                {
                    if (!this.creditNoteNumberRepository.GetAvailableForProduct(
                        quoteAggregate.TenantId,
                        quoteAggregate.ProductId,
                        quoteAggregate.Environment).Any())
                    {
                        throw new ReferenceNumberUnavailableException(Errors.NumberPool.NoneAvailable(quoteAggregate.TenantId.ToString(), quoteAggregate.ProductId.ToString(), "credit note"));
                    }
                }
            }
        }

        private void ThrowIfRenewalIsNotAllowedAtTheCurrentTime(Policy policy)
        {
            var policyStatus = policy.GetPolicyStatus(this.clock.Now());
            if (policyStatus != PolicyStatus.Expired)
            {
                return;
            }

            var productFeature = this.productFeatureSettingService.GetProductFeature(policy.Aggregate.TenantId, policy.Aggregate.ProductId);
            var numberOfDaysToExpire = policy.GetDaysToExpire(this.clock.Today());
            var allowableDaysToRenewAfterExpiry = LocalDateExtensions.SecondsToDays(productFeature.ExpiredPolicyRenewalDurationInSeconds);
            var expiryDateIsWithInRenewalPeriod = numberOfDaysToExpire <= 0 && allowableDaysToRenewAfterExpiry >= Math.Abs(numberOfDaysToExpire);
            var isRenewalAllowedAtTheCurrentTime = policyStatus == PolicyStatus.Expired && (productFeature.IsRenewalAllowedAfterExpiry && expiryDateIsWithInRenewalPeriod);

            if (!isRenewalAllowedAtTheCurrentTime)
            {
                throw new ErrorException(Errors.Policy.Renewal.ExpiredPolicyRenewalNotAllowed(policy.PolicyNumber));
            }
        }
    }
}
