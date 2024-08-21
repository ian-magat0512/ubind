﻿// <copyright file="ExpiryDateFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;

    /// <summary>
    /// This class is needed to retrieve expiry date data from formdata or calculation and convert it to LocalDate type.
    /// </summary>
    public class ExpiryDateFieldRetriever : BaseFieldRetriever
    {
        /// <inheritdoc/>
        public override object? Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            // retrieve quote data in this order:
            // 1. Use the new data locator in the product.json
            // 2. if not found, then use the quote data locator.
            var dataLocations = this.GetDataLocations(config.DataLocators?.ExpiryDate, config.QuoteDataLocations?.ExpiryDate);
            var expiryDate = this.GetDataValue(dataLocations, formData, calculationData);
            if (!string.IsNullOrEmpty(expiryDate))
            {
                return expiryDate.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy(nameof(expiryDate));
            }

            dataLocations = this.GetDataLocations(config.DataLocators?.InceptionDate, config.QuoteDataLocations?.InceptionDate);
            var inceptionDate = this.GetDataValue(dataLocations, formData, calculationData);
            if (!string.IsNullOrEmpty(inceptionDate))
            {
                return inceptionDate.TryParseAsLocalDate().Value.PlusMonths(12);
            }

            return default;
        }
    }
}
