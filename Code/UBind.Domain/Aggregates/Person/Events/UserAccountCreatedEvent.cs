// <copyright file="UserAccountCreatedEvent.cs" company="uBind">
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
    /// Aggregate for people.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// The user has been created for person.
        /// </summary>
        public class UserAccountCreatedEvent : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserAccountCreatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant the person is for.</param>
            /// <param name="personId">A unique ID for the Person.</param>
            /// <param name="userId">The ID of the user.</param>
            /// <param name="email">The email of the user.</param>
            /// <param name="performingUserId">The user authentication data.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public UserAccountCreatedEvent(Guid tenantId, Guid personId, Guid userId, string email, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.UserId = userId;
                this.Email = email;
            }

            /// <summary>
            /// Gets the ID of the user to be assigned to person.
            /// </summary>
            [JsonProperty]
            public Guid UserId { get; private set; }

            /// <summary>
            /// Gets the email used to be assigned to user account.
            /// </summary>
            [JsonProperty]
            public string Email { get; private set; }
        }
    }
}
