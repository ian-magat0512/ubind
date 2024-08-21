// <copyright file="CurrencyCodeFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using UBind.Domain.Json;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// This class is needed to retrieve the currency code data from formdata or calculation.
    /// </summary>
    public class CurrencyCodeFieldRetriever : BaseFieldRetriever
    {
        /// <inheritdoc/>
        public override object Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            // retrieve quote data in this order:
            // 1. Use the new data locator in the product.json
            // 2. if not found, then use the quote data locator.
            var dataLocations = this.GetDataLocations(config.DataLocators?.CurrencyCode, config.QuoteDataLocations?.CurrencyCode);
            var value = this.GetDataValue(dataLocations, formData, calculationData);
            return value ?? PriceBreakdown.DefaultCurrencyCode;
        }
    }
}
