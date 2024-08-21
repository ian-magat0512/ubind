// <copyright file="PersonReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <inheritdoc/>
    public class PersonReadModelWriter : IPersonReadModelWriter
    {
        private readonly IWritableReadModelRepository<PersonReadModel> personReadModelRepository;
        private readonly IWritableReadModelRepository<CustomerReadModel> customerReadModelRepository;
        private readonly IWritableReadModelRepository<EmailAddressReadModel> emailAddressRepository;
        private readonly IWritableReadModelRepository<PhoneNumberReadModel> phoneNumberRepository;
        private readonly IWritableReadModelRepository<StreetAddressReadModel> streetAddressRepository;
        private readonly IWritableReadModelRepository<WebsiteAddressReadModel> websiteAddressRepository;
        private readonly IWritableReadModelRepository<MessengerIdReadModel> messengerIdRepository;
        private readonly IWritableReadModelRepository<SocialMediaIdReadModel> socialMediaIdRepository;
        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonReadModelWriter"/> class.
        /// </summary>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        /// <param name="emailAddressRepository">The email address read model repository.</param>
        /// <param name="phoneRepository">The phone number read model repository.</param>
        /// <param name="addressRepository">The address read model repository.</param>
        /// <param name="webAddressRepository">The web address read model repository.</param>
        /// <param name="messengerRepository">The messenger read model repository.</param>
        /// <param name="socialRepository">The social read model repository.</param>
        /// <param name="propertyTypeEvaluatorService">Property type evaluator service.</param>
        public PersonReadModelWriter(
            IWritableReadModelRepository<PersonReadModel> personReadModelRepository,
            IWritableReadModelRepository<CustomerReadModel> customerReadModelRepository,
            IWritableReadModelRepository<EmailAddressReadModel> emailAddressRepository,
            IWritableReadModelRepository<PhoneNumberReadModel> phoneRepository,
            IWritableReadModelRepository<StreetAddressReadModel> addressRepository,
            IWritableReadModelRepository<WebsiteAddressReadModel> webAddressRepository,
            IWritableReadModelRepository<MessengerIdReadModel> messengerRepository,
            IWritableReadModelRepository<SocialMediaIdReadModel> socialRepository,
            PropertyTypeEvaluatorService propertyTypeEvaluatorService)
        {
            this.personReadModelRepository = personReadModelRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.emailAddressRepository = emailAddressRepository;
            this.phoneNumberRepository = phoneRepository;
            this.streetAddressRepository = addressRepository;
            this.websiteAddressRepository = webAddressRepository;
            this.messengerIdRepository = messengerRepository;
            this.socialMediaIdRepository = socialRepository;
            this.propertyTypeEvaluatorService = propertyTypeEvaluatorService;
        }

        public void Dispatch(
            PersonAggregate aggregate,
            IEvent<PersonAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
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
            CustomerAggregate aggregate,
            IEvent<CustomerAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonInitializedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.personReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person == null)
            {
                if (@event.PersonData != null)
                {
                    var personReadModel = PersonReadModel.CreateFromPersonData(
                        @event.TenantId,
                        @event.AggregateId,
                        @event.CustomerId,
                        @event.UserId,
                        @event.PersonData,
                        @event.Timestamp,
                        @event.IsTestData);
                    this.personReadModelRepository.Add(personReadModel);
                    this.UpdatePersonRepeatingFields(personReadModel, @event.PersonData);
                }
                else
                {
                    var personReadModel = PersonReadModel.CreatePerson(
                        @event.TenantId, @event.OrganisationId, @event.AggregateId, @event.Timestamp, @event.IsTestData);
                    this.personReadModelRepository.Add(personReadModel);
                }
            }
            else
            {
                if (@event.UserId.HasValue && person.UserId != @event.UserId)
                {
                    person.UserId = @event.UserId;
                }

                if (@event.CustomerId != Guid.Empty && person.CustomerId != @event.CustomerId)
                {
                    person.CustomerId = @event.CustomerId;
                }
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonImportedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.personReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person == null)
            {
                if (@event.PersonData != null)
                {
                    var personReadModel = PersonReadModel.CreateFromPersonData(
                        @event.TenantId,
                        @event.AggregateId,
                        @event.CustomerId,
                        null,
                        @event.PersonData,
                        @event.Timestamp,
                        @event.IsTestData);
                    this.personReadModelRepository.Add(personReadModel);
                }
                else
                {
                    var personReadModel = PersonReadModel.CreateImportedPerson(
                        @event.TenantId,
                        @event.OrganisationId,
                        @event.AggregateId,
                        @event.FullName,
                        @event.PreferredName,
                        @event.Email,
                        @event.AlternativeEmail,
                        @event.MobilePhone,
                        @event.HomePhone,
                        @event.WorkPhone,
                        @event.Timestamp,
                        @event.IsTestData);
                    this.personReadModelRepository.Add(personReadModel);
                }
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.FullNameUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.FullName = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PreferredNameUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.PreferredName = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.EmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.Email = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.AlternativeEmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.AlternativeEmail = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.MobilePhoneUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.MobilePhoneNumber = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.HomePhoneUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.HomePhoneNumber = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.WorkPhoneUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.WorkPhoneNumber = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.NamePrefixUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.NamePrefix = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.FirstNameUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.FirstName = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.MiddleNamesUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.MiddleNames = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.LastNameUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.LastName = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.NameSuffixUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.NameSuffix = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.CompanyUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.Company = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.TitleUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.Title = @event.Value;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.FullName = @event.PersonData.FullName;
                person.PreferredName = @event.PersonData.PreferredName;
                person.FirstName = @event.PersonData.FirstName;
                person.LastName = @event.PersonData.LastName;
                person.NameSuffix = @event.PersonData.NameSuffix;
                person.NamePrefix = @event.PersonData.NamePrefix;
                person.MiddleNames = @event.PersonData.MiddleNames;
                person.Company = @event.PersonData.Company;
                person.Title = @event.PersonData.Title;
                person.Email = @event.PersonData.Email;
                person.AlternativeEmail = @event.PersonData.AlternativeEmail;
                person.MobilePhoneNumber = @event.PersonData.MobilePhone;
                person.HomePhoneNumber = @event.PersonData.HomePhone;
                person.WorkPhoneNumber = @event.PersonData.WorkPhone;
                this.UpdatePersonRepeatingFields(person, @event);
                person.LastModifiedTimestamp = @event.Timestamp;
                this.UpdatePersonRepeatingFields(person, @event.PersonData);
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.AssociatedWithCustomerEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.CustomerId = @event.CustomerId;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.UserAccountCreatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null && !person.UserId.HasValue)
            {
                person.UserId = @event.UserId;
                person.UserHasBeenInvitedToActivate = true;
                person.LastModifiedTimestamp = @event.Timestamp;
            }

            if (person != null && string.IsNullOrEmpty(person.Email))
            {
                person.Email = @event.Email;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonDeletedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                if (@event.IsPermanentDelete)
                {
                    person.EmailAddresses.ToList().ForEach(e => this.emailAddressRepository.DeleteById(@event.TenantId, e.Id));
                    person.PhoneNumbers.ToList().ForEach(p => this.phoneNumberRepository.DeleteById(@event.TenantId, p.Id));
                    person.StreetAddresses.ToList().ForEach(s => this.streetAddressRepository.DeleteById(@event.TenantId, s.Id));
                    person.WebsiteAddresses.ToList().ForEach(w => this.websiteAddressRepository.DeleteById(@event.TenantId, w.Id));
                    person.MessengerIds.ToList().ForEach(m => this.messengerIdRepository.DeleteById(@event.TenantId, m.Id));
                    person.SocialMediaIds.ToList().ForEach(s => this.socialMediaIdRepository.DeleteById(@event.TenantId, s.Id));
                    this.personReadModelRepository.DeleteById(@event.TenantId, person.Id);
                }
                else
                {
                    person.IsDeleted = true;
                    person.LastModifiedTimestamp = @event.Timestamp;
                }
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonUndeletedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.IsDeleted = false;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserInitializedEvent @event, int sequenceNumber)
        {
            if (!@event.CustomerId.IsNullOrEmpty())
            {
                var person = this.GetPersonById(@event.TenantId, @event.Person.PersonId);
                person.UserId = @event.AggregateId;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserImportedEvent @event, int sequenceNumber)
        {
            if (!@event.CustomerId.IsNullOrEmpty())
            {
                var person = this.GetPersonById(@event.TenantId, @event.Person.PersonId);
                person.UserId = @event.AggregateId;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.RoleAddedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserTypeUpdatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserBlockedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.UserIsBlocked = true;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserUnblockedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.UserIsBlocked = false;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.ActivationInvitationCreatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.UserHasBeenInvitedToActivate = true;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserActivatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.UserHasBeenActivated = true;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.LoginEmailSetEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.PasswordResetInvitationCreatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.PasswordChangedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.RoleAssignedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.RoleRetractedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(UserAggregate aggregate, UserAggregate.ProfilePictureAssignedToUserEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerInitializedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerModifiedTimeUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.LastModifiedTimestamp = @event.ModifiedTime;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerImportedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonData.PersonId);
            if (person == null)
            {
                if (@event.PersonData != null)
                {
                    var personReadModel = PersonReadModel.CreateFromPersonData(
                        @event.TenantId,
                        @event.PersonData.PersonId,
                        @event.AggregateId,
                        null,
                        @event.PersonData,
                        @event.Timestamp,
                        @event.IsTestData);
                    this.personReadModelRepository.Add(personReadModel);
                }
            }
            else
            {
                person.CustomerId = @event.AggregateId;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.OrganisationId = @event.OrganisationId;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        [Obsolete]
        public void Handle(CustomerAggregate aggregate, CustomerAggregate.UserAssociatedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.AggregateId);
            if (customer != null)
            {
                var person = this.GetPersonById(@event.TenantId, customer.PrimaryPersonId);
                person.UserId = @event.UserId;
                person.UserHasBeenInvitedToActivate = @event.UserHasBeenInvitedToActivate;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerUndeletedEvent @event, int sequenceNumber)
        {
            var customer = this.GetCustomerById(@event.TenantId, @event.CustomerId);
            if (customer != null)
            {
                var person = this.GetPersonById(@event.TenantId, customer.PrimaryPersonId);
                if (person.CustomerId == customer.Id)
                {
                    person.IsDeleted = false;
                    person.LastModifiedTimestamp = @event.Timestamp;
                }
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerSetPrimaryPersonEvent @event, int sequenceNumber)
        {
            // Nothing to do.
            // Assigning a new primary person for a customer only changes the reference of the customer read model.
        }

        public void Handle(UserAggregate aggregate, UserAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.TenantId = @event.TenantId;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.TenantId = @event.TenantId;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.TenantId = @event.TenantId;
            }
        }

        public void Handle(
            PersonAggregate aggregate,
            AdditionalPropertyValueInitializedEvent<PersonAggregate, IPersonEventObserver> @event,
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
            PersonAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<PersonAggregate, IPersonEventObserver> @event,
            int sequenceNumber)
        {
            this.propertyTypeEvaluatorService.UpdateAdditionalPropertyValue(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);

            var person = this.GetPersonById(@event.TenantId, @event.EntityId);
            if (person != null)
            {
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserModifiedTimeUpdatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.LastModifiedTimestamp = @event.ModifiedTime;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.MissingTenantIdAssignedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.TenantId = @event.TenantId;
            }
        }

        public void Handle(CustomerAggregate aggregate, CustomerAggregate.CustomerTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.PersonId);
            if (person != null)
            {
                person.OrganisationId = @event.OrganisationId;
                person.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            if (person != null)
            {
                person.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.UserAssociatedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);
            person.UserId = @event.UserId;
            person.UserHasBeenInvitedToActivate = true;
            person.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonCustomerCommonPropertiesMigratedEvent @event, int sequenceNumber)
        {
            var person = this.GetPersonById(@event.TenantId, @event.AggregateId);

            // Correct the flagging for primary person because every customer has its own primary person
            person.UserId = @event.UserId != person.UserId && !@event.UserId.IsNullOrEmpty()
                ? @event.UserId : person.UserId;

            // Only applies if the current customer property is different from the recorded person property to maintain
            // the integrity of the data
            person.FullName = this.GetLatestStringPropertyValueFromCustomer(@event.FullName, person.FullName);
            person.PreferredName = this.GetLatestStringPropertyValueFromCustomer(@event.PreferredName, person.PreferredName);
            person.NamePrefix = this.GetLatestStringPropertyValueFromCustomer(@event.NamePrefix, person.NamePrefix);
            person.NameSuffix = this.GetLatestStringPropertyValueFromCustomer(@event.NameSuffix, person.NameSuffix);
            person.FirstName = this.GetLatestStringPropertyValueFromCustomer(@event.FirstName, person.FirstName);
            person.LastName = this.GetLatestStringPropertyValueFromCustomer(@event.LastName, person.LastName);
            person.MiddleNames = this.GetLatestStringPropertyValueFromCustomer(@event.MiddleNames, person.MiddleNames);
            person.Company = this.GetLatestStringPropertyValueFromCustomer(@event.Company, person.Company);
            person.Title = this.GetLatestStringPropertyValueFromCustomer(@event.Title, person.Title);
            person.Email = this.GetLatestStringPropertyValueFromCustomer(@event.Email, person.Email);
            person.AlternativeEmail = this.GetLatestStringPropertyValueFromCustomer(@event.AlternativeEmail, person.AlternativeEmail);
            person.MobilePhoneNumber = this.GetLatestStringPropertyValueFromCustomer(@event.MobilePhoneNumber, person.MobilePhoneNumber);
            person.HomePhoneNumber = this.GetLatestStringPropertyValueFromCustomer(@event.HomePhoneNumber, person.HomePhoneNumber);
            person.WorkPhoneNumber = this.GetLatestStringPropertyValueFromCustomer(@event.WorkPhoneNumber, person.WorkPhoneNumber);
            person.UserHasBeenInvitedToActivate = this.GetLatestBooleanPropertyValueFromCustomer(@event.UserHasBeenInvitedToActivate, person.UserHasBeenInvitedToActivate);
            person.UserHasBeenActivated = this.GetLatestBooleanPropertyValueFromCustomer(@event.UserHasBeenActivated, person.UserHasBeenActivated);
            person.UserIsBlocked = this.GetLatestBooleanPropertyValueFromCustomer(@event.UserIsBlocked, person.UserIsBlocked);
        }

        public void Handle(UserAggregate.CustomerIdAndEnvironmentUpdatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        private string GetLatestStringPropertyValueFromCustomer(string customerValue, string personValue)
            => customerValue != personValue && customerValue.IsNotNullOrEmpty() ? customerValue : personValue;

        private bool GetLatestBooleanPropertyValueFromCustomer(bool customerValue, bool personValue)
            => customerValue != personValue ? customerValue : personValue;

        private CustomerReadModel GetCustomerById(Guid tenantId, Guid customerId)
        {
            return this.customerReadModelRepository.Where(tenantId, c => c.Id == customerId).FirstOrDefault();
        }

        private PersonReadModel GetPersonById(Guid tenantId, Guid personId)
        {
            var includes = new List<Expression<Func<PersonReadModel, object>>>
            {
                e => e.EmailAddresses,
                p => p.PhoneNumbers,
                a => a.StreetAddresses,
                w => w.WebsiteAddresses,
                m => m.MessengerIds,
                s => s.SocialMediaIds,
            };

            return this.personReadModelRepository
                .WhereWithIncludes(tenantId, includes, u => u.Id == personId)
                .FirstOrDefault();
        }

        private PersonReadModel UpdatePersonRepeatingFields(
            PersonReadModel person, PersonData contactData)
        {
            // Email addresses
            if (contactData.EmailAddresses != null)
            {
                person = this.UpdateEmailFields(person, contactData.EmailAddresses);
            }

            // Phone numbers
            if (contactData.PhoneNumbers != null)
            {
                person = this.UpdatePhoneNumberFields(person, contactData.PhoneNumbers);
            }

            // Street addresses
            if (contactData.StreetAddresses != null)
            {
                person = this.UpdateStreetAddressFields(person, contactData.StreetAddresses);
            }

            // Website addresses
            if (contactData.WebsiteAddresses != null)
            {
                person = this.UpdateWebsiteAddressFields(person, contactData.WebsiteAddresses);
            }

            // Messenger Ids
            if (contactData.MessengerIds != null)
            {
                person = this.UpdateMessengerIdFields(person, contactData.MessengerIds);
            }

            // Social media Ids
            if (contactData.SocialMediaIds != null)
            {
                person = this.UpdateSocialMediaFields(person, contactData.SocialMediaIds);
            }

            return person;
        }

        private PersonReadModel UpdateEmailFields(
            PersonReadModel person, IEnumerable<EmailAddressField> newEmailAddresses)
        {
            var defaultEmailAddress = person.EmailAddresses?.LastOrDefault(e => e.IsDefault);

            foreach (var oldEmailAddress in person.EmailAddresses)
            {
                this.emailAddressRepository.DeleteById(person.TenantId, oldEmailAddress.Id);
            }

            person.EmailAddresses.Clear();

            foreach (var updatedEmailAddress in newEmailAddresses)
            {
                var newEmailAddressValue = updatedEmailAddress.EmailAddressValueObject.ToString();
                updatedEmailAddress.IsDefault = defaultEmailAddress?.EmailAddress == newEmailAddressValue;
                person.EmailAddresses.Add(new EmailAddressReadModel(person.TenantId, updatedEmailAddress));
            }

            return person;
        }

        private PersonReadModel UpdatePhoneNumberFields(
            PersonReadModel person, IEnumerable<PhoneNumberField> newPhoneNumbers)
        {
            var defaultPhoneNumber = person.PhoneNumbers?.LastOrDefault(p => p.IsDefault);

            foreach (var oldPhoneNumber in person.PhoneNumbers)
            {
                this.phoneNumberRepository.DeleteById(person.TenantId, oldPhoneNumber.Id);
            }

            person.PhoneNumbers.Clear();

            foreach (var updatedPhoneNumber in newPhoneNumbers)
            {
                var newPhoneNumberValue = updatedPhoneNumber.PhoneNumberValueObject.ToString();
                updatedPhoneNumber.IsDefault = defaultPhoneNumber?.PhoneNumber == newPhoneNumberValue;
                person.PhoneNumbers.Add(new PhoneNumberReadModel(person.TenantId, updatedPhoneNumber));
            }

            return person;
        }

        private PersonReadModel UpdateStreetAddressFields(
            PersonReadModel person, IEnumerable<StreetAddressField> newStreetAddresses)
        {
            var defaultStreetAddress = person.StreetAddresses.LastOrDefault(s => s.IsDefault);

            foreach (var oldStreetAddress in person.StreetAddresses)
            {
                this.streetAddressRepository.DeleteById(person.TenantId, oldStreetAddress.Id);
            }

            person.StreetAddresses.Clear();

            foreach (var updatedStreetAddress in newStreetAddresses)
            {
                var newStreetAddressValue = updatedStreetAddress.Address;
                updatedStreetAddress.IsDefault = defaultStreetAddress?.Address == newStreetAddressValue;
                person.StreetAddresses.Add(new StreetAddressReadModel(person.TenantId, updatedStreetAddress));
            }

            return person;
        }

        private PersonReadModel UpdateWebsiteAddressFields(
            PersonReadModel person, IEnumerable<WebsiteAddressField> newWebsiteAddresses)
        {
            var defaultWebsite = person.WebsiteAddresses.LastOrDefault(w => w.IsDefault);

            foreach (var oldWebsiteAddress in person.WebsiteAddresses)
            {
                this.websiteAddressRepository.DeleteById(person.TenantId, oldWebsiteAddress.Id);
            }

            person.WebsiteAddresses.Clear();

            foreach (var updatedWebsiteAddress in newWebsiteAddresses)
            {
                var newWebsiteAddressValue = updatedWebsiteAddress.WebsiteAddressValueObject.ToString();
                updatedWebsiteAddress.IsDefault = defaultWebsite?.WebsiteAddress == newWebsiteAddressValue;
                person.WebsiteAddresses.Add(new WebsiteAddressReadModel(person.TenantId, updatedWebsiteAddress));
            }

            return person;
        }

        private PersonReadModel UpdatePersonRepeatingFields(
            PersonReadModel person, PersonAggregate.PersonUpdatedEvent @event)
        {
            EmailAddressReadModel defaultEmail = null;
            foreach (var oldEmail in person.EmailAddresses.ToList())
            {
                if (oldEmail.IsDefault)
                {
                    defaultEmail = oldEmail;
                }

                this.emailAddressRepository.Delete(@event.TenantId, d => d.Id == oldEmail.Id);
            }

            foreach (var updatedEmail in @event.PersonData.EmailAddresses.ToList())
            {
                updatedEmail.IsDefault = defaultEmail != null
                    && defaultEmail.EmailAddress == updatedEmail.EmailAddressValueObject.ToString();
                person.EmailAddresses.Add(new EmailAddressReadModel(@event.TenantId, updatedEmail));
            }

            PhoneNumberReadModel defaultPhoneNumber = null;
            foreach (var oldPhone in person.PhoneNumbers.ToList())
            {
                if (oldPhone.IsDefault)
                {
                    defaultPhoneNumber = oldPhone;
                }

                this.phoneNumberRepository.Delete(@event.TenantId, d => d.Id == oldPhone.Id);
            }

            foreach (var updatePhone in @event.PersonData.PhoneNumbers.ToList())
            {
                updatePhone.IsDefault = defaultPhoneNumber != null
                    && defaultPhoneNumber.PhoneNumber == updatePhone.PhoneNumberValueObject.ToString();
                person.PhoneNumbers.Add(new PhoneNumberReadModel(@event.TenantId, updatePhone));
            }

            StreetAddressReadModel defaultStreetAddress = null;
            foreach (var oldAddress in person.StreetAddresses.ToList())
            {
                if (oldAddress.IsDefault)
                {
                    defaultStreetAddress = oldAddress;
                }

                this.streetAddressRepository.Delete(@event.TenantId, d => d.Id == oldAddress.Id);
            }

            foreach (var updateAddress in @event.PersonData.StreetAddresses.ToList())
            {
                updateAddress.IsDefault = defaultStreetAddress != null
                    && defaultStreetAddress.Address == updateAddress.Address;
                person.StreetAddresses.Add(new StreetAddressReadModel(@event.TenantId, updateAddress));
            }

            WebsiteAddressReadModel defaultWebsite = null;
            foreach (var oldWebAddress in person.WebsiteAddresses.ToList())
            {
                if (oldWebAddress.IsDefault)
                {
                    defaultWebsite = oldWebAddress;
                }

                this.websiteAddressRepository.Delete(@event.TenantId, d => d.Id == oldWebAddress.Id);
            }

            foreach (var webAddress in @event.PersonData.WebsiteAddresses.ToList())
            {
                webAddress.IsDefault = defaultWebsite != null
                    && defaultWebsite.WebsiteAddress == webAddress.WebsiteAddressValueObject.ToString();
                person.WebsiteAddresses.Add(new WebsiteAddressReadModel(@event.TenantId, webAddress));
            }

            MessengerIdReadModel defaultMessenger = null;
            foreach (var oldMessenger in person.MessengerIds.ToList())
            {
                if (oldMessenger.IsDefault)
                {
                    defaultMessenger = oldMessenger;
                }

                this.messengerIdRepository.Delete(@event.TenantId, d => d.Id == oldMessenger.Id);
            }

            foreach (var messenger in @event.PersonData.MessengerIds.ToList())
            {
                messenger.IsDefault
                    = defaultMessenger != null && defaultMessenger.MessengerId == messenger.MessengerId;
                person.MessengerIds.Add(new MessengerIdReadModel(@event.TenantId, messenger));
            }

            SocialMediaIdReadModel defaultSocialMediaId = null;
            foreach (var oldSocial in person.SocialMediaIds.ToList())
            {
                if (oldSocial.IsDefault)
                {
                    defaultSocialMediaId = oldSocial;
                }

                this.socialMediaIdRepository.Delete(@event.TenantId, d => d.Id == oldSocial.Id);
            }

            foreach (var social in @event.PersonData.SocialMediaIds.ToList())
            {
                social.IsDefault
                    = defaultSocialMediaId != null && defaultSocialMediaId.SocialMediaId == social.SocialMediaId;
                person.SocialMediaIds.Add(new SocialMediaIdReadModel(@event.TenantId, social));
            }

            return person;
        }

        private PersonReadModel UpdateMessengerIdFields(
            PersonReadModel person, IEnumerable<MessengerIdField> newMessengerIds)
        {
            var defaultMessenger = person.MessengerIds.LastOrDefault(m => m.IsDefault);

            foreach (var oldMessengerId in person.MessengerIds)
            {
                this.messengerIdRepository.DeleteById(person.TenantId, oldMessengerId.Id);
            }

            person.MessengerIds.Clear();

            foreach (var updatedMessengerId in newMessengerIds)
            {
                var newMessengerIdValue = updatedMessengerId.MessengerId;
                updatedMessengerId.IsDefault = defaultMessenger?.MessengerId == newMessengerIdValue;
                person.MessengerIds.Add(new MessengerIdReadModel(person.TenantId, updatedMessengerId));
            }

            return person;
        }

        private PersonReadModel UpdateSocialMediaFields(
            PersonReadModel person, IEnumerable<SocialMediaIdField> newSocialMediaIds)
        {
            var defaultSocialMediaId = person.SocialMediaIds.LastOrDefault(s => s.IsDefault);

            foreach (var oldSocialMediaId in person.SocialMediaIds)
            {
                this.socialMediaIdRepository.DeleteById(person.TenantId, oldSocialMediaId.Id);
            }

            person.SocialMediaIds.Clear();

            foreach (var updatedSocialMediaId in newSocialMediaIds)
            {
                var newSocialMediaIdValue = updatedSocialMediaId.SocialMediaId;
                updatedSocialMediaId.IsDefault = defaultSocialMediaId?.SocialMediaId == newSocialMediaIdValue;
                person.SocialMediaIds.Add(new SocialMediaIdReadModel(person.TenantId, updatedSocialMediaId));
            }

            return person;
        }
    }
}
