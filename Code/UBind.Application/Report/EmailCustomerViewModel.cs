// <copyright file="EmailCustomerViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;
    using UBind.Domain.ReadModel.Email;

    /// <summary>
    /// Email customer view model for liquid template.
    /// </summary>
    public class EmailCustomerViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailCustomerViewModel"/> class.
        /// </summary>
        /// <param name="customer">The customer data. </param>
        public EmailCustomerViewModel(CustomerData customer)
        {
            this.PreferredName = customer.PreferredName;
            this.FullName = customer.FullName;
            this.Email = customer.Email;
            this.AlternativeEmail = customer.AlternativeEmail;
            this.MobilePhoneNumber = customer.MobilePhoneNumber;
            this.WorkPhoneNumber = customer.WorkPhoneNumber;
            this.HomePhoneNumber = customer.HomePhoneNumber;
        }

        /// <summary>
        /// Gets the preffered name of the customer.
        /// </summary>
        public string PreferredName { get; }

        /// <summary>
        /// Gets the fullname of the customer.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the email of the customer.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets the alternative email of the customer.
        /// </summary>
        public string AlternativeEmail { get; }

        /// <summary>
        /// Gets the mobile phone numberof the customer.
        /// </summary>
        public string MobilePhoneNumber { get; }

        /// <summary>
        /// Gets the work phone number of the customer.
        /// </summary>
        public string WorkPhoneNumber { get; }

        /// <summary>
        /// Gets the home phone number of the customer.
        /// </summary>
        public string HomePhoneNumber { get; }
    }
}
