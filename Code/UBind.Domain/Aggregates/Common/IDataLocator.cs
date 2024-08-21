// <copyright file="IDataLocator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Common
{
    /// <summary>
    /// Helper for extracting data from form data and calculation result JSON.
    /// </summary>
    /// <typeparam name="TData">The generic data.</typeparam>
    public interface IDataLocator<TData>
    {
        /// <summary>
        /// Extract known quote data from form data and calculation result JSON.
        /// </summary>
        /// <param name="formDataJson">The form data.</param>
        /// <param name="calculationJson">The calculation result.</param>
        /// <returns>The data source.</returns>
        TData Invoke(string formDataJson, string calculationJson);
    }
}
