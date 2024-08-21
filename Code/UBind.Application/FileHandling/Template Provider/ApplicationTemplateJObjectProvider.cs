// <copyright file="ApplicationTemplateJObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;

    /// <summary>
    /// Policy template JSON object provider.
    /// </summary>
    public class ApplicationTemplateJObjectProvider : IJObjectProvider
    {
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
            jsonObject.ApplicationId = jsonObject.QuoteId = applicationEvent.Aggregate.Id;
            jsonObject.QuoteProductId = applicationEvent.Aggregate.ProductId;
            jsonObject.QuoteCreationDate = applicationEvent.Aggregate.CreatedTimestamp.ToString();

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
