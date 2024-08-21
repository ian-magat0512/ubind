// <copyright file="AdditionalPropertyValueInitializedEvent.cs" company="uBind">
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
    /// Generic initialize event which can be used by aggregate implementing
    /// IApplyAdditionalPropertyValueInitializedEvent.
    /// </summary>
    /// <typeparam name="TAggregate">Aggregate entity.</typeparam>
    /// <typeparam name="TEventObserver">Event observer.</typeparam>
    public class AdditionalPropertyValueInitializedEvent<TAggregate, TEventObserver>
        : Event<TAggregate, Guid>,
        IAdditionalPropertyValueEventDetails
        where TEventObserver
        : IAggregateEventObserver<
            TAggregate,
            AdditionalPropertyValueInitializedEvent<TAggregate, TEventObserver>>,
            IAggregateEventObserver<TAggregate, IEvent<TAggregate, Guid>>
        where TAggregate :
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueInitializedEvent<TAggregate, TEventObserver>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyValueInitializedEvent{TAggregate, TEventObserver}"/> class.
        /// Creates an instance of this class.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="aggregateId">The ID of the aggregate (e.g. quote/claim aggregate etc)
        /// that this additional property value is set against.</param>
        /// <param name="additionalPropertyValueId">The ID of the additional property value.</param>
        /// <param name="additionalPropertyDefinitionId">The ID of additional property definition.</param>
        /// <param name="entityId">The ID where it is associated to.</param>
        /// <param name="defaultValue">The default value to be mapped.</param>
        /// <param name="additionalPropertyDefinitionType"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="performingUserId">The userId who created the quote.</param>
        /// <param name="createdTimestamp">A created timestamp.</param>
        public AdditionalPropertyValueInitializedEvent(
            Guid tenantId,
            Guid aggregateId, // this has to be the quote/claim aggregate ID
            Guid additionalPropertyValueId,
            Guid additionalPropertyDefinitionId,
            Guid entityId,
            string defaultValue,
            AdditionalPropertyDefinitionType additionalPropertyDefinitionType,
            Guid? performingUserId,
            Instant createdTimestamp)
            : base(tenantId, aggregateId, performingUserId, createdTimestamp)
        {
            this.AdditionalPropertyDefinitionId = additionalPropertyDefinitionId;
            this.EntityId = entityId;
            this.Value = defaultValue;
            this.AdditionalPropertyDefinitionType = additionalPropertyDefinitionType;
            this.AdditionalPropertyValueId = additionalPropertyValueId;
        }

        [JsonConstructor]
        private AdditionalPropertyValueInitializedEvent()
        {
        }

        /// <summary>
        /// Gets the value of the ID of additional property definition.
        /// </summary>
        [JsonProperty]
        public Guid AdditionalPropertyDefinitionId { get; private set; }

        /// <summary>
        /// Gets the value of the ID where this is associated to.
        /// </summary>
        [JsonProperty]
        public Guid EntityId { get; private set; }

        /// <summary>
        /// Gets the value to be persisted.
        /// </summary>
        [JsonProperty]
        public string Value { get; private set; }

        [JsonProperty]
        public Guid AdditionalPropertyValueId { get; private set; }

        /// <summary>
        /// Gets the type of the value of the property.
        /// </summary>
        [JsonProperty]
        public AdditionalPropertyDefinitionType AdditionalPropertyDefinitionType { get; private set; }
    }
}
