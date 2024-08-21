// <copyright file="PortalCreatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Portal
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    public partial class PortalAggregate
    {
        public class PortalCreatedEvent : Event<PortalAggregate, Guid>
        {
            public PortalCreatedEvent(
                Guid tenantId,
                Guid portalId,
                string name,
                string alias,
                string title,
                PortalUserType userType,
                Guid organisationId,
                Guid? performingUserId,
                Instant timestamp)
                : base(tenantId, portalId, performingUserId, timestamp)
            {
                this.Name = name;
                this.Alias = alias;
                this.Title = title;
                this.UserType = userType;
                this.OrganisationId = organisationId;
            }

            [JsonProperty]
            public string Name { get; private set; }

            [JsonProperty]
            public string Alias { get; private set; }

            [JsonProperty]
            public string Title { get; private set; }

            public PortalUserType UserType { get; private set; } = PortalUserType.Customer;

            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
