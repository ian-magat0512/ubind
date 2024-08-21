// <copyright file="DebitOptionRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;

using UBind.Domain.Json;

/// <summary>
/// Retrieves the value if 'Credit Card' or 'Direct Debit'
/// </summary>
public class DebitOptionRetriever : BaseFieldRetriever
{
    public override object? Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
    {
        // retrieve quote data in this order:
        // 1. Use the new data locator in the product.json
        // 2. if not found, then use the quote data locator.
        var dataLocations = this.GetDataLocations(config.DataLocators?.DebitOption, config.QuoteDataLocations?.DebitOption);
        return this.GetDataValue(dataLocations, formData, calculationData);
    }
}
