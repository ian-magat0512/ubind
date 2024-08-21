// <copyright file="RedPlanetPremiumFundingServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Funding.RedPlanetPremiumFunding;

using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Extensions;
using UBind.Domain.Json;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadWriteModel;
using UBind.Domain.Tests.Fakes;
using Xunit;
using UBind.Web.ResourceModels;
using UBind.Domain.Helpers;
using UBind.Application.Funding.RedPlanetPremiumFunding;
using UBind.Application.Funding.RedPlanetPremiumFunding.Arteva;
using UBind.Application.Funding.RedPlanetPremiumFunding.Models;
using UBind.Domain.ValueTypes;

public class RedPlanetPremiumFundingServiceTests
{
    private readonly IClock clock = SystemClock.Instance;
    private readonly RedPlanetPremiumFundingConfiguration fundingConfiguration;
    private readonly Mock<ICachingResolver> mockCachingResolver = new Mock<ICachingResolver>();

    public RedPlanetPremiumFundingServiceTests()
    {
        this.fundingConfiguration = this.CreateDefaultConfig();
        this.fundingConfiguration.SetDataLocators(DefaultQuoteDatumLocations.Instance, null);
    }

    [Fact]
    public async Task RedPlanetPremiumFundingService_ShouldReturnCorrectFundingProposalValues_WhenQuickQuoteEndpointIsUsed()
    {
        // Arrange
        decimal amountToFund = 2500.65m;
        decimal applicationFee = 60.0m;
        decimal instalmentAmount = amountToFund / this.fundingConfiguration.NumberOfInstalments;
        decimal initialFee = instalmentAmount + applicationFee;
        Mock<IRedPlanetPremiumFundingApiClient> mockApiClient = new Mock<IRedPlanetPremiumFundingApiClient>();
        var quickQuoteResult = new QuickQuoteDetail
        {
            AmountFinanced = 2500.65m,
            NumberOfInstalments = this.fundingConfiguration.NumberOfInstalments,
            InstalmentAmount = instalmentAmount,
            ApplicationFee = applicationFee,
            InitialAmountDue = instalmentAmount + applicationFee,
        };
        mockApiClient.Setup(x => x.QuickQuote(It.IsAny<QuickQuoteModel>(), default))
            .ReturnsAsync(quickQuoteResult);

        // Act
        FundingProposal fundingProposal = await this.TestCreateFundingProposal(amountToFund, mockApiClient, initialQuoteState: StandardQuoteStates.Incomplete);

        // Assert
        fundingProposal.Should().NotBeNull();
        fundingProposal.PaymentBreakdown.RegularInstalmentAmount.Should().Be(instalmentAmount);
        fundingProposal.PaymentBreakdown.InitialInstalmentAmount.Should().Be(initialFee);
    }

    [Fact]
    public async Task RedPlanetPremiumFundingService_ShouldReturnCorrectFundingProposalValues_WhenCreateQuoteEndpointIsUsed()
    {
        // Arrange
        decimal amountToFund = 2500.65m;
        decimal applicationFee = 60.0m;
        decimal instalmentAmount = amountToFund / this.fundingConfiguration.NumberOfInstalments;
        decimal initialFee = instalmentAmount + applicationFee;
        decimal instalmentMerchantFeeMultiplier = 0.005m;
        Mock<IRedPlanetPremiumFundingApiClient> mockApiClient = new Mock<IRedPlanetPremiumFundingApiClient>();
        var createQuoteResult = new QuoteDetail
        {
            QuoteNumber = "3029303",
            ContractDetails = new ContractDetails
            {
                InitialAmountDue = initialFee,
                InstalmentAmount = instalmentAmount,
                ApplicationFee = applicationFee,
                AmountFinanced = amountToFund,
            },
            PaymentDetails = new Application.Funding.RedPlanetPremiumFunding.Models.PaymentDetails
            {
                MerchantFeeRate = instalmentMerchantFeeMultiplier,
            },
        };
        mockApiClient.Setup(x => x.CreateQuote(It.IsAny<CreateUpdateQuoteModel>(), It.IsAny<bool>(), default))
            .ReturnsAsync(createQuoteResult);
        mockApiClient.Setup(x => x.GetQuoteDocuments(It.IsAny<string>(), default))
           .ReturnsAsync(new List<QuoteDocumentModel>
           {
               new QuoteDocumentModel("Contract", "888888"),
           });
        var customFormModel = new Dictionary<string, object>
        {
            { "debitOption", "Direct Debit" },
        };

        // Act
        FundingProposal fundingProposal = await this.TestCreateFundingProposal(
            amountToFund, mockApiClient, customFormModel, StandardQuoteStates.Approved);

        // Assert
        fundingProposal.Should().NotBeNull();
        fundingProposal.PaymentBreakdown.RegularInstalmentAmount.Should().Be(instalmentAmount);
        fundingProposal.PaymentBreakdown.InitialInstalmentAmount.Should().Be(initialFee);
        fundingProposal.PaymentBreakdown.InstalmentMerchantFeeMultiplier.Should().Be(instalmentMerchantFeeMultiplier);
    }

