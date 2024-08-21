// <copyright file="QuoteCustomerAssociationDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using global::DotLiquid;

    /// <summary>
    /// A drop model for quote association invitation.
    /// </summary>
    public class QuoteCustomerAssociationDrop : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCustomerAssociationDrop"/> class.
        /// </summary>
        /// <param name="link">The quote association link request.</param>
        /// <param name="invitationId">The quote association Id.</param>
        public QuoteCustomerAssociationDrop(string link, string invitationId)
        {
            this.Link = link;
            this.InvitationId = invitationId;
        }

        /// <summary>
        /// Gets or sets quote association link.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the asssociation Id.
        /// </summary>
        public string InvitationId { get; set; }

        /// <summary>
        /// Removes quote association email link.
        /// </summary>
        /// <returns>Instance of QuoteAssociationDrop.</returns>
        public static QuoteCustomerAssociationDrop CreateMaskedDrop()
        {
            string link = "(Quote association link removed for security reasons)";
            return new QuoteCustomerAssociationDrop(link, default);
        }
    }
}
