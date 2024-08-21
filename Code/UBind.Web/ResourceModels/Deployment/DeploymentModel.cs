// <copyright file="DeploymentModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Deployment
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NodaTime.Text;
    using UBind.Domain;
    using UBind.Web.ResourceModels.ProductRelease;

    /// <summary>
    /// A view model for a product deployment.
    /// </summary>
    public class DeploymentModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentModel"/> class.
        /// </summary>
        /// <param name="deployment">The deployment.</param>
        /// <param name="releaseVM">release view model.</param>
        public DeploymentModel(Deployment deployment, ReleaseModel releaseVM)
        {
            this.Id = deployment.Id;
            this.Release = releaseVM;
            this.Environment = deployment.Environment;
            this.CreatedTimestampIso8601 = InstantPattern.ExtendedIso.Format(deployment.CreatedTimestamp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        public DeploymentModel()
        {
        }

        /// <summary>
        /// Gets or sets the release ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the release used in the deployment.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public ReleaseModel Release { get; set; }

        /// <summary>
        /// Gets or sets the environment the deployment is for.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonConverter(typeof(StringEnumConverter))]
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the time the deployment was made in ISO 8601 format.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public string CreatedTimestampIso8601 { get; set; }
    }
}
