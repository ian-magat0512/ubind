// <copyright file="ProductAliasChangeDomainEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Notifications
{
    using System;
    using NodaTime;

    /// <summary>
    /// An event class which fires the product alias changed.
    /// </summary>
    public class ProductAliasChangeDomainEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductAliasChangeDomainEvent"/> class.
        /// </summary>
        /// <param name="tenantId">ID of the tenant.</param>
        /// <param name="productId">ID of the product.</param>
        /// <param name="oldProductAlias">The old product alias.</param>
        /// <param name="newProductAlias">The new product alias.</param>
        /// <param name="performingUserId">ID of the performing user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        public ProductAliasChangeDomainEvent(
            Guid tenantId,
            Guid productId,
            string oldProductAlias,
            string newProductAlias,
            Guid? performingUserId,
            Instant createdTimestamp)
            : base(performingUserId, createdTimestamp)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.OldProductAlias = oldProductAlias;
            this.NewProductAlias = newProductAlias;
        }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the product Id.
        /// </summary>
        public Guid ProductId { get; }

        /// <summary>
        /// Gets the old product alias.
        /// </summary>
        public string OldProductAlias { get; }

        /// <summary>
        /// Gets the new product alias.
        /// </summary>
        public string NewProductAlias { get; }
    }
}
