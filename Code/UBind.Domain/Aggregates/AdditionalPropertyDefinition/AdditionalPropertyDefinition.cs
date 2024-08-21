// <copyright file="AdditionalPropertyDefinition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// Suppress IDE0060 because there are Apply Event method in which parameter are not in used.
// And we cannot remove the apply method otherwise it will throw an exception.
#pragma warning disable IDE0060 // Remove unused parameter

namespace UBind.Domain.Aggregates.AdditionalPropertyDefinition
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// The class that contains aggregate operations and objects for additional property definition.
    /// </summary>
    public partial class AdditionalPropertyDefinition : AggregateRootEntity<AdditionalPropertyDefinition, Guid>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public AdditionalPropertyDefinition()
        {
        }

        private AdditionalPropertyDefinition(IEnumerable<IEvent<AdditionalPropertyDefinition, Guid>> events)
             : base(events)
        {
        }

        private AdditionalPropertyDefinition(
            Guid tenantId,
            string alias,
            string name,
            AdditionalPropertyEntityType entityType,
            AdditionalPropertyDefinitionContextType contextType,
            bool isRequired,
            bool isUnique,
            Guid contextId,
            Guid? parentContextId,
            AdditionalPropertyDefinitionType propertyType,
            string defaultValue,
            Guid? performingUserId,
            Instant localTime,
            AdditionalPropertyDefinitionSchemaType? schemaType,
            string? customSchema)
        {
            if (propertyType != AdditionalPropertyDefinitionType.StructuredData
                && schemaType != null)
            {
                throw new ArgumentException("You cannot specify a schema type for property types of non-StructuredData");
            }

            var @event = new AdditionalPropertyDefinitionInitializedEvent(
                                tenantId,
                                Guid.NewGuid(),
                                contextId,
                                alias,
                                name,
                                entityType,
                                contextType,
                                isRequired,
                                isUnique,
                                parentContextId,
                                propertyType,
                                defaultValue,
                                performingUserId,
                                localTime,
                                schemaType,
                                customSchema);

            this.ApplyNewEvent(@event);
        }

        public override AggregateType AggregateType => AggregateType.AdditionalPropertyDefinition;

        /// <summary>
        /// Gets the tenantId of the property.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the alias of the property.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the entity type <see cref="AdditionalPropertyEntityType"/> of the property.
        /// </summary>
        public AdditionalPropertyEntityType EntityType { get; private set; }

        /// <summary>
        /// Gets the context type <see cref="AdditionalPropertyDefinitionContextType"/> of the property.
        /// </summary>
        public AdditionalPropertyDefinitionContextType ContextType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the the property is required.
        /// </summary>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the property is unique.
        /// </summary>
        public bool IsUnique { get; private set; }

        /// <summary>
        /// Gets the ID of the context where the property is under.
        /// </summary>
        public Guid ContextId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the property is soft deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the ID of the parent context which the immediate context is under.
        /// </summary>
        public Guid? ParentContextId { get; private set; }

        /// <summary>
        /// Gets type of property.
        /// </summary>
        public AdditionalPropertyDefinitionType PropertyType { get; private set; }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public string DefaultValue { get; private set; }

        public AdditionalPropertyDefinitionSchemaType? SchemaType { get; private set; }

        public string? CustomSchema { get; private set; }

        public static AdditionalPropertyDefinition CreateForText(
             Guid tenantId,
             string alias,
             string name,
             AdditionalPropertyEntityType entityType,
             AdditionalPropertyDefinitionContextType contextType,
             bool isRequired,
             bool isUnique,
             Guid contextId,
             Guid? parentContextId,
             string defaultValue,
             Guid? performingUserId,
             Instant localTime)
        {
            return new AdditionalPropertyDefinition(
                tenantId,
                alias,
                name,
                entityType,
                contextType,
                isRequired,
                isUnique,
                contextId,
                parentContextId,
                AdditionalPropertyDefinitionType.Text,
                defaultValue,
                performingUserId,
                localTime,
                null,
                null);
        }

        public static AdditionalPropertyDefinition CreateForStructedData(
             Guid tenantId,
             string alias,
             string name,
             AdditionalPropertyEntityType entityType,
             AdditionalPropertyDefinitionContextType contextType,
             bool isRequired,
             bool isUnique,
             Guid contextId,
             Guid? parentContextId,
             string defaultValue,
             Guid? performingUserId,
             Instant localTime,
             AdditionalPropertyDefinitionSchemaType schemaType,
             string? customSchema)
        {
            if (schemaType == AdditionalPropertyDefinitionSchemaType.Custom
                && string.IsNullOrEmpty(customSchema))
            {
                throw new ArgumentNullException();
            }

            return new AdditionalPropertyDefinition(
                tenantId,
                alias,
                name,
                entityType,
                contextType,
                isRequired,
                isUnique,
                contextId,
                parentContextId,
                AdditionalPropertyDefinitionType.StructuredData,
                defaultValue,
                performingUserId,
                localTime,
                schemaType,
                customSchema);
        }

        /// <summary>
        /// Static method in creating an instance of additional property <see cref="AdditionalPropertyDefinition"/>.
        /// </summary>
        /// <param name="events">List of events.</param>
        /// <returns>Instance of additional property.</returns>
        public static AdditionalPropertyDefinition LoadFromEvents(IEnumerable<IEvent<AdditionalPropertyDefinition, Guid>> events)
        {
            return new AdditionalPropertyDefinition(events);
        }

        /// <summary>
        /// Sends update event for additional property definition.
        /// </summary>
        /// <param name="id">primary key of additional property definition.</param>
        /// <param name="additionalPropertyDetails">The details that contains the updates.</param>
        /// <param name="performingUserid">Performing user id.</param>
        /// <param name="instantTime">Current timestamp.</param>
        public void Update(
            Guid id,
            IAdditionalPropertyDefinitionDetails additionalPropertyDetails,
            Guid? performingUserid,
            Instant instantTime)
        {
            this.ApplyNewEvent(new AdditionalPropertyDetailsUpdatedEvent(
                this.TenantId, id, additionalPropertyDetails, performingUserid, instantTime));
        }

        /// <summary>
        /// Sets the mark to be deleted.
        /// </summary>
        /// <param name="id">Primary key value.</param>
        /// <param name="performingUserId">Performing User id.</param>
        /// <param name="instantTime">Current timestamp.</param>
        public void MarkAsDeleted(Guid id, Guid? performingUserId, Instant instantTime)
        {
            this.ApplyNewEvent(new AdditionalPropertyMarkAsDeletedEvent(this.TenantId, id, performingUserId, instantTime));
        }

        public override AdditionalPropertyDefinition ApplyEventsAfterSnapshot(IEnumerable<IEvent<AdditionalPropertyDefinition, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(AdditionalPropertyMarkAsDeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = @event.Value;
        }

        private void Apply(AdditionalPropertyDetailsUpdatedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.Alias = @event.Value.Alias;
            this.Name = @event.Value.Name;
            this.IsRequired = @event.Value.IsRequired;
            this.IsUnique = @event.Value.IsUnique;
            this.DefaultValue = @event.Value.DefaultValue;
            this.PropertyType = @event.Value.Type;
            this.SchemaType = @event.Value.SchemaType;
            this.CustomSchema = @event.Value.CustomSchema;
        }

        private void Apply(AdditionalPropertyDefinitionInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.TenantId = @event.TenantId;
            this.Name = @event.Name;
            this.IsRequired = @event.IsRequired;
            this.IsUnique = @event.IsUnique;
            this.Alias = @event.Alias;
            this.ContextType = @event.ContextType;
            this.CreatedTimestamp = @event.Timestamp;
            this.EntityType = @event.EntityType;
            this.ContextId = @event.ContextId;
            this.ParentContextId = @event.ParentContextId;
            this.DefaultValue = @event.DefaultValue;
            this.PropertyType = @event.PropertyType;
            this.SchemaType = @event.SchemaType;
            this.CustomSchema = @event.CustomSchema;
        }
    }
}
