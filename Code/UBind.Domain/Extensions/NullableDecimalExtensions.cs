// <copyright file="NullableDecimalExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    /// <summary>
    /// Extension methods for nullable decimals.
    /// </summary>
    public static class NullableDecimalExtensions
    {
        /// <summary>
        /// Formats a nullable decimal as a currency value with two decimal places, treating null as zero.
        /// </summary>
        /// <param name="value">The nullable decimal to format as a currency value.</param>
        /// <returns>An string containing the value as a currency value.</returns>
        public static string ToDollarsAndCents(this decimal? value)
        {
            return (value ?? 0).ToDollarsAndCents();
        }
    }
}
