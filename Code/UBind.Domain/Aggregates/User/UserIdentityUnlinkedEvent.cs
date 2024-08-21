// <copyright file="UserIdentityUnlinkedEvent.cs" company="uBind">
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
        /// Unlinks the user account from an identity provider.
        /// </summary>
        public class UserIdentityUnlinkedEvent : Event<UserAggregate, Guid>
        {
            public UserIdentityUnlinkedEvent(
                Guid tenantId,
                Guid userId,
                Guid authenticationMethodId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.AuthenticationMethodId = authenticationMethodId;
            }

            [JsonConstructor]
            private UserIdentityUnlinkedEvent()
                : base()
            {
            }

            [JsonProperty]
            public Guid AuthenticationMethodId { get; private set; }
        }
    }
}
