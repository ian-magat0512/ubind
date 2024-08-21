// <copyright file="SettingDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// A uBind tenant setting.
    /// </summary>
    public class SettingDetails : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingDetails"/> class.
        /// </summary>
        /// <param name="disabled">If the setting is disabled.</param>
        /// <param name="tenant">The parent tenant.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        public SettingDetails(bool disabled, Tenant tenant, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Disabled = disabled;
            this.Tenant = tenant;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingDetails"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private SettingDetails()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets a value indicating whether the setting is disabled.
        /// </summary>
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets the parent tenant for the tenant setting.
        /// </summary>
        public Tenant Tenant { get; private set; }
    }
}
