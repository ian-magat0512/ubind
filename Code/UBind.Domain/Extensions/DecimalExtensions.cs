// <copyright file="DecimalExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Extension methods for decimals.
    /// </summary>
    public static class DecimalExtensions
    {
        /// <summary>
        /// Formats a decimal as a currency value with two decimal places.
        /// </summary>
        /// <param name="value">The decimal to format as a currency value.</param>
        /// <returns>An string containing the currency value.</returns>
        public static string ToDollarsAndCents(this decimal value)
        {
            return value.ToString("C2", CultureInfo.GetCultureInfo("en-AU"));
        }

        /// <summary>
        /// Rounds a decimal representing a dollar amount to a whole number of cents.
        /// Usually defined as the standard rounding method.
        /// </summary>
        /// <param name="value">The amount to round.</param>
        /// <returns>The amount rounded to two decimal places using midpoint rounding "away from zero".</returns>
        public static decimal RoundTo2DecimalPlacesAwayFromZero(this decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Rounds a decimal representing a dollar amount to a whole number of cents.
        /// </summary>
        /// <param name="value">The amount to round.</param>
        /// <returns>The amount rounded to two decimal places using midpoint rounding "to even".</returns>
        public static decimal RoundToWholeCents(this decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.ToEven);
        }

        /// <summary>
        /// Rounds a decimal value to a specific number of decimal places, ensuring that the result is rounded down.
        /// </summary>
        /// <param name="value">The amount to round.</param>
        /// <param name="decimalPlaces">The numeber if places to included.</param>
        /// <returns>The rounded down decimal value specific to the number of decimal places.</returns>
        public static decimal FloorToDecimalPlace(this decimal value, int decimalPlaces = 2)
        {
            decimal scale = (decimal)Math.Pow(10, decimalPlaces);
            return Math.Floor(value * scale) / scale;
        }
    }
}
