// <copyright file="CurrencyParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Text.RegularExpressions;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// A helper for parsing currency values.
    /// </summary>
    public class CurrencyParser
    {
        /// <summary>
        /// Parses a currency string and returns a decimal, wrapped in a Result.
        /// </summary>
        /// <param name="currencyString">The currency value as a string, potentially with a currency symbol.</param>
        /// <returns>A decimal or an Error.</returns>
        public static Result<decimal, Error> TryParseToDecimalWithResult(string currencyString)
        {
            if (currencyString == null)
            {
                return Result.Failure<decimal, Error>(Errors.Calculation.UnableToParseCurrencyString(currencyString));
            }

            currencyString = StripToNumeric(currencyString);
            decimal number;
            try
            {
                number = decimal.Parse(currencyString);
            }
            catch (FormatException)
            {
                return Result.Failure<decimal, Error>(Errors.Calculation.UnableToParseCurrencyString(currencyString));
            }

            return Result.Success<decimal, Error>(number);
        }

        /// <summary>
        /// Parses a currency string and returns a decimal, or throws an ErrorException if the parse was unsuccessful.
        /// </summary>
        /// <param name="currencyString">The currency value as a string, potentially with a currency symbol.</param>
        /// <returns>A decimal value.</returns>
        public static decimal ParseToDecimalOrThrow(string currencyString)
        {
            if (currencyString == null)
            {
                throw new ErrorException(Errors.Calculation.UnableToParseCurrencyString(currencyString));
            }

            currencyString = StripToNumeric(currencyString);
            decimal number;
            try
            {
                number = decimal.Parse(currencyString);
            }
            catch (FormatException)
            {
                throw new ErrorException(Errors.Calculation.UnableToParseCurrencyString(currencyString));
            }

            return number;
        }

        private static string StripToNumeric(string currencyString)
        {
            try
            {
                return Regex.Replace(currencyString, @"[^\d.-]?", string.Empty);
            }
            catch
            {
                return currencyString;
            }
        }
    }
}
