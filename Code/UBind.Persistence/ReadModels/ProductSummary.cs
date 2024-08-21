// <copyright file="ProductSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// For projecting read model summaries from the database.
    /// </summary>
    internal class ProductSummary : IProductSummary
    {
        /// <summary>
        /// Gets or sets the entity's string unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets the instant in time the entity was created.
        /// </summary>
        public Instant CreatedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch); }
            private set { this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        /// <summary>
        /// Gets or sets the entity created time (in ticks since Epoch).
        /// </summary>
        /// <remarks> Primitive typed property for EF to store created time.</remarks>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets tenant name.
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// Gets or sets the string tenant id.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product's tenant is disabled.
        /// </summary>
        public bool TenantDisabled { get; set; }

        /// <summary>
        /// Gets the product details.
        /// </summary>
        public ProductDetails Details => this.History.FirstOrDefault();

        /// <summary>
        /// Gets all the details versions with most recent first.
        /// </summary>
        public IEnumerable<ProductDetails> History
        {
            get
            {
                return this.DetailsCollection.OrderByDescending(d => d.CreatedTimestamp);
            }
        }

        /// <summary>
        /// Gets or sets historic product details.
        /// </summary>
        /// <remarks>
        /// Required for EF to persist all historic and current details (unordered).
        /// .</remarks>
        public virtual Collection<ProductDetails> DetailsCollection { get; set; }
            = new Collection<ProductDetails>();

        /// <summary>
        /// Gets or sets product events.
        /// </summary>
        public virtual Collection<ProductEvent> Events { get; set; } = new Collection<ProductEvent>();
    }
}
