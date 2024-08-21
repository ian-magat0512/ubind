// <copyright file="OptionalEmailAddressAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using MimeKit;

    /// <summary>
    /// A Validation attribute for validating valid list of email separated by semicolon.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptionalEmailAddressAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalEmailAddressAttribute"/> class.
        /// </summary>
        public OptionalEmailAddressAttribute()
        {
        }

        /// <summary>
        /// Validate if string is a Valid list of email separated by semicolon.
        /// </summary>
        /// <param name="value">The List of email separated by semicolon.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object value)
        {
            if (this.ErrorMessageString.Contains("invalid"))
            {
                this.ErrorMessage = "{0} must be in email format.";
            }

            if (value == null)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true;
            }

            try
            {
                MailboxAddress.Parse(value.ToString());
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
