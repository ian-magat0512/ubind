// <copyright file="ReleaseBase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Represents a base class for product releases.
    /// </summary>
    public abstract class ReleaseBase : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseBase"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of tenant.</param>
        /// <param name="productId">The ID of product.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public ReleaseBase(Guid tenantId, Guid productId, Instant createdTimestamp)
         : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseBase"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        [Obsolete]
        protected ReleaseBase()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the product ID.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets or sets the Quote details.
        /// </summary>
        public ReleaseDetails? QuoteDetails { get; set; }

        /// <summary>
        /// Gets or sets the claim details.
        /// </summary>
        public ReleaseDetails? ClaimDetails { get; set; }

        /// <summary>
        /// Gets or sets the timestamp in ticks when the release was last modified/synced.
        /// </summary>
        public long LastModifiedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the release was last modified/synced.
        /// </summary>
        public Instant LastModifiedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.LastModifiedTicksSinceEpoch);
            }

            set
            {
                this.LastModifiedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }
    }
}
