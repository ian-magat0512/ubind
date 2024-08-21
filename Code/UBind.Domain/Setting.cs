// <copyright file="Setting.cs" company="uBind">
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
    using System.IO;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// A feature setting which determines whether the feature is available to the tenancy.
    /// </summary>
    /// TODO: Rename this to FeatureSetting
    public class Setting : Entity<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Setting"/> class.
        /// </summary>
        /// <param name="identifier">
        /// A unique ID for the setting. Must not contain illegal characters.
        /// .</param>
        /// <param name="name">A descriptive name for the setting.</param>
        /// <param name="icon">The icon symbol of the setting.</param>
        /// <param name="sortOrder">The sort order of the setting.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        public Setting(string identifier, string name, string icon, int sortOrder, Instant createdTimestamp, IconLibrary iconLibrary)
            : base(identifier, createdTimestamp)
        {
            Contract.Assert(!string.IsNullOrEmpty(identifier));

            var invalidCharacters = Path.GetInvalidFileNameChars();
            if (identifier.IndexOfAny(invalidCharacters) != -1)
            {
                throw new ArgumentException(
                    "Identifier cannot contain any of the following characters: "
                        + string.Join(" ", invalidCharacters));
            }

            this.Name = name;
            this.Icon = icon;
            this.IconLibrary = iconLibrary;
            this.SortOrder = sortOrder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private Setting()
            : base(default(string), default(Instant))
        {
        }

        /// <summary>
        /// Gets the setting name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the setting icon.
        /// </summary>
        public string Icon { get; private set; }

        public IconLibrary IconLibrary { get; private set; }

        /// <summary>
        /// Gets the setting sort order.
        /// </summary>
        public int SortOrder { get; private set; }

        /// <summary>
        /// Gets the setting details.
        /// </summary>
        public SettingDetails Details => this.History.FirstOrDefault();

        /// <summary>
        /// Gets all the details versions with most recent first.
        /// </summary>
        public IEnumerable<SettingDetails> History
        {
            get
            {
                return this.DetailsCollection.OrderByDescending(d => d.CreatedTimestamp);
            }
        }

        /// <summary>
        /// Gets or sets historic setting details.
        /// </summary>
        /// <remarks>
        /// Required for EF to persist all historic and current details (unordered).
        /// .</remarks>
        public virtual Collection<SettingDetails> DetailsCollection { get; set; }
            = new Collection<SettingDetails>();

        /// <summary>
        /// Gets or sets collection of portal settings for this tenant setting.
        /// </summary>
        public virtual ICollection<PortalSettings> PortalSettingsCollection { get; set; }
            = new Collection<PortalSettings>();

        /// <summary>
        /// Update the tenant with new details.
        /// </summary>
        /// <param name="details">The new tenant details.</param>
        public void Update(SettingDetails details)
        {
            this.DetailsCollection.Add(details);
        }

        /// <summary>
        /// Adds portal settings under the tenant setting.
        /// </summary>
        /// <param name="settings">The new portal settings.</param>
        public void AddPortalSettings(PortalSettings settings)
        {
            this.PortalSettingsCollection.Add(settings);
        }

        /// <summary>
        /// Update the name of the setting.
        /// </summary>
        /// <param name="name">The new name.</param>
        public void UpdateName(string name) => this.Name = name;

        /// <summary>
        /// Update the icons of the setting.
        /// </summary>
        /// <param name="icon">The new icon.</param>
        public void UpdateIcon(string icon) => this.Icon = icon;
    }
}
