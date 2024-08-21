// <copyright file="StrongPasswordAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A Validation attribute for strong passwords.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class StrongPasswordAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StrongPasswordAttribute"/> class.
        /// </summary>
        public StrongPasswordAttribute()
        {
        }

        /// <summary>
        /// Validate if string is a valid strong password.
        /// </summary>
        /// <param name="value">the value.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object value)
        {
            if (this.ErrorMessageString.Contains("invalid"))
            {
                this.ErrorMessage = "{0} must be at least 12 characters long and must contain at least one uppercase letter, one lowercase letter, one number and one special character (a character that is neither a letter nor a number).";
            }

            // its optional
            if (value == null)
            {
                return true;
            }

            // its optional
            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true;
            }

            try
            {
                Regex regex = new Regex(ValidationExpressions.StrongPassword);
                Match match = regex.Match(value.ToString());

                return match.Success;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
