// <copyright file="SetOrganisationDefaultPortalEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Organisation
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    public partial class Organisation
    {
        public class SetOrganisationDefaultPortalEvent : Event<Organisation, Guid>
        {
            /// <summary>
            /// To unset a portal as the default on an org, pass in null for portalId.
            /// </summary>
            public SetOrganisationDefaultPortalEvent(
                Guid tenantId,
                Guid organisationId,
                Guid? portalId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, organisationId, performingUserId, createdTimestamp)
            {
                this.PortalId = portalId;
            }

            [JsonProperty]
            public Guid? PortalId { get; set; }
        }
    }
}
