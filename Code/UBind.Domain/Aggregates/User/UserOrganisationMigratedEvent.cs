// <copyright file="UserOrganisationMigratedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        /// <summary>
        /// Event raised when a user has been modified due to the added organisation Id property.
        /// </summary>
        public partial class UserOrganisationMigratedEvent
            : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserOrganisationMigratedEvent"/> class.
            /// </summary>
            /// <param name="organisationId">The ID of the organisation the user belongs to.</param>
            /// <param name="personId">The ID of the person.</param>
            /// <param name="userId">The ID of the user.</param>
            /// <param name="performingUserId">The userId who performed the migration for the user.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public UserOrganisationMigratedEvent(
                Guid tenantId, Guid userId, Guid organisationId, Guid personId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
                this.PersonId = personId;
            }

            [JsonConstructor]
            private UserOrganisationMigratedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the user person.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }

            /// <summary>
            /// Gets the ID of the organisation the user is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
