// <copyright file="CustomerUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Form data for an application POSTed from client.
    /// </summary>
    public class CustomerUpdateModel : IQuoteResourceModel
    {
        /// <summary>
        /// Gets or sets the ID of the application the data belongs to.
        /// </summary>
        [Required]
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the customer details.
        /// </summary>
        public CustomerPersonalDetailsModel CustomerDetails { get; set; }

        /// <summary>
        /// Gets or sets the portal ID associated with this quote, if any.
        /// </summary>
        public Guid? PortalId { get; set; }

        public Guid? ProductReleaseId { get; set; }
    }
}
