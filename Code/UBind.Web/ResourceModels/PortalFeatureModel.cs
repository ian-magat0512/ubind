// <copyright file="PortalFeatureModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Resource model for portalFeatures.
    /// </summary>
    public class PortalFeatureModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalFeatureModel"/> class.
        /// </summary>
        /// <param name="portalFeature">The portalFeature.</param>
        /// <param name="portalId">The portal Id for which the portalFeature is requested for.</param>
        public PortalFeatureModel(Setting portalFeature, Guid portalId = default)
        {
            if (portalFeature != null)
            {
                this.Name = portalFeature.Name;
                this.Disabled = portalFeature.Details?.Disabled ?? true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalFeatureModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        [JsonConstructor]
        private PortalFeatureModel()
        {
        }

        /// <summary>
        /// Gets the portalFeature name.
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the tenant is disabled.
        /// </summary>
        [JsonProperty]
        public bool Disabled { get; private set; }
    }
}
