// <copyright file="EmailAddressHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using System.Collections.Generic;
    using MimeKit;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Helper for Validating Email Address.
    /// </summary>
    public static class EmailAddressHelper
    {
        /// <summary>
        /// Throws an exception if the email address is not valid.
        /// </summary>
        /// <param name="emailAddress">The email address to validate.</param>
        /// <param name="action">The action being performed.</param>
        public static void ThrowIfEmailAddressIsNotValid(string emailAddress, string action)
        {
            try
            {
                InternetAddress.Parse(emailAddress);
            }
            catch (ParseException)
            {
                throw new ErrorException(Errors.General.InvalidEmailAddress(emailAddress, action));
            }
        }

        /// <summary>
        /// Throws an exception if one of email address is not valid.
        /// </summary>
        /// <param name="emailAddresses">The email address to validate.</param>
        /// <param name="action">The action being performed.</param>
        public static void ThrowIfEmailAddressIsNotValid(List<string> emailAddresses, string action)
        {
            emailAddresses.ForEach((string emailAddress) =>
            {
                ThrowIfEmailAddressIsNotValid(emailAddress, action);
            });
        }
    }
}
