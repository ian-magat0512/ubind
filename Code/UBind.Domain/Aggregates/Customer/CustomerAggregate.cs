// <copyright file="CustomerAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Aggregate for customers.
    /// </summary>
    public partial class CustomerAggregate
        : AggregateRootEntity<CustomerAggregate, Guid>,
        IAdditionalPropertyValueEntityAggregate,
        IAdditionalProperties,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueInitializedEvent<CustomerAggregate, ICustomerEventObserver>>,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueUpdatedEvent<CustomerAggregate, ICustomerEventObserver>>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public CustomerAggregate()
        {
        }

        private CustomerAggregate(
            Guid tenantId,
            PersonAggregate person,
            DeploymentEnvironment environment,
            Guid? performingUserId,
            Guid? portalId,
            Instant createdTimestamp,
            Guid? ownerUserId = null,
            bool isTestData = false)
            : this(tenantId, Guid.NewGuid(), person, environment, performingUserId, portalId, createdTimestamp, ownerUserId, isTestData)
        {
        }

        private CustomerAggregate(
            Guid tenantId,
            Guid personId,
            PersonAggregate person,
            DeploymentEnvironment environment,
            Guid? performingUserId,
            Guid? portalId,
            Instant createdTimestamp,
            Guid? ownerUserId = null,
            bool isTestData = false)
        {
            var initializedEvent = new CustomerInitializedEvent(
                tenantId,
                personId,
                new PersonData(person),
                environment,
                performingUserId,
                portalId,
                createdTimestamp,
                ownerUserId,
                isTestData);
            this.ApplyNewEvent(initializedEvent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerAggregate"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        private CustomerAggregate(IEnumerable<IEvent<CustomerAggregate, Guid>> events)
            : base(events)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerAggregate"/> class.
        /// </summary>
        /// <param name="data">The person data to use for import event.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="performingUserId">The user id of the one who perform the initialization.</param>
        /// <param name="portalId">The ID of the portal the customer would be expected to login to,
        /// if they end up having a user account.</param>
        /// <param name="createdTimestamp">The created timestamp.</param>
        /// <param name="isTestData">A value indicating whether to return a test data.</param>
        private CustomerAggregate(
            Guid tenantId,
            PersonData data,
            DeploymentEnvironment environment,
            Guid? performingUserId,
            Guid? portalId,
            Instant createdTimestamp,
            bool isTestData = false)
        {
            var importedEvent = new CustomerImportedEvent(
                tenantId, data, environment, performingUserId, portalId, createdTimestamp, isTestData);
            this.ApplyNewEvent(importedEvent);
        }

        public override AggregateType AggregateType => AggregateType.Customer;

        /// <summary>
        /// Gets the ID of the tenant the customer belongs to.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the organisation the customer belongs to.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the environment the customer is in.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the ID of the primary person for this customer.
        /// </summary>
        public Guid PrimaryPersonId { get; private set; }

        /// <summary>
        /// Gets the user ID of the current owner, if any, otherwise null.
        /// </summary>
        public Guid? OwnerUserId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the customer aggregate is deleted.
        /// </summary>
        /// <remarks>
        /// Currently, this property is being used to identify whether the customer aggregate is orphaned from embedded
        /// quotes.
        /// </remarks>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the ID of the portal which the customer would log into by default,
        /// If there is no specific portal required for a given product.
        /// This would be null if the customer doesn't log into a portal, or the customer
        /// is expected to login to the default portal for the tenanacy.
        /// This needed for the generation of links in emails, e.g. the user activation link.
        /// </summary>
        public Guid? PortalId { get; private set; }

        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the current collection of additional property values.
        /// </summary>
        public List<AdditionalPropertyValue> AdditionalPropertyValues { get; private set; }
            = new List<AdditionalPropertyValue>();

        /// <summary>
        /// Initializes static members of the <see cref="CustomerAggregate"/> class.
        /// </summary>
        /// <param name="person">The person the customer represents.</param>
        /// <param name="environment">The environment the customer is associated with.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="portalId">The ID of the portal the customer would be expected to login to,
        /// if they end up having a user account.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        /// <param name="isTestData">A value indicating whether to return a test data.</param>
        /// <returns>The new instance of the active customer aggregate.</returns>
        public static CustomerAggregate CreateNewCustomer(
            Guid tenantId,
            PersonAggregate person,
            DeploymentEnvironment environment,
            Guid? performingUserId,
            Guid? portalId,
            Instant createdTimestamp,
            bool isTestData = false,
            Guid? ownerUserId = null)
        {
            return new CustomerAggregate(
                tenantId, person, environment, performingUserId, portalId, createdTimestamp, ownerUserId, isTestData);
        }

        /// <summary>
        /// Static factory method that creates a customer with imported event.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="personAggregate">The person aggregate instance.</param>
        /// <param name="environment">The data environment the customer is associated with.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="portalId">The ID of the portal the customer would be expected to login to,
        /// if they end up having a user account.</param>
        /// <param name="createdTimestamp">The created timestamp.</param>
        /// <returns>Returns a new stand-alone instance of customer aggregate.</returns>
        public static CustomerAggregate CreateImportedCustomer(
            Guid tenantId,
            PersonAggregate personAggregate,
            DeploymentEnvironment environment,
            Guid? performingUserId,
            Guid? portalId,
            Instant createdTimestamp)
        {
            return new CustomerAggregate(
                tenantId, new PersonData(personAggregate), environment, portalId, performingUserId, createdTimestamp);
        }

        /// <summary>
        /// Initializes static members of the <see cref="CustomerAggregate"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        /// <returns>The new instnace of the customer aggregate.</returns>
        public static CustomerAggregate LoadFromEvents(IEnumerable<IEvent<CustomerAggregate, Guid>> events)
        {
            return new CustomerAggregate(events);
        }

        /// <summary>
        /// Assign ownership of the customer to a given user.
        /// </summary>
        /// <param name="userId">The user ID of the new owner.</param>
        /// <param name="person">The person who is the new owner.</param>
        /// <param name="performingUserId">The userId who assign to new owner.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignOwnership(Guid userId, PersonAggregate person, Guid? performingUserId, Instant timestamp)
        {
            var @event = new OwnershipAssignedEvent(
                this.TenantId, this.Id, userId, person.Id, person.FullName, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void UnassignOwnership(Guid performingUserId, Instant timestamp)
        {
            var @event = new OwnershipUnassignedEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void ChangePortal(Guid? portalId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new PortalChangedEvent(this.TenantId, this.Id, portalId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Mark the customer aggregate as deleted.
        /// </summary>
        /// <remarks>
        /// It is not advisable to delete a customer aggregate record, we just mark customers as "deleted".
        /// </remarks>
        /// <param name="performingUserId">The userId who mark as delete.</param>
        /// <param name="timestamp">Timestamp when the customer read model is deleted.</param>
        public void MarkAsDeleted(Guid? performingUserId, Instant timestamp)
        {
            var @event = new CustomerDeletedEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Deletes the customer and its associated records.
        /// </summary>
        /// <param name="performingUserId">The performing user Id.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void DeleteCustomerRecords(Guid? performingUserId, Instant timestamp)
        {
            var @event = new CustomerDeletedEvent(this.TenantId, this.Id, performingUserId, timestamp, true);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Restore a deleted customer.
        /// </summary>
        /// <param name="performingUserId">The userId who restored the customer.</param>
        /// <param name="timestamp">Timestamp when the customer read model was restored.</param>
        public void MarkAsUndeleted(Guid? performingUserId, Instant timestamp)
        {
            var @event = new CustomerUndeletedEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Trigger the ApplyNewIdEvent that applies new id to this aggregate.
        /// </summary>
        /// <param name="tenantNewId">The tenant new Id.</param>
        /// <param name="performingUserId">The userId who did the action.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void TriggerApplyNewIdEvent(Guid tenantNewId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ApplyNewIdEvent(tenantNewId, this.Id, this.PrimaryPersonId, performingUserId, timestamp);
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

            var @event = new CustomerOrganisationMigratedEvent(
                this.TenantId, organisationId, this.PrimaryPersonId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);

            return Result.Success<Guid, Error>(@event.AggregateId);
        }

        /// <summary>
        /// Rolls back the aggregate to its previous modified time if the latest is from organisation migration.
        /// </summary>
        /// <param name="modifiedTime">The modified time of the customer.</param>
        /// <param name="performingUserId">The user Id who updates the aggregate.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        public void RollbackDateModifiedBeforeOrganisationMigration(
            Instant modifiedTime, Guid? performingUserId, Instant timestamp)
        {
            var @event = new CustomerModifiedTimeUpdatedEvent(
                this.TenantId, this.Id, this.PrimaryPersonId, modifiedTime, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Set primary person for customer.
        /// </summary>
        /// <param name="personId">The person ID to be set as primary for the customer.</param>
        /// <param name="performingUserId">The userId who sets the person as primary.</param>
        /// <param name="timestamp">Timestamp when the customer set a person as primary.</param>
        public void SetPrimaryPerson(Guid personId, Guid? performingUserId, Instant timestamp)
        {
            var @event
                = new CustomerSetPrimaryPersonEvent(this.TenantId, this.Id, personId, performingUserId, timestamp);
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
            var initializedEvent =
                new AdditionalPropertyValueInitializedEvent<CustomerAggregate, ICustomerEventObserver>(
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
            var updateEvent = new AdditionalPropertyValueUpdatedEvent<CustomerAggregate, ICustomerEventObserver>(
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
            AdditionalPropertyValueUpdatedEvent<CustomerAggregate, ICustomerEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.AddOrUpdate(this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueInitializedEvent<CustomerAggregate, ICustomerEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.Add(this.AdditionalPropertyValues, aggregateEvent);
        }

        public void TransferToAnotherOrganisation(Guid organisationId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new CustomerTransferredToAnotherOrganisationEvent(
                this.TenantId, organisationId, this.Id, this.PrimaryPersonId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public override CustomerAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<CustomerAggregate, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(CustomerInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.PrimaryPersonId = @event.PersonData.PersonId;
            this.TenantId = @event.TenantId == default ? @event.PersonData.TenantId : @event.TenantId;
            this.OrganisationId = @event.PersonData.OrganisationId;
            this.PortalId = @event.PortalId;
            this.Environment = @event.Environment;
            this.CreatedTimestamp = @event.Timestamp;
            this.DisplayName = @event.PersonData.DisplayName;
            this.OwnerUserId = @event.OwnerUserId;
        }

        private void Apply(CustomerImportedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.PrimaryPersonId = @event.PersonData.PersonId;
            this.TenantId = @event.TenantId;
            this.OrganisationId = @event.PersonData.OrganisationId;
            this.Environment = @event.Environment;
            this.CreatedTimestamp = @event.Timestamp;
            this.PortalId = @event.PortalId;
            this.DisplayName = @event.PersonData.DisplayName;
        }

        private void Apply(OwnershipAssignedEvent @event, int sequenceNumber)
        {
            this.OwnerUserId = @event.UserId;
        }

        private void Apply(OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            this.OwnerUserId = null;
        }

        [Obsolete]
        private void Apply(UserAssociatedEvent @event, int sequenceNumber)
        {
            // Nop
        }

        private void Apply(CustomerDeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = true;
        }

        private void Apply(CustomerUndeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = false;
        }

        private void Apply(ApplyNewIdEvent @event, int sequenceNumber)
        {
            this.TenantId = @event.TenantId;
        }

        private void Apply(CustomerOrganisationMigratedEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(CustomerModifiedTimeUpdatedEvent @event, int sequenceNumber)
        {
            // Nothing to do
        }

        private void Apply(CustomerTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(PortalChangedEvent @event, int sequenceNumber)
        {
            this.PortalId = @event.Value;
        }

        private void Apply(CustomerSetPrimaryPersonEvent @event, int sequenceNumber)
        {
            this.PrimaryPersonId = @event.Value;
        }
    }
}
