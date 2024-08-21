// <copyright file="IUserActivationInvitationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Represents the user activation invitation service.
    /// </summary>
    public interface IUserActivationInvitationService
    {
        /// <summary>
        /// Set the user password in response to an activation invitation.
        /// </summary>
        /// <param name="tenantId">The tenant Id for change password.</param>
        /// <param name="userId">The user Id for change password.</param>
        /// <param name="activationInvitationId">The activation invitation Id.</param>
        /// <param name="password">The new password.</param>
        /// <returns>A task for completion checking.</returns>
        Task SetPasswordFromActivation(
            Guid tenantId,
            Guid userId,
            Guid activationInvitationId,
            string password);

        /// <summary>
        /// Method that sends an activation to a user with a given ID.
        /// </summary>
        /// <param name="tenant">The tenant for the user belongs to.</param>
        /// <param name="organisationId">The organisation Id for queueing activation email.</param>
        /// <param name="userId">The user Id for queueing activation email.</param>
        /// <param name="performingUserId">The ID of the user who did the action.</param>
        /// <param name="portalId">The portalId (alias).</param>
        /// <remarks>
        /// TODO:
        ///     There is a future ticket that includes the organisation Id for properly handling activation emails.
        ///     By that time, you can get rid of the tenant Id and just use the organisation Id.
        /// Reason for not implementing it in this ticket: Out of scope.
        /// </remarks>
        /// <returns>The invitation ID.</returns>
        Task<Guid> CreateActivationInvitationAndSendEmail(
            Domain.Tenant tenant,
            Guid organisationId,
            Guid userId,
            DeploymentEnvironment environment,
            Guid? portalId = null);

        /// <summary>
        /// Method that sends an activation email to a given user for an existing invitation.
        /// </summary>
        /// <param name="invitationId">The ID of the invitation.</param>
        /// <param name="userAggregate">The user aggregate.</param>
        /// <param name="personAggregate">The person aggregate.</param>
        /// <param name="tenant">The tenant the user belongs to.</param>
        /// <param name="organisation">The organisation read model.</param>
        /// <param name="performingUserId">The ID of the user who did the action.</param>
        /// <param name="portalId">The portalId (alias).</param>
        /// <param name="environment">The deployment environment.</param>
        /// <returns>The user data transfer object.</returns>
        Task<UserModel> QueueActivationEmail(
            Guid invitationId,
            UserAggregate userAggregate,
            PersonAggregate personAggregate,
            Domain.Tenant tenant,
            OrganisationReadModel organisation,
            DeploymentEnvironment environment,
            Guid? portalId = null);

        /// <summary>
        /// Method that sends an activation email to a given user for an existing invitation.
        /// </summary>
        /// <param name="userAggregate">The user aggregate.</param>
        /// <param name="personAggregate">The person aggregate.</param>
        /// <param name="tenant">The tenant the user belongs to.</param>
        /// <param name="organisation">The organisation read model.</param>
        /// <param name="portalId">The portalId (alias).</param>
        /// <returns>The user data transfer object.</returns>
        Task<UserModel> QueueAccountAlreadyActivatedEmail(
            UserAggregate userAggregate,
            PersonAggregate personAggregate,
            Domain.Tenant tenant,
            OrganisationReadModel organisation,
            DeploymentEnvironment environment,
            Guid? portalId = null);

        /// <summary>
        /// Creates an activation invitation for the specified user and queues an email notification.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant associated with the user account.</param>
        /// <param name="userId">The ID of the user to create the activation invitation for.</param>
        /// <param name="environment">The environment to use when generating the link to the portal.</param>
        /// <param name="portalId">An optional ID of the portal related to the account activation.</param>
        Task CreateActivationInvitationAndQueueEmail(
            Guid tenantId, Guid userId, DeploymentEnvironment environment, Guid? portalId = null);

        /// <summary>
        /// Sends an email to inform a user that their account has already been activated.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant associated with the user account.</param>
        /// <param name="userId">The ID of the user whose account is already activated.</param>
        /// <param name="environment">The environment to use when generating the link to the portal.</param>
        /// <param name="portalId">An optional ID of the portal related to the account activation.</param>
        Task SendAccountAlreadyActivatedEmail(
            Guid tenantId, Guid userId, DeploymentEnvironment environment, Guid? portalId = null);

        /// <summary>
        /// Checks the user activation status from invitation Id. If there are any problems, an exception is thrown.
        /// </summary>
        /// <param name="tenantId">The tenant Id for queueing activation email.</param>
        /// <param name="userId">The user Id to check for activation.</param>
        /// <param name="invitationId">The invitation Id for activation.</param>
        void CheckUserActivationInvitationStatus(
            Guid tenantId,
            Guid userId,
            Guid invitationId);
    }
}
