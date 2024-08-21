// <copyright file="NullableDoubleExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;

    /// <summary>
    /// Extension methods for nullable doubles.
    /// </summary>
    public static class NullableDoubleExtensions
    {
        /// <summary>
        /// Converts a nullable double to the equivalent decimal amount or zero if null.
        /// </summary>
        /// <param name="value">The nullable double to convert.</param>
        /// <returns>The value as a decimal, or zero if null.</returns>
        /// <exception cref="System.OverflowException">Thrown in double value is too large or small to represent as a decimal.</exception>
        public static decimal ToDecimalOrZero(this double? value)
        {
            return value.HasValue
                ? Convert.ToDecimal(value.Value)
                : 0m;
        }
    }
}
