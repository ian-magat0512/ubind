// <copyright file="Portal.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NodaTime;

    /// <summary>
    /// A uBind portal.
    /// </summary>
    public class Portal : Entity<Guid>
    {
        /// <summary>
        /// Initializes the static properties.
        /// </summary>
        static Portal()
        {
            SupportsAdditionalProperties = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Portal"/> class.
        /// </summary>
        /// <param name="name">A descriptive name for the portal.</param>
        /// <param name="alias">A descriptive alias for the portal.</param>
        /// <param name="title">A descriptive title for the portal.</param>
        /// <param name="stylesheetUrl">The portal stylesheet ( optional ).</param>
        /// <param name="disabled">If portal is disabled.</param>
        /// <param name="deleted">If portal is deleted.</param>
        /// <param name="tenant">the parent tenant.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        public Portal(
            string name,
            string alias,
            string title,
            string stylesheetUrl,
            bool disabled,
            bool deleted,
            Instant createdTimestamp,
            Tenant tenant,
            Guid organisationId)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Tenant = tenant;
            this.OrganisationId = organisationId;
            var details = PortalDetails.Create(name, alias, title, stylesheetUrl, disabled, deleted, createdTimestamp);
            this.PortalDetailsCollection.Add(details);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Portal"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private Portal()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the portal details.
        /// </summary>
        public PortalDetails Details => this.History.FirstOrDefault();

        /// <summary>
        /// Gets the parent tenant for the portal.
        /// </summary>
        public virtual Tenant Tenant { get; private set; }

        /// <summary>
        /// Gets the parent organisation for the portal.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets all the details versions with most recent first.
        /// </summary>
        public IEnumerable<PortalDetails> History
        {
            get
            {
                return this.PortalDetailsCollection.OrderByDescending(d => d.CreatedTimestamp);
            }
        }

        /// <summary>
        /// Gets the active deployment targets for the portal.
        /// </summary>
        public IEnumerable<DeploymentTarget> ActiveDeploymentTargets
        {
            get
            {
                return this.DeploymentTargetCollection.Where(prop => !prop.IsDeleted);
            }
        }

        /// <summary>
        /// Gets or sets historic portal details.
        /// </summary>
        /// <remarks>
        /// Required for EF to persist all historic and current details (unordered).
        /// .</remarks>
        public virtual Collection<PortalDetails> PortalDetailsCollection { get; set; }
            = new Collection<PortalDetails>();

        /// <summary>
        /// Gets or sets collection of deployment targets for this portal.
        /// </summary>
        public virtual ICollection<DeploymentTarget> DeploymentTargetCollection { get; set; }
            = new Collection<DeploymentTarget>();

        /// <summary>
        /// Update the portal with new details.
        /// </summary>
        /// <param name="details">The new tenant details.</param>
        public void Update(PortalDetails details)
        {
            this.PortalDetailsCollection.Add(details);
        }

        /// <summary>
        /// Update the portal with new deployment targets.
        /// </summary>
        /// <param name="deploymentTarget">The new deployment target.</param>
        public void AddDeploymentTarget(DeploymentTarget deploymentTarget)
        {
            this.DeploymentTargetCollection.Add(deploymentTarget);
        }
    }
}
