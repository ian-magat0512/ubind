// <copyright file="UserReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using static UBind.Domain.Aggregates.User.UserAggregate;

    /// <summary>
    /// Manages the updating of the user read model data.
    /// </summary>
    public class UserReadModelWriter : IUserReadModelWriter
    {
        private readonly IWritableReadModelRepository<UserReadModel> userReadModelRepository;
        private readonly IWritableReadModelRepository<UserLoginEmail> writableUserLoginEmailRepository;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IRoleRepository roleRepository;
        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserReadModelWriter"/> class.
        /// </summary>
        /// <param name="userReadModelUpdateRepository">The repository for updating user read models.</param>
        /// <param name="userLoginEmailRepository">The repository for the user login email.</param>
        /// <param name="roleRepository">roleRepository.</param>
        /// <param name="propertyTypeEvaluatorService">Property type evaluator service.</param>
        public UserReadModelWriter(
            IWritableReadModelRepository<UserReadModel> userReadModelUpdateRepository,
            IWritableReadModelRepository<UserLoginEmail> writableUserLoginEmailRepository,
            IUserLoginEmailRepository userLoginEmailRepository,
            IRoleRepository roleRepository,
            PropertyTypeEvaluatorService propertyTypeEvaluatorService)
        {
            this.userReadModelRepository = userReadModelUpdateRepository;
            this.writableUserLoginEmailRepository = writableUserLoginEmailRepository;
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.roleRepository = roleRepository;
            this.propertyTypeEvaluatorService = propertyTypeEvaluatorService;
        }

        public void Dispatch(
            UserAggregate aggregate,
            IEvent<UserAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Dispatch(
            PersonAggregate aggregate,
            IEvent<PersonAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(UserAggregate aggregate, UserInitializedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.userReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
                this.writableUserLoginEmailRepository.Delete(
                    @event.TenantId,
                    (p) => p.OrganisationId == aggregate.OrganisationId
                    && p.Id == @event.AggregateId);
            }

            var user = new UserReadModel(
                @event.AggregateId,
                @event.Person,
                @event.CustomerId,
                @event.PortalId,
                @event.Timestamp,
                this.ResolveUserType(aggregate, @event.UserType, @event.CustomerId),
                @event.Environment)
            {
                TenantId = @event.Person.TenantId == Guid.Empty ? @event.TenantId : @event.Person.TenantId,
                LastModifiedTimestamp = @event.Timestamp,
                UserType = @event.UserType.ToString(),
            };
            if (@event.RoleIds != null)
            {
                foreach (var roleId in @event.RoleIds)
                {
                    var role = this.roleRepository.GetRoleById(user.TenantId, roleId);
                    if (role != null)
                    {
                        user.Roles.Add(role);
                    }
                }
            }

            this.userReadModelRepository.Add(user);

            // Also add a record to UserLoginEmails
            var userLogin = this.writableUserLoginEmailRepository.GetById(user.TenantId, user.Id);
            if (userLogin == null)
            {
                this.CreateUserLoginEmail(aggregate, aggregate.LoginEmail, @event.Timestamp);
            }

            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserImportedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.userReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
                this.writableUserLoginEmailRepository.Delete(
                    @event.TenantId,
                    (p) => p.OrganisationId == aggregate.OrganisationId
                    && p.Id == @event.AggregateId);
            }

            var user = new UserReadModel(
                @event.AggregateId,
                @event.Person,
                @event.CustomerId,
                @event.PortalId,
                @event.Timestamp,
                this.ResolveUserType(aggregate, null, @event.CustomerId),
                @event.Environment)
            {
                TenantId = @event.TenantId,
                LastModifiedTimestamp = @event.Timestamp,
            };

            this.userReadModelRepository.Add(user);
            if (aggregate.LoginEmail.IsNotNullOrEmpty())
            {
                this.CreateUserLoginEmail(aggregate, aggregate.LoginEmail, @event.Timestamp);
            }

            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.OrganisationId = @event.OrganisationId;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, LoginEmailSetEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.LoginEmail = @event.LoginEmail;
            user.LastModifiedTimestamp = @event.Timestamp;
            this.UpdateUserLoginEmail(aggregate, @event.LoginEmail);
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, ProfilePictureAssignedToUserEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.ProfilePictureId = @event.ProfilePictureId;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, RoleAddedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.UserType = @event.RoleName;
            user.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserTypeUpdatedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.UserType = @event.UserType;
            user.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, RoleAssignedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            var role = this.roleRepository.GetRoleById(user.TenantId, @event.RoleId);
            user.Roles.Add(role);
            user.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, RoleRetractedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.Roles.Remove(user.Roles.Single(r => r.Id == @event.RoleId));
            user.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserBlockedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.IsDisabled = true;
            user.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserUnblockedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.IsDisabled = false;
            user.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, ActivationInvitationCreatedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.HasBeenInvitedToActivate = true;
            user.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserActivatedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            if (user == null)
            {
                var additionalDetails = new[]
                {
                    $"Invitation Id: {@event.ResetInvitationId}",
                    $"Organisation Id: {aggregate.OrganisationId}",
                };
                throw new ErrorException(
                    Errors.User.NotFound(@event.AggregateId, additionalDetails));
            }

            user.HasBeenActivated = true;
            user.LastModifiedTimestamp = @event.Timestamp;
            user.PasswordLastChangedTimestamp = @event.Timestamp;

            var userLogin = this.GetUserLoginEmail(aggregate);
            if (userLogin == null)
            {
                throw new ErrorException(Errors.General.Unexpected(
                    "When activating a user, there was no UserLoginEmail record for this user, so we couldn't set their password."));
            }

            userLogin.SaltedHashedPassword = @event.NewSaltedHashedPassword;
            userLogin.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, PasswordResetInvitationCreatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, PasswordChangedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.PasswordLastChangedTimestamp = @event.Timestamp;

            var userLogin = this.GetUserLoginEmail(aggregate);
            if (userLogin == null)
            {
                throw new ErrorException(Errors.General.Unexpected(
                    "When changing a password for a user, there was no UserLoginEmail record for this user, so we couldn't set their password."));
            }

            userLogin.SaltedHashedPassword = @event.NewSaltedHashedPassword;
            userLogin.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserModifiedTimeUpdatedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.LastModifiedTimestamp = @event.ModifiedTime;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.FullNameUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.FullName = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.NamePrefixUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.NamePrefix = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.FirstNameUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.FirstName = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.MiddleNamesUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.MiddleNames = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.LastNameUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.LastName = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.NameSuffixUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.NameSuffix = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.CompanyUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.Company = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.TitleUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.Title = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PreferredNameUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.PreferredName = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.EmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.Email = @event.Value, @event.Timestamp);

            var user = this.GetUserByPersonId(@event.TenantId, @event.AggregateId);
            if (user != null)
            {
                var userLogin = this.writableUserLoginEmailRepository.Where(@event.TenantId, u => u.LoginEmail == user.Email).FirstOrDefault();
                if (userLogin != null)
                {
                    userLogin.LoginEmail = @event.Value;
                    userLogin.LastModifiedTimestamp = @event.Timestamp;
                }

                user.Email = @event.Value;
                user.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.AlternativeEmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.AlternativeEmail = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.MobilePhoneUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.MobilePhoneNumber = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.HomePhoneUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.HomePhoneNumber = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.WorkPhoneUpdatedEvent @event, int sequenceNumber)
        {
            this.UpdateUserProperty(@event.TenantId, @event.AggregateId, u => u.WorkPhoneNumber = @event.Value, @event.Timestamp);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonUpdatedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserByPersonId(@event.TenantId, @event.AggregateId);
            if (user == null)
            {
                return;
            }

            var userLogin = this.writableUserLoginEmailRepository.Where(@event.TenantId, u => u.LoginEmail == user.Email).FirstOrDefault();
            if (userLogin != null)
            {
                var userLoginEmail = this.userLoginEmailRepository.GetUserLoginEmailByEmail(
                    aggregate.TenantId,
                    aggregate.OrganisationId,
                    user.PortalUserType,
                    @event.PersonData.Email);
                if (userLoginEmail != null && userLoginEmail.Id != aggregate.UserId)
                {
                    throw new DuplicateUserEmailException(Domain.Errors.Customer.EmailAddressInUseByAnotherUser(@event.PersonData.Email));
                }
                userLogin.LoginEmail = @event.PersonData.Email;
                userLogin.LastModifiedTimestamp = @event.Timestamp;
            }

            user.FirstName = @event.PersonData.FirstName;
            user.LastName = @event.PersonData.LastName;
            user.FullName = @event.PersonData.FullName;
            user.PreferredName = @event.PersonData.PreferredName;
            user.FirstName = @event.PersonData.FirstName;
            user.LastName = @event.PersonData.LastName;
            user.Email = @event.PersonData.Email;
            user.AlternativeEmail = @event.PersonData.AlternativeEmail;
            user.MobilePhoneNumber = @event.PersonData.MobilePhone;
            user.HomePhoneNumber = @event.PersonData.HomePhone;
            user.WorkPhoneNumber = @event.PersonData.WorkPhone;
            user.NameSuffix = @event.PersonData.NameSuffix;
            user.NamePrefix = @event.PersonData.NamePrefix;
            user.MiddleNames = @event.PersonData.MiddleNames;
            user.Company = @event.PersonData.Company;
            user.Title = @event.PersonData.Title;
            user.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserByPersonId(@event.TenantId, @event.AggregateId);
            if (user == null)
            {
                return;
            }

            user.OrganisationId = @event.OrganisationId;
        }

        public void Handle(UserAggregate aggregate, UserAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.TenantId = @event.TenantId;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var user = this.userReadModelRepository
                .SingleMaybe(@event.TenantId, u => u.PersonId == @event.AggregateId);
            if (user.HasValue)
            {
                user.Value.TenantId = @event.TenantId;
            }
        }

        public void Handle(
            UserAggregate aggregate,
            AdditionalPropertyValueInitializedEvent<UserAggregate, IUserEventObserver> @event,
            int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.propertyTypeEvaluatorService.DeleteAdditionalPropertyValue(
                    (TGuid<Tenant>)@event.TenantId,
                    @event.AdditionalPropertyDefinitionType,
                    (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId);
            }

            this.propertyTypeEvaluatorService.CreateNewAdditionalPropertyValueByPropertyType(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);
        }

        public void Handle(
            UserAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<UserAggregate, IUserEventObserver> @event,
            int sequenceNumber)
        {
            this.propertyTypeEvaluatorService.UpdateAdditionalPropertyValue(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);

            var user = this.GetUserById(@event.TenantId, @event.EntityId);
            if (user != null)
            {
                user.LastModifiedTimestamp = @event.Timestamp;
            }

            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);

            // Get the user login email first before applying the changes to the user properties.
            var userLoginEmail = this.GetUserLoginEmail(user);
            if (userLoginEmail != null)
            {
                userLoginEmail.SetOrganisationId(@event.OrganisationId);
                userLoginEmail.LastModifiedTimestamp = @event.Timestamp;
            }

            user.OrganisationId = @event.OrganisationId;
            user.LastModifiedTimestamp = @event.Timestamp;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var user = this.GetUserByPersonId(@event.TenantId, @event.AggregateId);
            if (user == null)
            {
                return;
            }

            // Update the user login email first before applying the changes to the user properties.
            var userLoginEmail = this.GetUserLoginEmail(user);
            if (userLoginEmail != null)
            {
                userLoginEmail.SetOrganisationId(@event.OrganisationId);
                userLoginEmail.LastModifiedTimestamp = @event.Timestamp;
            }

            user.OrganisationId = @event.OrganisationId;
        }

        public void Handle(UserAggregate aggregate, UserAggregate.PortalChangedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            user.PortalId = @event.Value;
            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserDeletedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            if (user == null)
            {
                return;
            }

            user.IsDeleted = true;
            user.LastModifiedTimestamp = @event.Timestamp;

            // Mark the associated UserLoginEmail for the user as deleted
            var userLoginEmail = this.GetUserLoginEmail(user);
            if (userLoginEmail != null)
            {
                userLoginEmail.IsDeleted = true;
                userLoginEmail.LastModifiedTimestamp = @event.Timestamp;
            }

            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, UserUndeletedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.AggregateId);
            if (user == null)
            {
                return;
            }

            user.IsDeleted = false;
            user.LastModifiedTimestamp = @event.Timestamp;

            // Mark the associated UserLoginEmail for the user as deleted
            var userLoginEmail = this.GetUserLoginEmail(user);
            if (userLoginEmail != null)
            {
                userLoginEmail.IsDeleted = false;
                userLoginEmail.LastModifiedTimestamp = @event.Timestamp;
            }

            aggregate.LatestProjectedReadModel = user;
        }

        public void Handle(UserAggregate aggregate, CustomerIdAndEnvironmentUpdatedEvent @event, int sequenceNumber)
        {
            var user = this.GetUserById(@event.TenantId, @event.UserId);
            if (user.CustomerId == null)
            {
                user.CustomerId = @event.CustomerId;
            }

            if (user.Environment == null)
            {
                user.UpdateEnvironment(@event.Environment);
            }

            aggregate.LatestProjectedReadModel = user;
        }

        private UserReadModel GetUserById(Guid tenantId, Guid userId)
        {
            return this.userReadModelRepository.GetById(tenantId, userId);
        }

        private UserReadModel GetUserByPersonId(Guid tenantId, Guid personId)
        {

            // implemented retries because error caused by timeout.
            UserReadModel Execute()
            {
                return this.userReadModelRepository.Where(tenantId, u => u.PersonId == personId).FirstOrDefault();
            }

            var result = RetryPolicyHelper.Execute<Exception, UserReadModel>(() => Execute(), 3, 30, 75);
            return result;
        }

        private UserLoginEmail? GetUserLoginEmail(UserReadModel user)
        {
            var userLoginEmail = this.writableUserLoginEmailRepository
                .Where(user.TenantId, u => u.OrganisationId == user.OrganisationId && u.LoginEmail == user.LoginEmail)
                .FirstOrDefault();
            return userLoginEmail;
        }

        private UserLoginEmail? GetUserLoginEmail(UserAggregate userAggregate)
        {
            var userLoginEmail = this.writableUserLoginEmailRepository
                .Where(userAggregate.TenantId, u => u.OrganisationId == userAggregate.OrganisationId && u.LoginEmail == userAggregate.LoginEmail)
                .FirstOrDefault();
            return userLoginEmail;
        }

        private void UpdateUserProperty(Guid tenantId, Guid aggregateId, Action<UserReadModel> propertyUpdater, Instant timestamp)
        {
            var user = this.GetUserByPersonId(tenantId, aggregateId);
            if (user != null)
            {
                propertyUpdater(user);
                user.LastModifiedTimestamp = timestamp;
            }
        }

        private void UpdateUserLoginEmail(UserAggregate aggregate, string loginEmail)
        {
            var userLoginEmail = this.userLoginEmailRepository.GetUserLoginEmailByEmail(
                aggregate.TenantId, aggregate.OrganisationId, aggregate.PortalUserType, loginEmail);
            if (userLoginEmail != null)
            {
                if (userLoginEmail.Id != aggregate.Id)
                {
                    throw new DuplicateUserEmailException(Domain.Errors.Customer.EmailAddressInUseByAnotherUser(loginEmail));
                }
            }
            else
            {
                // We check the existing UserLoginEmail and update it
                userLoginEmail = this.userLoginEmailRepository.GetUserLoginEmailById(
                    aggregate.TenantId, aggregate.OrganisationId, aggregate.PortalUserType, aggregate.Id);
                if (userLoginEmail != null && userLoginEmail.LoginEmail != loginEmail)
                {
                    userLoginEmail.LoginEmail = loginEmail;
                }
            }
        }

        private void CreateUserLoginEmail(UserAggregate aggregate, string loginEmail, Instant timestamp)
        {
            var userLoginEmail = this.userLoginEmailRepository.GetUserLoginEmailByEmail(
                aggregate.TenantId, aggregate.OrganisationId, aggregate.PortalUserType, loginEmail);
            if (userLoginEmail != null)
            {
                if (userLoginEmail.Id != aggregate.Id)
                {
                    throw new DuplicateUserEmailException(Domain.Errors.Customer.EmailAddressInUseByAnotherUser(loginEmail));
                }
            }
            else
            {
                userLoginEmail = new UserLoginEmail(aggregate, timestamp);
                this.writableUserLoginEmailRepository.Add(userLoginEmail);
            }
        }

        private UserType ResolveUserType(UserAggregate aggregate, UserType? eventUserType, Guid? customerId)
        {
            UserType userType = UserType.Client;
            if (eventUserType != null)
            {
                userType = eventUserType.Value;
            }
            else
            {
                var userTypeStr = aggregate.UserType;
                UserType? resolvedUserType = null;
                if (userTypeStr != null)
                {
                    resolvedUserType = userTypeStr.ToEnumOrNull<UserType>();
                }

                if (resolvedUserType == null)
                {
                    userType = customerId.HasValue ? UserType.Customer : UserType.Client;
                }
            }

            return userType;
        }
    }
}
