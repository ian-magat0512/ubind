// <copyright file="PolicyTemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Policy template JSON object provider.
    /// </summary>
    public class PolicyTemplateJObjectProvider : IJObjectProvider
    {
        private IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTemplateJObjectProvider"/> class.
        /// </summary>
        /// <param name="clock">the clock to get current time.</param>
        public PolicyTemplateJObjectProvider(IClock clock)
        {
            this.clock = clock;
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            if (!quote.TransactionCompleted)
            {
                return Task.CompletedTask;
            }

            dynamic jsonObject = new JObject();
            var inceptionDate = (applicationEvent.Aggregate.Policy?.InceptionTimestamp).GetValueOrDefault();
            var expireDate = (applicationEvent.Aggregate.Policy?.ExpiryTimestamp).GetValueOrDefault();
            var cancellationTime = (applicationEvent.Aggregate.Policy?.CancellationEffectiveTimestamp).GetValueOrDefault();
            var createdTimestamp = (applicationEvent.Aggregate?.CreatedTimestamp).GetValueOrDefault();

            jsonObject.PolicyisCancelled = applicationEvent.Aggregate.Policy.IsCancelled ? "Yes" : "No";
            jsonObject.PolicyNumber = applicationEvent.Aggregate.Policy?.PolicyNumber;
            jsonObject.PolicyCancellationDate = cancellationTime.ToRfc5322DateStringInAet();
            jsonObject.PolicyCancellationTime = cancellationTime.To12HourClockTimeInAet();
            jsonObject.PolicyId = applicationEvent.Aggregate.Id;
            jsonObject.PolicyCreationDate = createdTimestamp.ToRfc5322DateStringInAet();
            jsonObject.CancellationDate = cancellationTime.ToRfc5322DateStringInAet();
            jsonObject.PolicyCreatedTimestamp = createdTimestamp.To12HourClockTimeInAet();
            jsonObject.PolicyInceptionDate = inceptionDate.ToRfc5322DateStringInAet();
            jsonObject.PolicyInceptionTime = inceptionDate.To12HourClockTimeInAet();
            jsonObject.PolicyExpiryDate = expireDate.ToRfc5322DateStringInAet();
            jsonObject.PolicyExpiryTime = expireDate.To12HourClockTimeInAet();
            jsonObject.PolicyStatus = applicationEvent.Aggregate.Policy?.GetPolicyStatus(this.clock.Now()).Humanize();

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
