// <copyright file="PersonDeletedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using NodaTime;

    /// <summary>
    /// Aggregate for people.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// The person has been deleted.
        /// </summary>
        public class PersonDeletedEvent : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonDeletedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The Id of the tenant.</param>
            /// <param name="personId">A unique ID for the Person.</param>
            /// <param name="performingUserId">The user authentication data.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            /// <param name="isPermanentDelete">(Optional) Determines whether to delete the person permanently with its records.</param>
            public PersonDeletedEvent(
                Guid tenantId,
                Guid personId,
                Guid? performingUserId,
                Instant createdTimestamp,
                bool isPermanentDelete = false)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.IsPermanentDelete = isPermanentDelete;
            }

            /// <summary>
            /// Determines whether to delete the person permanently with its records.
            /// </summary>
            public bool IsPermanentDelete { get; private set; }
        }
    }
}
