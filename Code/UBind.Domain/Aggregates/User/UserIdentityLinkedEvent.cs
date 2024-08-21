// <copyright file="UserIdentityLinkedEvent.cs" company="uBind">
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
        /// Links the user account to an identity provider.
        /// </summary>
        public class UserIdentityLinkedEvent : Event<UserAggregate, Guid>
        {
            public UserIdentityLinkedEvent(
                Guid tenantId,
                Guid userId,
                Guid authenticationMethodId,
                string uniqueId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.AuthenticationMethodId = authenticationMethodId;
                this.UniqueId = uniqueId;
            }

            [JsonConstructor]
            private UserIdentityLinkedEvent()
                : base()
            {
            }

            [JsonProperty]
            public Guid AuthenticationMethodId { get; private set; }

            [JsonProperty]
            public string UniqueId { get; private set; }
        }
    }
}
