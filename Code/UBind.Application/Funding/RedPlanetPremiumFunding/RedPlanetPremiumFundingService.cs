// <copyright file="RedPlanetPremiumFundingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.RedPlanetPremiumFunding.Arteva;

using CreditCardValidator;
using System.Text.Json;
using NodaTime;
using StackExchange.Profiling;
using UBind.Application.Funding.PremiumFunding;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
using UBind.Domain.Aggregates.Quote.Payment;
using UBind.Domain.Exceptions;
using UBind.Domain.ValueTypes;
using UBind.Domain.Helpers;
using UBind.Domain.Extensions;
using UBind.Application.Funding;
using UBind.Application.Funding.RedPlanetPremiumFunding.Models;
using UBind.Domain.Json;
using UBind.Domain.Product;
using UBind.Domain.Payment;
using UBind.Domain.ReadWriteModel;
using Newtonsoft.Json.Linq;

public class RedPlanetPremiumFundingService : IFundingService
{
    private readonly ICachingResolver cachingResolver;
    private readonly IRedPlanetFundingConfiguration configuration;
    private readonly IRedPlanetPremiumFundingApiClient apiClient;
    private readonly IClock clock;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

    // Values are extracted from the API client endpoint GET /common/payment-types
    private readonly Dictionary<string, string> supportedCreditCardTypeCodeMap = new Dictionary<string, string>
    {
        { "mastercard", "MAS" },
        { "american express", "AMX" },
        { "visa", "VIS" },
        { "diners club", "DIN" },
    };

    public RedPlanetPremiumFundingService(
        IRedPlanetFundingConfiguration configuration,
        IClock clock,
        ICachingResolver cachingResolver,
        IRedPlanetPremiumFundingApiClient apiClient,
        IQuoteAggregateRepository quoteAggregateRepository,
        IHttpContextPropertiesResolver httpContextPropertiesResolver)
    {
        this.cachingResolver = cachingResolver;
        this.clock = clock;
        this.configuration = configuration;
        this.apiClient = apiClient;
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
    }

    public bool PricingSupported => true;

    public bool DirectDebitSupported => true;

    public bool CreditCardSupported => true;

    public bool CanAcceptWithoutRedirect => true;

    public bool AcceptancePerformedViaApi => true;

    public async Task<FundingProposal> AcceptFundingProposal(
        Domain.Aggregates.Quote.Quote quote,
        Guid fundingProposalId,
        IPaymentMethodDetails paymentMethodDetails,
        bool isTestData,
        CancellationToken cancellationToken)
    {
        using (MiniProfiler.Current.Step(nameof(RedPlanetPremiumFundingService) + "." + nameof(this.AcceptFundingProposal)))
        {
            var quoteDataRetriever = new StandardQuoteDataRetriever(
               this.configuration,
               quote.LatestFormData.Data,
               quote.LatestCalculationResult.Data);
            var fundingProposalCreationResult = quote.LatestFundingProposalCreationResult;
            var cardNumber = (paymentMethodDetails as CreditCardDetails)?.Number;
            var contractData = new RedPlanetPremiumFundingContractData(
                quoteDataRetriever,
                this.clock,
                quote.Id,
                quote.QuoteNumber,
                quote.LatestCalculationResult.Data.PayablePrice.TotalPayable);
            var contract = contractData.ToQuoteContract(this.configuration);
            var contractPaymentDetails = await this.GetPaymentMethodDetailsOrThrow(paymentMethodDetails);
            FundingProposal fundingProposal = EntityHelper.ThrowIfNotFound(fundingProposalCreationResult?.Proposal, "FundingProposal");
            contract.PaymentDetails = contractPaymentDetails;

            if (fundingProposal != null && fundingProposal?.ProposalResponse != null)
            {
                QuoteDetail? contractResponse = JsonSerializer.Deserialize<QuoteDetail>(fundingProposal.ProposalResponse);
                decimal initialAmountDue = contractResponse?.ContractDetails?.InitialAmountDue ?? 0;
                string? externalQuoteNumber = contractResponse?.QuoteNumber;
                string? contractUrl = fundingProposal.ContractUrl;
                if (contractResponse == null || string.IsNullOrEmpty(externalQuoteNumber))
                {
                    QuoteDetail newContractResponse = await this.CreateOrUpdateContract(contract, cancellationToken, true, fundingProposal.ExternalId);
                    newContractResponse = EntityHelper.ThrowIfNotFound(newContractResponse, "ContractResponse");
                    initialAmountDue = newContractResponse?.ContractDetails?.InitialAmountDue ?? 0;
                    externalQuoteNumber = newContractResponse?.QuoteNumber;
                    contractUrl = await this.GetQuoteContractDocumentUrl(externalQuoteNumber, cancellationToken);
                    fundingProposal = this.MapContractDetailsToFundingProposal(contractData, newContractResponse, contractUrl);
                }

                if (string.IsNullOrEmpty(externalQuoteNumber))
                {
                    throw new ErrorException(
                        Errors.Payment.Funding.RedPlanetPremiumFunding.ProposalExternalQuoteNumberWasNotCreated(
                            this.configuration.FundingType, await this.GetErrorDataObject()));
                }

                var quoteSubmissionModel = this.CreateSubmissionModel(contractPaymentDetails, initialAmountDue);
                var submittedContractResponse = await this.apiClient.SubmitQuote(quoteSubmissionModel, externalQuoteNumber, cancellationToken);
                var submittedFundingProposal = this.MapContractDetailsToFundingProposal(contractData, submittedContractResponse, contractUrl);

                // Due to not being allowed to persist a proposal on their API when there's no card number / account number,
                // we are only getting the final contract details with the complete amount and merchants rates only at this stage.
                // This is why we are persisting this funding proposal here.
                await this.RecordNewFundingProposal(quote, submittedFundingProposal);

                return submittedFundingProposal;
            }

            throw new ErrorException(
                Errors.Payment.Funding.RedPlanetPremiumFunding.UnableToRetrieveFundingProposal(await this.GetErrorDataObject()));
        }
    }

