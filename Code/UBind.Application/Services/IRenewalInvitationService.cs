// <copyright file="IRenewalInvitationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// Service that allows application events to be raised.
    /// </summary>
    public interface IRenewalInvitationService
    {
        /// <summary>
        /// Send Policy Renewal Invitation.
        /// </summary>
        /// <param name="tenantId">The Tenant Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="policyId">The Quote ID.</param>
        /// <param name="userId">The user tenantId.</param>
        /// <param name="isMutual">The indicator if quote is mutual.</param>
        /// <param name="isUserAccountRequired">A flag indicating whether to create a user account if there is no account present yet.</param>
        /// <param name="parentUrl">The parent Url.</param>
        /// <returns>The Policy renewal request status.</returns>
        Task SendPolicyRenewalInvitation(
            Guid tenantId,
            DeploymentEnvironment environment,
            Guid policyId,
            Guid userId,
            bool isMutual,
            bool isUserAccountRequired,
            string parentUrl = "");
    }
}
