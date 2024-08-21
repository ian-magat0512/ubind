// <copyright file="CustomerNameFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using UBind.Domain.Json;

    /// <summary>
    /// This class is needed to retrieve the customer name data from form data or calculation.
    /// </summary>
    public class CustomerNameFieldRetriever : BaseFieldRetriever
    {
        /// <inheritdoc/>
        public override object? Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            // retrieve quote data in this order:
            // 1. Use the new data locator in the product.json
            // 2. if not found, then use the quote data locator.
            var dataLocations = this.GetDataLocations(config.DataLocators?.CustomerName, config.QuoteDataLocations?.ContactName);
            return this.GetDataValue(dataLocations, formData, calculationData);
        }
    }
}
