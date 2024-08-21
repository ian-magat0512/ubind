// <copyright file="RoleAssignedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// IDE0060: Removed unused parameter.
// disable IDE0060 because there are unused sequence number parameter.
#pragma warning disable IDE0060

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
        private void Apply(RoleAssignedEvent roleAssignedEvent, int sequenceNumber)
        {
            this.roleIds.Add(roleAssignedEvent.RoleId);
        }

        /// <summary>
        /// A role has been added for a user.
        /// </summary>
        public class RoleAssignedEvent : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RoleAssignedEvent"/> class.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="roleId">The ID of the role.</param>
            /// <param name="performingUserId">The userId who assigned teh role.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public RoleAssignedEvent(Guid tenantId, Guid userId, Guid roleId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.RoleId = roleId;
            }

            [JsonConstructor]
            private RoleAssignedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the role that was assigned.
            /// </summary>
            [JsonProperty]
            public Guid RoleId { get; private set; }
        }
    }
}
