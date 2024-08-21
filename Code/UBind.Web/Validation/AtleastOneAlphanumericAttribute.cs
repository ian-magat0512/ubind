// <copyright file="AtleastOneAlphanumericAttribute.cs" company="uBind">
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
    /// A Validation attribute to check atleast one alphanumeric character.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AtleastOneAlphanumericAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtleastOneAlphanumericAttribute"/> class.
        /// </summary>
        public AtleastOneAlphanumericAttribute()
        {
        }

        /// <summary>
        /// Validate if string if it only has one alphanumeric regex.
        /// </summary>
        /// <param name="value">the value.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object value)
        {
            if (this.ErrorMessageString.Contains("invalid"))
            {
                this.ErrorMessage = "{0} must contain at least one alphanumeric character.";
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
                Regex regex = new Regex(@".*[a-zA-Z0-9].*");
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
