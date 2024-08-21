// <copyright file="EmailInvitationDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using global::DotLiquid;

    /// <summary>
    /// A drop model for a Email Invitation.
    /// </summary>
    public class EmailInvitationDrop : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailInvitationDrop"/> class.
        /// </summary>
        /// <param name="link">The password reset link.</param>
        /// <param name="invitationId">The password reset invitationId.</param>
        public EmailInvitationDrop(string link, string invitationId)
        {
            this.Link = link;
            this.InvitationId = invitationId;
        }

        /// <summary>
        /// Gets or sets Password reset link.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the Password invitation Id link.
        /// </summary>
        public string InvitationId { get; set; }

        /// <summary>
        /// Remove account activation Invatation link.
        /// </summary>
        /// <returns>EmailInvitationDrop.</returns>
        public static EmailInvitationDrop CreateMaskedAccountActivationDrop()
        {
            string link = "(Account activation link removed for security reasons)";
            return new EmailInvitationDrop(link, default);
        }

        /// <summary>
        /// Remove password reset Invatation link.
        /// </summary>
        /// <returns>EmailInvitationDrop.</returns>
        public static EmailInvitationDrop CreateMaskedPasswordResetDrop()
        {
            string link = "(Password reset link removed for security reasons)";
            return new EmailInvitationDrop(link, default);
        }

        /// <summary>
        /// Remove password expired reset Invatation link.
        /// </summary>
        /// <returns>EmailInvitationDrop.</returns>
        public static EmailInvitationDrop CreateMaskedPasswordExpiredResetDrop()
        {
            string link = "(Password expired reset link removed for security reasons)";
            return new EmailInvitationDrop(link, default);
        }
    }
}
