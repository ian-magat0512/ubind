// <copyright file="SmtpServerHostValidationAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A Validation attribute for validating valid host.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class SmtpServerHostValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpServerHostValidationAttribute"/> class.
        /// </summary>
        public SmtpServerHostValidationAttribute()
        {
        }

        /// <summary>
        /// Validate if string is a Valid host.
        /// </summary>
        /// <param name="hostname">The Host Name.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object hostname)
        {
            if (hostname == null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(hostname.ToString()))
            {
                return true;
            }

            var result = Uri.CheckHostName(hostname.ToString());
            return result != UriHostNameType.Unknown;
        }
    }
}
