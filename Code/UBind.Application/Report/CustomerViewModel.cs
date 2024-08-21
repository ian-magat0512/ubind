// <copyright file="CustomerViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;

    /// <summary>
    /// Customer view model providing data for use in liquid report templates.
    /// </summary>
    public class CustomerViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerViewModel"/> class.
        /// </summary>
        /// <param name="preferredName">The preferred name of the customer.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="primaryEmail">The primary email.</param>
        /// <param name="alternativeEmail">The alternative email.</param>
        /// <param name="mobilePhone">The mobile phone.</param>
        /// <param name="homePhone">The home phone.</param>
        /// <param name="workPhone">The work phone.</param>
        public CustomerViewModel(string preferredName, string fullName, string primaryEmail, string alternativeEmail, string mobilePhone, string homePhone, string workPhone)
        {
            this.PreferredName = preferredName;
            this.FullName = fullName;
            this.PrimaryEmail = primaryEmail;
            this.AlternativeEmail = alternativeEmail;
            this.MobilePhone = mobilePhone;
            this.HomePhone = homePhone;
            this.WorkPhone = workPhone;
        }

        /// <summary>
        /// Gets the preferred name.
        /// </summary>
        public string PreferredName { get; }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the primary email.
        /// </summary>
        public string PrimaryEmail { get; }

        /// <summary>
        /// Gets the secondary email.
        /// </summary>
        public string AlternativeEmail { get; }

        /// <summary>
        /// Gets the mobile phone.
        /// </summary>
        public string MobilePhone { get; }

        /// <summary>
        /// Gets the home phone.
        /// </summary>
        public string HomePhone { get; }

        /// <summary>
        /// Gets the work phone.
        /// </summary>
        public string WorkPhone { get; }
    }
}
