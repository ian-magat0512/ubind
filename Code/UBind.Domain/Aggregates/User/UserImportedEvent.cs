// <copyright file="UserImportedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        /// <summary>
        /// The user has to be imported.
        /// </summary>
        public class UserImportedEvent
            : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserImportedEvent"/> class.
            /// </summary>
            /// <param name="customerId">The ID of the customer the user account is for, otherwise default.</param>
            /// <param name="data">The information of the person the customer represents.</param>
            /// <param name="performingUserId">The userId who imported the user.</param>
            /// <param name="environment">The data environment the user is associated with, or null if they can access all environments.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public UserImportedEvent(
                Guid tenantId,
                Guid? customerId,
                PersonData data,
                Guid? performingUserId,
                Guid? portalId,
                Instant createdTimestamp,
                DeploymentEnvironment? environment = null)
                : base(tenantId, Guid.NewGuid(), performingUserId, createdTimestamp)
            {
                this.Person = data;
                this.Environment = environment;
                this.CustomerId = customerId;
                this.PortalId = portalId;
            }

            [JsonConstructor]
            private UserImportedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the tenant string Id.
            /// Note: for backward compatibility only.
            /// </summary>
            /// Remark: used for UB-7141 migration, you can remove right after.
            [JsonProperty("TenantId")]
            public string TenantStringId { get; private set; }

            /// <summary>
            /// Gets a DTO capturing data from the person the user refers to.
            /// </summary>
            [JsonProperty]
            public PersonData Person { get; private set; }

            /// <summary>
            /// Gets the environment the user belongs to.
            /// </summary>
            [JsonProperty]
            public DeploymentEnvironment? Environment { get; private set; }

            /// <summary>
            /// Gets or the ID of the customer the user is for, if any, otherwise default.
            /// </summary>
            [JsonProperty]
            public Guid? CustomerId { get; private set; }

            /// <summary>
            /// Gets the ID of the portal the user should login to.
            /// </summary>
            [JsonProperty]
            public Guid? PortalId { get; private set; }
        }
    }
}
