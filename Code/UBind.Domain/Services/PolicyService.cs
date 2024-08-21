// <copyright file="PolicyService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.ValueTypes;

    public class PolicyService : IPolicyService
    {
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly ICachingResolver cachingResolver;
        private readonly IUniqueIdentifierService uniqueIdentifierService;
        private readonly IQuoteReferenceNumberGenerator quoteReferenceNumberGenerator;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly ISystemAlertService systemAlertService;
        private readonly IQuoteDocumentReadModelRepository quoteDocumentRepository;
        private readonly IPolicyNumberRepository policyNumberRepository;
        private readonly IUBindDbContext dbContext;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;
        private readonly IClock clock;

        public PolicyService(
            IQuoteAggregateRepository quoteAggregateRepository,
            IClock clock,
            ICachingResolver cachingResolver,
            IClaimReadModelRepository claimReadModelRepository,
            IProductConfigurationProvider productConfigurationProvider,
            IUniqueIdentifierService uniqueIdentifierService,
            IQuoteDocumentReadModelRepository quoteDocumentRepository,
            IPolicyNumberRepository policyNumberSource,
            IPolicyReadModelRepository policyReadModelRepository,
            ISystemAlertService systemAlertService,
            IQuoteReferenceNumberGenerator numberGenerator,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IProductFeatureSettingService productFeatureSettingService,
            IQuoteWorkflowProvider quoteWorkflowProvider,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.clock = clock;
            this.cachingResolver = cachingResolver;
            this.claimReadModelRepository = claimReadModelRepository;
            this.productConfigurationProvider = productConfigurationProvider;
            this.uniqueIdentifierService = uniqueIdentifierService;
            this.quoteDocumentRepository = quoteDocumentRepository;
            this.policyNumberRepository = policyNumberSource;
            this.policyReadModelRepository = policyReadModelRepository;
            this.systemAlertService = systemAlertService;
            this.quoteReferenceNumberGenerator = numberGenerator;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.productFeatureSettingService = productFeatureSettingService;
            this.quoteWorkflowProvider = quoteWorkflowProvider;
            this.timeOfDayScheme = timeOfDayScheme;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IClaimReadModel> GetPolicyHistoricalClaims(
            Policy policy)
        {
            return this.claimReadModelRepository
                     .GetClaimsByPolicyNumberInPastFiveYears(
                         policy.Aggregate.TenantId,
                         policy.Aggregate.ProductId,
                         policy.PolicyNumber)
                     .Where(pc => pc.Status != ClaimState.Nascent)
                     .ToList();
        }

        public async Task<string> GenerateQuoteNumber(ReleaseContext releaseContext)
        {
            var configuration = await this.productConfigurationProvider.GetProductConfiguration(
                releaseContext, WebFormAppType.Quote);
            if (configuration.QuoteNumberSource == NumberSource.Preloaded)
            {
                var tenant = await this.cachingResolver.GetTenantOrThrow(releaseContext.TenantId);
                return await this.uniqueIdentifierService.ConsumeUniqueIdentifier(
                    IdentifierType.Quote,
                    tenant,
                    releaseContext.ProductId,
                    releaseContext.Environment);
            }

            this.quoteReferenceNumberGenerator.SetProperties(
                releaseContext.TenantId,
                releaseContext.ProductId,
                releaseContext.Environment);
            return this.quoteReferenceNumberGenerator.Generate();
        }

        public async Task IssuePolicy(
            Guid calculationResultId,
            Guid? performingUserId,
            QuoteAggregate quoteAggregate,
            NewBusinessQuote newBusinessQuote,
            Domain.Configuration.IProductConfiguration productConfiguration,
            IQuoteWorkflow quoteWorkflow,
            bool progressQuoteState = true,
            string? externalPolicyNumber = null)
        {
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(quoteAggregate.TenantId);
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            var policyNumberCallback = () => externalPolicyNumber.IsNullOrEmpty()
                ? this.ConsumePolicyNumber(quoteAggregate.ProductContext)
                : this.ConsumeCustomPolicyNumber(quoteAggregate, externalPolicyNumber);
            newBusinessQuote.IssuePolicy(
                calculationResultId,
                policyNumberCallback,
                productConfiguration,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                performingUserId,
                this.clock.Now(),
                quoteWorkflow,
                progressQuoteState);
        }

        public async Task IssuePolicy(
            Guid calculationResultId,
            Guid? performingUserId,
            QuoteAggregate quoteAggregate,
            NewBusinessQuote newBusinessQuote,
            Domain.Configuration.IProductConfiguration productConfiguration,
            IQuoteWorkflow quoteWorkflow)
        {
            await this.IssuePolicy(
                calculationResultId, performingUserId, quoteAggregate, newBusinessQuote, productConfiguration, quoteWorkflow, true);
        }

        /// <inheritdoc/>
        public bool HasPolicy(Guid policyId, Guid? tenantId = null)
        {
            if (tenantId == null)
            {
                return this.policyReadModelRepository.HasPolicy(policyId);
            }
            else
            {
                return this.policyReadModelRepository.HasPolicyForTenant(tenantId.Value, policyId);
            }
        }

        /// <inheritdoc/>
        public async Task<IPolicyReadModelDetails> GetPolicy(Guid tenantId, Guid policyId)
        {
            var policy = this.policyReadModelRepository.GetPolicyDetails(tenantId, policyId);
            if (policy == null)
            {
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                var isMutual = TenantHelper.IsMutual(tenantAlias);
                throw new ErrorException(Errors.General
                    .NotFound(TenantHelper.CheckAndChangeTextToMutual("policy", isMutual), policyId));
            }

            return policy;
        }

        /// <inheritdoc/>
        public IPolicyReadModelDetails GetPolicyTransactionForUser(
            Guid tenantId, Guid policyTransactionId)
        {
            var policy = this.policyReadModelRepository.GetPolicyTransactionDetails(tenantId, policyTransactionId);

            return policy;
        }

        /// <inheritdoc/>
        public IFileContentReadModel GetPolicyDocumentContent(
            Guid tenantId,
            Guid documentId)
        {
            return this.quoteDocumentRepository.GetDocumentContent(tenantId, documentId);
        }

        /// <inheritdoc/>
        public bool HasPoliciesForCustomer(PolicyReadModelFilters filters, IEnumerable<Guid> excludedPolicyIds)
        {
            return this.policyReadModelRepository.HasPoliciesForCustomer(filters, excludedPolicyIds);
        }

        /// <inheritdoc/>
        public async Task UpdatePastClaimsIfNotApproved(Quote quote)
        {
            var now = this.clock.Now();
            var quoteState = quote.QuoteStatus;
            if (quoteState.EqualsIgnoreCase(StandardQuoteStates.Approved)
                || quote.IsExpired(now)
                || quoteState.EqualsIgnoreCase(StandardQuoteStates.Complete)
                || quote.Aggregate.Policy == null)
            {
                // we don't want to update anything about a quote which has been approved, or is expired, or complete.
                // This is important since an approved quote could have been endorsed by an underwriter so we don't have
                // any authority to change anything after that.
                return;
            }

            var pastClaims = this.GetPolicyHistoricalClaims(quote.Aggregate.Policy);
            var newFormData = quote.LatestFormData != null
                ? quote.LatestFormData.Data.Clone()
                : new FormData("{}");
            newFormData.AddRecentClaims(pastClaims);
            quote.UpdateFormData(
                newFormData, this.httpContextPropertiesResolver.PerformingUserId, now);
            await this.quoteAggregateRepository.Save(quote.Aggregate);
        }

        public bool IsRenewalAllowedAtTheCurrentTime(IPolicyReadModelDetails policyDetails, bool isMutual)
        {
            var policyStatus = policyDetails.GetPolicyStatus(this.clock.Now());
            if (policyStatus != PolicyStatus.Expired)
            {
                return true;
            }

            var productFeature = this.productFeatureSettingService.GetProductFeature(((IReadModel<Guid>)policyDetails).TenantId, policyDetails.ProductId);
            var numberOfDaysToExpire = policyDetails.GetDaysToExpire(this.clock.Today());
            return IsRenewalAllowed(policyStatus, productFeature, numberOfDaysToExpire);
        }

        /// <inheritdoc/>
        public string ConsumePolicyNumber(IProductContext productContext)
        {
            var policyNumber = this.policyNumberRepository.ConsumeForProduct(
                productContext.TenantId,
                productContext.ProductId,
                productContext.Environment);
            this.systemAlertService.QueuePolicyNumberThresholdAlertCheck(
                 productContext.TenantId,
                 productContext.ProductId,
                 productContext.Environment);
            return policyNumber;
        }

        /// <inheritdoc/>
        public string ConsumePolicyNumberAndPersist(IProductContext productContext)
        {
            var policyNumber = this.policyNumberRepository.ConsumeAndSave(productContext);
            this.systemAlertService.QueuePolicyNumberThresholdAlertCheck(
                 productContext.TenantId,
                 productContext.ProductId,
                 productContext.Environment);
            return policyNumber;
        }

        /// <inheritdoc/>
        public void UnConsumePolicyNumberAndPersist(IProductContext productContext, string number)
        {
            this.policyNumberRepository.UnconsumeAndSave(productContext, number);
        }

        public bool IsRefundAllowed(Policy policy, StandardQuoteDataRetriever quoteDataRetriever, ProductFeatureSetting productFeatureSetting)
        {
            switch (productFeatureSetting.RefundRule)
            {
                case RefundRule.RefundsAreAlwaysProvided:
                    return true;
                case RefundRule.RefundsAreNeverProvided:
                    return false;
                case RefundRule.RefundsAreProvidedIfNoClaimsWereMade:
                    return !this.HasClaimsMadeDuringSpecificPeriod(policy, productFeatureSetting);
                case RefundRule.RefundsCanOptionallyBeProvided:
                    var isRefundApproved = quoteDataRetriever.Retrieve(StandardQuoteDataField.IsRefundApproved);
                    if (isRefundApproved == null)
                    {
                        throw new ErrorException(Errors.Product.MisConfiguration("cancellation provideRefund was not set."));
                    }

                    return bool.Parse(isRefundApproved);
                default: throw new ErrorException(Errors.RefundRules.InvalidCancellationRefundRule(productFeatureSetting.RefundRule));
            }
        }

        public async Task ThrowIfPolicyNumberInUse(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            string policyNumber)
        {
            if (string.IsNullOrEmpty(policyNumber))
            {
                return;
            }

            var policy = this.policyReadModelRepository.GetPolicyByNumber(tenantId, productId, environment, policyNumber);

            if (policy != null)
            {
                var product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);
                throw new ErrorException(
                        Errors.Policy.Issuance.PolicyIssuancePolicyNumberNotUnique(environment, product.Details.Name, policyNumber));
            }
        }

        public async Task<string> CompletePolicyTransaction(
            ReleaseContext releaseContext,
            Quote quote,
            Guid calculationResultId,
            CancellationToken cancellationToken,
            FormData? formData = null,
            string? externalPolicyNumber = null,
            bool progressQuoteState = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = this.clock.Now();
            if (quote.IsExpired(now))
            {
                throw new ErrorException(Errors.Policy.Issuance.InvalidStateDetected(quote.QuoteStatus));
            }

            var productConfiguration = await this.productConfigurationProvider
                .GetProductConfiguration(releaseContext, WebFormAppType.Quote);
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(releaseContext.TenantId);
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            var quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            if (formData != null)
            {
                quote.UpdateFormData(formData, performingUserId, now);
            }

            var newPolicyNumber = string.Empty;
            switch (quote)
            {
                case NewBusinessQuote newBusinessQuote:
                    if (externalPolicyNumber.IsNullOrEmpty())
                    {
                        newPolicyNumber = this.policyNumberRepository.ConsumeAndSave(quote.ProductContext);
                        await this.systemAlertService.QueuePolicyNumberThresholdAlertCheck(
                             releaseContext.TenantId,
                             releaseContext.ProductId,
                             releaseContext.Environment);
                    }
                    else
                    {
                        newPolicyNumber = externalPolicyNumber;
                    }

                    newBusinessQuote.IssuePolicy(
                        calculationResultId,
                        () =>
                        newPolicyNumber,
                        productConfiguration,
                        new DefaultPolicyTransactionTimeOfDayScheme(),
                        performingUserId,
                        this.clock.Now(),
                        quoteWorkflow,
                        progressQuoteState);
                    break;
                case AdjustmentQuote adjustmentQuote:
                    adjustmentQuote.AdjustPolicy(
                        calculationResultId,
                        productConfiguration,
                        this.timeOfDayScheme,
                        performingUserId,
                        now,
                        quoteWorkflow,
                        isMutual,
                        progressQuoteState);
                    break;
                case RenewalQuote renewalQuote:
                    renewalQuote.RenewPolicy(
                        calculationResultId,
                        productConfiguration,
                        this.timeOfDayScheme,
                        performingUserId,
                        now,
                        quoteWorkflow,
                        isMutual,
                        progressQuoteState);
                    break;
                case CancellationQuote cancellationQuote:
                    cancellationQuote.CancelPolicy(
                        calculationResultId,
                        productConfiguration,
                        this.timeOfDayScheme,
                        performingUserId,
                        now,
                        quoteWorkflow,
                        isMutual,
                        progressQuoteState);
                    break;
            }

            return newPolicyNumber;
        }

        private static bool IsRenewalAllowed(PolicyStatus policyStatus, ProductFeatureSetting productFeature, int numberOfDaysToExpire)
        {
            var allowableDaysToRenewAfterExpiry = LocalDateExtensions.SecondsToDays(productFeature.ExpiredPolicyRenewalDurationInSeconds);
            var expiryDateIsWithInRenewalPeriod = numberOfDaysToExpire <= 0 && allowableDaysToRenewAfterExpiry >= Math.Abs(numberOfDaysToExpire);
            var isRenewalAllowedAtTheCurrentTime = policyStatus == PolicyStatus.Expired && (productFeature.IsRenewalAllowedAfterExpiry && expiryDateIsWithInRenewalPeriod);
            return isRenewalAllowedAtTheCurrentTime;
        }

        private string ConsumeCustomPolicyNumber(QuoteAggregate quoteAggregate, string? externalPolicyNumber)
        {
            var policyNumber = this.policyNumberRepository.ConsumePolicyNumber(quoteAggregate.TenantId, quoteAggregate.ProductId,
                                externalPolicyNumber, quoteAggregate.Environment);
            return policyNumber;
        }

        private bool HasClaimsMadeDuringSpecificPeriod(Policy policy, ProductFeatureSetting productFeatureSetting)
        {
            switch (productFeatureSetting.PeriodOfNoClaimsToQualifyForRefund)
            {
                case PolicyPeriodCategory.CurrentPolicyPeriod:
                    return this.HasClaimsMadeDuringCurrentPolicyPeriod(policy);
                case PolicyPeriodCategory.LifeTimeOfThePolicy:
                    return this.HasClaimsMadeWithinPolicyLifetime(policy);
                case PolicyPeriodCategory.LastNumberOfYears:
                    var hasClaimsMadeDuringLastNumberOfYears = this.HasClaimsMadeDuringLastNumberOfYears(policy, productFeatureSetting.LastNumberOfYearsOfNoClaimsToQualifyForRefund);
                    if (productFeatureSetting.LastNumberOfYearsOfNoClaimsToQualifyForRefund == null || productFeatureSetting.LastNumberOfYearsOfNoClaimsToQualifyForRefund <= 0)
                    {
                        throw new ErrorException(Errors.RefundRules.InvalidLastNumberOfYears(productFeatureSetting.LastNumberOfYearsOfNoClaimsToQualifyForRefund));
                    }

                    return hasClaimsMadeDuringLastNumberOfYears;
                case PolicyPeriodCategory.AtAnyTime:
                    return this.HasClaimMadeAtAnyTime(policy);
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(
                        productFeatureSetting.PeriodOfNoClaimsToQualifyForRefund, typeof(PolicyPeriodCategory)));
            }
        }

        private bool HasClaimsMadeDuringCurrentPolicyPeriod(Policy policy)
        {
            return this.HasClaimMadeAgainstPolicy(
                policy,
                policy.LatestPolicyPeriodStartDateTime,
                policy.ExpiryDateTime);
        }

        private bool HasClaimsMadeWithinPolicyLifetime(Policy policy)
        {
            return this.HasClaimMadeAgainstPolicy(
                policy,
                policy.InceptionDateTime,
                policy.ExpiryDateTime);
        }

        private bool HasClaimsMadeDuringLastNumberOfYears(Policy policy, int? lastNumberOfYearsWhichNoClaimsMade)
        {
            var startDate = this.clock.Now().ToLocalDateInAet().Minus(Period.FromYears((int)lastNumberOfYearsWhichNoClaimsMade));
            var endDate = this.clock.Now().ToLocalDateInAet();
            return this.HasClaimMadeAgainstPolicy(
                policy,
                startDate.At(LocalTime.MinValue),
                endDate.At(LocalTime.MaxValue));
        }

        private bool HasClaimMadeAtAnyTime(Policy policy)
        {
            return this.HasClaimMadeAgainstPolicy(policy);
        }

        private bool HasClaimMadeAgainstPolicy(
             Policy policy, LocalDateTime? startDateTime = null, LocalDateTime? endDateTime = null)
        {
            var filters = new EntityListFilters
            {
                PolicyId = policy.PolicyId,
                TenantId = policy.TenantId,
            };

            var claimsMade = this.claimReadModelRepository.ListAllClaimsByCustomer(policy.TenantId, (Guid)policy.Aggregate.CustomerId, filters)
            .Where(c => (!c.Status.EqualsIgnoreCase(ClaimState.Nascent)
                && !c.Status.EqualsIgnoreCase(ClaimState.Withdrawn)
                && !c.Status.EqualsIgnoreCase(ClaimState.Declined)));

            if (startDateTime.HasValue && endDateTime.HasValue)
            {
                claimsMade = claimsMade.Where(c => c.IncidentDateTime == null
                    || (c.IncidentDateTime >= startDateTime.Value && c.IncidentDateTime <= endDateTime.Value));
            }

            return claimsMade.Any();
        }
    }
}
