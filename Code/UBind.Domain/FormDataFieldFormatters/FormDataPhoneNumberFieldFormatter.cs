// <copyright file="FormDataPhoneNumberFieldFormatter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.FormDataFieldFormatters
{
    using System;
    using System.Text.RegularExpressions;

    /// <inheritdoc/>
    public class FormDataPhoneNumberFieldFormatter : IFormDataFieldFormatter
    {
        /// <inheritdoc/>
        public string Format(string value, IQuestionMetaData metaData)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value?.Trim();
            }

            value = value.Replace("(", string.Empty)
                         .Replace(")", string.Empty)
                         .Replace("-", string.Empty)
                         .Replace(" ", string.Empty)
                         .Trim();
            return this.GetFormattedPhoneNumber(value);
        }

        private string GetFormattedPhoneNumber(string value)
        {
            var phoneNumberFormatPattern = string.Empty;

            if (value.Contains("+613"))
            {
                phoneNumberFormatPattern = "+## (#) ####-####";
            }

            if (value.Contains("+614"))
            {
                phoneNumberFormatPattern = "+## ### ### ###";
            }

            if (value.Length >= 2 && value.Substring(0, 1) == "0" && value.Substring(1, 1) == "4")
            {
                phoneNumberFormatPattern = "0### ### ###";
            }

            if (value.Length >= 2 && value.Substring(0, 1) == "0" && value.Substring(1, 1) != "4")
            {
                phoneNumberFormatPattern = "(0#) ####-####";
            }

            if (value.Length >= 4 && (value.Substring(0, 4) == "1300" || value.Substring(0, 4) == "1800"))
            {
                phoneNumberFormatPattern = "#### ### ###";
            }

            if (value.Length == 6 && value.Substring(0, 2) == "13")
            {
                phoneNumberFormatPattern = "## ## ##";
            }

            return this.FormatPhoneNumber(value, phoneNumberFormatPattern);
        }

        private string FormatPhoneNumber(string phoneNum, string phoneFormat)
        {
            if (string.IsNullOrEmpty(phoneFormat))
            {
                phoneFormat = "(###) ###-####";
            }

            Regex regexObj = new Regex(@"[^\d]");
            phoneNum = regexObj.Replace(phoneNum, string.Empty);

            if (phoneNum.Length > 0)
            {
                phoneNum = Convert.ToInt64(phoneNum).ToString(phoneFormat);
            }

            return phoneNum;
        }
    }
}
