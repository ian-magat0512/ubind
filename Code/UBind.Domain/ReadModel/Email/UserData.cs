// <copyright file="UserData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Email
{
    using System;

    /// <summary>
    /// User of the IEmailSummary model.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user type.
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the preffered name of the user.
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the email of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the alternative email of the user.
        /// </summary>
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the mobile phone number of the user.
        /// </summary>
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the home phone number of the user.
        /// </summary>
        public string HomePhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the work phone number of the user.
        /// </summary>
        public string WorkPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the organisation id of the record.
        /// </summary>
        public Guid? OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the customer id of the record.
        /// </summary>
        public Guid? CustomerId { get; set; }
    }
}