    public async Task<FundingProposal> AcceptFundingProposal(
        Domain.Aggregates.Quote.Quote quote,
        Guid fundingProposalId,
        bool isTestData)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<FundingProposal?> CreateFundingProposal(
           IProductContext productContext,
           CachingJObjectWrapper formData,
           CachingJObjectWrapper calculationResultData,
           PriceBreakdown priceBreakdown,
           Domain.Aggregates.Quote.Quote? quote,
           PaymentData? paymentData,
           bool isTestData,
           CancellationToken cancellationToken)
    {
        // Do not create funding proposal if there is no payable amount
        if (priceBreakdown.TotalPayable < 1 || cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        using (MiniProfiler.Current.Step(nameof(RedPlanetPremiumFundingService) + "." + nameof(this.CreateFundingProposal)))
        {
            var quoteDataRetriever = new StandardQuoteDataRetriever(this.configuration, formData, calculationResultData);
            var debitOption = quoteDataRetriever.Retrieve(StandardQuoteDataField.DebitOption);
            string? paymentTypeCode = this.GetSupportedPaymentTypeCode(paymentData?.ZeroPaddedCardBin, debitOption);

            if (paymentData != null && paymentTypeCode == null)
            {
                var supportedTypes = this.supportedCreditCardTypeCodeMap.Keys.Select(x => x.ToTitleCase());
                throw new ErrorException(Errors.Payment.Funding.RedPlanetPremiumFunding.CreditCardTypeIsNotSupported(
                    this.configuration.FundingType, supportedTypes, await this.GetErrorDataObject()));
            }

            if (quote?.QuoteStatus != StandardQuoteStates.Approved || quote?.LatestCalculationResult?.Data == null)
            {
                var quickQuoteModel = this.GetQuickQuoteModel(priceBreakdown.TotalPayable, quote?.QuoteNumber);
                return await this.GetQuickQuoteProposal(quickQuoteModel, cancellationToken);
            }

            var contractData = new RedPlanetPremiumFundingContractData(
                quoteDataRetriever,
                this.clock,
                quote.Id,
                quote.QuoteNumber,
                priceBreakdown.TotalPayable);
            var contract = contractData.ToQuoteContract(this.configuration, paymentTypeCode);
            var previousContractQuoteNumber = quote?.LatestFundingProposalCreationResult?.Proposal?.ExternalId ?? null;
            QuoteDetail contractResponse = await this.CreateOrUpdateContract(contract, cancellationToken, true, previousContractQuoteNumber);
            var proposalContractUrl = await this.GetQuoteContractDocumentUrl(contractResponse.QuoteNumber, cancellationToken);
            var proposal = this.MapContractDetailsToFundingProposal(contractData, contractResponse, proposalContractUrl);
            return proposal;
        }
    }

    public async Task<FundingProposal?> UpdateFundingProposal(
           IProductContext productContext,
           string? providerContractId,
           CachingJObjectWrapper formData,
           CachingJObjectWrapper calculationResultData,
           PriceBreakdown priceBreakdown,
           Domain.Aggregates.Quote.Quote? quote,
           PaymentData? paymentData,
           bool isTestData,
        CancellationToken cancellationToken)
    {
        return await this.CreateFundingProposal(
           productContext,
           formData,
           calculationResultData,
           priceBreakdown,
           quote,
           paymentData,
           isTestData,
           cancellationToken);
    }

    private async Task<QuoteDetail> CreateOrUpdateContract(
        CreateUpdateQuoteModel contract,
        CancellationToken cancellationToken,
        bool saveQuote = false,
        string? previousContractQuoteNumber = null)
    {
        if (!string.IsNullOrEmpty(previousContractQuoteNumber))
        {
            return await this.apiClient.UpdateQuote(contract, previousContractQuoteNumber, cancellationToken);
        }
        return await this.apiClient.CreateQuote(contract, saveQuote, cancellationToken);
    }

    /// <summary>
    /// Determine the Credit Card Type based on the card number given.
    /// </summary>
    /// <param name="cardNumber"></param>
    /// <returns>Arteva's payment type code that matches</returns>
    private string? GetSupportedPaymentTypeCode(string? cardNumber, string? debitOption)
    {
        if (debitOption == "Direct Debit")
        {
            return "DDR";
        }

        if (string.IsNullOrEmpty(cardNumber))
        {
            return null;
        }

        var detector = new CreditCardDetector(cardNumber);
        if (!string.IsNullOrEmpty(detector.BrandName))
        {
            this.supportedCreditCardTypeCodeMap.TryGetValue(detector.BrandName.ToLower(), out string? cardTypeCode);
            return cardTypeCode;
        }

        return null;
    }

    private async Task<FundingProposal> GetQuickQuoteProposal(QuickQuoteModel quickQuoteModel, CancellationToken cancellationToken)
    {
        var quickQuoteDetail = await this.apiClient.QuickQuote(quickQuoteModel, cancellationToken);

        var paymentBreakdown = new FundingProposalPaymentBreakdown(
            quickQuoteDetail?.AmountFinanced ?? 0,
            Frequency.Monthly,
            this.configuration.NumberOfInstalments,
            quickQuoteDetail?.InitialAmountDue ?? 0,
            quickQuoteDetail?.InstalmentAmount ?? 0,
            0,
            quickQuoteDetail?.Interest ?? 0,
            quickQuoteDetail?.FlatRate ?? 0);

        var serializedProposalData = JsonSerializer.Serialize(quickQuoteModel, SystemJsonHelper.GetSerializerOptions());
        var serializedProposalResponse = JsonSerializer.Serialize(quickQuoteDetail, SystemJsonHelper.GetSerializerOptions());
        var proposal = new FundingProposal(
            null,
            paymentBreakdown,
            serializedProposalData,
            serializedProposalResponse,
            false);
        return proposal;
    }

    /// <summary>
    /// Get a quick quote for sending to api client from given premium amount and a quote number.
    /// </summary>
    /// <param name="premiumAmount"></param>
    /// <param name="quoteNumber">Quote number coming from <see cref="Quote.QuoteNumber"> aggregate.</param>
    /// <returns></returns>
    private QuickQuoteModel GetQuickQuoteModel(decimal premiumAmount, string? quoteNumber)
    {
        return new QuickQuoteModel
        {
            Amount = premiumAmount,
            Key = quoteNumber ?? string.Empty,
            NumberOfInstalments = this.configuration.NumberOfInstalments,
            Product = this.configuration.Product,
            PaymentFrequency = this.configuration.PaymentFrequency,
            CommissionRate = this.configuration.CommissionRate,
        };
    }

    private FundingProposal MapContractDetailsToFundingProposal(
        RedPlanetPremiumFundingContractData contractData,
        QuoteDetail? contractResponse,
        string? contractUrl)
    {
        var contractExternalId = contractResponse?.QuoteNumber;
        var paymentBreakdown = new FundingProposalPaymentBreakdown(
            contractResponse?.ContractDetails?.AmountFinanced ?? 0,
            Frequency.Monthly,
            this.configuration.NumberOfInstalments,
            contractResponse?.ContractDetails?.InitialAmountDue ?? 0,
            contractResponse?.ContractDetails?.InstalmentAmount ?? 0,
            contractResponse?.PaymentDetails?.MerchantFeeRate ?? 0,
            contractResponse?.ContractDetails?.Interest ?? 0,
            contractResponse?.ContractDetails?.FlatRate ?? 0);
        var serializedProposalData = JsonSerializer.Serialize(contractData, SystemJsonHelper.GetSerializerOptions());
        var serializedProposalResponse = JsonSerializer.Serialize(contractResponse, SystemJsonHelper.GetSerializerOptions());
        var proposal = new FundingProposal(
                        contractExternalId,
                        paymentBreakdown,
                        contractResponse?.AccessDetails?.ClientAcceptanceUrl,
                        contractUrl,
                        serializedProposalData,
                        serializedProposalResponse,
                        false);
        return proposal;
    }

    private QuoteSubmissionModel CreateSubmissionModel(
        RedPlanetPremiumFunding.Models.PaymentDetails paymentDetails,
        decimal initialInstalmentAmount)
    {
        return new QuoteSubmissionModel
        {
            PaymentDetails = paymentDetails,
            InitialInstalmentAmount = initialInstalmentAmount,
            ProcessRealtimePayment = true,
        };
    }

    private async Task<RedPlanetPremiumFunding.Models.PaymentDetails> GetPaymentMethodDetailsOrThrow(IPaymentMethodDetails paymentMethodDetails)
    {
        System.Type paymentMethodType = paymentMethodDetails.GetType();
        if (typeof(BankAccountDetails) == paymentMethodType)
        {
            var bankDetails = paymentMethodDetails as BankAccountDetails;
            return new RedPlanetPremiumFunding.Models.PaymentDetails
            {
                PaymentType = "DDR",
                PaymentTypeCode = "DDR",
                AccountNumber = bankDetails?.Number,
                BankBranchCode = bankDetails?.BSB,
                BillingName = bankDetails?.Name ?? string.Empty,
            };
        }
        else if (typeof(CreditCardDetails) == paymentMethodType)
        {
            var cardDetails = paymentMethodDetails as CreditCardDetails;
            var cardType = this.GetSupportedPaymentTypeCode(cardDetails?.Number, string.Empty);

            if (cardType == null)
            {
                throw new ErrorException(Errors.General.Unexpected("Credit card type is not supported.", await this.GetErrorDataObject()));
            }

            return new RedPlanetPremiumFunding.Models.PaymentDetails
            {
                PaymentType = "CC",
                PaymentTypeCode = cardType,
                CardNumber = cardDetails?.Number,
                CardExpiry = cardDetails?.ExpiryMMyy,
                CardCvv = cardDetails?.Cvv,
                BillingName = cardDetails?.Name ?? string.Empty,
            };
        }

        throw new ErrorException(Errors.General.Unexpected("Unable to extract payment method details.", await this.GetErrorDataObject()));
    }

    private async Task RecordNewFundingProposal(Domain.Aggregates.Quote.Quote quote, FundingProposal fundingProposal)
    {
        quote.RecordFundingProposalCreated(fundingProposal, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quote.Id);
        await this.quoteAggregateRepository.Save(quote.Aggregate);
    }

    private async Task<string?> GetQuoteContractDocumentUrl(string? quoteNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(quoteNumber))
        {
            return null;
        }
        var documents = await this.apiClient.GetQuoteDocuments(quoteNumber, cancellationToken);
        const string contractDocumentName = "Contract";
        var quoteContractDocument = documents.Where(x => x.Name == contractDocumentName).FirstOrDefault();
        if (quoteContractDocument == null)
        {
            throw new ErrorException(Errors.Payment.Funding.RedPlanetPremiumFunding.UnableToRetrieveContractDocumentUrl(
                this.configuration.FundingType, await this.GetErrorDataObject()));
        }
        return $"{this.apiClient.ClientUrl}/quotes/document/{quoteContractDocument.AccessCode}";
    }

    private async Task<JObject> GetErrorDataObject()
    {
        var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(this.configuration.ReleaseContext.TenantId);
        var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(
            this.configuration.ReleaseContext.TenantId,
            this.configuration.ReleaseContext.ProductId);

        return new JObject
        {
            { "username",  this.configuration.Username },
            { "fundingType", this.configuration.FundingType.ToString() },
            { "tenantAlias", tenantAlias },
            { "productAlias", productAlias },
            { "environment", this.configuration.ReleaseContext.Environment.ToString() },
        };
    }
}
