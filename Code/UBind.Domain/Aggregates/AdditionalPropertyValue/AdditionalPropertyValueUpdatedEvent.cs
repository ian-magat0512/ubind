// <copyright file="AdditionalPropertyValueUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// Generic update event for additional property value which is being raised by different aggregates.
    /// </summary>
    /// <typeparam name="TAggregate">Aggregate object.</typeparam>
    /// <typeparam name="TEventObserver">Aggregate event.</typeparam>
    public class AdditionalPropertyValueUpdatedEvent<TAggregate, TEventObserver>
        : Event<TAggregate, Guid>,
        IAdditionalPropertyValueEventDetails
        where TEventObserver
        : IAggregateEventObserver<
            TAggregate,
            AdditionalPropertyValueUpdatedEvent<TAggregate, TEventObserver>>,
        IAggregateEventObserver<TAggregate, IEvent<TAggregate, Guid>>
        where TAggregate :
            IApplyAdditionalPropertyValueEvent<AdditionalPropertyValueUpdatedEvent<
                TAggregate, TEventObserver>>
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AdditionalPropertyValueUpdatedEvent{TAggregate, TEventObserver}"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="aggregateId">The ID of the aggregate (e.g. quote/claim aggregate etc)
        /// that this additional property value is set against.</param>
        /// <param name="value">Set value from an edit form.</param>
        /// <param name="performingUserId">ID of the performing user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        /// <param name="type"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="additionalPropertyDefinitionId">ID of the additional property definition.</param>
        /// <param name="entityId">ID of an associated entity.</param>
        public AdditionalPropertyValueUpdatedEvent(
            Guid tenantId,
            Guid aggregateId,
            string value,
            Guid? performingUserId,
            Instant createdTimestamp,
            AdditionalPropertyDefinitionType type,
            Guid additionalPropertyDefinitionId,
            Guid additionalPropertyValueId,
            Guid entityId)
            : base(tenantId, aggregateId, performingUserId, createdTimestamp)
        {
            this.Value = value;
            this.AdditionalPropertyDefinitionType = type;
            this.AdditionalPropertyDefinitionId = additionalPropertyDefinitionId;
            this.EntityId = entityId;
            this.AdditionalPropertyValueId = additionalPropertyValueId;
        }

        [JsonConstructor]
        public AdditionalPropertyValueUpdatedEvent()
        {
        }

        /// <summary>
        /// Gets the value of a set value from edit form.
        /// </summary>
        [JsonProperty]
        public string Value { get; private set; }

        /// <summary>
        /// Gets the value of <see cref="AdditionalPropertyDefinitionType"/>.
        /// </summary>
        [JsonProperty]
        public AdditionalPropertyDefinitionType AdditionalPropertyDefinitionType { get; private set; }

        /// <summary>
        /// Gets the value of an ID of additional property definition.
        /// </summary>
        [JsonProperty]
        public Guid AdditionalPropertyDefinitionId { get; private set; }

        [JsonProperty]
        public Guid AdditionalPropertyValueId { get; private set; }

        /// <summary>
        /// Gets the value of an associated entity ID.
        /// </summary>
        [JsonProperty]
        public Guid EntityId { get; private set; }
    }
}
