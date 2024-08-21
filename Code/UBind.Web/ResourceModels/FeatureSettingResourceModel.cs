// <copyright file="FeatureSettingResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Resource model for Portal Settings.
    /// </summary>
    public class FeatureSettingResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureSettingResourceModel"/> class.
        /// </summary>
        /// <param name="settings">The detail of this portal setting for the specific portal.</param>
        /// <param name="portalId">The portal Id.</param>
        public FeatureSettingResourceModel(PortalSettings settings, Guid portalId = default)
        {
            this.PortalSettingId = settings.Id;
            this.Name = settings.Name;

            if (portalId != default && settings.DetailCollection.Any())
            {
                this.Active = settings.DetailCollection.OrderByDescending(x => x.CreatedTimestamp)
                    .FirstOrDefault(x => x.PortalId == portalId)?.Active ?? false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureSettingResourceModel"/> class.
        /// </summary>
        /// <param name="setting">The detail of this setting.</param>
        /// <param name="portalId">This is currently unused, but will likely be used in the future when we allow
        /// setting enabled features per portal, so it's here as a placeholder.</param>
        public FeatureSettingResourceModel(Setting setting, Guid portalId = default)
        {
            this.Name = setting.Name;
            this.Active = !setting.Details.Disabled;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureSettingResourceModel"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for JSON deserializer.</remarks>
        [JsonConstructor]
        private FeatureSettingResourceModel()
        {
        }

        /// <summary>
        /// Gets or sets the id of the feature setting this detail is for.
        /// </summary>
        [JsonProperty("id")]
        public Guid PortalSettingId { get; set; }

        /// <summary>
        /// Gets or sets the name of the feature setting.
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value indicates whether this feature setting is enabled
        /// for a specific portal.
        /// </summary>
        [JsonProperty]
        public bool Active { get; set; }
    }
}
