// <copyright file="UserUndeletedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User;

using NodaTime;

public partial class UserAggregate
{
    /// <summary>
    /// Represents the user undeleted event associated with the user aggregate.
    /// </summary>
    public class UserUndeletedEvent : Event<UserAggregate, Guid>
    {
        public UserUndeletedEvent(
            Guid tenantId, Guid userId, Guid? performingUserId, Instant timestamp)
            : base(tenantId, userId, performingUserId, timestamp)
        {
        }
    }
}
