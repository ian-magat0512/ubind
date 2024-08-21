// <copyright file="PolicyRenewalDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using global::DotLiquid;

    /// <summary>
    /// A drop model for Policy renewal.
    /// </summary>
    public class PolicyRenewalDrop : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyRenewalDrop"/> class.
        /// </summary>
        /// <param name="link">The policy renewal invitation link.</param>
        /// <param name="portalLoginLink">The portal login link.</param>
        /// <param name="portalActivationLink">The portal activation link.</param>
        /// <param name="productAlias">The product alias for which the policy is for.</param>
        public PolicyRenewalDrop(
            string link,
            string portalLoginLink,
            string portalActivationLink,
            string productAlias,
            Guid? quoteId)
        {
            this.Link = link;
            this.PortalLoginLink = portalLoginLink;
            this.PortalActivationLink = portalActivationLink;
            this.ProductAlias = productAlias;
            if (quoteId.HasValue)
            {
                this.QuoteId = quoteId.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the policy renewal invitation link.
        /// </summary>
        public string Link { get; set; }

        public string PortalLoginLink { get; set; }

        public string PortalActivationLink { get; set; }

        public string ProductAlias { get; set; }

        /// <summary>
        /// Gets or sets the Quote ID of an existing renewal quote, if it exists.
        /// </summary>
        public string QuoteId { get; set; }
    }
}
