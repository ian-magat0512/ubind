// <copyright file="IqumulateFundingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.Iqumulate
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;

    /// <summary>
    /// Service for getting the mapping configurations for IQumulate Premium Funding.
    /// </summary>
    public class IqumulateFundingService : IIqumulateService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IFundingConfigurationProvider fundingConfigurationProvider;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="IqumulateFundingService"/> class.
        /// </summary>
        /// <param name="fundingConfigurationProvider">Service for loading the funding configuration.</param>
        /// <param name="quoteAggregateResolverService">The service to resolve the quote aggregate for a given quote ID.</param>
        /// <param name="customerAggregateRepository">The customer aggregate repository.</param>
        /// <param name="personAggregateRepository">Repository for retrieving person aggregates.</param>
        public IqumulateFundingService(
            IFundingConfigurationProvider fundingConfigurationProvider,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            ICustomerAggregateRepository customerAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.fundingConfigurationProvider = fundingConfigurationProvider;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.customerAggregateRepository = customerAggregateRepository;
            this.personAggregateRepository = personAggregateRepository;
        }

        /// <inheritdoc/>
        public bool PricingSupported => false;

        /// <inheritdoc/>
        public bool DirectDebitSupported => false;

        /// <inheritdoc/>
        public bool CreditCardSupported => false;

        /// <inheritdoc/>
        public bool CanAcceptWithoutRedirect => false;

        /// <inheritdoc/>
        public bool AcceptancePerformedViaApi => false;

        /// <inheritdoc/>
        public void ValidateConfiguration(IIqumulateConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.AffinitySchemeCode))
            {
                throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration("The configuration needs the Affinity Scheme Code."));
            }
        }

        /// <inheritdoc/>
        public Task<FundingProposal> AcceptFundingProposal(
            Domain.Aggregates.Quote.Quote quote,
            Guid fundingProposalId,
            IPaymentMethodDetails paymentMethodDetails,
            bool isTestData,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("iQumulate only supports accepting funding proposals externally.");
        }

        /// <inheritdoc/>
        public Task<FundingProposal> AcceptFundingProposal(Quote quote, Guid fundingProposalId, bool isTestData)
        {
            throw new NotSupportedException("iQumulate only supports accepting funding proposals externally.");
        }

        /// <inheritdoc/>
        public Task<FundingProposal> CreateFundingProposal(
            IProductContext productContext,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData,
            PriceBreakdown priceBreakdown, Domain.Aggregates.Quote.Quote quote,
            PaymentData paymentData,
            bool isTestData,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("iQumulate creates proposals on acceptance.");
        }

        /// <inheritdoc/>
        public Task<FundingProposal> UpdateFundingProposal(
            IProductContext productContext,
            string providerContractId,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData,
            PriceBreakdown priceBreakdown, Domain.Aggregates.Quote.Quote quote,
            PaymentData paymentData,
            bool isTestData,
            CancellationToken cancellationToken)
        {
            if (calculationResultData is null)
            {
                throw new ArgumentNullException(nameof(calculationResultData));
            }

            throw new NotSupportedException("iQumulate does not support updating funding proposals.");
        }

        /// <inheritdoc/>
        public async Task<IqumulateRequestData> GetIQumulateFundingRequestData(
            ReleaseContext releaseContext,
            Guid quoteId,
            Guid calculationResultId)
        {
            var maybeConfig = await this.fundingConfigurationProvider.GetConfigurationAsync(releaseContext);
            var configuration = maybeConfig.HasValue
                ? maybeConfig.Value as IqumulateConfiguration
                : null;

            if (configuration == null)
            {
                throw new ProductConfigurationException(
                    Errors.Product.MisConfiguration(
                    $"IQumulate Premium Funding is not configured for product {releaseContext.ProductId} in environment {releaseContext.Environment}."));
            }

            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(releaseContext.TenantId, quoteId);
            quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteId);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(quoteAggregate.TenantId);
            var isMutual = TenantHelper.IsMutual(tenantAlias);

            var noCustomerError = new ErrorException(Errors.Payment.Funding.NoCustomerFound(isMutual));
            if (!quoteAggregate.HasCustomer)
            {
                throw noCustomerError;
            }
            var customerId = quoteAggregate.CustomerId.GetValueOrThrow(noCustomerError);
            var customerAggregate = this.customerAggregateRepository.GetById(quoteAggregate.TenantId, customerId);
            customerAggregate = EntityHelper.ThrowIfNotFound(customerAggregate, customerId);
            quote.ThrowIfGivenCalculationIdNotMatchingTheLatest(calculationResultId);
            var calculationResult = quote.LatestCalculationResult.Data;
            var formData = quote.LatestFormData.Data;
            var dataRetriever = new IQumulateQuoteDataRetriever(configuration.QuoteDataLocations, formData, calculationResult);
            var quoteData = new IQumulateQuoteData()
            {
                General = dataRetriever.Retrieve<GeneralData>(IQumulateQuoteDataField.General),
                Client = dataRetriever.Retrieve<ClientData>(IQumulateQuoteDataField.Client),
                Introducer = dataRetriever.Retrieve<IntroducerData>(IQumulateQuoteDataField.Introducer),
                Policies = dataRetriever.Retrieve<List<Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.PolicyData>>(IQumulateQuoteDataField.Policies),
            };
            var personAggregate = this.personAggregateRepository.GetById(quoteAggregate.TenantId, customerAggregate.PrimaryPersonId);
            personAggregate = EntityHelper.ThrowIfNotFound(personAggregate, customerAggregate.PrimaryPersonId);
            return new IqumulateRequestData(quoteData, configuration, quote, personAggregate, isMutual);
        }
    }
}
