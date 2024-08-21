// <copyright file="RoleRetractedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable IDE0060 // Remove unused parameter

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
        private void Apply(RoleRetractedEvent roleRetractedEvent, int sequenceNumber)
        {
            this.roleIds.Remove(roleRetractedEvent.RoleId);
        }

        /// <summary>
        /// A role has been retracted from a user.
        /// </summary>
        public class RoleRetractedEvent : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RoleRetractedEvent"/> class.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="roleId">The ID of the role that was retracted.</param>
            /// <param name="performingUserId">The userId who retracted the role.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public RoleRetractedEvent(Guid tenantId, Guid userId, Guid roleId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.RoleId = roleId;
            }

            [JsonConstructor]
            private RoleRetractedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the role.
            /// </summary>
            [JsonProperty]
            public Guid RoleId { get; private set; }
        }
    }
}
