// <copyright file="FormDataNumberFieldFormatter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataFieldFormatters
{
    /// <inheritdoc/>
    public class FormDataNumberFieldFormatter : IFormDataFieldFormatter
    {
        /// <inheritdoc/>
        public string Format(string value, IQuestionMetaData metaData)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value?.Trim();
            }

            bool isDecimal = value.Contains('.');
            value = value.Trim().Replace(",", string.Empty);
            if (!decimal.TryParse(value, out _))
            {
                return value;
            }

            if (isDecimal)
            {
                return decimal.Parse(value).ToString("#,0.00############");
            }

            if (long.TryParse(value, out _))
            {
                return long.Parse(value).ToString("n0");
            }

            return value;
        }
    }
}
