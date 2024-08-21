// <copyright file="PortalAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel.Portal;

    public partial class PortalAggregate
        : AggregateRootEntity<PortalAggregate, Guid>,
        IAdditionalPropertyValueEntityAggregate,
        IAdditionalProperties,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueInitializedEvent<PortalAggregate,
                IPortalEventObserver>>,
        IApplyAdditionalPropertyValueEvent<AdditionalPropertyValueUpdatedEvent<PortalAggregate, IPortalEventObserver>>
    {
        public const string DefaultAgentPortalName = "Agent Portal";
        public const string DefaultAgentPortalAlias = "agent";
        public const string DefaultCustomerPortalName = "Customer Portal";
        public const string DefaultCustomerPortalAlias = "customer";

        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public PortalAggregate()
        {
        }

        public PortalAggregate(IEnumerable<IEvent<PortalAggregate, Guid>> events)
            : base(events)
        {
        }

        public PortalAggregate(
            Guid tenantId,
            Guid portalId,
            string name,
            string alias,
            string title,
            PortalUserType userType,
            Guid organisationId,
            Guid? performingUserId,
            Instant timestamp)
        {
            var @event = new PortalCreatedEvent(
                tenantId,
                portalId,
                name,
                alias,
                title,
                userType,
                organisationId,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        public Guid TenantId { get; private set; }

        public string Name { get; private set; }

        public string Alias { get; private set; }

        public string Title { get; private set; }

        public PortalUserType UserType { get; private set; }

        public Guid OrganisationId { get; private set; }

        public bool Disabled { get; private set; }

        public bool Deleted { get; private set; }

        public bool IsDefault { get; private set; }

        public string StylesheetUrl { get; private set; }

        public string Styles { get; private set; }

        public string? ProductionUrl { get; private set; }

        public string? StagingUrl { get; private set; }

        public string? DevelopmentUrl { get; private set; }

        public List<PortalSignInMethod> SignInMethods { get; private set; } = new List<PortalSignInMethod>();

        public List<AdditionalPropertyValue> AdditionalPropertyValues { get; private set; } =
            new List<AdditionalPropertyValue>();

        public override AggregateType AggregateType => AggregateType.Portal;

        /// <summary>
        /// Gets or sets the latest projected read model for this portal.
        /// </summary>
        public PortalReadModel ReadModel { get; set; }

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
            var initalizedEvent = new AdditionalPropertyValueInitializedEvent<PortalAggregate, IPortalEventObserver>(
                tenantId,
                this.Id,
                Guid.NewGuid(), // the additional property value id is generated here
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
            var updateEvent = new AdditionalPropertyValueUpdatedEvent<PortalAggregate, IPortalEventObserver>(
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
            AdditionalPropertyValueInitializedEvent<PortalAggregate, IPortalEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.Add(this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueUpdatedEvent<PortalAggregate, IPortalEventObserver> aggregateEvent, int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.AddOrUpdate(
                this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <summary>
        /// Updates the portal properties.
        /// </summary>
        /// <param name="userType">This is temporary, and we won't necessarily allow the user type to be updated in
        /// the long term. Instead you have to delete the portal and create a new one.</param>
        public void Update(
            string name,
            string alias,
            string title,
            Guid? performingUserId,
            Instant timestamp,
            PortalUserType? userType)
        {
            var @event = new PortalUpdatedEvent(
                this.TenantId,
                this.Id,
                name,
                alias,
                title,
                performingUserId,
                timestamp,
                userType);
            this.ApplyNewEvent(@event);
        }

        public void SetLocation(
            DeploymentEnvironment environment,
            Domain.ValueTypes.Url? url,
            Guid? performingUserId,
            Instant timestamp)
        {
            var @event = new SetPortalLocationEvent(
                this.TenantId,
                this.Id,
                environment,
                url?.ToString(),
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        public void UpdateStyles(
            string stylesheetUrl,
            string styles,
            Guid? performingUserId,
            Instant timestamp)
        {
            var @event = new UpdatePortalStylesEvent(
                this.TenantId,
                this.Id,
                stylesheetUrl,
                styles,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        public void Disable(Guid? performingUserId, Instant timestamp)
        {
            var @event = new DisablePortalEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void Enable(Guid? performingUserId, Instant timestamp)
        {
            var @event = new EnablePortalEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void Delete(Guid? performingUserId, Instant timestamp)
        {
            var @event = new DeletePortalEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void SetDefault(bool isDefault, Guid? performingUserId, Instant timestamp)
        {
            var @event = new SetDefaultPortalEvent(this.TenantId, this.Id, isDefault, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void EnableSignInMethod(Guid authenticationMethodId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new PortalSignInMethodEnabledEvent(this.TenantId, this.Id, authenticationMethodId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void DisableSignInMethod(Guid authenticationMethodId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new PortalSignInMethodDisabledEvent(this.TenantId, this.Id, authenticationMethodId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void UpdateSignInMethodSortOrder(Guid authenticationMethodId, int sortOrder, Guid? performingUserId, Instant timestamp)
        {
            var @event = new PortalSignInMethodSortOrderChangedEvent(
                this.TenantId, this.Id, authenticationMethodId, sortOrder, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public override PortalAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<PortalAggregate, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(PortalCreatedEvent @event, int sequenceNumber)
        {
            this.TenantId = @event.TenantId;
            this.Id = @event.AggregateId;
            this.Name = @event.Name;
            this.Alias = @event.Alias;
            this.Title = @event.Title;
            this.UserType = @event.UserType;
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(PortalUpdatedEvent @event, int sequenceNumber)
        {
            this.Name = @event.Name;
            this.Alias = @event.Alias;
            this.Title = @event.Title;
            this.UserType = @event.UserType ?? this.UserType;
        }

        private void Apply(SetPortalLocationEvent @event, int sequenceNumber)
        {
            switch (@event.Environment)
            {
                case DeploymentEnvironment.Production:
                    this.ProductionUrl = @event.Url;
                    break;
                case DeploymentEnvironment.Staging:
                    this.StagingUrl = @event.Url;
                    break;
                case DeploymentEnvironment.Development:
                    this.DevelopmentUrl = @event.Url;
                    break;
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(@event.Environment, typeof(DeploymentEnvironment)));
            }
        }

        private void Apply(UpdatePortalStylesEvent @event, int sequenceNumber)
        {
            this.StylesheetUrl = @event.StylesheetUrl;
            this.Styles = @event.Styles;
        }

        private void Apply(DisablePortalEvent @event, int sequenceNumber)
        {
            this.Disabled = true;
            this.IsDefault = false;
        }

        private void Apply(EnablePortalEvent @event, int sequenceNumber)
        {
            this.Disabled = false;
        }

        private void Apply(DeletePortalEvent @event, int sequenceNumber)
        {
            this.Deleted = true;
        }

        private void Apply(SetDefaultPortalEvent @event, int sequenceNumber)
        {
            this.IsDefault = @event.IsDefault;
        }

        private void Apply(PortalSignInMethodEnabledEvent @event, int sequenceNumber)
        {
            var existingSignInMethod
                = this.SignInMethods.SingleOrDefault(x => x.AuthenticationMethodId == @event.AuthenticationMethodId);
            if (existingSignInMethod != null)
            {
                existingSignInMethod.IsEnabled = true;
            }
            else
            {
                int maxSortOrderValue = this.SignInMethods.Any()
                    ? this.SignInMethods.Max(x => x.SortOrder)
                    : -1;
                this.SignInMethods.Add(new PortalSignInMethod
                {
                    AuthenticationMethodId = @event.AuthenticationMethodId,
                    IsEnabled = true,
                    SortOrder = maxSortOrderValue + 1,
                });
            }
        }

        private void Apply(PortalSignInMethodDisabledEvent @event, int sequenceNumber)
        {
            var existingSignInMethod
                = this.SignInMethods.SingleOrDefault(x => x.AuthenticationMethodId == @event.AuthenticationMethodId);
            if (existingSignInMethod != null)
            {
                existingSignInMethod.IsEnabled = false;
            }
            else
            {
                int maxSortOrderValue = this.SignInMethods.Max(x => x.SortOrder);
                this.SignInMethods.Add(new PortalSignInMethod
                {
                    AuthenticationMethodId = @event.AuthenticationMethodId,
                    IsEnabled = false,
                    SortOrder = maxSortOrderValue + 1,
                });
            }
        }

        private void Apply(PortalSignInMethodSortOrderChangedEvent @event, int sequenceNumber)
        {
            var signInMethodMoved = this.SignInMethods.Single(x => x.AuthenticationMethodId == @event.AuthenticationMethodId);
            signInMethodMoved.SortOrder = @event.SortOrder;
            this.SortAndRenumberSignInMethods(@event.AuthenticationMethodId);
        }

        private void SortAndRenumberSignInMethods(Guid priorityAuthenticationMethodId)
        {
            var sortedMethods = this.SignInMethods.OrderBy(x => x.SortOrder == -1 ? int.MaxValue : x.SortOrder)
                           .ThenBy(x => x.AuthenticationMethodId == priorityAuthenticationMethodId ? 0 : 1)
                           .ToList();

            for (int i = 0; i < sortedMethods.Count; i++)
            {
                sortedMethods[i].SortOrder = i;
            }

            this.SignInMethods = sortedMethods;
        }
    }
}
