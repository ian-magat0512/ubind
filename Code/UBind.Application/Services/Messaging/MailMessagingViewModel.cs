// <copyright file="MailMessagingViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Messaging
{
    /// <summary>
    /// View model for mail messaging service.
    /// </summary>
    public class MailMessagingViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailMessagingViewModel"/> class.
        /// </summary>
        /// <param name="tenantName">The name of the tenant.</param>
        /// <param name="organisationName">The name of the organisation.</param>
        /// <param name="preferredName">The first name of the person.</param>
        /// <param name="invitationLink">The invitation link for verification.</param>
        public MailMessagingViewModel(
            string tenantName, string organisationName, string preferredName, string invitationLink)
        {
            this.TenantName = tenantName;
            this.OrganisationName = organisationName;
            this.PreferredName = preferredName;
            this.InvitationLink = invitationLink;
        }

        /// <summary>
        /// Gets the product ID.
        /// </summary>
        public string TenantName { get; }

        /// <summary>
        /// Gets the name of the organisation.
        /// </summary>
        public string OrganisationName { get; }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        public string PreferredName { get; }

        /// <summary>
        /// Gets the invitation link.
        /// </summary>
        public string InvitationLink { get; }
    }
}
