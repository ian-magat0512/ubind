// <copyright file="PremiumFundingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using Polly;
    using StackExchange.Profiling;
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
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Service for creating and submitting premium funding contracts.
    /// </summary>
    public class PremiumFundingService : IFundingService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IPremiumFundingConfiguration configuration;
        private readonly ICachingAccessTokenProvider accessTokenProvider;
        private readonly IClock clock;
        private readonly AsyncPolicy apiRetryPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumFundingService"/> class.
        /// </summary>
        /// <param name="quoteAggregateResolverService">The quote aggregate resolver service.</param>
        /// <param name="configuration">The configuration to use for premium funding proposals.</param>
        /// <param name="accessTokenProvider">Access token provider.</param>
        /// <param name="clock">A clock.</param>
        public PremiumFundingService(
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IPremiumFundingConfiguration configuration,
            ICachingAccessTokenProvider accessTokenProvider,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.configuration = configuration;
            this.accessTokenProvider = accessTokenProvider;
            this.clock = clock;

            // Premium Funding Company's API access tokens can be invalidated at any moment due to new deployments of
            // their API etc., so we should always retry one time with a new access token if we get a 401 response.
            this.apiRetryPolicy = Polly.Policy
                .Handle<PremiumFundingException>(ex => ex.StatusCode == (int)HttpStatusCode.Unauthorized)
                .RetryAsync(1, (ex, retryCount) => this.ClearCachedAccessToken());
        }

        /// <inheritdoc/>
        public bool PricingSupported => true;

        /// <inheritdoc/>
        public bool DirectDebitSupported => true;

        /// <inheritdoc/>
        public bool CreditCardSupported => true;

        /// <inheritdoc/>
        public bool CanAcceptWithoutRedirect => true;

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
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(quote.Aggregate.TenantId);
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(quote.Aggregate.TenantId, quote.Id);
            var proposal = this.RetrieveFundingProposal(quoteAggregate, fundingProposalId, quote.Id);
            var contractData = JsonConvert.DeserializeObject<PremiumFundingContractData>(proposal.ProposalData);
            if (isTestData)
            {
                contractData.UpdateContractDataForTesting();
            }

            var contract = contractData.ToContract();
            var contractId = int.Parse(proposal.ExternalId);
            this.AddPaymentDetails(contract, paymentMethodDetails);
            ContractResponse contractUpdateResult = await this.apiRetryPolicy
                .ExecuteAsync(() => this.UpdateContract(contractId, contract, isMutual));

            // TODO: Verify price breakdown has not changed.
            try
            {
                ResponseModel response = await this.apiRetryPolicy.ExecuteAsync(() => this.SubmitContract(contractId));
                return proposal;
            }
            catch (PremiumFundingException<ErrorResponseModel> ex)
            {
                // Note that:
                // 1. The exception message cannot be relied upon to be accurate, for example 409 responses with the title
                //    "The contract can't be submitted yet, please check the details to resolve the issue" get translated to
                //    "The contract has already been submitted". Instead we must read the title from the response body, or the
                //    messages from the details.
                // 2. The generated client does not include details in the ErrorResponse object in the thrown exception, so we
                //    need to parse the response body ourselves to extract them.
                // 3. Premium Funding have introduced a new error response for when immediate charging of the first instalment
                //    fails. It does not match the other error's Json format, and so needs to be checked for if error parsing
                //    fails.
                IEnumerable<string> errors = null;
                try
                {
                    PremiumFundingErrors json = JsonConvert.DeserializeObject<PremiumFundingErrors>(ex.Response);
                    errors = json.Errors.First().Details
                        .Where(e => e.Type == "requirement")
                        .Select(e => e.Message.Transform(To.LowerCase, To.SentenceCase) + ".")
                        .ToArray();
                }
                catch (JsonSerializationException)
                {
                    try
                    {
                        var jObject = JObject.Parse(ex.Response);

                        // Rethrow original premium funding exception if we couldn't understand it.
                        var errorDetail = jObject["errors"]?[0]?["detail"]?.ToString() ?? throw ex;
                        var expression = new Regex(
                            "Contract NOT submitted! Initial payment could not be taken, transaction response: (.*)");
                        Match regexResult = expression.Match(errorDetail);
                        if (regexResult.Success)
                        {
                            var transactionResponse =
                                regexResult.Groups[1].Value.Transform(To.LowerCase, To.SentenceCase);
                            var errorMessage =
                                $"We could not process your initial payment. The payment error was: {transactionResponse}.";
                            errors = new string[] { errorMessage };
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    catch (JsonReaderException)
                    {
                        // Rethrow original premium funding exception, since we couldn't understand it.
                        throw ex;
                    }
                }

                throw new ErrorException(Errors.Payment.Funding.ProviderError(errors, isMutual));
            }
        }

        /// <inheritdoc/>
        public Task<FundingProposal> AcceptFundingProposal(
            Domain.Aggregates.Quote.Quote quote,
            Guid fundingProposalId,
            bool isTestData)
        {
            throw new NotSupportedException("Premium Funding proposals cannot be accepted without payment.");
        }

        /// <inheritdoc/>
        public async Task<FundingProposal?> CreateFundingProposal(
            IProductContext productContext,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData,
            PriceBreakdown priceBreakdown,
            Domain.Aggregates.Quote.Quote quote,
            PaymentData paymentData,
            bool isTestData,
            CancellationToken cancellationToken)
        {
            using (MiniProfiler.Current.Step("PremiumFundingService." + nameof(this.CreateFundingProposal)))
            {
                var quoteDataRetriever = new StandardQuoteDataRetriever(this.configuration, formData, calculationResultData);
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(productContext.TenantId);
                var isMutual = TenantHelper.IsMutual(tenantAlias);
                var contractData = new PremiumFundingContractData(quoteDataRetriever, priceBreakdown, this.configuration, isMutual, this.clock);
                var contract = contractData.ToContract();
                var contractSummaryResponse = await this.apiRetryPolicy.ExecuteAsync(() => this.CreateContract(contract, isMutual));
                var result = this.MapContractCreateResponse(contractSummaryResponse, contractData);
                return result;
            }
        }

        /// <inheritdoc/>
        public async Task<FundingProposal?> UpdateFundingProposal(
          IProductContext productContext,
            string providerContractId,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData,
            PriceBreakdown priceBreakdown, Domain.Aggregates.Quote.Quote quote,
            PaymentData paymentData,
            bool isTestData,
            CancellationToken cancellationToken)
        {
            var quoteDataRetriever = new StandardQuoteDataRetriever(this.configuration, formData, calculationResultData);
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(productContext.TenantId);
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            var contractData = new PremiumFundingContractData(quoteDataRetriever, priceBreakdown, this.configuration, isMutual, this.clock);
            var contractId = int.Parse(providerContractId);
            var contract = contractData.ToContract();
            var contractResponse = await this.apiRetryPolicy
                .ExecuteAsync(() => this.UpdateContract(contractId, contract, isMutual));
            var proposal = this.MapContractUpdateResponse(contractResponse, contractData);
            return proposal;
        }

        private async Task<ContractSummaryResponse> CreateContract(Contract contract, bool isMutual)
        {
            var client = await this.GetPremiumFundingClient();
            using (MiniProfiler.Current.Step(
                $"{nameof(PremiumFundingClient)}.{nameof(PremiumFundingClient.PostContractAsync)}"))
            {
                return await this.CallClientWrappingAnyException(
                    () => client.PostContractAsync(contract), isMutual);
            }
        }

        private async Task<ContractResponse> UpdateContract(int contractId, Contract contract, bool isMutual)
        {
            var client = await this.GetPremiumFundingClient();
            using (MiniProfiler.Current.Step(
                $"{nameof(PremiumFundingClient)}.{nameof(PremiumFundingClient.PutContractAsync)}"))
            {
                return await this.CallClientWrappingAnyException(
                    () => client.PutContractAsync(contractId, contract), isMutual);
            }
        }

        private async Task<PremiumFundingClient> GetPremiumFundingClient()
        {
            var accessToken = await this.accessTokenProvider.GetAccessToken(
                this.configuration.Username,
                this.configuration.Password,
                this.configuration.ApiVersion);
            return PremiumFundingClient.CreateAuthenticatingClient(accessToken);
        }

        private async Task<ResponseModel> SubmitContract(int contractId)
        {
            var client = await this.GetPremiumFundingClient();
            using (MiniProfiler.Current.Step(
                $"{nameof(PremiumFundingClient)}.{nameof(PremiumFundingClient.PutSubmissionAsync)}"))
            {
                return await client.PutSubmissionAsync(contractId);
            }
        }

        private void ClearCachedAccessToken()
        {
            this.accessTokenProvider.ClearCachedAccessToken(this.configuration.Username, this.configuration.ApiVersion);
        }

        private void AddPaymentDetails(Contract contract, IPaymentMethodDetails paymentMethodDetails)
        {
            var creditCardDetails = paymentMethodDetails as CreditCardDetails;
            if (creditCardDetails != null)
            {
                contract.PaymentType = ContractPaymentType.CreditCard;
                contract.CreditCardType = this.DetermineCreditCardType(creditCardDetails.Number);
                contract.CreditCardNumber = creditCardDetails.Number;
                contract.CreditCardHolder = creditCardDetails.Name;
                contract.CreditCardExpiry = creditCardDetails.ExpiryMMyyyy;
                contract.CVN = creditCardDetails.Cvv;
                return;
            }

            var bankAccountDetails = paymentMethodDetails as BankAccountDetails;
            if (bankAccountDetails != null)
            {
                contract.PaymentType = ContractPaymentType.DirectDebit;
                contract.AccountName = bankAccountDetails.Name;
                contract.AccountNumber = bankAccountDetails.Number;
                contract.BSB = bankAccountDetails.BSB;
                return;
            }

            throw new NotSupportedException($"Unsupported payment method: {paymentMethodDetails.GetType().Name}.");
        }

        private FundingProposal MapContractCreateResponse(
            ContractSummaryResponse response,
            PremiumFundingContractData contractData)
        {
            Quote quote = response.Data.Attributes.Quote;
            var amountFunded = quote.AmountFinanced;
            Frequency paymentFrequency = quote.MonthlyPaymentAmount > 0
                ? Frequency.Monthly
                : Frequency.Weekly;
            var numberOfInstallments = paymentFrequency == Frequency.Monthly
                ? quote.NumberMonthlyPayments
                : quote.NumberWeeklyPayments;
            var initialInstallmentAmount = quote.FirstInstalmentAmount;
            var regularInstallmentAmounts = paymentFrequency == Frequency.Monthly
                ? quote.MonthlyPaymentAmount
                : quote.WeeklyPaymentAmount;
            var paymentBreakdown = new FundingProposalPaymentBreakdown(
                Convert.ToDecimal(amountFunded),
                paymentFrequency,
                Convert.ToInt32(numberOfInstallments),
                Convert.ToDecimal(initialInstallmentAmount),
                Convert.ToDecimal(regularInstallmentAmounts));
            var contractId = response.Data.Attributes.ContractSummary.ID.ToString();

            // TODO: serialize proposal data including contract ID.
            var serializedProposalData = JsonConvert.SerializeObject(contractData);
            var serializedProposalResponse = JsonConvert.SerializeObject(response);
            var proposal = new FundingProposal(
                contractId,
                paymentBreakdown,
                serializedProposalData,
                serializedProposalResponse,
                contractData.UsesPlaceholderData);
            return proposal;
        }

        private FundingProposal MapContractUpdateResponse(
            ContractResponse response,
            PremiumFundingContractData contractData)
        {
            Quote2 quote = response.Data.Attributes.Quote;
            var amountFunded = quote.AmountFinanced;
            Frequency paymentFrequency = quote.MonthlyPaymentAmount > 0
                ? Frequency.Monthly
                : Frequency.Weekly;
            var numberOfInstallments = paymentFrequency == Frequency.Monthly
                ? quote.NumberMonthlyPayments
                : quote.NumberWeeklyPayments;
            var initialInstallmentAmount = quote.FirstInstalmentAmount;
            var regularInstallmentAmounts = paymentFrequency == Frequency.Monthly
                ? quote.MonthlyPaymentAmount
                : quote.WeeklyPaymentAmount;
            var paymentDetails = new FundingProposalPaymentBreakdown(
                Convert.ToDecimal(amountFunded),
                paymentFrequency,
                Convert.ToInt32(numberOfInstallments),
                Convert.ToDecimal(initialInstallmentAmount),
                Convert.ToDecimal(regularInstallmentAmounts));
            var serializedProposalData = JsonConvert.SerializeObject(contractData);
            var serializedProposalResponse = JsonConvert.SerializeObject(response);
            var proposal = new FundingProposal(
                response.Data.Attributes.Contract.ID.Value.ToString(),
                paymentDetails,
                serializedProposalData,
                serializedProposalResponse,
                contractData.UsesPlaceholderData);
            return proposal;
        }

        private FundingProposal RetrieveFundingProposal(QuoteAggregate quoteAggregate, Guid proposalId, Guid quoteId)
        {
            var quote = quoteAggregate.GetQuote(quoteId);
            var proposalCreationResult = quote.LatestFundingProposalCreationResult;
            if (proposalCreationResult.Proposal.InternalId != proposalId)
            {
                // there's a mismatch here. This could be because the latest funding proposal hasn't been saved yet, so
                // the formsApp should just retry this request.
                throw new ErrorException(
                    Errors.Payment.Funding.FundingProposalMismatch(proposalId, proposalCreationResult.Proposal.InternalId));
            }

            if (proposalCreationResult == null || !proposalCreationResult.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"Could not find funding proposal with internal UBind ID {proposalId}.");
            }

            return proposalCreationResult.Proposal;
        }

        private async Task<ResultOld> AcceptFundingProposal(Contract contract, QuoteAggregate quoteAggregate, Guid proposalId)
        {
            // TODO: Fetch proposal and read contract ID from proposal data.
            var contractId = 0; // TODO: contract ID goes here.
            var client = await this.GetPremiumFundingClient();
            try
            {
                ResponseModel response = await client.PutSubmissionAsync(contractId);
                return ResultOld.Success();
            }
            catch (PremiumFundingException<ErrorResponseModel> ex)
            {
                // Note that:
                // 1. The exception message cannot be relied upon to be accurate, for example 409 responses with the title
                //    "The contract can't be submitted yet, please check the details to resolve the issue" get translated to
                //    "The contract has already been submitted". Instead we must read the title from the response body, or the
                //    messages from the details.
                // 2. The generated client does not include details in the ErrorResponse object in the thrown exception, so we
                //    need to parse the response body ourselves to extract them.
                PremiumFundingErrors json = JsonConvert.DeserializeObject<PremiumFundingErrors>(ex.Response);
                IEnumerable<string> errors = json.Errors.First().Details.Where(e => e.Type == "requirement").Select(e => e.Message);
                return ResultOld.Failure(errors);
            }
        }

        private async Task<TResponse> CallClientWrappingAnyException<TResponse>(Func<Task<TResponse>> clientCall, bool isMutual)
        {
            try
            {
                return await clientCall.Invoke();
            }
            catch (PremiumFundingException ex)
            {
                throw new ExternalServiceException(
                    Errors.Payment.Funding.ProviderError(new List<string>() { ex.Message }, isMutual), ex);
            }
        }

        private ContractCreditCardType DetermineCreditCardType(string creditCardNumber)
        {
            if (creditCardNumber.StartsWith("4"))
            {
                return ContractCreditCardType.Visa;
            }

            if (Regex.IsMatch(creditCardNumber, "^5[1-5]"))
            {
                return ContractCreditCardType.MasterCard;
            }

            throw new ErrorException(Errors.Payment.MethodNotSupported());
        }
    }
}
