// <copyright file="UserAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// IDE0060: Removed unused parameter.
// disable IDE0060 because there are unused sequence number parameter.
#pragma warning disable IDE0060

namespace UBind.Domain.Aggregates.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using J2N.Collections.Generic.Extensions;
    using NodaTime;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Common;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
        : AggregateRootEntity<UserAggregate, Guid>,
        IAdditionalPropertyValueEntityAggregate,
        IAdditionalProperties,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueInitializedEvent<UserAggregate,
                IUserEventObserver>>,
        IApplyAdditionalPropertyValueEvent<AdditionalPropertyValueUpdatedEvent<UserAggregate, IUserEventObserver>>
    {
        private readonly List<string> userTypes = new List<string>();
        private readonly IList<Invitation> replacedInvitations = new List<Invitation>();
        private readonly IList<Guid> acceptedInvitationIds = new List<Guid>();
        private readonly IList<PasswordLifespan> passwordHistory = new List<PasswordLifespan>();
        private readonly IList<Guid> roleIds = new List<Guid>();
        private Invitation? currentActivationInvitiation;
        private Invitation? currentPasswordResetInvitiation;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAggregate"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        public UserAggregate(IEnumerable<IEvent<UserAggregate, Guid>> events)
            : base(events)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAggregate"/> class.
        /// </summary>
        /// <param name="userId">A unique ID for the user.</param>
        /// <param name="person">The person the user represents.</param>
        /// <param name="performingUserId">The userId who created User.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        private UserAggregate(
            Guid tenantId,
            Guid userId,
            UserType userType,
            PersonAggregate person,
            Guid? performingUserId,
            Guid? portalId,
            Instant createdTimestamp,
            Guid[]? initialRoles)
        {
            var personData = new PersonData(person);
            var initializedEvent = new UserInitializedEvent(
                tenantId, userId, userType, personData, default, performingUserId, portalId, createdTimestamp, null, initialRoles);
            this.ApplyNewEvent(initializedEvent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAggregate"/> class for a customer user.
        /// </summary>
        /// <param name="userId">A unique ID for the user.</param>
        /// <param name="person">The person the user represents.</param>
        /// <param name="customerId">The Id of the customer the user account is for.</param>
        /// <param name="customerRole">The customer role for the user's tenant.</param>
        /// <param name="performingUserId">The current userId.</param>
        /// <param name="portalId">The ID of the portal which the user should log in to.</param>
        /// <param name="environment">The users environment.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        private UserAggregate(
            Guid tenantId,
            Guid userId,
            UserType userType,
            PersonAggregate person,
            Guid customerId,
            Role customerRole,
            Guid? performingUserId,
            Guid? portalId,
            DeploymentEnvironment? environment,
            Instant createdTimestamp)
        {
            if (person.TenantId != customerRole.TenantId)
            {
                throw new ErrorException(Errors.Tenant.Mismatch("role", customerRole.TenantId, person.TenantId));
            }

            var roleIds = new[] { customerRole.Id };
            var personData = new PersonData(person);
            var initializedEvent = new UserInitializedEvent(
                tenantId, userId, userType, personData, customerId, performingUserId, portalId, createdTimestamp, environment, roleIds);
            this.ApplyNewEvent(initializedEvent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAggregate"/> class.
        /// </summary>
        /// <param name="customerId">The referenced customer ID where the user belongs to.</param>
        /// <param name="data">The person data the user is for.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="portalId">The ID of the portal which the user should log in to.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        private UserAggregate(
            Guid tenantId, Guid customerId, PersonData data, Guid? performingUserId, Guid? portalId, Instant createdTimestamp)
        {
            var importedEvent = new UserImportedEvent(tenantId, customerId, data, performingUserId, portalId, createdTimestamp);
            this.ApplyNewEvent(importedEvent);
        }

        public override AggregateType AggregateType => AggregateType.User;

        /// <summary>
        /// Gets the email the user uses to login.
        /// </summary>
        public string? LoginEmail { get; private set; }

        public string? DisplayName { get; private set; }

        public IList<LinkedIdentity> LinkedIdentities { get; private set; } = new List<LinkedIdentity>();

        /// <summary>
        /// Gets the profile picture id of the user.
        /// </summary>
        public Guid ProfilePictureId { get; private set; }

        /// <summary>
        /// Gets the ID of the person this user is for.
        /// </summary>
        public Guid PersonId { get; private set; }

        /// <summary>
        /// Gets the ID of the tenant the user is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the organisation the user is for.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the environment the user belongs to, or null of they have access to all environments.
        /// </summary>
        public DeploymentEnvironment? Environment { get; private set; }

        /// <summary>
        /// Gets the ID of the customer this user relates to, if any, otherwise default.
        /// </summary>
        public Guid? CustomerId { get; private set; }

        /// <summary>
        /// Gets the ID of the portal which the user would log into by default.
        /// This would be null if the user doesn't log into a portal, or the customer
        /// is expected to login to the default portal for the tenanacy.
        /// This needed for the generation of links in emails, e.g. the user activation link.
        /// </summary>
        public Guid? PortalId { get; private set; }

        /// <summary>
        /// Gets the UserTypes currently assigned to the user (old roles implementation).
        /// </summary>
        public IEnumerable<string> UserTypes => this.userTypes;

        /// <summary>
        /// Gets the last user type assigned to the user.
        /// </summary>
        public string? UserType => this.userTypes.LastOrDefault();

        public PortalUserType PortalUserType => this.UserType.ToPortalUserType();

        /// <summary>
        /// Gets the IDs of roles currently assigned to the user (new roles implementation).
        /// </summary>
        public IReadOnlyList<Guid> RoleIds => this.roleIds.AsReadOnly();

        /// <summary>
        /// Gets a value indicating whether the user is activated.
        /// </summary>
        public bool Activated { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user is blocked.
        /// </summary>
        public bool Blocked { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the password last changed time the user was modified in ticks in the epoch.
        /// </summary>
        public Instant PasswordLastChangedTimestamp { get; private set; }

        /// <summary>
        /// Gets the current, salted, hashed password for the user, if any, otherwise null.
        /// </summary>
        public string? CurrentSaltedHashedPassword => this.passwordHistory.LastOrDefault()?.SaltedHashedPassword;

        /// <inheritdoc/>
        public List<AdditionalPropertyValue> AdditionalPropertyValues { get; private set; } =
            new List<AdditionalPropertyValue>();

        /// <summary>
        /// Gets the user status.
        /// </summary>
        public UserStatus UserStatus =>
            this.Blocked ? UserStatus.Deactivated :
            this.Activated ? UserStatus.Active :
            this.currentActivationInvitiation != null ? UserStatus.Invited :
            UserStatus.New;

        /// <summary>
        /// Gets a value indicating whether the user has been marked as deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        public UserReadModel? LatestProjectedReadModel { get; set; }

        /// <summary>
        /// Factory method for creating users.
        /// </summary>
        /// <param name="userId">The ID to use for the user.</param>
        /// <param name="person">The person the user is for.</param>
        /// <param name="performingUserId">The userId who created user.</param>
        /// <param name="portalId">The ID of the portal which the user should log in to.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        /// <returns>A new instance of <see cref="UserAggregate"/>.</returns>
        public static UserAggregate CreateUser(
            Guid tenantId,
            Guid userId,
            UserType userType,
            PersonAggregate person,
            Guid? performingUserId,
            Guid? portalId,
            Instant createdTimestamp,
            Guid[]? initialRoles = null)
        {
            return new UserAggregate(tenantId, userId, userType, person, performingUserId, portalId, createdTimestamp, initialRoles);
        }

        /// <summary>
        /// Factory method for creating customer users.
        /// </summary>
        /// <param name="userId">The ID to use for the user.</param>
        /// <param name="person">The person the user is for.</param>
        /// <param name="customerId">The ID of the customer the user is for.</param>
        /// <param name="customerRole">The customer role for the user's tenant.</param>
        /// <param name="performinguserId">The User who created customer user.</param>
        /// <param name="environment">The customer environment.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        /// <returns>A new instance of <see cref="UserAggregate"/>.</returns>
        public static UserAggregate CreateCustomerUser(
            Guid tenantId,
            Guid userId,
            PersonAggregate person,
            Guid customerId,
            Role customerRole,
            Guid? performinguserId,
            Guid? portalId,
            DeploymentEnvironment? environment,
            Instant createdTimestamp)
        {
            return new UserAggregate(
                tenantId,
                userId,
                Domain.UserType.Customer,
                person,
                customerId,
                customerRole,
                performinguserId,
                portalId,
                environment,
                createdTimestamp);
        }

        /// <summary>
        /// Factory method for creating users.
        /// </summary>
        /// <param name="customerId">The referenced customer ID where the user belongs to.</param>
        /// <param name="person">The person the user is for.</param>
        /// <param name="performingUserId">The userId who imported user.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        /// <returns>A new instance of <see cref="UserAggregate"/>.</returns>
        public static UserAggregate CreateImportedUser(
            Guid tenantId,
            Guid customerId,
            PersonAggregate person,
            Guid? performingUserId,
            Guid? portalId,
            Instant createdTimestamp)
        {
            return new UserAggregate(
                tenantId,
                customerId,
                new PersonData(person),
                performingUserId,
                portalId,
                createdTimestamp);
        }

        /// <summary>
        /// Set the email the user will use to login.
        /// </summary>
        /// <param name="loginEmail">The email address the user will use to login.</param>
        /// <param name="performingUserId">The userId who sets the login email.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void SetLoginEmail(string loginEmail, Guid? performingUserId, Instant timestamp)
        {
            var @event = new LoginEmailSetEvent(this.TenantId, this.Id, performingUserId, loginEmail, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Set the profile picture id of the user.
        /// </summary>
        /// <param name="profilePictureId">The profile picture id to be set.</param>
        /// <param name="performingUserId">The performing user id.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void SetProfilePictureId(Guid profilePictureId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ProfilePictureAssignedToUserEvent(this.TenantId, this.Id, performingUserId, profilePictureId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Change user type.
        /// </summary>
        /// <param name="userType">The userType.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="bypassValidationCheck">The validation check will be bypassed mostly used on migration scenarios to correct/update data.</param>
        public void ChangeUserType(UserType userType, Guid? performingUserId, Instant timestamp, bool bypassValidationCheck = false)
        {
            string userTypeHumanize = userType.Humanize();
            if (this.UserType != userTypeHumanize || bypassValidationCheck)
            {
                this.ApplyNewEvent(new UserTypeUpdatedEvent(this.TenantId, this.Id, userTypeHumanize, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Assign a role to the user.
        /// </summary>
        /// <param name="role">The role to assign.</param>
        /// <param name="performingUserId">The userId who assign the role.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignRole(Role role, Guid? performingUserId, Instant timestamp)
        {
            if (role.TenantId != this.TenantId)
            {
                throw new ErrorException(Errors.Tenant.Mismatch("role", role.TenantId, this.TenantId));
            }

            if (this.roleIds.Contains(role.Id))
            {
                throw new ErrorException(Errors.Role.RoleAlreadyAssigned(this.LoginEmail ?? string.Empty, role.Name));
            }

            this.ApplyNewEvent(new RoleAssignedEvent(this.TenantId, this.Id, role.Id, performingUserId, timestamp));
        }

        /// <summary>
        /// Unassign a role from the user.
        /// </summary>
        /// <param name="role">The role to unassign.</param>
        /// <param name="performingUserId">The userId who retracted the role.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void RetractRole(Role role, Guid? performingUserId, Instant timestamp)
        {
            if (!this.roleIds.Contains(role.Id))
            {
                throw new ErrorException(Errors.Role.UnassignRoleNotAssigned(this.LoginEmail ?? string.Empty, role.Name));
            }

            this.ApplyNewEvent(new RoleRetractedEvent(this.TenantId, this.Id, role.Id, performingUserId, timestamp));
        }

        /// <summary>
        /// Mark the user as blocked.
        /// </summary>
        /// <param name="performingUserId">The userId who blocked user.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void Block(Guid? performingUserId, Instant timestamp)
        {
            if (!this.Blocked)
            {
                var @event = new UserBlockedEvent(
                    this.TenantId, this.Id, this.CustomerId, this.PersonId, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Mark the user as unblocked.
        /// </summary>
        /// <param name="performingUserId">The userId who unlock user.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void Unblock(Guid? performingUserId, Instant timestamp)
        {
            if (this.Blocked)
            {
                var @event = new UserUnblockedEvent(
                    this.TenantId, this.Id, this.CustomerId, this.PersonId, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Create a new activation invitation.
        /// </summary>
        /// <param name="performingUserId">The userId who creates the activation invitation.</param>
        /// <param name="createdTimestamp">Activation invitation created time.</param>
        /// <returns>The ID of the created invitation.</returns>
        public Guid CreateActivationInvitation(Guid? performingUserId, Instant createdTimestamp)
        {
            var @event = new ActivationInvitationCreatedEvent(this.TenantId, this.Id, this.CustomerId, this.PersonId, performingUserId, createdTimestamp);
            this.ApplyNewEvent(@event);
            return @event.InvitationId;
        }

        /// <summary>
        /// Create a new activation invitation.
        /// </summary>
        /// <param name="performingUserId">The userId who create password reset invitation.</param>
        /// <param name="createdTimestamp">Activation invitation created time.</param>
        /// <returns>The ID of the created invitation.</returns>
        public Guid CreatePasswordResetInvitation(Guid? performingUserId, Instant createdTimestamp)
        {
            // Currently we do not prevent invitation creation for un-activated or blocked users?
            var @event = new PasswordResetInvitationCreatedEvent(this.TenantId, this.Id, performingUserId, createdTimestamp);
            this.ApplyNewEvent(@event);
            return @event.InvitationId;
        }

        /// <summary>
        /// Create a new activation invitation.
        /// </summary>
        /// <param name="performingUserId">The userId who created renewal invitation.</param>
        /// <param name="createdTimestamp">Activation invitation created time.</param>
        /// <returns>The ID of the created invitation.</returns>
        public Guid CreateRenewalInvitation(Guid? performingUserId, Instant createdTimestamp)
        {
            // Currently we do not prevent invitation creation for un-activated or blocked users?
            var @event = new PasswordResetInvitationCreatedEvent(this.TenantId, this.Id, performingUserId, createdTimestamp);
            this.ApplyNewEvent(@event);
            return @event.InvitationId;
        }

        /// <summary>
        /// Verifies an activation invitation, and throws an exception upon failure.
        /// </summary>
        /// <param name="invitationId">The ID of the invitation to verify.</param>
        /// <param name="verificationTime">THe time the verification is being performed.</param>
        public void VerifyActivationInvitation(Guid invitationId, Instant verificationTime)
        {
            if (this.Activated)
            {
                throw new ErrorException(Errors.User.Activation.AlreadyActive(this.Id, invitationId));
            }

            if (this.Blocked)
            {
                throw new ErrorException(Errors.User.Activation.UserBlocked(this.Id));
            }

            if (this.currentActivationInvitiation == null)
            {
                throw new ErrorException(Errors.User.Activation.TokenNotFound(this.Id, invitationId));
            }

            if (this.currentActivationInvitiation.Id != invitationId)
            {
                if (this.acceptedInvitationIds.Contains(invitationId))
                {
                    throw new ErrorException(Errors.User.Activation.TokenAlreadyUsed(this.Id, invitationId));
                }

                var targetInvitation = this.replacedInvitations.FirstOrDefault(i => i.Id == invitationId);

                if (targetInvitation != null && targetInvitation.IsExpired(verificationTime))
                {
                    throw new ErrorException(Errors.User.Activation.TokenExpired(this.Id, invitationId));
                }

                if (targetInvitation == null)
                {
                    throw new ErrorException(Errors.User.Activation.TokenNotFound(this.Id, invitationId));
                }
            }

            if (this.currentActivationInvitiation.IsExpired(verificationTime))
            {
                throw new ErrorException(Errors.User.Activation.TokenExpired(this.Id, invitationId));
            }
        }

        /// <summary>
        /// Consume an activation invitation.
        /// </summary>
        /// <param name="invitationId">The ID of the invitation to verify.</param>
        /// <param name="newSaltedHashedPassword">The new salted, hashed password.</param>
        /// <param name="performingUserId">The userId who activate user.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void Activate(
            Guid invitationId, string newSaltedHashedPassword, Guid? performingUserId, Instant timestamp)
        {
            this.VerifyActivationInvitation(invitationId, timestamp);
            var @event = new UserActivatedEvent(this.TenantId, this.Id, invitationId, newSaltedHashedPassword, this.CustomerId, this.PersonId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Activate user.
        /// </summary>
        /// <param name="newSaltedHashedPassword">The new salted, hashed password.</param>
        /// <param name="performingUserId">The userId who activate user.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void Activate(string? newSaltedHashedPassword, Guid? performingUserId, Instant timestamp)
        {
            var @event = new UserActivatedEvent(this.TenantId, this.Id, default, newSaltedHashedPassword, this.CustomerId, this.PersonId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Set customer Id and environment.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="userId">The user Id.</param>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="environment">The deployment environment.</param>
        public void SetCustomerIdAndEnvironment(
            Guid tenantId, Guid userId, Guid customerId, DeploymentEnvironment environment)
        {
            var @event = new CustomerIdAndEnvironmentUpdatedEvent(tenantId, userId, customerId, environment);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Deletes the user and records the deletion as an event.
        /// </summary>
        /// <param name="performingUserId">The unique identifier of the user performing the deletion.</param>
        /// <param name="timestamp">The timestamp at which the deletion occurred.</param>
        public void SoftDelete(Guid? performingUserId, Instant timestamp)
        {
            var @event = new UserDeletedEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void Restore(Guid? performingUserId, Instant timestamp)
        {
            var @event = new UserUndeletedEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Run checks on a password reset invitation and throw an exception upon failure.
        /// </summary>
        /// <param name="invitationId">The ID of the invitation to verify.</param>
        /// <param name="verificationTime">THe time the verification is being performed.</param>
        public void VerifyPasswordResetInvitation(Guid invitationId, Instant verificationTime)
        {

            if (this.Blocked)
            {
                throw new ErrorException(Errors.User.ResetPassword.UserBlocked(this.Id));
            }

            if (this.currentPasswordResetInvitiation == null ||
                this.currentPasswordResetInvitiation.Id != invitationId)
            {
                if (this.acceptedInvitationIds.Contains(invitationId))
                {
                    throw new ErrorException(Errors.User.ResetPassword.TokenAlreadyUsed(this.Id, invitationId));
                }

                var targetInvitation = this.replacedInvitations.FirstOrDefault(i => i.Id == invitationId);

                if (targetInvitation == null)
                {
                    throw new ErrorException(Errors.User.ResetPassword.TokenSuperseded(this.Id, invitationId));
                }

                throw new ErrorException(Errors.User.ResetPassword.TokenNotFound(this.Id, invitationId));
            }

            if (this.currentPasswordResetInvitiation.IsExpired(verificationTime))
            {
                throw new ErrorException(Errors.User.ResetPassword.TokenExpired(this.Id, invitationId));
            }
        }

        /// <summary>
        /// Consume a password reset invitation.
        /// </summary>
        /// <param name="invitationId">The ID of the invitation to verify.</param>
        /// <param name="cleartextPassword">The new cleartext password.</param>
        /// <param name="verificationTime">THe time the verification is being performed.</param>
        /// <param name="passwordHashingService"> A service for hashing and verifying passwords.</param>
        /// <param name="passwordReuseValidator">Service for validating password reuse restrictions.</param>
        /// <param name="performingUserId">The userId who change the password.</param>
        /// <returns>A new instance of <see cref="PasswordSettingResult"/> indicating the result.</returns>
        public PasswordSettingResult ChangePassword(
            Guid invitationId,
            string cleartextPassword,
            Instant verificationTime,
            IPasswordHashingService passwordHashingService,
            IPasswordReuseValidator passwordReuseValidator,
            Guid? performingUserId)
        {
            this.VerifyPasswordResetInvitation(invitationId, verificationTime);

            var reuseValidationResult = passwordReuseValidator.Validate(
                cleartextPassword, this.passwordHistory, verificationTime, passwordHashingService);
            if (!reuseValidationResult.IsSuccess)
            {
                throw new ErrorException(Errors.User.ChangePassword.ReuseDetected());
            }

            var newSaltedHashedPassword = passwordHashingService.SaltAndHash(cleartextPassword);
            var @event = new PasswordChangedEvent(
                this.TenantId, this.Id, invitationId, newSaltedHashedPassword, performingUserId, verificationTime);
            this.ApplyNewEvent(@event);

            return PasswordSettingResult.Success();
        }

        public void ChangePortal(Guid? portalId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new PortalChangedEvent(this.TenantId, this.Id, portalId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Evaluates the user aggregate (which is expected to be a customer) if it can be associated with quotes.
        /// </summary>
        /// <returns>Boolean result whether the customer user aggregate is ready for quote association.</returns>
        public bool CanBeAssociatedWithQuotes()
        {
            return this.Activated && !this.Blocked && this.UserTypes.Contains(UBind.Domain.UserType.Customer.Humanize());
        }

        /// <summary>
        /// Trigger the ApplyNewIdEvent that applies new id to this aggregate.
        /// </summary>
        /// <param name="newTenantId">The tenant new Id.</param>
        /// <param name="performingUserId">The userId who did the action.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void TriggerApplyNewIdEvent(Guid newTenantId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ApplyNewIdEvent(newTenantId, this.Id, this.PersonId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record organisation migration and only applicable for an aggregate with an empty organisation Id.
        /// </summary>
        /// <param name="organisationId">The Id of the organisation to persist in this aggregate.</param>
        /// <param name="performingUserId">The user Id who updates the aggregate.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        /// <returns>A result indicating success or any error.</returns>
        public Result<Guid, Error> RecordOrganisationMigration(
            Guid organisationId, Guid? performingUserId, Instant timestamp)
        {
            if (this.OrganisationId != Guid.Empty)
            {
                return Result.Failure<Guid, Error>(
                    Errors.Organisation.FailedToMigrateForOrganisation(this.Id, this.OrganisationId));
            }

            var @event = new UserOrganisationMigratedEvent(
                this.TenantId, this.Id, organisationId, this.PersonId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);

            return Result.Success<Guid, Error>(@event.AggregateId);
        }

        /// <summary>
        /// Rolls back the aggregate to its previous modified time if the latest is from organisation migration.
        /// </summary>
        /// <param name="modifiedTime">The modified time of the user.</param>
        /// <param name="performingUserId">The user Id who updates the aggregate.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        public void RollbackDateModifiedBeforeOrganisationMigration(
            Instant modifiedTime, Guid? performingUserId, Instant timestamp)
        {
            var @event
                = new UserModifiedTimeUpdatedEvent(this.TenantId, this.Id, this.PersonId, modifiedTime, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <inheritdoc/>
        public void AddAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefinitionId,
            string value,
            AdditionalPropertyDefinitionType propertyType,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            var initalizedEvent = new AdditionalPropertyValueInitializedEvent<UserAggregate, IUserEventObserver>(
                tenantId,
                this.Id,
                Guid.NewGuid(),
                additionalPropertyDefinitionId,
                entityId,
                value,
                propertyType,
                performingUserId,
                createdTimestamp);
            this.ApplyNewEvent(initalizedEvent);
        }

        /// <inheritdoc/>
        public void UpdateAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            AdditionalPropertyDefinitionType type,
            Guid additionalPropertyDefinitionId,
            Guid additionalPropertyValueId,
            string value,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            var updateEvent = new AdditionalPropertyValueUpdatedEvent<UserAggregate, IUserEventObserver>(
                tenantId,
                this.Id,
                value,
                performingUserId,
                createdTimestamp,
                type,
                additionalPropertyDefinitionId,
                additionalPropertyValueId,
                entityId);
            this.ApplyNewEvent(updateEvent);
        }

        public void ChangePassword(
            Guid resetInvitationId,
            string newSaltedHashedPassword,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            var passwordChangedEvent = new PasswordChangedEvent(
                this.TenantId,
                this.Id,
                resetInvitationId,
                newSaltedHashedPassword,
                performingUserId,
                createdTimestamp);
            this.ApplyNewEvent(passwordChangedEvent);
        }

        public void ActivateUser(
            Guid resetInvitationId,
            string newSaltedHashedPassword,
            Guid? performingUserId,
            Instant timestamp,
            Guid? customerId = null,
            Guid? personId = null)
        {
            var userActivatedEvent = new UserActivatedEvent(
                this.TenantId,
                this.Id,
                resetInvitationId,
                newSaltedHashedPassword,
                customerId ?? this.CustomerId,
                personId ?? this.PersonId,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(userActivatedEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueInitializedEvent<UserAggregate, IUserEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.Add(this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueUpdatedEvent<UserAggregate, IUserEventObserver> aggregateEvent, int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.AddOrUpdate(
                this.AdditionalPropertyValues, aggregateEvent);
        }

        public void TransferToAnotherOrganisation(
            Guid tenantId,
            Guid currentOrganisationId,
            Guid newOrganisationId,
            bool fromDefaultOrganisation,
            Guid? performingUserId,
            Instant timestamp)
        {
            var @event = new UserTransferredToAnotherOrganisationEvent(
                tenantId,
                this.Id,
                currentOrganisationId,
                newOrganisationId,
                this.PersonId,
                fromDefaultOrganisation,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        public void LinkIdentity(Guid authenticationMethodId, string uniqueId, Guid? performingUserId, Instant timestamp)
        {
            // find an existing identity with the same provider
            var existingIdentity
                = this.LinkedIdentities.FirstOrDefault(i => i.AuthenticationMethodId == authenticationMethodId);
            if (existingIdentity != null)
            {
                throw new ErrorException(
                    Errors.User.LinkedIdentityProviderAlreadyExists(this.LoginEmail ?? string.Empty, authenticationMethodId));
            }

            var @event = new UserIdentityLinkedEvent(
                this.TenantId,
                this.Id,
                authenticationMethodId,
                uniqueId,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        public void UnlinkIdentity(Guid authenticationMethodId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new UserIdentityUnlinkedEvent(
                this.TenantId,
                this.Id,
                authenticationMethodId,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        public override UserAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<UserAggregate, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(UserInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.PersonId = @event.Person.PersonId;
            this.TenantId =
                @event.TenantId == default
                ? @event.Person.TenantId
                : @event.TenantId;
            this.OrganisationId = @event.Person.OrganisationId;
            this.Environment = @event.Environment;
            this.CustomerId = @event.CustomerId;
            this.CreatedTimestamp = @event.Timestamp;
            this.PortalId = @event.PortalId;
            this.LoginEmail = @event.Person.LoginEmail ?? @event.Person.Email;
            this.DisplayName = @event.Person.DisplayName;
            if (@event.UserType.HasValue)
            {
                this.userTypes.Add(@event.UserType.Value.ToString());
            }

            if (@event.RoleIds != null)
            {
                foreach (var roleId in @event.RoleIds)
                {
                    this.roleIds.Add(roleId);
                }
            }
        }

        private void Apply(UserImportedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.PersonId = @event.Person.PersonId;
            this.TenantId = @event.TenantId;
            this.OrganisationId = @event.Person.OrganisationId;
            this.Environment = @event.Environment;
            this.CustomerId = @event.CustomerId;
            this.CreatedTimestamp = @event.Timestamp;
            this.PortalId = @event.PortalId;
            this.LoginEmail = @event.Person.LoginEmail ?? @event.Person.Email;
            this.DisplayName = @event.Person.DisplayName;
        }

        private void Apply(LoginEmailSetEvent @event, int sequenceNumber)
        {
            this.LoginEmail = @event.LoginEmail;
        }

        private void Apply(ProfilePictureAssignedToUserEvent @event, int sequenceNumber)
        {
            this.ProfilePictureId = @event.ProfilePictureId;
        }

        // the event name does not match what its doing, so renaming it to UserTypeUpdatedEvent is better.
        // but we cannot remove this since other aggregates still use this.
        private void Apply(RoleAddedEvent @event, int sequenceNumber)
        {
            this.Apply(
                    new UserTypeUpdatedEvent(
                        @event.TenantId,
                        @event.AggregateId,
                        @event.RoleName,
                        @event.PerformingUserId,
                        @event.Timestamp),
                    sequenceNumber);
        }

        private void Apply(UserTypeUpdatedEvent @event, int sequenceNumber)
        {
            string userType = @event.UserType;

            // we have removed the "Agent" userType, so now it's just called "Client"
            if (userType == "Agent")
            {
                userType = Domain.UserType.Client.Humanize();
            }

            // make sure it's last in the order
            if (this.userTypes.Contains(@event.UserType))
            {
                this.userTypes.Remove(@event.UserType);
            }

            this.userTypes.Add(@event.UserType);
        }

        private void Apply(UserBlockedEvent @event, int sequenceNumber)
        {
            this.Blocked = true;
        }

        private void Apply(UserUnblockedEvent @event, int sequenceNumber)
        {
            this.Blocked = false;
        }

        private void Apply(ActivationInvitationCreatedEvent @event, int sequenceNumber)
        {
            if (this.currentActivationInvitiation != null)
            {
                this.replacedInvitations.Add(this.currentActivationInvitiation);
            }

            this.currentActivationInvitiation = new Invitation(@event.InvitationId, @event.Timestamp);
        }

        private void Apply(PasswordResetInvitationCreatedEvent @event, int sequenceNumber)
        {
            if (this.currentPasswordResetInvitiation != null)
            {
                this.replacedInvitations.Add(this.currentPasswordResetInvitiation);
            }

            this.currentPasswordResetInvitiation = new Invitation(@event.InvitationId, @event.Timestamp);
        }

        private void Apply(UserActivatedEvent @event, int sequenceNumber)
        {
            this.Activated = true;
            this.currentActivationInvitiation = null;
            this.acceptedInvitationIds.Add(@event.InvitationId);
            if (@event.NewSaltedHashedPassword != null)
            {
                this.passwordHistory.Add(new PasswordLifespan(@event.NewSaltedHashedPassword, @event.Timestamp));
                this.PasswordLastChangedTimestamp = @event.Timestamp;
            }
        }

        private void Apply(UserDeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = true;
            this.Blocked = true;
        }

        private void Apply(UserUndeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = false;
            this.Blocked = true;
        }

        private void Apply(PasswordChangedEvent @event, int sequenceNumber)
        {
            this.currentPasswordResetInvitiation = null;
            this.acceptedInvitationIds.Add(@event.InvitationId);
            if (this.passwordHistory.Any())
            {
                this.passwordHistory.Last().Expire(@event.Timestamp);
            }

            this.passwordHistory.Add(new PasswordLifespan(@event.NewSaltedHashedPassword, @event.Timestamp));
            this.PasswordLastChangedTimestamp = @event.Timestamp;
        }

        private void Apply(ApplyNewIdEvent @event, int sequenceNumber)
        {
            this.TenantId = @event.TenantId;
        }

        private void Apply(UserOrganisationMigratedEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(UserModifiedTimeUpdatedEvent @event, int sequenceNumber)
        {
            // Nothing to do
        }

        private void Apply(UserTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(PortalChangedEvent @event, int sequenceNumber)
        {
            this.PortalId = @event.Value;
        }

        private void Apply(CustomerIdAndEnvironmentUpdatedEvent @event, int sequenceNumber)
        {
            this.CustomerId = @event.CustomerId;
            this.Environment = @event.Environment;
        }

        private void Apply(UserIdentityLinkedEvent @event, int sequenceNumber)
        {
            // add the new identity
            this.LinkedIdentities.Add(new LinkedIdentity
            {
                AuthenticationMethodId = @event.AuthenticationMethodId,
                UniqueId = @event.UniqueId,
            });
        }

        private void Apply(UserIdentityUnlinkedEvent @event, int sequenceNumber)
        {
            // find an existing identity with the same provider
            var existingIdentity
                = this.LinkedIdentities.FirstOrDefault(i => i.AuthenticationMethodId == @event.AuthenticationMethodId);
            if (existingIdentity != null)
            {
                // remove the existing identity
                this.LinkedIdentities.Remove(existingIdentity);
            }
        }
    }
}
