// <copyright file="TokenSession.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// The Token Session.
    /// </summary>
    public class TokenSession : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenSession"/> class.
        /// </summary>
        public TokenSession()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenSession"/> class.
        /// the constructor token session.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="userId">The user indentification associated with the token.</param>
        /// <param name="id">The token session id.</param>
        /// <param name="lastTimeUsed">Last time used.</param>
        /// <param name="createdTimestamp">The created time.</param>
        public TokenSession(
            Guid tenantId, Guid organisationId, Guid? userId, Guid id, Instant lastTimeUsed, Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
            this.UserId = userId;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.LastUsedTimestamp = lastTimeUsed;
        }

        /// <summary>
        /// Gets the guid tenant id its associated with.
        /// </summary>
        /// <remarks>tenant id.</remarks>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets or sets the associated organisation Id.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets the user id associated with.
        /// </summary>
        /// <remarks>user id.</remarks>
        public Guid? UserId { get; private set; }

        /// <summary>
        /// Gets the last time the session was used, e.g. by the user triggering an API call.
        /// </summary>
        /// <remarks> Primitive typed property for EF to store Last time Used.</remarks>
        public long LastUsedTicksSinceEpoch { get; private set; }

        public Instant LastUsedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.LastUsedTicksSinceEpoch); }
            private set { this.LastUsedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        /// <summary>
        /// updates latest time used.
        /// </summary>
        /// <param name="ticks">ticks.</param>
        public void UpdateLatestTimeUsed(long ticks)
        {
            this.LastUsedTicksSinceEpoch = ticks;
        }

        /// <summary>
        /// Sets the tenant ID.
        /// </summary>
        /// <param name="tenantId">The tenant guid to place.</param>
        public void SetTenantID(Guid tenantId)
        {
            this.TenantId = tenantId;
        }
    }
}
