// <copyright file="OrganisationAliasChangedDomainEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Notification
{
    using System;
    using NodaTime;
    using UBind.Domain.Notifications;

    /// <summary>
    /// An event class which fires the organisation alias changed.
    /// </summary>
    public class OrganisationAliasChangedDomainEvent : DomainEvent
    {
        public OrganisationAliasChangedDomainEvent(
            Guid tenantId,
            Guid organisationId,
            string oldOrganisationAlias,
            string newOrganisationAlias,
            Guid? performingUserId,
            Instant createdTimestamp)
            : base(performingUserId, createdTimestamp)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.OldOrganisationAlias = oldOrganisationAlias;
            this.NewOrganisationAlias = newOrganisationAlias;
        }

        public Guid TenantId { get; }

        public Guid OrganisationId { get; set; }

        public string OldOrganisationAlias { get; }

        public string NewOrganisationAlias { get; }
    }
}
