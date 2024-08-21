// <copyright file="Release.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// A product release.
    /// </summary>
    public class Release : ReleaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Release"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of tenant.</param>
        /// <param name="productId">The ID of product.</param>
        /// <param name="majorReleaseNumber">A major number for the release (should be consecutive and unique for product).</param>
        /// <param name="minorReleaseNumber">A minor number for the release (should be consecutive and unique for product).</param>
        /// <param name="description">A description of the release.</param>
        /// <param name="type">A type for the release.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public Release(
            Guid tenantId,
            Guid productId,
            int majorReleaseNumber,
            int minorReleaseNumber,
            string description,
            ReleaseType type,
            Instant createdTimestamp)
            : base(tenantId, productId, createdTimestamp)
        {
            this.Number = majorReleaseNumber;
            this.MinorNumber = minorReleaseNumber;
            this.Type = type;
            this.Description = description;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Release"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        [Obsolete]
        protected Release()
            : base()
        {
        }

        /// <summary>
        /// Gets the release number (unique for product).
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets the minor release number (unique for product).
        /// </summary>
        public int MinorNumber { get; set; }

        /// <summary>
        /// Gets the release type.
        /// </summary>
        public ReleaseType Type { get; set; }

        /// <summary>
        /// Gets or sets the description for the release.
        /// </summary>
        public string Description { get; set; }
    }
}
