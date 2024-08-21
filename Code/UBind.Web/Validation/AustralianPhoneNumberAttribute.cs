// <copyright file="AustralianPhoneNumberAttribute.cs" company="uBind">
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
    /// A Validation attribute for validing a valid australian phone number.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AustralianPhoneNumberAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AustralianPhoneNumberAttribute"/> class.
        /// </summary>
        public AustralianPhoneNumberAttribute()
        {
        }

        /// <summary>
        /// Validate if string is a valid australian phone number.
        /// </summary>
        /// <param name="value">the value.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object value)
        {
            if (this.ErrorMessageString.Contains("invalid"))
            {
                this.ErrorMessage = "{0} must be in australian format.";
            }

            if (value == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(value.ToString()))
            {
                return true;
            }

            try
            {
                Regex regex = new Regex(ValidationExpressions.AustralianPhoneNumber);
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
