// <copyright file="PortNumberValidationAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A Validation attribute for validating valid port number.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PortNumberValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortNumberValidationAttribute"/> class.
        /// </summary>
        public PortNumberValidationAttribute()
        {
        }

        /// <summary>
        /// Validate if string is a Valid port number.
        /// </summary>
        /// <param name="portNumberObject">Port number.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object portNumberObject)
        {
            if (portNumberObject == null)
            {
                return true;
            }

            if (int.TryParse(portNumberObject.ToString(), out int portNumber))
            {
                if (portNumber >= 1 && portNumber <= 65535)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
