// <copyright file="UserTypeUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        /// <summary>
        /// A user type has been updated for a user.
        /// </summary>
        public class UserTypeUpdatedEvent : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserTypeUpdatedEvent"/> class.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="userType">The UserType.</param>
            /// <param name="performingUserId">The userId who added the role.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public UserTypeUpdatedEvent(Guid tenantId, Guid userId, string userType, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.UserType = userType;
            }

            [JsonConstructor]
            private UserTypeUpdatedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the user type.
            /// </summary>
            [JsonProperty]
            public string UserType { get; private set; }
        }
    }
}
