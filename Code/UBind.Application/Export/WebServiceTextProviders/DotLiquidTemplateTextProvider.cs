// <copyright file="DotLiquidTemplateTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.WebServiceTextProviders
{
    using System;
    using System.Collections.Generic;
    using DotLiquid;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// Provides text formatted using DotLiquid model templates.
    /// </summary>
    public class DotLiquidTemplateTextProvider : IWebServiceTextProvider
    {
        private IWebServiceTextProvider templateStringProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotLiquidTemplateTextProvider"/> class.
        /// </summary>
        /// <param name="templateStringProvider">The template provider to be used.</param>
        public DotLiquidTemplateTextProvider(IWebServiceTextProvider templateStringProvider)
        {
            this.templateStringProvider = templateStringProvider;
        }

        /// <inheritdoc />
        public string Invoke(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId)
        {
            var templateProvided = this.templateStringProvider.Invoke(payloadJson, quoteAggregate, productConfiguration, quoteId);
            var jsonObject = JsonConvert.DeserializeObject<IDictionary<string, object>>(payloadJson, new DictionaryConverter());
            var jsonHash = Hash.FromDictionary(jsonObject);

            var template = Template.Parse(templateProvided);
            var result = template.Render(jsonHash);
            return result;
        }
    }
}
