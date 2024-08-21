// <copyright file="PersonOrganisationMigratedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for customers.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// Event raised when a person has been modified due to the added organisation Id property.
        /// </summary>
        public class PersonOrganisationMigratedEvent
             : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonOrganisationMigratedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant the person is under.</param>
            /// <param name="organisationId">The ID of the organisation the customer belongs to.</param>
            /// <param name="personId">The ID of the person.</param>
            /// <param name="performingUserId">The userId who performed the migration for the customer.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public PersonOrganisationMigratedEvent(
                Guid tenantId, Guid organisationId, Guid personId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.PersonId = personId;
                this.OrganisationId = organisationId;
            }

            [JsonConstructor]
            public PersonOrganisationMigratedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the customer person.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }

            /// <summary>
            /// Gets the ID of the organisation the customer is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
