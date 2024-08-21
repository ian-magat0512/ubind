// <copyright file="EFundExpressIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.EFundExpress
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Funding.EFundExpress;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class EFundExpressIntegrationTests
    {
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();

        public EFundExpressIntegrationTests()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task CreateFundingProposal_ReturnsFundingDocument_WhenApplicationIsValid()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var configuration = new TestEFundExpressProductConfiguration();
            var urlHelper = new Mock<IFundingServiceRedirectUrlHelper>();
            var quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();

            var clock = SystemClock.Instance;
            urlHelper
                .Setup(h => h.GetSuccessRedirectUrl(
                    It.IsAny<ProductContext>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns("https://sucess.example.com");
            urlHelper
                .Setup(h => h.GetCancellationRedirectUrl(
                    It.IsAny<ProductContext>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns("https://cancel.example.com");

            var sut = new EfundExpressService(configuration, urlHelper.Object, this.cachingResolver.Object, clock);
            var quote = QuoteFactory.CreateNewBusinessQuote(tenantId);
            var quoteAggregate = quote.Aggregate
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id)
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult.Data);

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
            fundingProposal.AcceptanceUrl.Should().NotBeNull();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task FundingDocumentRequestIsReturned_WhenApplicationIsIncompleteAndRequestUsesPlaceholderData()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var configuration = new TestEFundExpressProductConfiguration();
            var urlHelper = new Mock<IFundingServiceRedirectUrlHelper>();
            var clock = SystemClock.Instance;
            urlHelper
                .Setup(u => u.GetSuccessRedirectUrl(
                    It.IsAny<ProductContext>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns("https://success.example.com");
            urlHelper
                .Setup(u => u.GetCancellationRedirectUrl(
                    It.IsAny<ProductContext>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns("https://cancel.example.com");
            var sut = new EfundExpressService(configuration, urlHelper.Object, this.cachingResolver.Object, clock);
            var customerDetails = new FakePersonalDetails();
            customerDetails.Email = null;
            var quote = QuoteFactory.CreateNewBusinessQuote(tenantId);
            var quoteAggregate = quote.Aggregate
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id)
                .WithCustomerDetails(quote.Id, customerDetails);
            var priceBreakdown = PriceBreakdown.CreateFromCalculationResultData(quote.LatestCalculationResult?.Data);

            // Act
            var fundingProposal = await sut.CreateFundingProposal(
                quote.Aggregate.ProductContext,
                quote.LatestFormData.Data,
                quote.LatestCalculationResult.Data,
                priceBreakdown,
                quote,
                null,
                false, CancellationToken.None);

            // Assert
            fundingProposal.ArePlaceholdersUsed.Should().BeTrue();
        }
    }
}
