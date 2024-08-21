// <copyright file="BusinessEndDateFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;

    /// <summary>
    /// This class is needed to retrieve business end date data from formdata or calculation and convert it to LocalDate type.
    /// </summary>
    public class BusinessEndDateFieldRetriever : BaseFieldRetriever
    {
        /// <inheritdoc/>
        public override object? Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            // retrieve quote data in this order:
            // 1. Use the new data locator in the product.json
            // 2. if not found, then use the quote data locator.
            var dataLocations = this.GetDataLocations(config.DataLocators?.BusinessEndDate, config.QuoteDataLocations?.BusinessEndDate);
            var businessEndDate = this.GetDataValue(dataLocations, formData, calculationData);
            if (!string.IsNullOrEmpty(businessEndDate))
            {
                return businessEndDate.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy(nameof(businessEndDate));
            }
            return default;
        }
    }
}
