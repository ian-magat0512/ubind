// <copyright file="BaseDataRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Json;

    /// <summary>
    /// Base class for all data retriever.
    /// </summary>
    public abstract class BaseDataRetriever
    {
        /// <summary>
        /// Method for retrieving data.
        /// </summary>
        /// <param name="config">The product configuration.</param>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationData">The calculation data.</param>
        /// <returns>The quote data value.</returns>
        public abstract object Retrieve(IIQumulateQuoteDatumLocations config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData);

        /// <summary>
        /// Method for retrieving data from json object.
        /// </summary>
        /// <param name="location">The data locations.</param>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationData">The calculation data.</param>
        /// <returns>The data value.</returns>
        protected string GetDataValue(QuoteDatumLocation location, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            if (location == null)
            {
                return null;
            }

            var source = location.Object == QuoteDataLocationObject.FormData ? formData : calculationData;
            var value = source.JObject.SelectToken(location.Path);
            return value?.ToString();
        }

        /// <summary>
        /// Method for retrieving data from json object.
        /// </summary>
        /// <param name="dataLocations">The list of data locations.</param>
        /// <param name="datumLocation">The datum location.</param>
        /// <returns>The data locations.</returns>
        protected List<DataLocation> GetDataLocations(List<DataLocation> dataLocations, QuoteDatumLocation datumLocation)
        {
            var result = new List<DataLocation>();
            if (dataLocations != null &&
                dataLocations.Any())
            {
                result.AddRange(dataLocations);
            }

            // get the value using quote data locator; this is for backward compatibility.
            if (datumLocation != null)
            {
                var source = datumLocation.Object == QuoteDataLocationObject.FormData ? DataSource.FormData : DataSource.Calculation;
                var location = new DataLocation(source, datumLocation.Path);

                if (location != null)
                {
                    result.Add(location);
                }
            }

            return result;
        }
    }
}
