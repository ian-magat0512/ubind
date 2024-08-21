// <copyright file="PremiumFundingServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.PremiumFunding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Exceptions;
    using UBind.Application.Export.ViewModels;
    using UBind.Application.Funding.PremiumFunding;
    using UBind.Application.Person;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class PremiumFundingServiceTests
    {
        // This is an old token that has now expired.
        private const string BadToken = "c45f96mhsc4capgvbuaknm7es5";
        private readonly Guid userId = Guid.NewGuid();
        private readonly IClock clock;
        private readonly AccessTokenProvider realAccessTokenProvider = new AccessTokenProvider();
        private readonly Mock<IAccessTokenProvider> mockAccessTokenProvider = new Mock<IAccessTokenProvider>();
        private readonly TestPremiumFundingConfiguration configuration = new TestPremiumFundingConfiguration();
        private readonly CachingAccessTokenProvider cachingAccessTokenProvider;
        private readonly IPersonService personService = new Mock<IPersonService>().Object;
        private readonly IProductConfiguration productConfiguration = new Mock<IProductConfiguration>().Object;
        private readonly ICachingResolver cachingResolver = new Mock<ICachingResolver>().Object;
        private readonly IFormDataPrettifier formDataPrettifier = new Mock<IFormDataPrettifier>().Object;
        private readonly IEmailInvitationConfiguration emailConfiguration =
            new Mock<IEmailInvitationConfiguration>().Object;

        private readonly Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();

        private readonly IConfigurationService configurationService = new Mock<IConfigurationService>().Object;

        private IQuoteAggregateResolverService quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>().Object;
        private Guid quoteId;

        public PremiumFundingServiceTests()
        {
            var mockConfig = new Mock<IConfigurationService>();
            mockConfig.Setup(x => x.GetConfigurationAsync(It.IsAny<ReleaseContext>(), WebFormAppType.Quote))
                .Returns(Task.FromResult(new ReleaseProductConfiguration
                {
                    ConfigurationJson = "{ \"baseConfiguration\": { \"textElements\": { } } }",
                    ProductReleaseId = default,
                }));
            mockConfig.Setup(x => x.GetProductComponentConfiguration(It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
               .Returns(Task.FromResult(this.GetSampleProductComponentConfigurationForQuote()));
            this.configurationService = mockConfig.Object;
            this.clock = SystemClock.Instance;
            this.cachingAccessTokenProvider = new CachingAccessTokenProvider(this.mockAccessTokenProvider.Object, this.clock);
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CreateFundingProposal_Succeeds_WhenRequiredFieldsAreSupplied()
        {
            // Arrange
            var sut = this.CreateSut();
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);

            // Act
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            // Assert
            fundingProposal.Should().NotBeNull();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CreateFundingProposal_Succeeds_WhenRetryIsRequiredDueToBadAccessToken()
        {
            // Arrange
            var sut = this.CreateSut();
            await this.SetupMixedAccessTokens(true, false);
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);

            // Act
            var fundingProposal = await sut.CreateFundingProposal(
               quote.Aggregate.ProductContext,
               quote.LatestFormData.Data,
               quote.LatestCalculationResult.Data,
               priceBreakdown,
               quote,
               null,
               false,
               CancellationToken.None);

            // Assert
            fundingProposal.Should().NotBeNull();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CreateFundingProposal_Fails_WhenMultipleBadAccessTokenAreUsed()
        {
            // Arrange
            var sut = this.CreateSut();
            await this.SetupMixedAccessTokens(false, false);
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);

            // Action
            Func<Task> action = async () => await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<ExternalServiceException>();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task UpdateFundingProposal_Succeeds_WhenRequiredFieldsAreSupplied()
        {
            // Arrange
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var sut = this.CreateSut(quoteAggregate);

            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);

            // Act
            var updatedFundingProposal = await sut.UpdateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFundingProposalCreationResult.Proposal.ExternalId,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            // Assert
            updatedFundingProposal.Should().NotBeNull();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task UpdateFundingProposal_Succeeds_WhenRetryIsRequiredDueToBadToken()
        {
            // Arrange
            await this.SetupMixedAccessTokens(true, false, true); // First one is for proposal creation in "arrange"
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);

            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);

            // Act
            var updatedFundingProposal = await sut.UpdateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFundingProposalCreationResult.Proposal.ExternalId,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false, CancellationToken.None);

            // Assert
            updatedFundingProposal.Should().NotBeNull();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task UpdateFundingProposal_Fails_WhenRetryFailsDueToBadToken()
        {
            // Arrange
            await this.SetupMixedAccessTokens(true, false, false); // First one is for proposal creation in "arrange"
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            this.cachingAccessTokenProvider.ClearCachedAccessToken(this.configuration.Username, this.configuration.ApiVersion);

            // Act
            Func<Task> action = async () => await sut.UpdateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFundingProposalCreationResult.Proposal.ExternalId,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<ExternalServiceException>();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SubmitContract_Fails_WhenExpiredCreditCardDetailsUsed()
        {
            // Arrange
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var customerDetails = this.CreateCustomerDetails();
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var sut = this.CreateSut(quoteAggregate);

            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            var creditCardDetails = new CreditCardDetails(
                "4111111111111111",
                "John Smith",
                "12",
                2018,
                "123");

            // Act
            Func<Task<FundingProposal>> func = () => sut.AcceptFundingProposal(
                quote, fundingProposal.InternalId, creditCardDetails, false, default);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.AdditionalDetails.Should().Contain(e => e.IndexOf("Invalid expiry date") >= 0);
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SubmitContract_Fails_WhenBadCreditCardDetailsUsed()
        {
            // Arrange
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var customerDetails = this.CreateCustomerDetails();
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);
            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            var creditCardDetails = new CreditCardDetails(
                "4532074470211636", // Number provided by Premium Funding Co to trigger error.
                "Test Payment Failure", // Name  provided by Premium Funding Co to trigger error.
                "12",
                this.GetCurrentYear() + 1,
                "123");

            // Act
            Func<Task<FundingProposal>> func = () => sut.AcceptFundingProposal(
                quote, fundingProposal.InternalId, creditCardDetails, false, default);

            // Assert
            await func.Should().ThrowAsync<ErrorException>();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SubmitContract_Fails_WhenBadPostcodeIsUsed()
        {
            // Arrange
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithRejectedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var customerDetails = this.CreateCustomerDetails();
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);
            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            var creditCardDetails = new CreditCardDetails(
                "4111111111111111",
                "John Smith",
                "12",
                this.GetCurrentYear() + 1,
                "483");

            // Act
            Func<Task<FundingProposal>> func = () => sut.AcceptFundingProposal(
                quote, fundingProposal.InternalId, creditCardDetails, false, default);

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.AdditionalDetails.Should().Contain(e => e == "You must supply a valid postcode.");
        }

        [Fact(Skip = "No valid credit card details for testing. Current test number not accepted by gateway.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SubmitContract_Succeeds_WhenCorrectCreditCardDetailsProvided()
        {
            // Arrange
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var customerDetails = this.CreateCustomerDetails();
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            var creditCardDetails = new CreditCardDetails(
                "4111111111111111",
                "John Smith",
                "12",
                this.GetCurrentYear() + 1,
                "483");

            // Act
            Func<Task<FundingProposal>> func = () => sut.AcceptFundingProposal(
                quote, fundingProposal.InternalId, creditCardDetails, false, default);

            // Assert
            await func.Should().NotThrowAsync();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SubmitContract_Fails_ForTestQuote_WhenCorrectCreditCardDetailsProvided()
        {
            // Arrange
            var configuration = new TestPremiumFundingConfiguration();
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateTestQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var customerDetails = this.CreateCustomerDetails();
            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);
            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            var creditCardDetails = new CreditCardDetails(
                "4111111111111111",
                "John Smith",
                "12",
                this.GetCurrentYear() + 1,
                "483");

            // Act
            Func<Task<FundingProposal>> func = () => sut.AcceptFundingProposal(
                quote, fundingProposal.InternalId, creditCardDetails, true, default);

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.AdditionalDetails.First().Should().Be("The total premium amount must be greater than $1.00.");
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SubmitContract_Succeeds_WhenCorrectBankAccountDetailsProvided()
        {
            // Arrange
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var customerDetails = this.CreateCustomerDetails();
            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);
            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            var bankAccountDetails = new BankAccountDetails(
                "John Smith",
                "12345678",
                "123456");

            // Act
            Func<Task<FundingProposal>> func = () => sut.AcceptFundingProposal(
                quote, fundingProposal.InternalId, bankAccountDetails, false, default);

            // Assert
            await func.Should().NotThrowAsync();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SubmitContract_Fails_ForTestQuote_WhenCorrectBankAccountDetailsProvided()
        {
            // Arrange
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateTestQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);
            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            var bankAccountDetails = new BankAccountDetails(
                "John Smith",
                "12345678",
                "123456");

            // Act
            Func<Task<FundingProposal>> func = () => sut.AcceptFundingProposal(
                quote, fundingProposal.InternalId, bankAccountDetails, true, default);

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.AdditionalDetails.First().Should().Be("The total premium amount must be greater than $1.00.");
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task AcceptedProposalDetailsAreExposedOnApplicationEventModel()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();

            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);
            var sut = this.CreateSut(quoteAggregate);
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);
            quote.RecordFundingProposalCreated(fundingProposal, this.userId, this.clock.Now(), quote.Id);
            var bankAccountDetails = new BankAccountDetails(
                "John Smith",
                "12345678",
                "123456");

            var acceptedProposal = await sut.AcceptFundingProposal(
                quote, fundingProposal.InternalId, bankAccountDetails, false, default);
            DisplayableFieldDto displayableFieldsDto = new DisplayableFieldDto(new List<string>(), new List<string>(), true, true);

            var tenant = TenantFactory.Create();
            var product = ProductFactory.Create(tenant.Id);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, Guid.NewGuid(), this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.Now().Plus(Duration.FromMinutes(1)));
            var organisationReadModelSummary = new OrganisationReadModelSummary
            {
                TenantId = organisation.TenantId,
                Id = organisation.Id,
                Alias = organisation.Alias,
                Name = organisation.Name,
                IsActive = organisation.IsActive,
                IsDeleted = organisation.IsDeleted,
                IsDefault = true,
                CreatedTimestamp = organisation.CreatedTimestamp,
                LastModifiedTimestamp = organisation.CreatedTimestamp,
            };

            // Act
            quoteAggregate.RecordFundingProposalAccepted(acceptedProposal.InternalId, this.userId, this.clock.Now(), quote.Id);

            // Assert
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.FundingProposalAccepted,
                quoteAggregate,
                quote.Id,
                0,
                "1",
                quote.ProductReleaseId.Value);
            ApplicationEventViewModel viewModel = await ApplicationEventViewModel.Create(
                this.formDataPrettifier,
                applicationEvent,
                this.emailConfiguration,
                this.configurationService,
                this.productConfiguration,
                displayableFieldsDto,
                this.personService,
                tenant,
                product,
                organisationReadModelSummary,
                this.clock,
                this.mediator.Object);
            var pdfLink = $"https://api.premiumfunding.net.au{viewModel.Funding.Response["data.links.PdfURL"]}";

            viewModel.Funding.Data.Should().NotBeNull();
            viewModel.Funding.Response.Should().NotBeNull();
            pdfLink.Should().NotBeNull();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CreateProposal_CanGenerateProposal_WithoutCreditCardDetails()
        {
            // Arrange
            var sut = this.CreateSut();
            this.SetupAllValidAccessTokens();
            var createQuoteResult = this.CreateQuoteWithAcceptedStateAndPostcode();
            var quoteAggregate = createQuoteResult.Value;
            var quote = quoteAggregate.GetQuoteOrThrow(this.quoteId);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);

            // Act
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false,
                CancellationToken.None);

            // Assert
            fundingProposal.Should().NotBeNull();
        }

        private PremiumFundingService CreateSut(QuoteAggregate quoteAggregate = null)
        {
            if (quoteAggregate != null)
            {
                var mockQuoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
                mockQuoteAggregateResolverService.Setup(x => x.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(quoteAggregate);
                this.quoteAggregateResolverService = mockQuoteAggregateResolverService.Object;
            }

            return new PremiumFundingService(
                this.quoteAggregateResolverService,
                this.configuration,
                this.cachingAccessTokenProvider,
                this.cachingResolver,
                this.clock);
        }

        private void SetupAllValidAccessTokens(params bool[] tokenValidityList)
        {
            this.mockAccessTokenProvider
                .Setup(p => p.GetAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(this.realAccessTokenProvider.GetAccessToken(this.configuration.Username, this.configuration.Password, this.configuration.ApiVersion));
        }

        private async Task SetupMixedAccessTokens(params bool[] tokenValidityList)
        {
            var tokens = new Queue<string>();
            foreach (var tokenValiditySetting in tokenValidityList)
            {
                var token = tokenValiditySetting
                        ? await this.realAccessTokenProvider.GetAccessToken(this.configuration.Username, this.configuration.Password, this.configuration.ApiVersion)
                        : BadToken;
                tokens.Enqueue(token);
            }

            this.mockAccessTokenProvider
                .Setup(p => p.GetAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(tokens.Dequeue()));
        }

        private Result<QuoteAggregate, Error> CreateQuoteWithAcceptedStateAndPostcode()
        {
            return this.CreateQuote("VIC", "3333");
        }

        private Result<QuoteAggregate, Error> CreateTestQuoteWithAcceptedStateAndPostcode()
        {
            return this.CreateTestQuote("VIC", "3333", this.userId);
        }

        private Result<QuoteAggregate, Error> CreateQuoteWithRejectedStateAndPostcode()
        {
            return this.CreateQuote("ACT", "0200");
        }

        private Result<QuoteAggregate, Error> CreateQuote(string state, string postcode)
        {
            var workflowProvider = new DefaultQuoteWorkflowProvider();
            var quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DeploymentEnvironment.Staging,
                QuoteExpirySettings.Default,
                this.userId,
                this.clock.GetCurrentInstant(),
                Guid.NewGuid(),
                Timezones.AET);
            this.quoteId = quote.Id;
            var inceptionDate = this.clock.Now().InZone(Timezones.AET).Date;
            var inceptionTime = inceptionDate.AtStartOfDayInZone(Timezones.AET).PlusHours(16).ToInstant();
            var expiryDate = inceptionDate.PlusYears(1);
            var expiryTime = expiryDate.AtStartOfDayInZone(Timezones.AET).PlusHours(16).ToInstant();
            var formDataObject = new
            {
                formModel = new
                {
                    insuredName = "foo",
                    inceptionDate = inceptionDate.ToIso8601(),
                    policyStartDate = inceptionDate.ToIso8601(),
                    expiryDate = expiryDate.ToIso8601(),
                    policyEndDate = expiryDate.ToIso8601(),
                    contactAddressLine1 = "1 foo Street",
                    contactAddressSuburb = "Fooville",
                    contactAddressState = state,
                    contactAddressPostcode = postcode,
                },
            };
            var formData = new FormData(JsonConvert.SerializeObject(formDataObject));
            quote.UpdateFormData(formData, this.userId, this.clock.GetCurrentInstant());
            var calculationResultObject = new
            {
                payment = new
                {
                    total = new
                    {
                        premium = "$102.00",
                    },
                },
            };
            var formDataSchema = new FormDataSchema(new JObject());

            var calculationResultData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            quote.UpdateFormData(formData, this.userId, this.clock.GetCurrentInstant());
            var quoteDataRetriever = QuoteFactory.QuoteDataRetriever(formData, calculationResultData);
            quote.RecordCalculationResult(
                CalculationResult.CreateForNewPolicy(calculationResultData, quoteDataRetriever),
                calculationResultData,
                this.clock.GetCurrentInstant(),
                formDataSchema,
                false,
                this.userId);
            return Result.Success<QuoteAggregate, Error>(quote.Aggregate);
        }

        private Result<QuoteAggregate, Error> CreateTestQuote(string state, string postcode, Guid userId)
        {
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
                true);
            this.quoteId = quote.Id;
            var inceptionDate = this.clock.Now().InZone(Timezones.AET).Date;
            var inceptionTime = inceptionDate.AtStartOfDayInZone(Timezones.AET).PlusHours(16).ToInstant();
            var expiryDate = inceptionDate.PlusYears(1);
            var expiryTime = expiryDate.AtStartOfDayInZone(Timezones.AET).PlusHours(16).ToInstant();
            var formDataObject = new
            {
                formModel = new
                {
                    insuredName = "foo",
                    inceptionDate = inceptionDate.ToIso8601(),
                    policyStartDate = inceptionDate.ToIso8601(),
                    expiryDate = expiryDate.ToIso8601(),
                    policyEndDate = expiryDate.ToIso8601(),
                    inceptionTime = inceptionTime.ToUnixTimeTicks(),
                    expiryTime = expiryTime.ToUnixTimeTicks(),
                    contactAddressLine1 = "1 foo Street",
                    contactAddressSuburb = "Fooville",
                    contactAddressState = state,
                    contactAddressPostcode = postcode,
                },
            };
            var formData = new FormData(JsonConvert.SerializeObject(formDataObject));
            quote.UpdateFormData(formData, this.userId, this.clock.GetCurrentInstant());
            var calculationResultObject = new
            {
                payment = new
                {
                    total = new
                    {
                        premium = "$102.00",
                    },
                },
            };

            var formDataSchema = new FormDataSchema(new JObject());
            var calculationData = new CachingJObjectWrapper(JObject.FromObject(calculationResultObject));
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            quote.RecordCalculationResult(
                CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver),
                calculationData,
                this.clock.GetCurrentInstant(),
                formDataSchema,
                false,
                this.userId);
            return Result.Success<QuoteAggregate, Error>(quote.Aggregate);
        }

        private IPersonalDetails CreateCustomerDetails()
        {
            var customerDetails = new Mock<IPersonalDetails>().SetupAllProperties();
            customerDetails.Setup(d => d.FullName).Returns("John Smith");
            customerDetails.Setup(d => d.HomePhone).Returns("03 1234 1234");
            customerDetails.Setup(d => d.MobilePhone).Returns("04 1234 1234");
            customerDetails.Setup(d => d.Email).Returns("john.smith@email.com");
            return customerDetails.Object;
        }

        private IProductComponentConfiguration GetSampleProductComponentConfigurationForQuote()
        {
            Component component = new Component
            {
                Form = new Form
                {
                    TextElements = new List<TextElement>
                    {
                        new TextElement
                        {
                            Category = "Organisation",
                            Name = "Name",
                            Text = "ABC Insurance Ltd.",
                        },
                        new TextElement
                        {
                            Category = "Product",
                            Name = "Title",
                            Text = "General Liability",
                        },
                    },
                },
            };

            var mockConfig = new Mock<IProductComponentConfiguration>();
            mockConfig.Setup(x => x.Component).Returns(component);
            mockConfig.Setup(x => x.Version).Returns("2.0.0");
            mockConfig.Setup(x => x.IsVersion2OrGreater).Returns(true);
            mockConfig.Setup(x => x.IsVersion1).Returns(false);
            return mockConfig.Object;
        }

        private int GetCurrentYear()
        {
            return SystemClock.Instance.Now().InUtc().Year;
        }
    }
}
