// <copyright file="IUserPasswordResetInvitationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Domain;

    /// <summary>
    /// Represents the user password reset invitation service.
    /// </summary>
    public interface IUserPasswordResetInvitationService
    {
        /// <summary>
        /// Gets or sets the period to use for limiting password reset requests.
        /// </summary>
        int PasswordResetTrackingPeriodInMinutes { get; set; }

        /// <summary>
        /// Gets or sets the number of password resets in a given period that will trigger blocking.
        /// </summary>
        int PasswordResetRequestBlockingThreshold { get; set; }

        /// <summary>
        /// Checks the user password reset status from invitation Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="invitationId">The invitation Id.</param>
        void CheckPasswordResetInvitationStatus(Guid tenantId, Guid userId, Guid invitationId);

        /// <summary>
        /// Method that sets a password for a user.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the user.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="invitationId">The invitation ID.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetPasswordFromPasswordReset(
            Guid tenantId, Guid userId, Guid invitationId, string password);

        /// <summary>
        /// Checks email and send reset password invitation to the user.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the user.</param>
        /// <param name="organisationId">The organisation Id of the user.</param>
        /// <param name="email">The email address of the user.</param>
        /// <param name="portalId">the portal id.</param>
        /// <param name="productId">the product id.</param>
        /// <returns>The user password reset status.</returns>
        [DisplayName("Create and send password reset invitation | {2}")]
        Task<Result<Guid, Error>> CreateAndSendPasswordResetInvitation(
            Guid tenantId,
            Guid organisationId,
            string email,
            DeploymentEnvironment environment,
            bool isPasswordExpired,
            Guid? portalId = null,
            Guid? productId = null);
    }
}
