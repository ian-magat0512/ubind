// <copyright file="CustomerIdAndEnvironmentUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        public class CustomerIdAndEnvironmentUpdatedEvent : Event<UserAggregate, Guid>
        {
            public CustomerIdAndEnvironmentUpdatedEvent(Guid tenantId, Guid userId, Guid customerId, DeploymentEnvironment environment)
            {
                this.TenantId = tenantId;
                this.UserId = userId;
                this.CustomerId = customerId;
                this.Environment = environment;
            }

            [JsonConstructor]
            private CustomerIdAndEnvironmentUpdatedEvent()
            : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            [JsonProperty]
            public Guid UserId { get; private set; }

            [JsonProperty]
            public Guid CustomerId { get; private set; }

            [JsonProperty]
            public DeploymentEnvironment Environment { get; private set; }
        }
    }
}
