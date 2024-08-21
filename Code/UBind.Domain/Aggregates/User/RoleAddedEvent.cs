// <copyright file="RoleAddedEvent.cs" company="uBind">
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
        /// A role has been added for a user.
        /// </summary>
        public class RoleAddedEvent : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RoleAddedEvent"/> class.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="userType">The UserType.</param>
            /// <param name="performingUserId">The userId who added the role.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public RoleAddedEvent(Guid tenantId, Guid userId, string userType, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.RoleName = userType;
            }

            [JsonConstructor]
            private RoleAddedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the name of the role.
            /// </summary>
            [JsonProperty]
            public string RoleName { get; private set; }
        }
    }
}
