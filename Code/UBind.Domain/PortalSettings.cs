// <copyright file="PortalSettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using NodaTime;

    /// <summary>
    /// A uBind portal setting.
    /// </summary>
    public class PortalSettings : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettings"/> class.
        /// </summary>
        /// <param name="name">The name of the portal setting.</param>
        /// <param name="createdTimestamp">Creation time.</param>
        public PortalSettings(string name, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            Contract.Assert(!string.IsNullOrEmpty(name));

            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettings"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private PortalSettings()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the name of the portal setting under the tenant setting.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the tenant setting this portal setting is for.
        /// </summary>
        public virtual Setting Setting { get; private set; }

        /// <summary>
        /// Gets the latest setting details, if any.
        /// </summary>
        public PortalSettingDetails Details => this.History.FirstOrDefault();

        /// <summary>
        /// Gets all the detail versions with the most recent first.
        /// </summary>
        public IEnumerable<PortalSettingDetails> History
        {
            get
            {
                return this.DetailCollection.OrderByDescending(d => d.CreatedTimestamp);
            }
        }

        /// <summary>
        /// Gets or sets the full detail collection of this portal setting.
        /// </summary>
        public ICollection<PortalSettingDetails> DetailCollection { get; set; }
            = new Collection<PortalSettingDetails>();

        /// <summary>
        /// Updates the portal setting with a new detail.
        /// </summary>
        /// <param name="details">THe new portal setting details.</param>
        public void Update(PortalSettingDetails details)
        {
            this.DetailCollection.Add(details);
        }
    }
}
