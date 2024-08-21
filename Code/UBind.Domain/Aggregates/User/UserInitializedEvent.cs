// <copyright file="UserInitializedEvent.cs" company="uBind">
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
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        /// <summary>
        /// Event raised when a user has been created.
        /// </summary>
        public class UserInitializedEvent : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserInitializedEvent"/> class.
            /// </summary>
            /// <param name="userId">A unique ID for the user.</param>
            /// <param name="personData">A DTO capturing data from the person the user refers to.</param>
            /// <param name="customerId">The ID of the customer the user account is for (if it is a customer user), otherwise default.</param>
            /// <param name="performingUserId">The userId who created user.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            /// <param name="environment">The data environment the user is associated with, or null if they can access all enviroments.</param>
            public UserInitializedEvent(
                Guid tenantId,
                Guid userId,
                UserType userType,
                PersonData personData,
                Guid? customerId,
                Guid? performingUserId,
                Guid? portalId,
                Instant createdTimestamp,
                DeploymentEnvironment? environment = null,
                Guid[]? roleIds = null)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.UserType = userType;
                this.Person = personData;
                this.Environment = environment;
                this.CustomerId = customerId;
                this.PortalId = portalId;
                this.RoleIds = roleIds;
            }

            [JsonConstructor]
            private UserInitializedEvent()
                : base(default(Guid), default(Guid), default, default)
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
            /// Gets the environment the user belongs to, or null if they have access to all environments.
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

            [JsonProperty]
            public UserType? UserType { get; private set; }

            [JsonProperty]
            public Guid[]? RoleIds { get; private set; }
        }
    }
}
