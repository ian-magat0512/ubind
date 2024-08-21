// <copyright file="PortalSettingDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;
    using UBind.Domain.ReadModel.Portal;

    /// <summary>
    /// Holds the featured portal setting enabled for each portal.
    /// </summary>
    public class PortalSettingDetails : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingDetails"/> class.
        /// </summary>
        /// <param name="active">Indicates whether this setting is disabled.</param>
        /// <param name="portal">The portal this setting is for.</param>
        /// <param name="createdTimestamp">The time this setting has been created.</param>
        public PortalSettingDetails(bool active, PortalReadModel portal, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Active = active;
            this.PortalId = portal.Id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingDetails"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private PortalSettingDetails()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets a value indicating whether this setinng is active.
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// Gets the ID of the portal this detail is for.
        /// </summary>
        public Guid PortalId { get; private set; }
    }
}
