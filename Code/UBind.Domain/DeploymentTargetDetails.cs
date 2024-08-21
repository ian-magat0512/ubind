// <copyright file="DeploymentTargetDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Contains the referrer url details of a referrer url (the URL itself).
    /// </summary>
    public class DeploymentTargetDetails : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentTargetDetails"/> class.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public DeploymentTargetDetails(string url, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.URL = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentTargetDetails"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private DeploymentTargetDetails()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the URL description.
        /// </summary>
        public string URL { get; private set; }
    }
}
