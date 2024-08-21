﻿// <copyright file="NumberOfInstallmentsFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using UBind.Domain.Json;

    /// <summary>
    /// This class is needed to retrieve the number of installments data from formdata or calculation and convert it to int type.
    /// </summary>
    public class NumberOfInstallmentsFieldRetriever : BaseFieldRetriever
    {
        /// <inheritdoc/>
        public override object Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            // retrieve quote data in this order:
            // 1. Use the new data locator in the product.json
            // 2. if not found, then use the quote data locator.
            var dataLocations = this.GetDataLocations(config.DataLocators?.NumberOfInstallments, config.QuoteDataLocations?.NumberOfInstallments);
            var value = this.GetDataValue(dataLocations, formData, calculationData);

            int.TryParse(value, out int result);
            return result;
        }
    }
}
