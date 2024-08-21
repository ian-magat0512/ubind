// <copyright file="FormDataCurrencyFieldFormatter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataFieldFormatters
{
    using System;
    using System.Text.RegularExpressions;
    using NodaMoney;
    using UBind.Domain.Helpers;

    /// <inheritdoc/>
    public class FormDataCurrencyFieldFormatter : IFormDataFieldFormatter
    {
        /// <inheritdoc/>
        public string Format(string value, IQuestionMetaData metaData)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value?.Trim();
            }

            var result = CurrencyParser.TryParseToDecimalWithResult(value);
            if (result.IsFailure)
            {
                return value?.Trim();
            }

            var amount = new Money(result.Value, metaData.CurrencyCode, MidpointRounding.AwayFromZero).ToString();

            // Requirement: $1000.00 => $1000
            // Since the default Culture Settings for Australia does not support this
            // We replace the decimal values via regex if the minor units are zeroes
            var hasFractionalPart = result.Value - Math.Round(result.Value) != 0;

            return hasFractionalPart ? amount : Regex.Replace(amount, @"$[^OF0-9-,.]|\.[0-9]+", string.Empty);
        }
    }
}
