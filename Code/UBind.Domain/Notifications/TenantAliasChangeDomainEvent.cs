// <copyright file="TenantAliasChangeDomainEvent.cs" company="uBind">
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
    /// An event class which fires the tenant alias changed event.
    /// </summary>
    public class TenantAliasChangeDomainEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantAliasChangeDomainEvent"/> class.
        /// </summary>
        /// <param name="tenantId">ID of the tenant.</param>
        /// <param name="oldTenantAlias">The old tenant alias.</param>
        /// <param name="newTenantAlias">The new tenant alias.</param>
        /// <param name="performingUserId">ID of the user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        public TenantAliasChangeDomainEvent(
            Guid tenantId,
            string oldTenantAlias,
            string newTenantAlias,
            Guid? performingUserId,
            Instant createdTimestamp)
            : base(performingUserId, createdTimestamp)
        {
            this.TenantId = tenantId;
            this.OldTenantAlias = oldTenantAlias;
            this.NewTenantAlias = newTenantAlias;
        }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the old tenant alias.
        /// </summary>
        public string OldTenantAlias { get; }

        /// <summary>
        /// Gets the new tenant alias.
        /// </summary>
        public string NewTenantAlias { get; }
    }
}
