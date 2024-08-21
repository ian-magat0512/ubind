// <copyright file="StructuredDataAdditionalPropertyValue.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using NodaTime;

    /// <summary>
    /// This aggregate is used to store additional properties against an entity which is NOT an event sourced aggregate.
    /// Event source aggregates do not need their own additional propery aggregate, because it can store the additional
    /// property values as events against the entity aggregate.
    /// </summary>
    public partial class StructuredDataAdditionalPropertyValue : AggregateRootEntity<StructuredDataAdditionalPropertyValue, Guid>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public StructuredDataAdditionalPropertyValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredDataAdditionalPropertyValue"/> class.
        /// </summary>
        /// <param name="additionalPropertyId">Id of the additional property definition which the value had been taken from.</param>
        /// <param name="entityId">The primary key id of the record that owns this value.</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="value">The value taken from the default value of the additional property.</param>
        /// <param name="localTime">When this value is created.</param>
        public StructuredDataAdditionalPropertyValue(
            Guid tenantId,
            Guid additionalPropertyId,
            string value,
            Guid entityId,
            Instant localTime)
        {
            var @event = new StructuredDataAdditionalPropertyValueInitializedEvent(
                tenantId, Guid.NewGuid(), additionalPropertyId, entityId, value, localTime);
            this.ApplyNewEvent(@event);
        }

        private StructuredDataAdditionalPropertyValue(IEnumerable<IEvent<StructuredDataAdditionalPropertyValue, Guid>> events)
           : base(events)
        {
        }

        public override AggregateType AggregateType => AggregateType.StructuredDataAdditionalPropertyValue;

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the id of the additional property definition which the value had been taken from.
        /// </summary>
        public Guid AdditionalPropertyId { get; private set; }

        /// <summary>
        /// Gets the primary key id of the record that owns this value.
        /// </summary>
        public Guid EntityId { get; private set; }

        /// <summary>
        /// Gets the value taken from the default value of the additional property.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Static method in creating an instance of <see cref="StructuredDataAdditionalPropertyValue"/>.
        /// </summary>
        /// <param name="events">List of events.</param>
        /// <returns>Instance of <see cref="StructuredDataAdditionalPropertyValue"/>.</returns>
        public static StructuredDataAdditionalPropertyValue LoadFromEvents(
            IEnumerable<IEvent<StructuredDataAdditionalPropertyValue, Guid>> events)
        {
            return new StructuredDataAdditionalPropertyValue(events);
        }

        /// <summary>
        /// Updates the value of the structured data additional property value.
        /// </summary>
        /// <param name="id">Primery ID.</param>
        /// <param name="value">The udpated value.</param>
        /// <param name="localTime">The time that the update occurs.</param>
        public void UpdateValue(Guid id, string value, Instant localTime)
        {
            var @event = new StructuredDataAdditionalPropertyUpdateValueEvent(this.TenantId, id, value, localTime);
            this.ApplyNewEvent(@event);
        }

        public override StructuredDataAdditionalPropertyValue ApplyEventsAfterSnapshot(
            IEnumerable<IEvent<StructuredDataAdditionalPropertyValue, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(StructuredDataAdditionalPropertyValueInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.TenantId = @event.TenantId;
            this.AdditionalPropertyId = @event.AdditionalPropertyDefinitionId;
            this.EntityId = @event.EntityId;
            this.Value = @event.Value;
        }

        private void Apply(StructuredDataAdditionalPropertyUpdateValueEvent @event, int sequenceNumber)
        {
            this.Value = @event.Value;
        }
    }
}
