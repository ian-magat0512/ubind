// <copyright file="UserDeletedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User;

using NodaTime;

public partial class UserAggregate
{
    /// <summary>
    /// Represents the UserDeletedEvent associated with the UserAggregate.
    /// </summary>
    public class UserDeletedEvent : Event<UserAggregate, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the UserDeletedEvent class.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="performingUserId">The optional identifier of the user performing the action.</param>
        /// <param name="timestamp">The timestamp at which the event occurred.</param>
        public UserDeletedEvent(
            Guid tenantId, Guid userId, Guid? performingUserId, Instant timestamp)
            : base(tenantId, userId, performingUserId, timestamp)
        {
        }
    }
}
