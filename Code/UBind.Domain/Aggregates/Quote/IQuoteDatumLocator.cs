// <copyright file="IQuoteDatumLocator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using UBind.Domain.Json;

    /// <summary>
    /// Specifies the location of an item of quote data in form data or calculation result json.
    /// </summary>
    public interface IQuoteDatumLocator
    {
        /// <summary>
        /// Gets a value indicating which object the datum is located in (form data or calculation result).
        /// </summary>
        QuoteDataLocationObject Object { get; }

        /// <summary>
        /// Gets the JSON path to the datum in the json object indicated by <see cref="Object"/>.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Extract the quote data.
        /// </summary>
        /// <typeparam name="TDatum">The type of the datum.</typeparam>
        /// <param name="formData">The form data object.</param>
        /// <param name="calculationResultData">The calculation result data.</param>
        /// <returns>The datum value, if found, otherwise default.</returns>
        TDatum Invoke<TDatum>(CachingJObjectWrapper formData, CachingJObjectWrapper calculationResultData);
    }
}
