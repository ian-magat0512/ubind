// <copyright file="SettingModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Resource model for settings.
    /// </summary>
    public class SettingModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingModel"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <param name="portalId">The portal Id for which the setting is requested for.</param>
        public SettingModel(Setting setting, Guid portalId = default)
        {
            if (setting != null)
            {
                this.Id = setting.Id;
                this.Name = setting.Name;
                this.Disabled = setting.Details?.Disabled ?? true;
                this.Icon = setting.Icon;
                this.IconLibrary = setting.IconLibrary.Humanize();
                this.SortOrder = setting.SortOrder;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        [JsonConstructor]
        private SettingModel()
        {
        }

        /// <summary>
        /// Gets the setting ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the setting name.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the setting icon.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public string Icon { get; private set; }

        [JsonProperty]
        public string IconLibrary { get; private set; }

        /// <summary>
        /// Gets the setting sort order.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public int SortOrder { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the setting is disabled.
        /// </summary>
        [JsonProperty]
        public bool Disabled { get; private set; }
    }
}
