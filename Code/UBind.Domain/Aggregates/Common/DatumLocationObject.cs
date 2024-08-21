// <copyright file="DatumLocationObject.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Common
{
    /// <summary>
    /// Enumerates the objects where data can be found.
    /// </summary>
    public enum DatumLocationObject
    {
        /// <summary>
        /// Specifies that an item of data can be found in the form data.
        /// </summary>
        FormData = 0,

        /// <summary>
        /// Specifies that an item of data can be found in the calculation result.
        /// </summary>
        CalculationResult,
    }
}
