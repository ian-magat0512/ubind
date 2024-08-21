// <copyright file="PaymentAttemptResultsJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Payment template JSON object provider.
    /// </summary>
    public class PaymentAttemptResultsJObjectProvider : IJObjectProvider
    {
        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            dynamic jsonObject = new JObject();
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);

            // TODO: Currently there is no flow for payment, but only payment attempt records.
            // Uupdate this once payment flow is ready.
            if (quote.LatestPaymentAttemptResult != null)
            {
                if (quote.LatestPaymentAttemptResult.Errors.Any())
                {
                    this.JsonObject = new GenericJObjectParser(string.Empty, jsonObject).JsonObject;
                    return Task.CompletedTask;
                }

                var createdTimestamp = (quote?.LatestPaymentAttemptResult?.CreatedTimestamp).GetValueOrDefault();

                jsonObject.PaymentDate = createdTimestamp.ToRfc5322DateStringInAet();
                jsonObject.PaymentReference = quote.LatestPaymentAttemptResult.PaymentDetails?.Reference;
                jsonObject.PaymentSucceeded = quote.LatestPaymentAttemptResult.IsSuccess ? "Yes" : "No";
            }

            IJsonObjectParser parser
                = new GenericJObjectParser(string.Empty, jsonObject);
            if (parser.JsonObject != null)
            {
                this.JsonObject = parser.JsonObject;
            }

            return Task.CompletedTask;
        }
    }
}
