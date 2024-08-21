// <copyright file="UserTransferredToAnotherOrganisationEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    public partial class UserAggregate
    {
        /// <summary>
        /// Event raised when a user is about to be transferred to another organisation.
        /// </summary>
        public class UserTransferredToAnotherOrganisationEvent
            : Event<UserAggregate, Guid>
        {
            public UserTransferredToAnotherOrganisationEvent(
                Guid tenantId,
                Guid userId,
                Guid currentOrganisationId,
                Guid organisationId,
                Guid personId,
                bool fromDefaultOrganisation,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.CurrentOrganisationId = currentOrganisationId;
                this.OrganisationId = organisationId;
                this.PersonId = personId;
                this.FromDefaultOrganisation = fromDefaultOrganisation;
            }

            [JsonConstructor]
            private UserTransferredToAnotherOrganisationEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            [JsonProperty]
            public Guid CurrentOrganisationId { get; private set; }

            [JsonProperty]
            public Guid OrganisationId { get; private set; }

            [JsonProperty]
            public Guid PersonId { get; private set; }

            [JsonProperty]
            public bool FromDefaultOrganisation { get; private set; }
        }
    }
}
