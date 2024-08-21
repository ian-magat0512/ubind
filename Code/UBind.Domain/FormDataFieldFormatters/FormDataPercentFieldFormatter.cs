// <copyright file="FormDataPercentFieldFormatter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataFieldFormatters
{
    using System;
    using System.Text.RegularExpressions;

    /// <inheritdoc/>
    public class FormDataPercentFieldFormatter : IFormDataFieldFormatter
    {
        /// <inheritdoc/>
        public string Format(string value, IQuestionMetaData metaData)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value?.Trim();
            }

            value = value.Trim().Replace(",", string.Empty).Replace("%", string.Empty);

            // NIL is recognized as meaning 0%
            if (value.Equals("NIL", StringComparison.InvariantCultureIgnoreCase))
            {
                return "0%";
            }

            if (!Regex.IsMatch(value, "^-?(\\d*\\.)?\\d+$"))
            {
                return value;
            }

            bool isDecimal = value.Contains('.');
            bool isLastNumberZero = isDecimal && value.LastIndexOf("0") == value.Length - 1;
            value = isDecimal
                ? decimal.Parse(value).ToString("#,0.##############")
                : long.Parse(value).ToString("n0");

            value = isLastNumberZero ? value + "0" : value;
            return value.Contains('%') ? value : string.Concat(value, "%");
        }
    }
}
