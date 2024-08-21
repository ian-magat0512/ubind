// <copyright file="ProductPortalSetting.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Class to hold the product setting within the portal.
    /// </summary>
    public class ProductPortalSetting : MutableEntity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPortalSetting"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id the product belongs to.</param>
        /// <param name="portalId">The portal Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="isNewQuotesAllowed">Flag to restrict creation of new Quotes for a Product on a per-Portal basis.</param>
        /// <param name="createdTimestamp">The time this setting has been created.</param>
        public ProductPortalSetting(Guid tenantId, Guid portalId, Guid productId, bool isNewQuotesAllowed, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.TenantId = tenantId;
            this.PortalId = portalId;
            this.ProductId = productId;
            this.IsNewQuotesAllowed = isNewQuotesAllowed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPortalSetting"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private ProductPortalSetting()
            : base(default, default)
        {
        }

        /// <summary>
        /// Gets the tenant Id that the product belongs to.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the portal Id.
        /// </summary>
        public Guid PortalId { get; private set; }

        /// <summary>
        /// Gets the product Id.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether new quotes are allowed.
        /// </summary>
        public bool IsNewQuotesAllowed { get; private set; }

        /// <summary>
        /// Set new quotes allowed.
        /// </summary>
        /// <param name="isNewQuotesAllowed">The flag to determine if new quotes are allowed.</param>
        public void SetNewQuotesAllowed(bool isNewQuotesAllowed)
        {
            this.IsNewQuotesAllowed = isNewQuotesAllowed;
        }
    }
}
