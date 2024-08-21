// <copyright file="IQuoteDataLocator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using UBind.Domain.Json;

    /// <summary>
    /// Helper for extracting quote data from form data and calculation result JSON.
    /// </summary>
    /// <typeparam name="TData">The type of data to be returned.</typeparam>
    public interface IQuoteDataLocator<TData>
    {
        /// <summary>
        /// Extract known quote data from form data and calculation result JSON.
        /// </summary>
        /// <param name="formData">The form data.</param>
        /// <param name="calculationResultData">The calculation result data.</param>
        /// <returns>Quote data.</returns>
        TData Invoke(CachingJObjectWrapper formData, CachingJObjectWrapper calculationResultData);
    }
}