    [Fact]
    public async Task RedPlanetPremiumFundingService_ShouldGenerateCorrectPremiumFundingProposalModel_WhenFundingProposalIsCreated()
    {
        decimal amountToFund = 2500.65m;
        decimal applicationFee = 60.0m;
        decimal instalmentAmount = amountToFund / this.fundingConfiguration.NumberOfInstalments;
        decimal initialFee = instalmentAmount + applicationFee;
        decimal instalmentMerchantFeeMultiplier = 0.005m;
        decimal initialAmountWithMerchantFee = initialFee * (1 + instalmentMerchantFeeMultiplier);
        decimal instalmentAmountWithMerchantFee = instalmentAmount * (1 + instalmentMerchantFeeMultiplier);
        Mock<IRedPlanetPremiumFundingApiClient> mockApiClient = new Mock<IRedPlanetPremiumFundingApiClient>();
        var createQuoteResult = new QuoteDetail
        {
            QuoteNumber = "3029323",
            ContractDetails = new ContractDetails
            {
                InitialAmountDue = initialFee,
                InstalmentAmount = instalmentAmount,
                ApplicationFee = applicationFee,
                AmountFinanced = amountToFund,
            },
            PaymentDetails = new Application.Funding.RedPlanetPremiumFunding.Models.PaymentDetails
            {
                MerchantFeeRate = instalmentMerchantFeeMultiplier,
            },
        };
        mockApiClient.Setup(x => x.CreateQuote(It.IsAny<CreateUpdateQuoteModel>(), It.IsAny<bool>(), default))
            .ReturnsAsync(createQuoteResult);
        mockApiClient.Setup(x => x.GetQuoteDocuments(It.IsAny<string>(), default))
           .ReturnsAsync(new List<QuoteDocumentModel>
           {
               new QuoteDocumentModel("Contract", "888888"),
           });
        var customFormModel = new Dictionary<string, object>
        {
            { "debitOption", "Direct Debit" },
        };

        // Act
        FundingProposal fundingProposal = await this.TestCreateFundingProposal(
            amountToFund, mockApiClient, customFormModel, StandardQuoteStates.Approved);
        var testQuote = this.CreateTestQuote(amountToFund);
        var quote = testQuote.Value;
        var @event = new QuoteAggregate.QuoteInitializedEvent(
                Guid.NewGuid(),
                quote.Aggregate.Id,
                quote.Id,
                default,
                default,
                default,
                default,
                Guid.NewGuid(),
                default,
                Timezones.AET,
                false,
                null,
                false,
                null);
        var quoteReadModel = new NewQuoteReadModel(@event);
        quoteReadModel.RecordFunding(fundingProposal, @event.Timestamp);
        var outputModel = new PremiumFundingProposalModel(quoteReadModel);

        // Assert
        outputModel.InitialInstallmentAmount.Should().Be(initialAmountWithMerchantFee.ToDollarsAndCents());
        outputModel.RegularInstalmentAmount.Should().Be(instalmentAmountWithMerchantFee.ToDollarsAndCents());
    }

