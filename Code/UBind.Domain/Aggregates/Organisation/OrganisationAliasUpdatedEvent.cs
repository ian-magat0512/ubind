// <copyright file="OrganisationAliasUpdatedEvent.cs" company="uBind">
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
        /// An event that represents updating of the organisation name.
        /// </summary>
        public class OrganisationAliasUpdatedEvent
            : PropertyUpdateEvent<string, Organisation, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OrganisationAliasUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="organisationId">The organisation Id the event belongs to.</param>
            /// <param name="alias">The new organisation alias.</param>
            /// <param name="performingUserId">The userId who updates organization alias.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public OrganisationAliasUpdatedEvent(
                Guid tenantId,
                Guid organisationId,
                string alias,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, organisationId, alias, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private OrganisationAliasUpdatedEvent()
            {
            }
        }
    }
}
