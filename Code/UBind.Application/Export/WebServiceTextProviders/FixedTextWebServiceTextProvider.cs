// <copyright file="FixedTextWebServiceTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.WebServiceTextProviders
{
    using System;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Text provider that returns fixed text.
    /// </summary>
    public class FixedTextWebServiceTextProvider : IWebServiceTextProvider
    {
        private readonly string text;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedTextWebServiceTextProvider"/> class.
        /// </summary>
        /// <param name="text">The text to provide.</param>
        public FixedTextWebServiceTextProvider(string text)
        {
            this.text = text;
        }

        /// <inheritdoc />
        public string Invoke(string payloadJson, QuoteAggregate quoteAggregate, IProductConfiguration productConfiguration, Guid quoteId)
        {
            return this.text;
        }
    }
}
