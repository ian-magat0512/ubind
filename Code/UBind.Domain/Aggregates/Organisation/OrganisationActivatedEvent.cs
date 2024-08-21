﻿// <copyright file="OrganisationActivatedEvent.cs" company="uBind">
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
        /// An event that represents the organisation that has been marked as active.
        /// </summary>
        public class OrganisationActivatedEvent
            : Event<Organisation, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OrganisationActivatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The Id of the tenant.</param>
            /// <param name="organisationId">The organisation Id the event belongs to.</param>
            /// <param name="performingUserId">The userId who activate organization.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public OrganisationActivatedEvent(Guid tenantId, Guid organisationId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, organisationId, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private OrganisationActivatedEvent()
            {
            }
        }
    }
}
