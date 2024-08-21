// <copyright file="BaseFieldRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Json;

    /// <summary>
    /// Base class for all data retriever.
    /// </summary>
    public abstract class BaseFieldRetriever
    {
        /// <summary>
        /// Method for retrieving data.
        /// </summary>
        /// <param name="config">The product configuration.</param>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationData">The calculation data.</param>
        /// <returns>The quote data value.</returns>
        public abstract object? Retrieve(IDataLocatorConfig config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData);

        /// <summary>
        /// Method for retrieving data from json object.
        /// </summary>
        /// <param name="dataLocations">The list of data locations.</param>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationData">The calculation data.</param>
        /// <returns>The data value.</returns>
        protected string? GetDataValue(List<DataLocation>? dataLocations, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            if (dataLocations == null)
            {
                return null;
            }

            foreach (var location in dataLocations)
            {
                var source = this.GetSourceObject(location.Source, formData, calculationData);
                var value = source?.SelectToken(location.Path);

                if (value == null || value.Type == JTokenType.Null)
                {
                    continue;
                }

                return value.ToString();
            }

            return null;
        }

        /// <summary>
        /// Method for retrieving data from json object.
        /// </summary>
        /// <param name="dataLocations">The list of data locations.</param>
        /// <param name="datumLocation">The datum location.</param>
        /// <returns>The data locations.</returns>
        protected List<DataLocation> GetDataLocations(List<DataLocation>? dataLocations, QuoteDatumLocation? datumLocation)
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

        /// <summary>
        /// Method for retrieving data from json object.
        /// </summary>
        /// <param name="type">The object type.</param>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationData">The calculation data.</param>
        /// <returns>The source data.</returns>
        private JObject? GetSourceObject(DataSource type, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            if (type == DataSource.FormData)
            {
                var source = formData.JObject.SelectToken("formModel");
                if (source != null)
                {
                    return source as JObject;
                }

                return formData.JObject;
            }

            return calculationData?.JObject;
        }
    }
}
