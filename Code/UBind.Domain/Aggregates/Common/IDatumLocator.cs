// <copyright file="IDatumLocator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Common
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Specifies the location of an item of data in form data or calculation result json.
    /// </summary>
    public interface IDatumLocator
    {
        /// <summary>
        /// Gets a value indicating which object the datum is located in (form data or calculation result).
        /// </summary>
        DatumLocationObject Object { get; }

        /// <summary>
        /// Gets the JSON path to the datum in the json object indicated by <see cref="Object"/>.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Extract the data.
        /// </summary>
        /// <typeparam name="TDatum">The type of the datum.</typeparam>
        /// <param name="formData">The form data object.</param>
        /// <param name="calculationResult">The calculation result object.</param>
        /// <returns>The datum value, if found, otherwise default.</returns>
        TDatum Invoke<TDatum>(JObject formData, JObject calculationResult);

        /// <summary>
        /// Update the form data with a new value for this datum, if it is in the form data.
        /// </summary>
        /// <param name="formData">The form data to update.</param>
        /// <param name="newValue">The new value.</param>
        void UpdateFormModel(JObject formData, JValue newValue);
    }
}
