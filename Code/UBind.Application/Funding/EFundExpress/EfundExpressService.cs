// <copyright file="EfundExpressService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.EFundExpress
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using NodaTime;
    using Polly;
    using Polly.Retry;
    using StackExchange.Profiling;
    using UBind.Application.ConnectedServices.Funding.EFundExpress;
    using UBind.Application.Exceptions;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Funding service using eFundExpress premium funding interface.
    /// </summary>
    public class EfundExpressService : IFundingService
    {
        private const int IssueOptionCreateAndStore = 2;
        private const int IssueCopyAndLink = 2;
        private readonly ICachingResolver cachingResolver;
        private readonly IEFundExpressProductConfiguration configuration;
        private readonly IFundingServiceRedirectUrlHelper urlHelper;
        private IClock clock;
        private AsyncRetryPolicy retryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfundExpressService"/> class.
        /// </summary>
        /// <param name="productConfiguration">The configuration for EFundExpress per product basis.</param>
        /// <param name="urlHelper">Helper for generating redirect URLs.</param>
        /// <param name="clock">A clock for getting data and time.</param>
        public EfundExpressService(
            IEFundExpressProductConfiguration productConfiguration,
            IFundingServiceRedirectUrlHelper urlHelper,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.configuration = productConfiguration;
            this.urlHelper = urlHelper;
            this.clock = clock;
            this.retryPolicy = Polly.Policy.Handle<Exception>()
                .WaitAndRetryAsync(
                   3,
                   retryAttempt => TimeSpan.FromMilliseconds(2000 * retryAttempt)); // plus some jitter: up to 2 second
        }

        /// <inheritdoc/>
        public bool PricingSupported => true;

        /// <inheritdoc/>
        public bool DirectDebitSupported => false;

        /// <inheritdoc/>
        public bool CreditCardSupported => false;

        /// <inheritdoc/>
        public bool CanAcceptWithoutRedirect => false;

        /// <inheritdoc/>
        public bool AcceptancePerformedViaApi => true;

        /// <inheritdoc/>
        public async Task<FundingProposal> AcceptFundingProposal(
            Domain.Aggregates.Quote.Quote quote,
            Guid fundingProposalId,
            IPaymentMethodDetails paymentMethodDetails,
            bool isTestData,
            CancellationToken cancellationToken)
        {
            using (var client = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap))
            {
                await Task.CompletedTask;
                throw new ErrorException(Errors.Payment.Funding.AcceptFundingProposalNotSupported(quote.Id, "eFund Express"));
            }
        }

        /// <inheritdoc/>
        public async Task<FundingProposal> AcceptFundingProposal(
            Domain.Aggregates.Quote.Quote quote,
            Guid fundingProposalId,
            bool isTestData)
        {
            await Task.CompletedTask;
            throw new ErrorException(Errors.Payment.Funding.AcceptFundingProposalNotSupported(quote.Id, "eFund Express"));
        }

        /// <inheritdoc/>
        public async Task<FundingProposal> CreateFundingProposal(
            IProductContext productContext,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData,
            PriceBreakdown priceBreakdown, Domain.Aggregates.Quote.Quote quote,
            PaymentData paymentData,
            bool isTestData,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (priceBreakdown.TotalPayable <= 0)
            {
                throw new InvalidOperationException("You should not try to create a funding proposal when the total payable is less than or equal to 0.");
            }

            using (MiniProfiler.Current.Step("EFundExpressService." + nameof(this.CreateFundingProposal)))
            {
                async Task<FundingProposal> CreateFundingProposal(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using (var client = this.GenerateClient())
                    {
                        var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(productContext.TenantId);
                        var isMutual = TenantHelper.IsMutual(tenantAlias);
                        try
                        {
                            client.ClientCredentials.UserName.UserName = this.configuration.BrokerLoginId;
                            client.ClientCredentials.UserName.Password = this.configuration.Password;

                            var quoteDataRetriever = new StandardQuoteDataRetriever(this.configuration, formData, calculationResultData);
                            var request = new EFundExpressContractData(
                                quoteDataRetriever,
                                priceBreakdown,
                                this.configuration,
                                IssueOptionCreateAndStore,
                                IssueCopyAndLink,
                                this.urlHelper.GetSuccessRedirectUrl(productContext, quote?.Id ?? default, string.Empty),
                                this.urlHelper.GetCancellationRedirectUrl(productContext, quote?.Id ?? default, string.Empty),
                                isMutual,
                                quote?.QuoteNumber,
                                this.clock);

                            if (isTestData)
                            {
                                request.UpdateContractDataForTesting();
                            }

                            var contract = request.ToContract();

                            CreateFundingDocumentsResponse result;
                            using (MiniProfiler.Current.Step(
                                "EFundExpressService." + nameof(this.CreateFundingProposal) + ".CreateFundingDocumentsAsync"))
                            {
                                result = await client.CreateFundingDocumentsAsync(contract);
                            }
                            cancellationToken.ThrowIfCancellationRequested();
                            var outcome = this.MapResult(
                                result.Body.CreateFundingDocumentsResult,
                                request,
                                priceBreakdown,
                                isMutual);
                            if (outcome.IsSuccess)
                            {
                                var updateUrlResponse = await this.UpdateFundingUrls(
                                    productContext,
                                    quote?.Id ?? default,
                                    outcome.Value.InternalId,
                                    outcome.Value.ExternalId,
                                    client,
                                    isMutual);
                                return outcome.Value;
                            }
                            else
                            {
                                throw new ErrorException(outcome.Error);
                            }
                        }
                        catch (Exception ex) when (!(ex is ErrorException))
                        {
                            throw new ExternalServiceException(
                                Errors.Payment.Funding.ProviderError(new List<string>() { ex.Message }, isMutual),
                                ex);
                        }
                    }
                }

                return await this.retryPolicy.ExecuteAsync(async (cancellationToken) => await CreateFundingProposal(cancellationToken), cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<FundingProposal> UpdateFundingProposal(
             IProductContext productContext,
            string providerContractId,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData,
            PriceBreakdown priceBreakdown,
            Domain.Aggregates.Quote.Quote quote,
            PaymentData paymentData,
            bool isTestData,
            CancellationToken cancellationToken)
        {
            return await this.CreateFundingProposal(productContext, formData, calculationResultData, priceBreakdown, quote, paymentData, isTestData, cancellationToken);
        }

        /// <summary>
        /// Updates the redirect URLs for the funding acceptance page to include the internal and external funding proposal IDs.
        /// </summary>
        /// <param name="quoteId">The Id of the quote.</param>
        /// <param name="internalProposalId">The UBind system ID for the funding proposal.</param>
        /// <param name="externalProposalId">The external ID for the funding proposal.</param>
        /// <param name="client">The SOAP client to be used.</param>
        /// <param name="isMutual">If under a mutual tenant.</param>
        /// <returns>An eFundExpressUrl response object.</returns>
        private async Task<eFundExpressURL> UpdateFundingUrls(
            IProductContext productContext,
            Guid quoteId,
            Guid internalProposalId,
            string externalProposalId,
            ServiceSoap client,
            bool isMutual)
        {
            using (MiniProfiler.Current.Step("EFundExpressService." + nameof(this.UpdateFundingUrls)))
            {
                var successUrl = this.urlHelper.GetSuccessRedirectUrl(productContext, quoteId, internalProposalId.ToString());
                var cancellationUrl = this.urlHelper.GetCancellationRedirectUrl(productContext, quoteId, internalProposalId.ToString());
                try
                {
                    var request = new UpdateFundingUrlsRequest(new UpdateFundingUrlsRequestBody(this.configuration.BrokerLoginId, this.configuration.Password, cancellationUrl, successUrl, int.Parse(externalProposalId)));
                    var response = await client.UpdateFundingUrlsAsync(request);
                    return response.Body.UpdateFundingUrlsResult;
                }
                catch (Exception ex)
                {
                    throw new ExternalServiceException(
                        Errors.Payment.Funding.ProviderError(new List<string>() { ex.Message }, isMutual));
                }
            }
        }

        /// <summary>
        /// Generates a <see cref="ServiceSoapClient"/> for the SOAP client.
        /// </summary>
        /// <returns>An instance of <see cref="ChannelFactory"/> for the SOAP client with the bindings and endpoint declared.</returns>
        private ServiceSoapClient GenerateClient()
        {
            var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            binding.MaxBufferSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.SendTimeout = new TimeSpan(0, 0, 30);
            var endpoint = new EndpointAddress(new Uri(this.configuration.ClientUrl));
            return new ServiceSoapClient(binding, endpoint);
        }

        private Result<FundingProposal, Error> MapResult(eFundExpressQuote result, EFundExpressContractData contractData, PriceBreakdown priceBreakdown, bool isMutual)
        {
            if (result.ReturnCode == ReturnType.Success)
            {
                dynamic proposal = new
                {
                    ContractID = result.ContractID.ToString(),
                    EFundLink = result.eFundLink,
                };
                var serializedProposalResponse = JsonConvert.SerializeObject(proposal);
                var serializedProposalData = JsonConvert.SerializeObject(contractData);

                var paymentFrequency = this.configuration.FortnightlyInstalments == true ?
                    Frequency.Fortnightly
                    : Frequency.Monthly;
                var interestRate = Convert.ToDecimal(this.configuration.FixedInterestRate);
                var numberOfInstallments = this.configuration.NumberOfInstalments;
                var initialInstallment = EfundExpressInstalmentCalculator.CalculateFirstInstalmentAmount(
                    priceBreakdown.TotalPayable, interestRate, numberOfInstallments, this.configuration.FirstInstalmentFee);
                var regularInstallment = EfundExpressInstalmentCalculator.CalculateRegularInstalmentAmount(
                    priceBreakdown.TotalPayable, interestRate, numberOfInstallments);
                decimal amountFunded = EfundExpressInstalmentCalculator.CalculateAmountFunded(
                    initialInstallment,
                    regularInstallment,
                    numberOfInstallments);
                var paymentBreakdown = new FundingProposalPaymentBreakdown(
                    amountFunded,
                    paymentFrequency,
                    numberOfInstallments,
                    initialInstallment,
                    regularInstallment);
                var fundingProposal = new FundingProposal(
                    result.ContractID.ToString(),
                    paymentBreakdown,
                    result.eFundLink,
                    serializedProposalData,
                    serializedProposalResponse,
                    contractData.UsesPlaceholderData);
                return Result.Success<FundingProposal, Error>(fundingProposal);
            }

            return Result.Failure<FundingProposal, Error>(Errors.Payment.Funding.ProviderError(new string[] { result.ReturnMessage }, isMutual));
        }
    }
}
