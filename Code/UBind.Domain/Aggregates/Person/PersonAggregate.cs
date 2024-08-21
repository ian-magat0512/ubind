// <copyright file="PersonAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Imports;

    /// <summary>
    /// For representing a person and their contact details.
    /// </summary>
    public partial class PersonAggregate :
        AggregateRootEntity<PersonAggregate, Guid>,
        IPersonalDetails,
        IAdditionalPropertyValueEntityAggregate,
        IAdditionalProperties,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueInitializedEvent<PersonAggregate, IPersonEventObserver>>,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueUpdatedEvent<PersonAggregate, IPersonEventObserver>>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public PersonAggregate()
        {
        }

        private PersonAggregate(
            Guid tenantId, Guid id, Guid organisationId, Guid? performingUserId, Instant createdTimestamp, bool isTestData)
        {
            var @event = new PersonInitializedEvent(
                tenantId, id, default, default, null, organisationId, performingUserId, createdTimestamp, isTestData);
            this.ApplyNewEvent(@event);
        }

        private PersonAggregate(
            Guid tenantId,
            IPersonalDetails personDetails,
            Guid organisationId,
            Guid? performingUserId,
            Instant createdTimestamp,
            bool isTestData)
        {
            this.Id = Guid.NewGuid();
            var personData = new PersonData(personDetails, this.Id);
            var @event = new PersonInitializedEvent(
                tenantId, this.Id, default, default, personData, organisationId, performingUserId, createdTimestamp, isTestData);
            this.ApplyNewEvent(@event);
        }

        private PersonAggregate(IEnumerable<IEvent<PersonAggregate, Guid>> events)
            : base(events)
        {
            this.ExtractEmailsAndPhoneNumbersFromRepeatingFields();
        }

        // Among the private constructor, this calls person imported event and not initialised event.
        private PersonAggregate(
            Guid tenantId, Guid organisationId, CustomerImportData data, Guid? performingUserId, Instant createdTimestamp, bool isTestData = false)
        {
            this.Id = Guid.NewGuid();
            var personData = new PersonData(data, this.Id);
            var @event = new PersonImportedEvent(
                tenantId, this.Id, organisationId, personData, performingUserId, createdTimestamp, isTestData);
            this.ApplyNewEvent(@event);
        }

        public override AggregateType AggregateType => AggregateType.Person;

        /// <summary>
        /// Gets the ID of the customer associated to the person.
        /// </summary>
        public Guid? CustomerId { get; private set; }

        /// <summary>
        /// Gets the ID of the user associated to the person.
        /// </summary>
        public Guid? UserId { get; private set; }

        /// <summary>
        /// Gets the guid ID of the tenant this person record belongs to.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the organisation this person record belongs to.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the person's full name.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets a person's first name and last name.
        /// </summary>
        public string DisplayName => PersonPropertyHelper.GetDisplayName(this);

        /// <summary>
        /// Getsthe person's preferred name.
        /// </summary>
        public string PreferredName { get; private set; }

        /// <summary>
        /// Gets the person's primary email address.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the person's alternate email address.
        /// </summary>
        public string AlternativeEmail { get; private set; }

        /// <summary>
        /// Gets the person's mobile phone.
        /// </summary>
        public string MobilePhone { get; private set; }

        /// <summary>
        /// Gets the person's home phone.
        /// </summary>
        public string HomePhone { get; private set; }

        /// <summary>
        /// Gets the person's work phone.
        /// </summary>
        public string WorkPhone { get; private set; }

        /// <summary>
        /// Gets person's name prefix.
        /// </summary>
        public string NamePrefix { get; private set; }

        /// <summary>
        /// Gets  the person's first name.
        /// </summary>
        public string FirstName { get; private set; }

        /// <summary>
        /// Gets the person's middle names.
        /// </summary>
        public string MiddleNames { get; private set; }

        /// <summary>
        /// Gets the person's last name.
        /// </summary>
        public string LastName { get; private set; }

        /// <summary>
        /// Gets the person's name suffix.
        /// </summary>
        public string NameSuffix { get; private set; }

        /// <summary>
        /// Gets the person's company.
        /// </summary>
        public string Company { get; private set; }

        /// <summary>
        /// Gets the person's tile.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the person is deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the greeting name of the person.
        /// </summary>
        public string GreetingName => this.GetGreetingName();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "emailAddresses")]
        public List<EmailAddressField> EmailAddresses { get; set; } = new List<EmailAddressField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "streetAddresses")]
        public List<StreetAddressField> StreetAddresses { get; set; } = new List<StreetAddressField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "phoneNumbers")]
        public List<PhoneNumberField> PhoneNumbers { get; set; } = new List<PhoneNumberField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "messengerIds")]
        public List<MessengerIdField> MessengerIds { get; set; } = new List<MessengerIdField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "websiteAddresses")]
        public List<WebsiteAddressField> WebsiteAddresses { get; set; } = new List<WebsiteAddressField>();

        /// <inheritdoc/>
        [JsonProperty(PropertyName = "socialMediaIds")]
        public List<SocialMediaIdField> SocialMediaIds { get; set; } = new List<SocialMediaIdField>();

        /// <inheritdoc/>
        public string MobilePhoneNumber { get; set; }

        /// <inheritdoc/>
        public string HomePhoneNumber { get; set; }

        /// <inheritdoc/>
        public string WorkPhoneNumber { get; set; }

        /// <inheritdoc/>
        public List<AdditionalPropertyValue> AdditionalPropertyValues { get; private set; }
            = new List<AdditionalPropertyValue>();

        /// <summary>
        /// Static factory method that creates a person aggregate with imported event.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the person belongs to.</param>
        /// <param name="organisationId">The ID of the organisation the person belongs to.</param>
        /// <param name="data">The customer import data object.</param>
        /// <param name="performingUserId">The userId who imported customer.</param>
        /// <param name="createdTimestamp">The time the person aggregate was created.</param>
        /// <returns>Returns a new stand-alone instance of <see cref="PersonAggregate"/>.</returns>
        public static PersonAggregate CreateImportedPerson(
            Guid tenantId, Guid organisationId, CustomerImportData data, Guid? performingUserId, Instant createdTimestamp)
        {
            return new PersonAggregate(tenantId, organisationId, data, performingUserId, createdTimestamp);
        }

        /// <summary>
        /// Static factory method that creates an instance of the <see cref="PersonAggregate"/> class from event store.
        /// </summary>
        /// <param name="events">The existing events.</param>
        /// <returns>A new instance of <see cref="PersonAggregate"/>, loaded from the event store.</returns>
        public static PersonAggregate LoadFromEvents(IEnumerable<IEvent<PersonAggregate, Guid>> events)
        {
            return new PersonAggregate(events);
        }

        /// <summary>
        /// Static factory method that creates an instance of the <see cref="PersonAggregate"/> from personal details.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the person is for.</param>
        /// <param name="organisationId">The ID of the organisation the person is for.</param>
        /// <param name="personDetails">The person details.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="createdTimestamp">The time the person aggregate was created.</param>
        /// <returns>Returns a new stand-alone instance of <see cref="PersonAggregate"/>.</returns>
        public static PersonAggregate CreatePersonFromPersonalDetails(
            Guid tenantId,
            Guid organisationId,
            IPersonalDetails personDetails,
            Guid? performingUserId,
            Instant createdTimestamp,
            bool isTestData = false)
        {
            return new PersonAggregate(tenantId, personDetails, organisationId, performingUserId, createdTimestamp, isTestData);
        }

        /// <summary>
        /// Static factory method that creates an instance of the <see cref="PersonAggregate"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the person belongs to.</param>
        /// <param name="organisationId">The ID of the organisation the person is for.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="createdTimestamp">The time the person aggregate was created.</param>
        /// <returns>Returns a new stand-alone instance of <see cref="PersonAggregate"/>.</returns>
        public static PersonAggregate CreatePerson(
            Guid tenantId, Guid organisationId, Guid? performingUserId, Instant createdTimestamp, bool isTestData = false)
        {
            // This constructor defines null reference to person data.
            // Upon checking all references, all of them are just used in unit test and not in production.
            return new PersonAggregate(tenantId, Guid.NewGuid(), organisationId, performingUserId, createdTimestamp, isTestData);
        }

        /// <summary>
        /// Associates customer with person.
        /// </summary>
        /// <param name="customerId">The customer Id of the person.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="createdTimestamp">The time the person aggregate was created.</param>
        public void AssociateWithCustomer(Guid customerId, Guid? performingUserId, Instant createdTimestamp)
        {
            var @event = new AssociatedWithCustomerEvent(this.TenantId, this.Id, customerId, performingUserId, createdTimestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Associates the user account to a customer person record.
        /// </summary>
        /// <param name="userId">The Id of the user represented as <see cref="Guid"/>.</param>
        /// <param name="performingUserId">The Id of the performing user Id represented as nullable <see cref="Guid"/>.</param>
        /// <param name="timestamp">The time the event was created.</param>
        public void AssociateWithUserAccount(Guid userId, Guid? performingUserId, Instant timestamp)
        {
            if (this.UserId.HasValue)
            {
                throw new ErrorException(
                    Errors.Person.CannotAssociateAUserAccountForAPersonWithExistingUser(
                        this.Id));
            }

            var @event = new UserAssociatedEvent(this.TenantId, this.Id, userId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Records user account created for person.
        /// </summary>
        /// <param name="userId">The userId of the person.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="createdTimestamp">The time the person aggregate was created.</param>
        public void RecordUserAccountCreatedForPerson(Guid userId, Guid? performingUserId, Instant createdTimestamp)
        {
            if (this.UserId.HasValue)
            {
                throw new ErrorException(
                    Errors.Person.CannotCreateAUserAccountForAPersonWithExistingUser(
                        this.UserId.Value));
            }

            var @event = new UserAccountCreatedEvent(this.TenantId, this.Id, userId, this.Email, performingUserId, createdTimestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Updates the person's details from import data.
        /// </summary>
        /// <param name="data">The new personal details.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateWithImportedData(CustomerImportData data, Guid? performingUserId, Instant timestamp)
        {
            var @event = new PersonUpdatedEvent(
                this.TenantId, this.Id, new PersonData(data, this.Id), performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Mark the person aggregate as deleted.
        /// </summary>
        /// <remarks>
        /// It is not advisable to delete an aggregate record, we just mark person as "deleted".
        /// </remarks>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void MarkAsDeleted(Guid? performingUserId, Instant timestamp)
        {
            var @event = new PersonDeletedEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Deletes the person and its associated records.
        /// </summary>
        /// <param name="performingUserId">The performing user Id.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void DeletePersonRecords(Guid? performingUserId, Instant timestamp)
        {
            var @event = new PersonDeletedEvent(this.TenantId, this.Id, performingUserId, timestamp, true);
            this.ApplyNewEvent(@event);
        }

        public void Restore(Guid? performingUserId, Instant timestamp)
        {
            var @event = new PersonUndeletedEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Updates the person's details.
        /// </summary>
        /// <param name="personalDetails">The new personal details.</param>
        /// <param name="performingUserId">The userId who performed update.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void Update(IPersonalDetails personalDetails, Guid? performingUserId, Instant timestamp)
        {
            var personData = new PersonData(personalDetails, this.Id);
            var @event = new PersonUpdatedEvent(this.TenantId, this.Id, personData, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Update the person's email address.
        /// </summary>
        /// <param name="email">The new email address.</param>
        /// <param name="performingUserId">The userId who updated email.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateEmail(string email, Guid? performingUserId, Instant timestamp)
        {
            if (email != this.Email)
            {
                var @event = new EmailAddressUpdatedEvent(this.TenantId, this.Id, email, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Update the person's alternate email address.
        /// </summary>
        /// <param name="alternativeEmail">The new alternate email address.</param>
        /// <param name="performingUserId">The userId who updated alternative email.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateAlternativeEmail(string alternativeEmail, Guid? performingUserId, Instant timestamp)
        {
            if (alternativeEmail != this.AlternativeEmail)
            {
                this.ApplyNewEvent(
                    new AlternativeEmailAddressUpdatedEvent(
                        this.TenantId, this.Id, alternativeEmail, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's full name.
        /// </summary>
        /// <param name="fullName">The new full name.</param>
        /// <param name="performingUserId">The userId who update full name.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <returns>Itself, for fluent syntax.</returns>
        public PersonAggregate UpdateFullName(string fullName, Guid? performingUserId, Instant timestamp)
        {
            if (fullName != this.FullName)
            {
                this.ApplyNewEvent(
                    new FullNameUpdatedEvent(this.TenantId, this.Id, fullName, performingUserId, timestamp));
            }

            return this;
        }

        /// <summary>
        /// Update the person's preferred name.
        /// </summary>
        /// <param name="preferredName">The new preferred name.</param>
        /// <param name="performingUserId">The userId who update prefferred name.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdatePreferredName(string preferredName, Guid? performingUserId, Instant timestamp)
        {
            if (preferredName != this.PreferredName)
            {
                this.ApplyNewEvent(
                    new PreferredNameUpdatedEvent(this.TenantId, this.Id, preferredName, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's mobile phone number.
        /// </summary>
        /// <param name="mobilePhone">The new mobile phone number.</param>
        /// <param name="performingUserId">The userId who updates mobile phone.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateMobilePhone(string mobilePhone, Guid? performingUserId, Instant timestamp)
        {
            if (mobilePhone != this.MobilePhone)
            {
                this.ApplyNewEvent(
                    new MobilePhoneUpdatedEvent(this.TenantId, this.Id, mobilePhone, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's home phone number.
        /// </summary>
        /// <param name="homePhone">The new home phone number.</param>
        /// <param name="performingUserId">The userId who updates home phone.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateHomePhone(string homePhone, Guid? performingUserId, Instant timestamp)
        {
            if (homePhone != this.HomePhone)
            {
                this.ApplyNewEvent(
                    new HomePhoneUpdatedEvent(this.TenantId, this.Id, homePhone, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's work phone number.
        /// </summary>
        /// <param name="workPhone">The new work phone number.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateWorkPhone(string workPhone, Guid? performingUserId, Instant timestamp)
        {
            if (workPhone != this.WorkPhone)
            {
                this.ApplyNewEvent(
                    new WorkPhoneUpdatedEvent(this.TenantId, this.Id, workPhone, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's name prefix.
        /// </summary>
        /// <param name="namePrefix">The name prefix.</param>
        /// <param name="performingUserId">The userId who updates name prefix.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateNamePrefix(string namePrefix, Guid? performingUserId, Instant timestamp)
        {
            if (namePrefix != this.NamePrefix)
            {
                this.ApplyNewEvent(
                    new NamePrefixUpdatedEvent(this.TenantId, this.Id, namePrefix, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's first name.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="performingUserId">The userId who updates first name.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateFirstName(string firstName, Guid? performingUserId, Instant timestamp)
        {
            if (firstName != this.FirstName)
            {
                this.ApplyNewEvent(
                    new FirstNameUpdatedEvent(this.TenantId, this.Id, firstName, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's middle names.
        /// </summary>
        /// <param name="middleNames">The middle name.</param>
        /// <param name="performingUserId">The userId who updates middle name.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateMiddleNames(string middleNames, Guid? performingUserId, Instant timestamp)
        {
            if (middleNames != this.MiddleNames)
            {
                this.ApplyNewEvent(
                    new MiddleNamesUpdatedEvent(this.TenantId, this.Id, middleNames, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's last name.
        /// </summary>
        /// <param name="lastName">The last name.</param>
        /// <param name="performingUserId">The userId who updates last name.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateLastName(string lastName, Guid? performingUserId, Instant timestamp)
        {
            if (lastName != this.LastName)
            {
                this.ApplyNewEvent(
                    new LastNameUpdatedEvent(this.TenantId, this.Id, lastName, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's name suffix.
        /// </summary>
        /// <param name="nameSuffix">The name suffix.</param>
        /// <param name="performingUserId">The userId who update name suffix.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateNameSuffix(string nameSuffix, Guid? performingUserId, Instant timestamp)
        {
            if (nameSuffix != this.NameSuffix)
            {
                this.ApplyNewEvent(
                    new NameSuffixUpdatedEvent(this.TenantId, this.Id, nameSuffix, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's company.
        /// </summary>
        /// <param name="company">The company name.</param>
        /// <param name="performingUserId">The userId who update company.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateCompany(string company, Guid? performingUserId, Instant timestamp)
        {
            if (company != this.Company)
            {
                this.ApplyNewEvent(
                    new CompanyUpdatedEvent(this.TenantId, this.Id, company, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Update the person's title.
        /// </summary>
        /// <param name="title">The person's title.</param>
        /// <param name="performingUserId">The userId who updates title.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdateTitle(string title, Guid? performingUserId, Instant timestamp)
        {
            if (title != this.Title)
            {
                this.ApplyNewEvent(new TitleUpdatedEvent(this.TenantId, this.Id, title, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Trigger the ApplyNewIdEvent that applies new id to this aggregate.
        /// </summary>
        /// <param name="tenantNewId">The tenant new Id.</param>
        /// <param name="performingUserId">The userId who did the action.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void TriggerApplyNewIdEvent(Guid tenantNewId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ApplyNewIdEvent(tenantNewId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record organisation migration and only applicable for an aggregate with an empty organisation Id.
        /// </summary>
        /// <param name="organisationId">The Id of the organisation to persist in this aggregate.</param>
        /// <param name="performingUserId">The user Id who updates the aggregate.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        /// <param name="bypassChecking">Bypass the organisation id checking ( optional ).</param>
        /// <returns>A result indicating success or any error.</returns>
        public Result<Guid, Error> RecordOrganisationMigration(
            Guid organisationId,
            Guid? performingUserId,
            Instant timestamp,
            bool bypassChecking = false)
        {
            if (this.OrganisationId != Guid.Empty && bypassChecking == false)
            {
                return Result.Failure<Guid, Error>(
                    Errors.Organisation.FailedToMigrateForOrganisation(this.Id, this.OrganisationId));
            }

            var @event = new PersonOrganisationMigratedEvent(
                this.TenantId, organisationId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);

            return Result.Success<Guid, Error>(@event.AggregateId);
        }

        public void TransferToAnotherOrganisation(
            Guid tenantId, Guid organisationId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new PersonTransferredToAnotherOrganisationEvent(
                tenantId, organisationId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Trigger the ApplyNewIdEvent that applies new id to this aggregate.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="performingUserId">The userId who did the action.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignMissingTenantId(Guid tenantId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new MissingTenantIdAssignedEvent(tenantId, this.Id, performingUserId, timestamp);
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
            var initializedEvent
                = new AdditionalPropertyValueInitializedEvent<PersonAggregate, IPersonEventObserver>(
                    tenantId,
                    this.Id,
                    Guid.NewGuid(),
                    additionalPropertyDefinitionId,
                    entityId,
                    value,
                    propertyType,
                    performingUserId,
                    createdTimestamp);
            this.ApplyNewEvent(initializedEvent);
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
            var updateEvent = new AdditionalPropertyValueUpdatedEvent<PersonAggregate, IPersonEventObserver>(
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

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueInitializedEvent<PersonAggregate, IPersonEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.Add(this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueUpdatedEvent<PersonAggregate, IPersonEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.AddOrUpdate(this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <inheritdoc/>
        public void SetEmail(string email)
        {
            this.Email = email;
        }

        /// <inheritdoc/>
        public void SetAlternativeEmail(string email)
        {
            this.AlternativeEmail = email;
        }

        /// <inheritdoc/>
        public void SetEmailIfNull()
        {
            PersonDetailExtension.SetEmailIfNull(this);
        }

        public override PersonAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<PersonAggregate, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(MissingTenantIdAssignedEvent @event, int sequenceNumber)
        {
            this.TenantId = @event.TenantId;
        }

        private void Apply(PersonInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            if (this.TenantId == Guid.Empty)
            {
                this.TenantId = @event.TenantId;
            }

            this.OrganisationId = @event.OrganisationId;
            if (@event.PersonData != null)
            {
                this.FullName = @event.PersonData.FullName;
                this.FirstName = @event.PersonData.FirstName;
                this.LastName = @event.PersonData.LastName;
                this.PreferredName = @event.PersonData.PreferredName;
                this.Email = @event.PersonData.Email;
                this.AlternativeEmail = @event.PersonData.AlternativeEmail;
                this.MobilePhone = @event.PersonData.MobilePhone;
                this.HomePhone = @event.PersonData.HomePhone;
                this.WorkPhone = @event.PersonData.WorkPhone;
                this.NameSuffix = @event.PersonData.NameSuffix;
                this.NamePrefix = @event.PersonData.NamePrefix;
                this.MiddleNames = @event.PersonData.MiddleNames;
                this.Company = @event.PersonData.Company;
                this.Title = @event.PersonData.Title;
                this.EmailAddresses = @event.PersonData.EmailAddresses;
                this.PhoneNumbers = @event.PersonData.PhoneNumbers;
                this.StreetAddresses = @event.PersonData.StreetAddresses;
                this.WebsiteAddresses = @event.PersonData.WebsiteAddresses;
                this.MessengerIds = @event.PersonData.MessengerIds;
                this.SocialMediaIds = @event.PersonData.SocialMediaIds;
            }
        }

        private void Apply(PersonImportedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.TenantId = @event.TenantId;
            this.OrganisationId = @event.OrganisationId;
            this.FullName = @event.FullName;
            this.PreferredName = @event.PreferredName;
            this.Email = @event.Email;
            this.AlternativeEmail = @event.AlternativeEmail;
            this.MobilePhone = @event.MobilePhone;
            this.HomePhone = @event.HomePhone;
            this.WorkPhone = @event.WorkPhone;

            if (@event.PersonData != null)
            {
                this.FullName = @event.PersonData.FullName;
                this.FirstName = @event.PersonData.FirstName;
                this.LastName = @event.PersonData.LastName;
                this.PreferredName = @event.PersonData.PreferredName;
                this.Email = @event.PersonData.Email;
                this.AlternativeEmail = @event.PersonData.AlternativeEmail;
                this.MobilePhone = @event.PersonData.MobilePhone;
                this.HomePhone = @event.PersonData.HomePhone;
                this.WorkPhone = @event.PersonData.WorkPhone;
                this.NameSuffix = @event.PersonData.NameSuffix;
                this.NamePrefix = @event.PersonData.NamePrefix;
                this.MiddleNames = @event.PersonData.MiddleNames;
                this.Company = @event.PersonData.Company;
                this.Title = @event.PersonData.Title;
                this.EmailAddresses = @event.PersonData.EmailAddresses;
                this.PhoneNumbers = @event.PersonData.PhoneNumbers;
                this.StreetAddresses = @event.PersonData.StreetAddresses;
                this.WebsiteAddresses = @event.PersonData.WebsiteAddresses;
                this.MessengerIds = @event.PersonData.MessengerIds;
                this.SocialMediaIds = @event.PersonData.SocialMediaIds;
            }
        }

        private void Apply(PersonMigratedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.TenantId = @event.TenantId;
            this.OrganisationId = @event.OrganisationId;
            this.FullName = @event.Name;
            this.HomePhone = @event.PhoneNumber;
            this.MobilePhone = @event.MobileNumber;
        }

        private void Apply(FullNameUpdatedEvent @event, int sequenceNumber)
        {
            this.FullName = @event.Value;
        }

        private void Apply(PreferredNameUpdatedEvent @event, int sequenceNumber)
        {
            this.PreferredName = @event.Value;
        }

        private void Apply(EmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            this.Email = @event.Value;
        }

        private void Apply(AlternativeEmailAddressUpdatedEvent @event, int sequenceNumber)
        {
            this.AlternativeEmail = @event.Value;
        }

        private void Apply(MobilePhoneUpdatedEvent @event, int sequenceNumber)
        {
            this.MobilePhone = @event.Value;
        }

        private void Apply(WorkPhoneUpdatedEvent @event, int sequenceNumber)
        {
            this.WorkPhone = @event.Value;
        }

        private void Apply(HomePhoneUpdatedEvent @event, int sequenceNumber)
        {
            this.HomePhone = @event.Value;
        }

        private void Apply(NamePrefixUpdatedEvent @event, int sequenceNumber)
        {
            this.NamePrefix = @event.Value;
        }

        private void Apply(FirstNameUpdatedEvent @event, int sequenceNumber)
        {
            this.FirstName = @event.Value;
        }

        private void Apply(MiddleNamesUpdatedEvent @event, int sequenceNumber)
        {
            this.MiddleNames = @event.Value;
        }

        private void Apply(LastNameUpdatedEvent @event, int sequenceNumber)
        {
            this.LastName = @event.Value;
        }

        private void Apply(NameSuffixUpdatedEvent @event, int sequenceNumber)
        {
            this.NameSuffix = @event.Value;
        }

        private void Apply(CompanyUpdatedEvent @event, int sequenceNumber)
        {
            this.Company = @event.Value;
        }

        private void Apply(TitleUpdatedEvent @event, int sequenceNumber)
        {
            this.Title = @event.Value;
        }

        private void Apply(PersonUpdatedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            if (this.TenantId == Guid.Empty)
            {
                this.TenantId =
                    @event.TenantId == default
                    ? @event.PersonData.TenantId
                    : @event.TenantId;
            }

            this.FullName = @event.PersonData.FullName;
            this.FirstName = @event.PersonData.FirstName;
            this.LastName = @event.PersonData.LastName;
            this.PreferredName = @event.PersonData.PreferredName;
            this.Email = @event.PersonData.Email;
            this.AlternativeEmail = @event.PersonData.AlternativeEmail;
            this.MobilePhone = @event.PersonData.MobilePhone;
            this.HomePhone = @event.PersonData.HomePhone;
            this.WorkPhone = @event.PersonData.WorkPhone;
            this.NameSuffix = @event.PersonData.NameSuffix;
            this.NamePrefix = @event.PersonData.NamePrefix;
            this.MiddleNames = @event.PersonData.MiddleNames;
            this.Company = @event.PersonData.Company;
            this.Title = @event.PersonData.Title;
            this.EmailAddresses = @event.PersonData.EmailAddresses;
            this.PhoneNumbers = @event.PersonData.PhoneNumbers;
            this.StreetAddresses = @event.PersonData.StreetAddresses;
            this.WebsiteAddresses = @event.PersonData.WebsiteAddresses;
            this.MessengerIds = @event.PersonData.MessengerIds;
            this.SocialMediaIds = @event.PersonData.SocialMediaIds;
        }

        private void Apply(AssociatedWithCustomerEvent @event, int sequenceNumber)
        {
            this.CustomerId = @event.CustomerId;
        }

        private void Apply(UserAccountCreatedEvent @event, int sequenceNumber)
        {
            if (!this.UserId.HasValue)
            {
                this.UserId = @event.UserId;
            }

            if (!string.IsNullOrEmpty(@event.Email))
            {
                this.Email = @event.Email;
            }
        }

        private void Apply(PersonDeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = true;
        }

        private void Apply(PersonUndeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = false;
        }

        private void Apply(UserAssociatedEvent @event, int sequenceNumber)
        {
            this.UserId = @event.UserId;
        }

        private void Apply(PersonCustomerCommonPropertiesMigratedEvent @event, int sequenceNumber)
        {
            this.UserId = @event.UserId != this.UserId && !@event.UserId.IsNullOrEmpty() ? @event.UserId : this.UserId;

            // Only applies if the current customer property is different from the recorded person property to maintain
            // the integrity of the data
            this.FullName = this.GetLatestStringPropertyValueFromCustomer(@event.FullName, this.FullName);
            this.PreferredName = this.GetLatestStringPropertyValueFromCustomer(@event.PreferredName, this.PreferredName);
            this.NamePrefix = this.GetLatestStringPropertyValueFromCustomer(@event.NamePrefix, this.NamePrefix);
            this.NameSuffix = this.GetLatestStringPropertyValueFromCustomer(@event.NameSuffix, this.NameSuffix);
            this.FirstName = this.GetLatestStringPropertyValueFromCustomer(@event.FirstName, this.FirstName);
            this.LastName = this.GetLatestStringPropertyValueFromCustomer(@event.LastName, this.LastName);
            this.MiddleNames = this.GetLatestStringPropertyValueFromCustomer(@event.MiddleNames, this.MiddleNames);
            this.Company = this.GetLatestStringPropertyValueFromCustomer(@event.Company, this.Company);
            this.Title = this.GetLatestStringPropertyValueFromCustomer(@event.Title, this.Title);
            this.Email = this.GetLatestStringPropertyValueFromCustomer(@event.Email, this.Email);
            this.AlternativeEmail = this.GetLatestStringPropertyValueFromCustomer(@event.AlternativeEmail, this.AlternativeEmail);
            this.MobilePhoneNumber = this.GetLatestStringPropertyValueFromCustomer(@event.MobilePhoneNumber, this.MobilePhoneNumber);
            this.HomePhoneNumber = this.GetLatestStringPropertyValueFromCustomer(@event.HomePhoneNumber, this.HomePhoneNumber);
            this.WorkPhoneNumber = this.GetLatestStringPropertyValueFromCustomer(@event.WorkPhoneNumber, this.WorkPhoneNumber);
        }

        private string GetLatestStringPropertyValueFromCustomer(string customerValue, string personValue)
            => customerValue != personValue && customerValue.IsNotNullOrEmpty() ? customerValue : personValue;

        private void ExtractEmailsAndPhoneNumbersFromRepeatingFields()
        {
            this.SetEmailIfNull();
            var phoneFields = this.PhoneNumbers?.ToList();
            if (phoneFields.Any())
            {
                var workPhone = phoneFields.Where(p => p.Label == "work").FirstOrDefault();
                var mobilePhone = phoneFields.Where(p => p.Label == "mobile").FirstOrDefault();
                var homePhone = phoneFields.Where(p => p.Label == "home").FirstOrDefault();

                if (workPhone != null && this.WorkPhone == null)
                {
                    this.WorkPhone = workPhone.PhoneNumberValueObject?.ToString();
                }

                if (mobilePhone != null && this.MobilePhone == null)
                {
                    this.MobilePhone = mobilePhone.PhoneNumberValueObject?.ToString();
                }

                if (homePhone != null && this.HomePhone == null)
                {
                    this.HomePhone = homePhone.PhoneNumberValueObject?.ToString();
                }
            }
        }

        private void Apply(ApplyNewIdEvent @event, int sequenceNumber)
        {
            this.TenantId = @event.TenantId;
        }

        private void Apply(PersonOrganisationMigratedEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(PersonTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }
    }
}
