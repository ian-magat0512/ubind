// <copyright file="Deployment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Diagnostics.Contracts;
    using NodaTime;

    /// <summary>
    /// Class representing a deployment of a product release to an environment.
    /// "Deploying" a product release to an environment really just means that it will become the default release for that environment.
    /// It's also possible for a release to be used in an environment if quotes or policies are migrated to it, therefore not needing
    /// a deployment record to be created.
    /// </summary>
    public class Deployment : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Deployment"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the release belongs to.</param>
        /// <param name="productId">The ID of the product the release belongs to.</param>
        /// <param name="environment">The environment the release is deployed to.</param>
        /// <param name="release">The release to deploy.</param>
        /// <param name="createdTimestamp">The time the deployment was created.</param>
        public Deployment(Guid tenantId, Guid productId, DeploymentEnvironment environment, Release release, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            Contract.Assert(environment != DeploymentEnvironment.Development);

            this.Release = release;
            this.Environment = environment;
            this.ProductId = productId;
            this.TenantId = tenantId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deployment"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        protected Deployment()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the release that was deployed.
        /// </summary>
        public virtual Release Release { get; private set; }

        /// <summary>
        /// Gets the ID of the Tenant the deployed was for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product that was deployed.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the environment that the release is deployed to.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }
    }
}
