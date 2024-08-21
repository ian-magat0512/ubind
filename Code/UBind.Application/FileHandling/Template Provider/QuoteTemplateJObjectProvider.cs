// <copyright file="QuoteTemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Quote template JSON object provider.
    /// </summary>
    public class QuoteTemplateJObjectProvider : IJObjectProvider
    {
        private ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteTemplateJObjectProvider"/> class.
        /// </summary>
        /// <param name="productService">The product service.</param>
        public QuoteTemplateJObjectProvider(ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public async Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            if (quote.QuoteNumber == null)
            {
                return;
            }

            var product = await this.cachingResolver.GetProductOrThrow(
                applicationEvent.Aggregate.TenantId,
                applicationEvent.Aggregate.ProductId);
            dynamic jsonObject = new JObject();
            if (quote.VersionNumber != default)
            {
                jsonObject.QuoteVersionNumber = quote.VersionNumber;
            }

            var createdTimestamp = (quote?.CreatedTimestamp).GetValueOrDefault();
            var expiryTime = (quote?.ExpiryTimestamp).GetValueOrDefault();

            if (expiryTime != default && product.Details.QuoteExpirySetting.Enabled)
            {
                jsonObject.QuoteExpiryDate = expiryTime.ToRfc5322DateStringInAet();
                jsonObject.QuoteExpiryTime = expiryTime.To12HourClockTimeInAet();
            }

            jsonObject.QuoteTenantId = applicationEvent.Aggregate.TenantId;
            jsonObject.QuoteCreationDate = createdTimestamp.ToRfc5322DateStringInAet();
            jsonObject.QuoteCreatedTimestamp = createdTimestamp.To12HourClockTimeInAet();
            jsonObject.QuoteNumber = quote.QuoteNumber;
            jsonObject.QuoteId = applicationEvent.Aggregate.Id;

            IJsonObjectParser parser = new GenericJObjectParser(string.Empty, jsonObject);
            if (parser.JsonObject != null)
            {
                this.JsonObject = parser.JsonObject;
            }
        }
    }
}
