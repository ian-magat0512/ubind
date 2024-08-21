// <copyright file="TestStripeTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Payment.Stripe
{
    using System.Threading.Tasks;
    using Flurl.Http;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Extensions;

    /// <summary>
    /// For getting credit card tokens for use in testing.
    /// </summary>
    public class TestStripeTokenProvider
    {
        public async Task<string> GetTokenForValidCard()
        {
            var creditCardDetails = new CreditCardDetails(
                "4242424242424242",
                "John Smith",
                "12",
                this.GetCurrentYear() + 1,
                "123");
            return await this.GetToken(creditCardDetails);
        }

        public async Task<string> GetTokenForInvalidCard()
        {
            var creditCardDetails = new CreditCardDetails(
                "4444333322221111",
                "John Smith",
                "12",
                this.GetCurrentYear() + 1,
                "123");
            return await this.GetToken(creditCardDetails);
        }

        public async Task<string> GetTokenForExpiredCard()
        {
            var creditCardDetails = new CreditCardDetails(
                "4242424242424242",
                "John Smith",
                "12",
                this.GetCurrentYear() - 2,
                "123");
            return await this.GetToken(creditCardDetails);
        }

        private async Task<string> GetToken(CreditCardDetails creditCardDetails)
        {
            // We are fetching a token here for integration testing only.
            // In the application proper, tokens will be fetched in the front end.
            string tokenEndpoint = "https://api.stripe.com/v1/tokens";
            var requestString = string.Format(
                "card[number]={0}&card[exp_month]={1}&card[exp_year]={2}&card[cvc]={3}&card[name]={4}",
                creditCardDetails.Number,
                creditCardDetails.ExpiryMonth,
                creditCardDetails.ExpiryYear,
                creditCardDetails.Cvv,
                creditCardDetails.Name);
            var response = await tokenEndpoint
                .WithBasicAuth("pk_test_GueQNT3YWqCqJtkctdQEQZck", string.Empty)
                .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                .PostStringAsync(requestString)
                .ReceiveString();
            var json = JObject.Parse(response);
            var tokenId = (string)json["id"];
            return tokenId;
        }

        private int GetCurrentYear()
        {
            return SystemClock.Instance.Now().InUtc().Year;
        }
    }
}
