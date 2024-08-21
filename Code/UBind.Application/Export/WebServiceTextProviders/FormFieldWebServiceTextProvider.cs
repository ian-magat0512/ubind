// <copyright file="FormFieldWebServiceTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.WebServiceTextProviders
{
    using System;
    using System.Diagnostics.Contracts;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;

    /// <summary>
    /// For providing text parsed from the JSON payload.
    /// </summary>
    public class FormFieldWebServiceTextProvider : IWebServiceTextProvider
    {
        private readonly IWebServiceTextProvider fieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormFieldWebServiceTextProvider"/> class.
        /// </summary>
        /// <param name="fieldName">The name of the form field to read the address from.</param>
        public FormFieldWebServiceTextProvider(IWebServiceTextProvider fieldName)
        {
            Contract.Assert(fieldName != null);
            this.fieldName = fieldName;
        }

        /// <inheritdoc />
        public string Invoke(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId)
        {
            var jsonObject = JObject.Parse(payloadJson);
            var fieldName = this.fieldName.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            return JsonConvert.SerializeObject(jsonObject?[fieldName]) ??
                quoteAggregate.GetQuoteOrThrow(quoteId).LatestFormData?.Data?.GetValue(fieldName);
        }
    }
}
