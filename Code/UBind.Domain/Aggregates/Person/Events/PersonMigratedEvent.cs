// <copyright file="PersonMigratedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for people.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// The person has been imported from customer details.
        /// Note: This is not used anymore, kept for backward compatibility.
        /// </summary>
        public class PersonMigratedEvent : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonMigratedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant the person belongs to.</param>
            /// /// <param name="aggregateId">The aggregate Id.</param>
            /// <param name="organisationId">The organisation Id the person belongs to.</param>
            /// <param name="performingUserId">The userId who migrated from customer.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public PersonMigratedEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid organisationId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
            }

            [JsonConstructor]
            private PersonMigratedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the organisation the person belongs to.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            /// <summary>
            /// Gets the person's name if specified, otherwise null.
            /// </summary>
            [JsonProperty]
            public string Name { get; private set; }

            /// <summary>
            /// Gets the person's phone number if specified, otherwise null.
            /// </summary>
            [JsonProperty]
            public string PhoneNumber { get; private set; }

            /// <summary>
            /// Gets the person's email address if specified, otherwise null.
            /// </summary>
            [JsonProperty]
            public string Email { get; private set; }

            /// <summary>
            /// Gets the person's mobile number if specified, otherwise null.
            /// </summary>
            [JsonProperty]
            public string MobileNumber { get; private set; }
        }
    }
}
