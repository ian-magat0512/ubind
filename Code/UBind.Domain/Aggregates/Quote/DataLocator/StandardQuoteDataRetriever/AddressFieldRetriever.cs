// <copyright file="AddressFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using UBind.Domain.Json;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// This class is needed to retrieve the address data from form data or calculation.
    /// </summary>
    public class AddressFieldRetriever : BaseFieldRetriever
    {
        /// <inheritdoc/>
        public override object Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            // retrieve quote data in this order:
            // 1. Use the new data locator in the product.json
            // 2. if not found, then use the quote data locator.
            var dataLocations = this.GetDataLocations(config.DataLocators?.ContactAddressLine1, config.QuoteDataLocations?.ContactAddressLine1);
            var addressLine = this.GetDataValue(dataLocations, formData, calculationData) ?? string.Empty;

            dataLocations = this.GetDataLocations(config.DataLocators?.ContactAddressSuburb, config.QuoteDataLocations?.ContactAddressSuburb);
            var suburb = this.GetDataValue(dataLocations, formData, calculationData) ?? string.Empty;

            dataLocations = this.GetDataLocations(config.DataLocators?.ContactAddressState, config.QuoteDataLocations?.ContactAddressState);
            var state = this.GetDataValue(dataLocations, formData, calculationData) ?? string.Empty;

            dataLocations = this.GetDataLocations(config.DataLocators?.ContactAddressPostcode, config.QuoteDataLocations?.ContactAddressPostcode);
            var postal = this.GetDataValue(dataLocations, formData, calculationData) ?? string.Empty;

            return new Address(addressLine, suburb, postal, state);
        }
    }
}
