// <copyright file="EwayPaymentGatewayTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment.Eway
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NodaTime;
    using UBind.Application.Payment.Eway;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class EwayPaymentGatewayTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly PriceBreakdown samplePayment = PriceBreakdown.CreateFromCalculationFormatV1(
            new CalculatedPaymentTotal(82.64m, 0, 8.26m, 9.09m, 0, 0, 0, 0, 100m, "AUD"));

        private Quote quote;
        private EwayPaymentGateway sut;

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_Succeeds_WhenEndpointAndCredentialsAreCorrect()
        {
            // Arrange
            this.ConfigureGateway();
            var cardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "11",
                25,
                "123");

            // Act
            var response = await this.sut.MakePayment(this.quote, this.samplePayment, cardDetails, "foo");

            // Assert
            response.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_Fails_WhenCreditCardExpiryDateInPast()
        {
            // Arrange
            this.ConfigureGateway();
            var cardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "11",
                17,
                "123");

            // Act
            var response = await this.sut.MakePayment(this.quote, this.samplePayment, cardDetails, "foo");

            // Assert
            response.Success.Should().BeFalse();
            response.Details.Request.Should().NotBeNullOrEmpty();
            response.Details.Response.Should().NotBeNullOrEmpty();
            response.Errors.Should().Contain("\"Invalid Expiry Date\"");
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_Fails_WhenCreditCardNotValid()
        {
            // Arrange
            this.ConfigureGateway();
            var cardDetails = new CreditCardDetails(
                "1111111111114444333322221111",
                "John Smith",
                "11",
                25,
                "123");

            // Act
            var response = await this.sut.MakePayment(this.quote, this.samplePayment, cardDetails, "foo");

            // Assert
            response.Success.Should().BeFalse();
            response.Details.Request.Should().NotBeNullOrEmpty();
            response.Details.Response.Should().NotBeNullOrEmpty();
            response.Errors.Should().Contain("Invalid Card Number");
        }

        [Fact(Skip = "This test should be used when the payment gateway sandbox has been configured to trigger insufficient funds responses.")]
        public async Task MakePayment_ReturnsCorrectError_WhenCreditCardHasInsufficientFunds()
        {
            // Arrange
            this.ConfigureGateway();
            var cardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "11",
                25,
                "123");

            // Act
            var response = await this.sut.MakePayment(this.quote, this.samplePayment, cardDetails, "foo");

            // Assert
            response.Success.Should().BeFalse();
            response.Details.Request.Should().NotBeNullOrEmpty();
            response.Details.Response.Should().NotBeNullOrEmpty();
            response.Errors.Should().Contain("Insufficient Funds");
        }

        [Fact]
        public async Task MakePayment_ReturnsError_WhenGatewayCredentialsAreNotRecognized()
        {
            // Arrange
            var cardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "11",
                25,
                "123");
            this.ConfigureGateway(false);

            // Act
            var response = await this.sut.MakePayment(this.quote, this.samplePayment, cardDetails, "foo");

            // Assert
            response.Success.Should().BeFalse();
            response.Details.Request.Should().NotBeNullOrEmpty();
            response.Details.Response.Should().NotBeNullOrEmpty();
            response.Errors.Should().Contain("Authentication error");
        }

        private void ConfigureGateway(bool useValidCreds = true)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Guid organisationId = Guid.NewGuid();
            const DeploymentEnvironment environment = DeploymentEnvironment.Staging;
            var clock = SystemClock.Instance;
            var workflowProvider = new DefaultQuoteWorkflowProvider();
            var quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();
            this.quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                organisationId,
                ProductFactory.DefaultId,
                environment,
                QuoteExpirySettings.Default,
                this.performingUserId,
                clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);
            var configuration = useValidCreds
                ? TestEwayConfigurationFactory.CreateWithValidCredentials()
                : TestEwayConfigurationFactory.CreateWithInvalidCredentials();
            var logger = new Mock<ILogger>();
            this.sut = new EwayPaymentGateway(configuration, clock, logger.Object);
        }
    }
}
