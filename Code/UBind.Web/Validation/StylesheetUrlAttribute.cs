// <copyright file="StylesheetUrlAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A Validation attribute for validating a valid stylesheet pattern.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class StylesheetUrlAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StylesheetUrlAttribute"/> class.
        /// </summary>
        public StylesheetUrlAttribute()
        {
        }

        /// <summary>
        /// Validate if string is a valid stylesheet.
        /// </summary>
        /// <param name="value">the value.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object value)
        {
            if (this.ErrorMessageString.Contains("invalid"))
            {
                this.ErrorMessage = "{0} must contain a valid Stylesheet Url.";
            }

            // its optional
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return true;
            }

            try
            {
                var absoluteUrl = value?.ToString().ToLower();
                if (absoluteUrl.StartsWith("/assets"))
                {
                    absoluteUrl = $"https://app.ubind.com.au{absoluteUrl}";
                }

                var uri = new Uri(absoluteUrl);

                // URI can be ftp, http,  host, etc.
                // We will only check if it is well formed http/https.
                var isHttp = uri.Scheme == "http" || uri.Scheme == "https";
                return uri.IsWellFormedOriginalString() && isHttp;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
