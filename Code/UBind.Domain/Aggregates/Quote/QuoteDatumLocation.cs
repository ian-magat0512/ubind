// <copyright file="QuoteDatumLocation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using Newtonsoft.Json;
    using UBind.Domain.Json;

    /// <summary>
    /// A model for the location of some quote data.
    /// </summary>
    public class QuoteDatumLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDatumLocation"/> class.
        /// </summary>
        /// <param name="object">The object within the quote data which contains the property (FormData or CalculationResult).</param>
        /// <param name="path">The path within the object the data is expected to exist.</param>
        public QuoteDatumLocation(QuoteDataLocationObject @object, string path)
        {
            this.Object = @object;
            this.Path = path;
        }

        /// <summary>
        /// Gets the object within the quote data which contains the property (FormData or CalculationResult).
        /// </summary>
        [JsonProperty]
        public QuoteDataLocationObject Object { get; private set; }

        /// <summary>
        /// Gets the path within the object the data is expected to exist.
        /// </summary>
        [JsonProperty]
        public string Path { get; private set; }

        /// <summary>
        /// Creates a QuoteDatumLocator to resolve the value at this location.
        /// </summary>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationResult">The calculation result.</param>
        /// <returns>A new QuoteDatumLocator instance.</returns>
        public QuoteDatumLocator ToLocator(CachingJObjectWrapper formData, CachingJObjectWrapper calculationResult)
        {
            return new QuoteDatumLocator(this, formData, calculationResult);
        }
    }
}
