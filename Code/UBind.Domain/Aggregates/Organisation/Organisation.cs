// <copyright file="Organisation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// Suppress IDE0060 because there are Apply Event method in which parameter are not in used.
// And we cannot remove the apply method otherwise it will throw an exception.
#pragma warning disable IDE0060 // Remove unused parameter

namespace UBind.Domain.Aggregates.Organisation
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Humanizer;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Organisation.AuthenticationMethod;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;

    /// <summary>
    /// The aggregate for organisations.
    /// </summary>
    public partial class Organisation
        : AggregateRootEntity<Organisation, Guid>, IOrganisationDetails,
        IAdditionalProperties,
        IAdditionalPropertyValueEntityAggregate,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueInitializedEvent<Organisation, IOrganisationEventObserver>>,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueUpdatedEvent<Organisation, IOrganisationEventObserver>>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public Organisation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Organisation"/> class.
        /// </summary>
        /// <param name="events">Existing events.</param>
        private Organisation(IEnumerable<IEvent<Organisation, Guid>> events)
            : base(events)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Organisation"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation. </param>
        /// <param name="alias">The alias of the organisation.</param>
        /// <param name="name">The name of the organisation.</param>
        /// <param name="isActive">The value indicating whether the organisation is active or disabled.</param>
        /// <param name="isDeleted">The value indicating whether the organisation is marked as deleted.</param>
        /// <param name="performingUserId">The userId who initialized organization.</param>
        /// <param name="createdTimestamp">Represents an instant on the global timeline, with nanosecond resolution.</param>
        private Organisation(
            Guid tenantId,
            string alias,
            string name,
            Guid? managingOrganisationId,
            bool isActive,
            bool isDeleted,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            var @event = new OrganisationInitializedEvent(
                tenantId, Guid.NewGuid(), alias, name, managingOrganisationId, isActive, isDeleted, performingUserId, createdTimestamp);
            this.ApplyNewEvent(@event);
        }

        public override AggregateType AggregateType => AggregateType.Organisation;

        /// <summary>
        /// Gets the tenant Id of this organisation.
        /// </summary>
        public Guid TenantId { get; private set; }

        public Guid? DefaultPortalId { get; private set; }

        public Guid? ManagingOrganisationId { get; private set; }

        /// <summary>
        /// Gets the alias of the organisation.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets the name of the organisation.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the organisation is active or disabled.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the organisation is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        public bool IsDefault { get; private set; }

        /// <inheritdoc/>
        public List<AdditionalPropertyValue> AdditionalPropertyValues { get; private set; } = new List<AdditionalPropertyValue>();

        public IList<IAuthenticationMethod> AuthenticationMethods { get; private set; } = new List<IAuthenticationMethod>();

        public IList<LinkedIdentity> LinkedIdentities { get; private set; } = new List<LinkedIdentity>();

        public OrganisationReadModel? LatestProjectedReadModel { get; set; }

        public AuthenticationMethodReadModelSummary? LatestProjectedAuthenticationMethodReadModel { get; set; }

        public static Organisation CreateNewOrganisation(
            Guid tenantId, string alias, string name, Guid? managingOrganisationId, Guid? performingUserId, Instant createdTimestamp)
        {
            return new Organisation(tenantId, alias, name, managingOrganisationId, true, false, performingUserId, createdTimestamp);
        }

        /// <summary>
        /// Initializes static members of the <see cref="Organisation"/> class.
        /// </summary>
        /// <param name="events">Existing events.</param>
        /// <returns>The new instance of the organisation aggregate.</returns>
        public static Organisation LoadFromEvents(IEnumerable<IEvent<Organisation, Guid>> events)
        {
            return new Organisation(events);
        }

        /// <summary>
        /// Throws an exception if the tenant is null.
        /// </summary>
        /// <param name="organisation">The organisation.</param>
        /// <param name="organisationId">The ID of the organisation.</param>
        public static void ThrowIfNotFound(Organisation organisation, Guid organisationId)
        {
            if (organisation == null)
            {
                throw new ErrorException(Errors.General.NotFound("organisation", organisationId));
            }
        }

        public void Update(string alias, string name, Guid? performingUserId, Instant timestamp)
        {
            this.UpdateName(name, performingUserId, timestamp);
            this.UpdateAlias(alias, performingUserId, timestamp);
        }

        /// <summary>
        /// Set the organisation status as active.
        /// </summary>
        /// <param name="performingUserId">The userId who activates organization.</param>
        /// <param name="timestamp">Represents an instant on the global timeline, with nanosecond resolution.</param>
        public void Activate(Guid? performingUserId, Instant timestamp)
        {
            if (!this.IsActive)
            {
                this.ApplyNewEvent(new OrganisationActivatedEvent(this.TenantId, this.Id, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Set the organisation status as disabled.
        /// </summary>
        /// <param name="performingUserId">The userId who disables organisation.</param>
        /// <param name="timestamp">Represents an instant on the global timeline, with nanosecond resolution.</param>
        /// <remarks>
        /// This operation should only be applied for active and non-default organisations.
        /// </remarks>
        public void Disable(Guid? performingUserId, Instant timestamp)
        {
            if (this.IsActive)
            {
                this.ApplyNewEvent(new OrganisationDisabledEvent(this.TenantId, this.Id, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Mark the organisation as deleted.
        /// </summary>
        /// <param name="performingUserId">The userId who marked as deleted.</param>
        /// <param name="timestamp">Represents an instant on the global timeline, with nanosecond resolution.</param>
        /// <remarks>
        /// This operation should only be applied for active and non-default organisations.
        /// </remarks>
        public void MarkAsDeleted(Guid? performingUserId, Instant timestamp)
        {
            if (!this.IsDeleted)
            {
                this.ApplyNewEvent(new OrganisationDeletedEvent(this.TenantId, this.Id, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Updates the organisation tenant.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the organisation.</param>
        /// <param name="performingUserId">the userId who update tenant.</param>
        /// <param name="timestamp">Represents an instant on the global timeline, with nanosecond resolution.</param>
        public void UpdateTenant(Guid tenantId, Guid? performingUserId, Instant timestamp)
        {
            if (this.TenantId != tenantId)
            {
                this.ApplyNewEvent(new OrganisationTenantUpdatedEvent(tenantId, this.Id, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Updates the organisation name.
        /// </summary>
        /// <param name="name">The new name of the organisation.</param>
        /// <param name="performingUserId">The userId who update organisation name.</param>
        /// <param name="timestamp">Represents an instant on the global timeline, with nanosecond resolution.</param>
        public void UpdateName(string name, Guid? performingUserId, Instant timestamp)
        {
            if (this.Name != name)
            {
                this.ApplyNewEvent(new OrganisationNameUpdatedEvent(this.TenantId, this.Id, name, performingUserId, timestamp));
            }
        }

        /// <summary>
        /// Updates the organisation alias.
        /// </summary>
        /// <param name="alias">The new alias of the organisation.</param>
        /// <param name="performingUserId">The userId who update organization alias.</param>
        /// <param name="timestamp">Represents an instant on the global timeline, with nanosecond resolution.</param>
        public void UpdateAlias(string alias, Guid? performingUserId, Instant timestamp)
        {
            if (this.Alias != alias)
            {
                this.ApplyNewEvent(new OrganisationAliasUpdatedEvent(this.TenantId, this.Id, alias, performingUserId, timestamp));
            }
        }

        public void SetDefault(bool isDefault, Guid? performingUserId, Instant timestamp)
        {
            if (this.IsDefault != isDefault)
            {
                var @event = new SetDefaultOrganisationEvent(this.TenantId, this.Id, isDefault, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        public void SetManagingOrganisation(Guid? managingOrganisationId, Guid? performingUserId, Instant timestamp)
        {
            if (this.ManagingOrganisationId != managingOrganisationId)
            {
                var @event = new ManagingOrganisationUpdatedEvent(this.TenantId, this.Id, managingOrganisationId, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        public void SetDefaultPortal(Guid? portalId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new SetOrganisationDefaultPortalEvent(this.TenantId, this.Id, portalId, performingUserId, timestamp);
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
            var @event = new ApplyNewIdEvent(tenantNewId, this.Id, performingUserId, timestamp);
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
                new AdditionalPropertyValueInitializedEvent<Organisation, IOrganisationEventObserver>(
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
        public void Apply(
            AdditionalPropertyValueInitializedEvent<Organisation, IOrganisationEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.Add(this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueUpdatedEvent<Organisation, IOrganisationEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.AddOrUpdate(this.AdditionalPropertyValues, aggregateEvent);
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
            var updateEvent = new AdditionalPropertyValueUpdatedEvent<Organisation, IOrganisationEventObserver>(
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

        public void AddAuthenticationMethod(AuthenticationMethodBase authenticationMethod, Guid? performingUserId, Instant timestamp)
        {
            string localAccountTypeName = AuthenticationMethodType.LocalAccount.Humanize();
            if (authenticationMethod.TypeName == localAccountTypeName
                && this.AuthenticationMethods.Any(a => a.TypeName == localAccountTypeName))
            {
                // this organisation already has a local account auth setting. You can only have one.
                throw new ErrorException(Errors.Organisation.AlreadyHasALocalAccountAuthMethod(this.Name));
            }

            var @event = new OrganisationAuthenticationMethodAddedEvent(this.TenantId, this.Id, authenticationMethod, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void UpdateAuthenticationMethod(AuthenticationMethodBase authenticationMethod, Guid? performingUserId, Instant timestamp)
        {
            var itemToUpdate = this.AuthenticationMethods.FirstOrDefault(a => a.Id == authenticationMethod.Id);
            authenticationMethod.SetCreatedTimestamp(itemToUpdate.CreatedTimestamp);
            authenticationMethod.LastModifiedTimestamp = timestamp;
            var @event = new OrganisationAuthenticationMethodUpdatedEvent(this.TenantId, this.Id, authenticationMethod, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void DeleteAuthenticationMethod(Guid authenticationMethodId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new OrganisationAuthenticationMethodDeletedEvent(this.TenantId, this.Id, authenticationMethodId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void DisableAuthenticationMethod(Guid authenticationMethodId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new OrganisationAuthenticationMethodDisabledEvent(this.TenantId, this.Id, authenticationMethodId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void EnableAuthenticationMethod(Guid authenticationMethodId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new OrganisationAuthenticationMethodEnabledEvent(this.TenantId, this.Id, authenticationMethodId, performingUserId, timestamp);
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
                    Errors.Organisation.LinkedIdentityProviderAlreadyExists(this.Name, authenticationMethodId));
            }

            var @event = new OrganisationIdentityLinkedEvent(
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
            var @event = new OrganisationIdentityUnlinkedEvent(
                this.TenantId,
                this.Id,
                authenticationMethodId,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        public void UpdateLinkedIdentity(Guid authenticationMethodId, string uniqueId, Guid? performingUserId, Instant timestamp)
        {
            var existingIdentity
                = this.LinkedIdentities.FirstOrDefault(i => i.AuthenticationMethodId == authenticationMethodId);
            if (existingIdentity == null)
            {
                throw new ErrorException(
                    Errors.Organisation.LinkedIdentityProviderDoesNotExistWhenUpdating(this.Name, authenticationMethodId));
            }

            var @event = new OrganisationLinkedIdentityUpdatedEvent(
                this.TenantId,
                this.Id,
                authenticationMethodId,
                uniqueId,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        public override Organisation ApplyEventsAfterSnapshot(IEnumerable<IEvent<Organisation, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(OrganisationInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.TenantId = @event.TenantId;
            this.Alias = @event.Alias;
            this.Name = @event.Name;
            this.ManagingOrganisationId = @event.ManagingOrganisationId;
            this.IsActive = @event.IsActive;
            this.IsDeleted = @event.IsDeleted;
            this.CreatedTimestamp = @event.Timestamp;
        }

        private void Apply(OrganisationNameUpdatedEvent @event, int sequenceNumber)
        {
            this.Name = @event.Value;
        }

        private void Apply(OrganisationAliasUpdatedEvent @event, int sequenceNumber)
        {
            this.Alias = @event.Value;
        }

        private void Apply(OrganisationDeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = true;
            this.IsActive = false;
        }

        private void Apply(OrganisationActivatedEvent @event, int sequenceNumber)
        {
            this.IsActive = true;
        }

        private void Apply(OrganisationDisabledEvent @event, int sequenceNumber)
        {
            this.IsActive = false;
        }

        private void Apply(OrganisationTenantUpdatedEvent @event, int sequenceNumber)
        {
            this.TenantId = @event.TenantId;
        }

        private void Apply(ApplyNewIdEvent applyNewIdEvent, int sequenceNumber)
        {
            this.TenantId = applyNewIdEvent.TenantId;
        }

        private void Apply(SetOrganisationDefaultPortalEvent @event, int sequenceNumber)
        {
            this.DefaultPortalId = @event.PortalId;
        }

        private void Apply(SetDefaultOrganisationEvent @event, int sequenceNumber)
        {
            this.IsDefault = @event.IsDefault;
        }

        private void Apply(ManagingOrganisationUpdatedEvent @event, int sequenceNumber)
        {
            this.ManagingOrganisationId = @event.ManagingOrganisationId;
        }

        private void Apply(OrganisationAuthenticationMethodAddedEvent @event, int sequenceNumber)
        {
            this.AuthenticationMethods.Add(@event.AuthenticationMethod);
        }

        private void Apply(OrganisationAuthenticationMethodUpdatedEvent @event, int sequenceNumber)
        {
            var itemToUpdate = this.AuthenticationMethods.FirstOrDefault(a => a.Id == @event.AuthenticationMethod.Id);
            this.AuthenticationMethods.Remove(itemToUpdate);
            this.AuthenticationMethods.Add(@event.AuthenticationMethod);
        }

        private void Apply(OrganisationAuthenticationMethodDeletedEvent @event, int sequenceNumber)
        {
            var itemToRemove = this.AuthenticationMethods.FirstOrDefault(a => a.Id == @event.AuthenticationMethodId);
            this.AuthenticationMethods.Remove(itemToRemove);
        }

        private void Apply(OrganisationAuthenticationMethodDisabledEvent @event, int sequenceNumber)
        {
            var itemToDisable = this.AuthenticationMethods.FirstOrDefault(a => a.Id == @event.AuthenticationMethodId);
            itemToDisable.Disabled = true;
        }

        private void Apply(OrganisationAuthenticationMethodEnabledEvent @event, int sequenceNumber)
        {
            var itemToEnable = this.AuthenticationMethods.FirstOrDefault(a => a.Id == @event.AuthenticationMethodId);
            itemToEnable.Disabled = false;
        }

        private void Apply(OrganisationIdentityLinkedEvent @event, int sequenceNumber)
        {
            // add the new identity
            this.LinkedIdentities.Add(new LinkedIdentity
            {
                AuthenticationMethodId = @event.AuthenticationMethodId,
                UniqueId = @event.UniqueId,
            });
        }

        private void Apply(OrganisationIdentityUnlinkedEvent @event, int sequenceNumber)
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

        private void Apply(OrganisationLinkedIdentityUpdatedEvent @event, int sequenceNumber)
        {
            // add the new identity
            var existingIdentity
                = this.LinkedIdentities.FirstOrDefault(i => i.AuthenticationMethodId == @event.AuthenticationMethodId);
            existingIdentity.UniqueId = @event.UniqueId;
        }
    }
}
