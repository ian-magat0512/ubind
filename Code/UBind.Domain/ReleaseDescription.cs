// <copyright file="ReleaseDescription.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Contains the description for a release.
    /// </summary>
    public class ReleaseDescription : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseDescription"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="timestamp">The current timestamp.</param>
        public ReleaseDescription(string description, Instant timestamp)
            : base(Guid.NewGuid(), timestamp)
        {
            this.Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseDescription"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        [Obsolete]
        protected ReleaseDescription()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the description for a release.
        /// </summary>
        public string Description { get; private set; }
    }
}