    private async Task<FundingProposal> TestCreateFundingProposal(
        decimal amountToFund,
        Mock<IRedPlanetPremiumFundingApiClient> mockApiClient,
        Dictionary<string, object>? customFormModel = null,
        string initialQuoteState = "")
    {
        var createQuoteResult = this.CreateTestQuote(amountToFund, customFormModel, initialQuoteState);
        var quote = createQuoteResult.Value;
        var calculationData = EntityHelper.ThrowIfNotFound(quote.LatestCalculationResult?.Data, "FormDataId");
        var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(calculationData);

        var sut = this.SetupSut(mockApiClient);

        // Act
        var fundingProposal = EntityHelper.ThrowIfNotFound(
            await sut.CreateFundingProposal(
                    quote.Aggregate.ProductContext,
                    quote.LatestFormData.Data,
                    calculationData,
                    priceBreakdown,
                    quote,
                    null,
                    false,
                    default), "InternalId");
        return fundingProposal;
    }

    private RedPlanetPremiumFundingConfiguration CreateDefaultConfig()
    {
        var jsonConfig = @"{
            ""username"": ""ubind_dev"",
            ""password"": ""PG2^8Q&8ph"",
            ""baseUrl"": ""https://uat.redplanetsoftware.com/odyssey-api/"",
            ""fundingType"": ""Arteva"",
            ""product"": ""IPF"",
            ""paymentFrequency"": ""M"",
            ""numberOfInstalments"": 10,
            ""commissionRate"": 0.02,
            ""insuranceClassCode"": ""AV""
        }";
        var config = JsonConvert.DeserializeObject<RedPlanetPremiumFundingConfiguration>(jsonConfig);
        return EntityHelper.ThrowIfNotFound(config, "Username");
    }

    private RedPlanetPremiumFundingService SetupSut(Mock<IRedPlanetPremiumFundingApiClient> mockApiClient)
    {
        return new RedPlanetPremiumFundingService(
            this.fundingConfiguration,
            this.clock,
            this.mockCachingResolver.Object,
            mockApiClient.Object,
            new Mock<IQuoteAggregateRepository>().Object,
            new Mock<IHttpContextPropertiesResolver>().Object);
    }

    private Result<Quote, Error> CreateTestQuote(
        decimal totalPremium,
        Dictionary<string, object>? customFormModel = null,
        string initialQuoteState = "")
    {
        Guid userId = Guid.NewGuid();
        var quote = QuoteAggregate.CreateNewBusinessQuote(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DeploymentEnvironment.Staging,
            QuoteExpirySettings.Default,
            userId,
            this.clock.GetCurrentInstant(),
            Guid.NewGuid(),
            Timezones.AET,
            false,
            null,
            true,
            initialQuoteState: initialQuoteState);
        var formDataObject = new
        {
            formModel = this.CreateFormModel(customFormModel),
        };
        var formData = new FormData(JsonConvert.SerializeObject(formDataObject));
        quote.UpdateFormData(formData, userId, this.clock.GetCurrentInstant());
        var calculationResultObject = new
        {
            payment = new
            {
                total = new
                {
                    premium = totalPremium.ToDollarsAndCents(),
                },
            },
        };

        var formDataSchema = new FormDataSchema(new JObject());
        var calculationData = new CachingJObjectWrapper(JObject.FromObject(calculationResultObject));
        var quoteDataRetriever = QuoteFactory.QuoteDataRetriever(formData, calculationData);

        quote.RecordCalculationResult(
            CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetriever),
            calculationData,
            this.clock.GetCurrentInstant(),
            formDataSchema,
            false,
            userId);
        return Result.Success<Quote, Error>(quote);
    }

    private Dictionary<string, object> CreateFormModel(Dictionary<string, object>? customFormModel)
    {
        var inceptionDate = this.clock.Now().InZone(Timezones.AET).Date;
        var expiryDate = inceptionDate.PlusYears(1);

        var formModel = new Dictionary<string, object>();

        formModel["insuredName"] = "foo";
        formModel["inceptionDate"] = inceptionDate.ToIso8601();
        formModel["policyStartDate"] = inceptionDate.ToIso8601();
        formModel["expiryDate"] = expiryDate.ToIso8601();
        formModel["policyEndDate"] = expiryDate.ToIso8601();
        formModel["contactAddressLine1"] = "1 foo Street";
        formModel["contactAddressSuburb"] = "Fooville";
        formModel["contactAddressState"] = "VIC";
        formModel["contactAddressPostcode"] = "3109";

        if (customFormModel != null)
        {
            foreach (var formDataItem in customFormModel)
            {
                formModel[formDataItem.Key] = formDataItem.Value;
            }
        }

        return formModel;
    }
}