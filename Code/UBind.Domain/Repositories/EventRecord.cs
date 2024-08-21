// <copyright file="EventRecord.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Helpers;

    /// <summary>
    /// For persisting aggregate events.
    /// </summary>
    /// <typeparam name="TId">The type of the ID of the aggregate the event belongs to.</typeparam>
    public class EventRecord<TId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventRecord{TId}"/> class.
        /// </summary>
        /// <param name="tenantId">The id of the tenant this event record is for.</param>
        /// <param name="aggregateId">The ID of the aggregate the event belongs to.</param>
        /// <param name="sequence">The sequence number for ordering the event in the aggregate.</param>
        /// <param name="eventJson">The event serialized as JSON.</param>
        /// <param name="aggregateType">The aggregates type.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        public EventRecord(
            Guid tenantId,
            TId aggregateId,
            int sequence,
            string eventJson,
            AggregateType aggregateType,
            Instant createdTimestamp)
        {
            this.AggregateId = aggregateId;
            this.AggregateType = aggregateType;
            this.Sequence = sequence;
            this.TenantId = tenantId;
            this.EventJson = eventJson;
            this.TicksSinceEpoch = createdTimestamp.ToUnixTimeTicks();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRecord{TId}"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        protected EventRecord()
        {
        }

        /// <summary>
        /// Gets the ID of the aggregate the event belongs to.
        /// </summary>
        /// <remarks>Forms composite key with <see cref="Sequence"/>.</remarks>
        [Key]
        [Column(Order = 0)]
        public TId AggregateId { get; private set; }

        /// <summary>
        /// Gets the sequence number for ordering the event in the aggregate.
        /// </summary>
        /// <remarks>Forms composite key with <see cref="AggregateId"/>.</remarks>
        [Key]
        [Column(Order = 1)]
        public int Sequence { get; private set; }

        /// <summary>
        /// Gets the record created timestamp.
        /// </summary>
        public Instant Timestamp => Instant.FromUnixTimeTicks(this.TicksSinceEpoch);

        /// <summary>
        /// Gets or sets serialized event for persisting.
        /// </summary>
        public string EventJson { get; set; }

        /// <summary>
        /// Gets or sets the timestamp in ticks since the epoch.
        /// </summary>
        public long TicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the ID of the tenant this event is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the type of the aggregate.
        /// </summary>
        public AggregateType AggregateType { get; private set; }

        /// <summary>
        /// Serializer settings for including type information.
        /// </summary>
        public static JsonSerializerSettings SerializerSettings()
        {
            var jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                SerializationBinder = new EventSourcedAggregateStringToTypeBinder(),
            };

            jsonSettings.Converters.Add(new NodaLocalDateConverter());
            jsonSettings.Converters.Add(new NodaLocalDateTimeConverter());
            jsonSettings.Converters.Add(new InstantConverter());
            return jsonSettings;
        }

        /// <summary>
        /// Gets the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to deserialize.</typeparam>
        /// <typeparam name="TAggregate">The type of the root entity of the aggregate the event belongs to.</typeparam>
        /// <returns>The deserialized event.</returns>
        public TEvent GetEvent<TEvent, TAggregate>()
            where TEvent : IEvent<TAggregate, TId>
        {
            TEvent @event = JsonConvert.DeserializeObject<TEvent>(this.EventJson, CustomSerializerSetting.AggregateEventSerializerSettings);
            @event.TenantId = this.TenantId;
            @event.Timestamp = this.Timestamp;
            @event.AggregateId = this.AggregateId;
            @event.Sequence = this.Sequence;
            return @event;
        }
    }
}
