// <copyright file="EmailListValidationAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Net.Mail;

    /// <summary>
    /// A Validation attribute for validating valid list of email separated by semicolon.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class EmailListValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailListValidationAttribute"/> class.
        /// </summary>
        public EmailListValidationAttribute()
        {
        }

        /// <summary>
        /// Validate if string is a Valid list of email separated by semicolon.
        /// </summary>
        /// <param name="emailList">The List of email separated by semicolon.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object emailList)
        {
            if (emailList == null)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(emailList.ToString()))
            {
                return true;
            }

            var list = new MailAddressCollection();
            try
            {
                list.Add(emailList.ToString());
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
