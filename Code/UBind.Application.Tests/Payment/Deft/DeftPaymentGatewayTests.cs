// <copyright file="DeftPaymentGatewayTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment.Deft
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Payment.Deft;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Extensions;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Payment;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="DeftPaymentGatewayTests" />.
    /// </summary>
    public class DeftPaymentGatewayTests
    {
        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid? performingUserId = Guid.NewGuid();

        /// <summary>
        /// Defines the sampleVisaPayment.
        /// Macquarie DEFT seems to ROUND DOWN it's surcharge amount, so instead of being 1.5 it's 1.49, and therefore a total of 101.48.
        /// </summary>
        private readonly PriceBreakdown sampleVisaPayment = PriceBreakdown.CreateFromCalculationFormatV1(
                new CalculatedPaymentTotal(82.64m, 0, 8.26m, 9.09m, 0, 0, 0.39m, 0, 101.48m, "AUD"));

        /// <summary>
        /// Defines the sampleAmexPayment.
        /// </summary>
        private readonly PriceBreakdown sampleAmexPayment = PriceBreakdown.CreateFromCalculationFormatV1(
            new CalculatedPaymentTotal(82.64m, 0, 8.26m, 9.09m, 0, 0, 0, 0, 100m, "AUD"));

        /// <summary>
        /// Defines the quote.
        /// </summary>
        private readonly Quote quote;

        /// <summary>
        /// Defines the sut.
        /// </summary>
        private DeftPaymentGateway sut;

        public DeftPaymentGatewayTests()
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
            var deftConfiguration = new TestDeftConfiguration();
            var accessTokenProvider = new DeftAccessTokenProvider(deftConfiguration, clock);
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            crnGenerator
                .Setup(g => g.GenerateDeftCrnNumber(
                     TenantFactory.DefaultId, ProductFactory.DefaultId, environment, deftConfiguration.CrnGeneration))
                .Returns("1234567890");
            this.sut = new DeftPaymentGateway(deftConfiguration, accessTokenProvider, crnGenerator.Object, clock);
        }

        [Fact]
        public void GenerateCRNNumber_ShouldAlwaysGenerate_ValidTenDigitSequence()
        {
            // Arrange
            var sequenceGenerator = new Mock<IUniqueNumberSequenceGenerator>();

            for (int i = 0; i < 50; i++)
            {
                sequenceGenerator.Setup(x => x.Next(
                    default,
                    default,
                    DeploymentEnvironment.Development,
                    UniqueNumberUseCase.DeftCrn))
                    .Returns(i);
                var crnGenerator = new DeftCustomerReferenceNumberGenerator(sequenceGenerator.Object);

                // Act
                var crn = crnGenerator.GenerateDeftCrnNumber(
                    default,
                    default,
                    DeploymentEnvironment.Development,
                    new CrnGenerationConfiguration(false, CrnGenerationMethod.Unique10DigitNumber));

                // Assert
                crn.Should().NotContain("-");
                long crnNumber = long.Parse(crn);
                crnNumber.Should().BeGreaterThan(-1);
            }
        }

        [Fact]
        public void GenerateCRNNumber_ShouldAlwaysGenerate_Valid6DigitSequence()
        {
            // Arrange
            var sequenceGenerator = new Mock<IUniqueNumberSequenceGenerator>();

            for (int i = 0; i < 50; i++)
            {
                sequenceGenerator.Setup(x => x.Next(
                    default,
                    default,
                    DeploymentEnvironment.Development,
                    UniqueNumberUseCase.DeftCrn))
                    .Returns(i);
                var crnGenerator = new DeftCustomerReferenceNumberGenerator(sequenceGenerator.Object);

                // Act
                var crn = crnGenerator.GenerateDeftCrnNumber(
                    default,
                    default,
                    DeploymentEnvironment.Development,
                    new CrnGenerationConfiguration(false, CrnGenerationMethod.Fixed4DigitPrefxWithUniqueSixDigitSuffix, "ABCD"));

                // Assert
                crn.Should().NotContain("-");
                long crnNumber = long.Parse(crn.Substring(4));
                crnNumber.Should().BeGreaterThan(-1);
            }
        }

        /// <summary>
        /// The MakePayment_PaysCorrectAmount_WhenMerchantFeesMatchCardSchemeSurcharge.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_PaysCorrectAmount_WhenMerchantFeesMatchCardSchemeSurcharge()
        {
            // Arrange
            var cardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "11",
                22,
                "123");

            // Act
            var response = await this.sut.MakePayment(this.quote, this.sampleVisaPayment, cardDetails, "foo");

            // Assert
            Assert.True(response.Success);
            Assert.Equal(this.sampleVisaPayment.TotalPayable, response.Details.TotalAmount);
        }

        /// <summary>
        /// The MakePayment_Fails_WhenCreditCardHasExpired.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_Fails_WhenCreditCardHasExpired()
        {
            // Arrange
            var cardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "11",
                10,
                "123");

            // Act
            var response = await this.sut.MakePayment(this.quote, this.sampleVisaPayment, cardDetails, "foo");

            // Assert
            response.Success.Should().BeFalse();
            response.Outcome.Should().Be(PaymentAttemptOutcome.Failed);
            Assert.Contains(response.Errors, e => e.Contains("This card has expired"));
        }

        /// <summary>
        /// The MakePayment_Errors_WhenSurchargeIsIncorrect.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_Errors_WhenSurchargeIsIncorrect()
        {
            // Arrange
            var cardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "11",
                10,
                "123");

            // Act
            var response = await this.sut.MakePayment(this.quote, this.sampleAmexPayment, cardDetails, "foo");

            // Assert
            response.Success.Should().BeFalse();
            response.Outcome.Should().Be(PaymentAttemptOutcome.Error);
            response.Details.Request.Should().NotBeNullOrEmpty();
            Assert.Contains(response.Errors, e => e.Contains("Incorrect surcharge"));
        }

        /// <summary>
        /// The MakePayment_Fails_WhenInvalidCreditCard.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_Fails_WhenInvalidCreditCard()
        {
            // Arrange
            var cardDetails = new CreditCardDetails(
                "11111111123283144612312323700003333",
                "John Smith",
                "11",
                10,
                "123");

            // Act
            var response = await this.sut.MakePayment(this.quote, this.sampleVisaPayment, cardDetails, "foo");

            // Assert
            response.Success.Should().BeFalse();
            response.Outcome.Should().Be(PaymentAttemptOutcome.Failed);
            response.Details.Request.Should().NotBeNullOrEmpty();
            response.Details.Response.Should().NotBeNullOrEmpty();
            Assert.Contains(response.Errors, e => e.Contains("Cardnumber Not Valid"));
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_PaymentSuccessful_WhenUsingDefaultProductConfiguration()
        {
            // Arrange
            var clock = SystemClock.Instance;

            var newQuote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);

            var deftConfiguration = new TestDeftConfiguration();
            var accessTokenProvider = new DeftAccessTokenProvider(deftConfiguration, clock);
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            crnGenerator
                .Setup(g => g.GenerateDeftCrnNumber(
                    TenantFactory.DefaultId,
                    ProductFactory.DefaultId,
                    DeploymentEnvironment.Development,
                    deftConfiguration.CrnGeneration))
                .Returns($"{deftConfiguration.CrnGeneration.Prefix}123456");
            this.sut = new DeftPaymentGateway(deftConfiguration, accessTokenProvider, crnGenerator.Object, clock);

            var cardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "11",
                DateTime.Now.Year + 2,
                "123");

            // Act
            var response = await this.sut.MakePayment(newQuote, this.sampleVisaPayment, cardDetails, "foo");

            // Assert
            Assert.True(response.Success);
            Assert.Equal(this.sampleVisaPayment.TotalPayable, response.Details.TotalAmount);
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_PaymentSuccesful_WhenUsingV3Payment()
        {
            // Arrange
            var clock = SystemClock.Instance;

            var newQuote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);

            var deftConfiguration = new TestDeftConfiguration();
            var accessTokenProvider = new DeftAccessTokenProvider(deftConfiguration, clock);
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            crnGenerator
                .Setup(g => g.GenerateDeftCrnNumber(
                    TenantFactory.DefaultId,
                    ProductFactory.DefaultId,
                    DeploymentEnvironment.Development,
                    deftConfiguration.CrnGeneration))
                .Returns($"{deftConfiguration.CrnGeneration.Prefix}123456");
            this.sut = new DeftPaymentGateway(deftConfiguration, accessTokenProvider, crnGenerator.Object, clock);
            var cardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "12",
                DateTime.Now.Year + 2,
                "123");

            // Act
            var response = await this.sut.MakePayment(newQuote, this.sampleVisaPayment, cardDetails, "foo");

            // Assert
            Assert.True(response.Success);
            Assert.Equal(this.sampleVisaPayment.TotalPayable, response.Details.TotalAmount);
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_ShouldNotThrowAnException_WhenErrorResponseIsNotJson()
        {
            // Arrange
            var clock = SystemClock.Instance;

            var newQuote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                clock.Now(),
                Guid.NewGuid(),
                Timezones.AET);

            // Let's set the payment URL to something that we are sure that will not return a JSON
            // By setting it to the ubind portal, we can expect an HTML as response's body
            var paymentUrl = "https://app.ubind.com.au/portal/ubind";

            var deftConfiguration = new TestDeftConfiguration(paymentUrl);
            var accessTokenProvider = new DeftAccessTokenProvider(deftConfiguration, clock);
            var crnGenerator = new Mock<IDeftCustomerReferenceNumberGenerator>();
            crnGenerator
                .Setup(g => g.GenerateDeftCrnNumber(
                    TenantFactory.DefaultId,
                    ProductFactory.DefaultId,
                    DeploymentEnvironment.Development,
                    deftConfiguration.CrnGeneration))
                .Returns($"{deftConfiguration.CrnGeneration.Prefix}123456");

            this.sut = new DeftPaymentGateway(deftConfiguration, accessTokenProvider, crnGenerator.Object, clock);
            var cardDetails = new CreditCardDetails(
                "4283144693700003",
                "John Smith",
                "13",
                DateTime.Now.Year + 2,
                "123");

            Func<Task<PaymentGatewayResult>> func = () =>
            {
                return this.sut.MakePayment(newQuote, this.sampleVisaPayment, cardDetails, "foo");
            };

            // Act/Assert
            await func.Should().NotThrowAsync();
        }
    }
}
