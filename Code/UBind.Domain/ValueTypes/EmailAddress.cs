// <copyright file="EmailAddress.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// For representing addresses.
    /// </summary>
    public class EmailAddress : ValueObject
    {
        private readonly MailAddress emailAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddress"/> class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        public EmailAddress(string emailAddress)
        {
            try
            {
                this.emailAddress = new MailAddress(emailAddress);
            }
            catch (Exception)
            {
                throw new ErrorException(Errors.Email.AddressInvalid(emailAddress));
            }
        }

        /// <summary>
        /// Method to override default ToString method.
        /// </summary>
        /// <returns>The email address.</returns>
        public override string ToString()
        {
            return this.emailAddress.Address;
        }

        /// <summary>
        /// Method for overriding the GetEqualityCompnents method.
        /// </summary>
        /// <returns>The list of equality components.</returns>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.emailAddress;
        }
    }
}
