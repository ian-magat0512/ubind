// <copyright file="FormDataBooleanFieldFormatter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataFieldFormatters
{
    using System.Globalization;

    /// <inheritdoc/>
    public class FormDataBooleanFieldFormatter : IFormDataFieldFormatter
    {
        /// <inheritdoc/>
        public string Format(string value, IQuestionMetaData metaData)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value?.Trim();
            }

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            value = value.ToLower().Trim();
            return (value.Equals("yes") || value.Equals("no"))
                ? textInfo.ToTitleCase(value)
                : value.Equals("true") ? "Yes" : "No";
        }
    }
}
