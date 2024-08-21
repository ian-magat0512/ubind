﻿// <copyright file="OrganisationIdentityUnlinkedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Organisation
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for Organisations.
    /// </summary>
    public partial class Organisation
    {
        /// <summary>
        /// Unlinks the Organisation account from an identity provider.
        /// </summary>
        public class OrganisationIdentityUnlinkedEvent : Event<Organisation, Guid>
        {
            public OrganisationIdentityUnlinkedEvent(
                Guid tenantId,
                Guid organisationId,
                Guid authenticationMethodId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, organisationId, performingUserId, createdTimestamp)
            {
                this.AuthenticationMethodId = authenticationMethodId;
            }

            [JsonConstructor]
            private OrganisationIdentityUnlinkedEvent()
                : base()
            {
            }

            [JsonProperty]
            public Guid AuthenticationMethodId { get; private set; }
        }
    }
}
