// <copyright file="OrganisationInitializedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Organisation
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// The aggregate for organisations.
    /// </summary>
    public partial class Organisation
    {
        /// <summary>
        /// An event that represents the organisation that has been initialised.
        /// </summary>
        public class OrganisationInitializedEvent
            : Event<Organisation, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OrganisationInitializedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id of the organisation.</param>
            /// <param name="organisationId">The Id of the organisation.</param>
            /// <param name="alias">The alias of the organisation.</param>
            /// <param name="name">The name of the organisation.</param>
            /// <param name="isActive">The value indicating whether the organisation is active or disabled.</param>
            /// <param name="isDeleted">The value indicating whether the organisation is marked as deleted.</param>
            /// <param name="performingUserId">The userId who initialized organization.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public OrganisationInitializedEvent(
                Guid tenantId,
                Guid organisationId,
                string alias,
                string name,
                Guid? managingOrganisationId,
                bool isActive,
                bool isDeleted,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, organisationId, performingUserId, createdTimestamp)
            {
                this.Alias = alias;
                this.Name = name;
                this.ManagingOrganisationId = managingOrganisationId;
                this.IsActive = isActive;
                this.IsDeleted = isDeleted;
            }

            [JsonConstructor]
            private OrganisationInitializedEvent()
                : base(default, default, default, default)
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
            /// Gets the tenant alias of the organisation.
            /// </summary>
            [JsonProperty]
            public string Alias { get; private set; }

            /// <summary>
            /// Gets the tenant name of the organisation.
            /// </summary>
            [JsonProperty]
            public string Name { get; private set; }

            [JsonProperty]
            public Guid? ManagingOrganisationId { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the organisation is active or disabled.
            /// </summary>
            [JsonProperty]
            public bool IsActive { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the organisation is marked as deleted.
            /// </summary>
            [JsonProperty]
            public bool IsDeleted { get; private set; }
        }
    }
}
