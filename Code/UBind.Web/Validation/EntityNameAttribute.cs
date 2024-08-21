// <copyright file="EntityNameAttribute.cs" company="uBind">
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
    /// A Validation attribute for validating a valid name for a person.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class EntityNameAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNameAttribute"/> class.
        /// </summary>
        public EntityNameAttribute()
        {
        }

        /// <summary>
        /// Validate if string is a valid entity name.
        /// </summary>
        /// <param name="value">the value.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object value)
        {
            if (this.ErrorMessageString.Contains("invalid"))
            {
                this.ErrorMessage = "{0} must start with a letter or digit, and may only contain letters, digits, spaces, hyphens, apostrophes, commas and period characters.";
            }

            // its optional
            if (value == null)
            {
                return true;
            }

            // its optional
            if (string.IsNullOrEmpty(value.ToString()))
            {
                return true;
            }

            try
            {
                Regex regex = new Regex(ValidationExpressions.EntityName);
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
