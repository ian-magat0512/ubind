// <copyright file="StripePaymentGatewayTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment.Stripe
{
    using System.Net;
    using System.Threading.Tasks;
    using UBind.Application.Payment.Stripe;
    using UBind.Domain;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests;
    using Xunit;

    public class StripePaymentGatewayTests
    {
        public StripePaymentGatewayTests()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_Succeeds_UsingValidToken()
        {
            // Arrange
            var token = await new TestStripeTokenProvider().GetTokenForValidCard();
            var configuration = new TestStripeConfiguration();
            var payment = PriceBreakdown.CreateFromCalculationFormatV1(
                new CalculatedPaymentTotal(1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 100m, "AUD"));

            var sut = new StripePaymentGateway(configuration);

            // Act
            var response = await sut.MakePayment(payment, token, "foo");

            // Assert
            Assert.True(response.Success);
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task MakePayment_Fails_UsingValidTokenForInvalidCard()
        {
            // Arrange
            var token = await new TestStripeTokenProvider().GetTokenForInvalidCard();
            var configuration = new TestStripeConfiguration();
            var payment = PriceBreakdown.CreateFromCalculationFormatV1(
                new CalculatedPaymentTotal(1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 100m, "AUD"));

            var sut = new StripePaymentGateway(configuration);

            // Act
            var response = await sut.MakePayment(payment, token, "foo");

            // Assert
            Assert.False(response.Success);
        }
    }
}
