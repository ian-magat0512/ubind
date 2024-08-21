// <copyright file="InceptionTimeFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Services;

    /// <summary>
    /// This class is needed to retrieve inception time data from formdata or calculation and convert it to LocalDate type.
    /// </summary>
    public class InceptionTimeFieldRetriever : BaseFieldRetriever
    {
        /// <inheritdoc/>
        public override object? Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            // retrieve quote data in this order:
            // 1. Use the new data locator in the product.json
            // 2. if not found, then use the quote data locator.
            var dataLocations = this.GetDataLocations(config.DataLocators?.InceptionDate, config.QuoteDataLocations?.InceptionDate);
            var inceptionDate = this.GetDataValue(dataLocations, formData, calculationData);
            if (!string.IsNullOrEmpty(inceptionDate))
            {
                var policyLimitTimesPolicy = new DefaultPolicyTransactionTimeOfDayScheme();
                var inceptionLocalDate = inceptionDate.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy(nameof(inceptionDate));
                return policyLimitTimesPolicy.GetInceptionTimestamp(inceptionLocalDate);
            }

            return default(Instant);
        }
    }
}
