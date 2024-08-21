// <copyright file="DeploymentTargetsDto.cs" company="uBind">
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
    /// Resource model for referrer urls of a portal.
    /// </summary>
    public class DeploymentTargetsDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentTargetsDto"/> class.
        /// </summary>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <param name="deploymentTarget">The referrer url.</param>
        public DeploymentTargetsDto(string tenant, DeploymentTarget deploymentTarget)
        {
            this.Id = deploymentTarget.Id;
            this.Url = deploymentTarget.LatestUrl;
            this.IsDeleted = deploymentTarget.IsDeleted;
            this.Tenant = tenant;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentTargetsDto"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for JSON deserializer.</remarks>
        [JsonConstructor]
        private DeploymentTargetsDto()
        {
        }

        /// <summary>
        /// Gets or sets the Id of the referrer url.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the current url.
        /// </summary>
        [JsonProperty]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the model is deleted.
        /// </summary>
        [JsonProperty]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the tenant Id or Alias.
        /// </summary>
        [JsonProperty]
        public string Tenant { get; set; }
    }
}
