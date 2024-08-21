// <copyright file="QuoteFormCustomerUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Form data for an application POSTed from client.
    /// </summary>
    public class QuoteFormCustomerUpdateModel : IQuoteResourceModel
    {
        /// <summary>
        /// Gets or sets the ID of the application the data belongs to.
        /// </summary>
        [Required(ErrorMessage = "Quote identifier is required")]
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the customer details.
        /// </summary>
        [Required(ErrorMessage = "Customer Details field is required")]
        public CustomerPersonalDetailsModel CustomerDetails { get; set; }

        /// <summary>
        /// Gets or sets the portal ID which the customer would log into once their account is created.
        /// </summary>
        public Guid? PortalId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the customer associated with this quote, if it exists.
        /// </summary>
        public Guid? CustomerId { get; set; }

        public Guid? ProductReleaseId { get; set; }
    }
}
