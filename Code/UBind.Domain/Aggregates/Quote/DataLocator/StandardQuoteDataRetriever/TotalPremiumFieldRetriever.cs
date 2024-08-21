// <copyright file="TotalPremiumFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;

    /// <summary>
    /// This class is needed to retrieve the total premium from calculation.
    /// </summary>
    public class TotalPremiumFieldRetriever : BaseFieldRetriever
    {
        /// <inheritdoc/>
        public override object? Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            // retrieve quote data in this order:
            // 1. Use the new data locator in the product.json
            // 2. if not found, then use the quote data locator.
            var dataLocations = this.GetDataLocations(config.DataLocators?.TotalPremium, config.QuoteDataLocations?.TotalPremium);
            var value = this.GetDataValue(dataLocations, formData, calculationData);

            if (value != null)
            {
                return CurrencyParser.ParseToDecimalOrThrow(value);
            }

            return null;
        }
    }
}
