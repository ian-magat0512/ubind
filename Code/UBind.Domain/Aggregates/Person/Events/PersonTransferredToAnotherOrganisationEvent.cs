// <copyright file="PersonTransferredToAnotherOrganisationEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    public partial class PersonAggregate
    {
        /// <summary>
        /// Event raised when a person is about to be transferred to another organisation.
        /// </summary>
        public class PersonTransferredToAnotherOrganisationEvent
            : Event<PersonAggregate, Guid>
        {
            public PersonTransferredToAnotherOrganisationEvent(
                Guid tenantId, Guid organisationId, Guid personId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
            }

            [JsonConstructor]
            private PersonTransferredToAnotherOrganisationEvent()
                : base(default, default(Guid), default, default)
            {
            }

            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
