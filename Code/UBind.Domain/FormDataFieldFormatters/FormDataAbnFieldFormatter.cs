// <copyright file="FormDataAbnFieldFormatter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.FormDataFieldFormatters
{
    using System.Text.RegularExpressions;

    /// <inheritdoc/>
    public class FormDataAbnFieldFormatter : IFormDataFieldFormatter
    {
        /// <inheritdoc/>
        public string? Format(string value, IQuestionMetaData metaData)
        {
            if (value == null)
            {
                return value;
            }
            value = Regex.Replace(value, @"[^\d]?", string.Empty);
            if (string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
            return long.Parse(value).ToString("00 000 000 000");
        }
    }
}
