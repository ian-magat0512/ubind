// <copyright file="DeploymentTarget.cs" company="uBind">
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
    /// Stores the deployment referrer urls for a portal.
    /// </summary>
    public class DeploymentTarget : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentTarget"/> class.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public DeploymentTarget(string url, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Update(new DeploymentTargetDetails(url, createdTimestamp));
            this.IsDeleted = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentTarget"/> class.
        /// </summary>
        /// <remarks>Parameterless construcctor for EF.</remarks>
        private DeploymentTarget()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the latest url for this referrer url.
        /// </summary>
        public string LatestUrl => this.DeploymentTargetDetailsCollection.OrderByDescending(x => x.CreatedTimestamp).FirstOrDefault().URL;

        /// <summary>
        /// Gets or sets historic collection of the urls for this referrer url.
        /// </summary>
        public ICollection<DeploymentTargetDetails> DeploymentTargetDetailsCollection { get; set; }
            = new Collection<DeploymentTargetDetails>();

        /// <summary>
        /// Gets a value indicating whether a value indicating if the referrer url is deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Updates the current detail/s of this record.
        /// </summary>
        /// <param name="details">The latest details.</param>
        public void Update(DeploymentTargetDetails details)
        {
            this.DeploymentTargetDetailsCollection.Add(details);
        }

        /// <summary>
        /// Toggles the object's property to be deleted.
        /// </summary>
        public void Delete()
        {
            this.IsDeleted = true;
        }
    }
}
