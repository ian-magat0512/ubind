// <copyright file="Product.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// A uBind product.
    /// </summary>
    public class Product : Entity<Guid>, IProduct
    {
        /// <summary>
        /// Initializes the static properties.
        /// </summary>
        static Product()
        {
            SupportsAdditionalProperties = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of he product's tenant.</param>
        /// <param name="productId">
        /// A unique ID for the product. This will be used as the folder name in One Drive and so must not contain illegal characters.
        /// .</param>
        /// <param name="name">A descriptive name for the product.</param>
        /// <param name="alias">The alias for the product.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        public Product(
            Guid tenantId,
            Guid productId,
            string name,
            string alias,
            Instant createdTimestamp)
            : base(productId, createdTimestamp)
        {
            Contract.Assert(productId != Guid.Empty);
            Contract.Assert(tenantId != Guid.Empty);

            var invalidCharacters = Path.GetInvalidFileNameChars();
            if (alias.IndexOfAny(invalidCharacters) != -1)
            {
                throw new ArgumentException(
                    "Alias cannot contain any of the following characters: "
                        + string.Join(" ", invalidCharacters));
            }

            this.TenantId = tenantId;
            var details = new ProductDetails(name, alias, false, false, createdTimestamp);
            this.DetailsCollection.Add(details);
            this.Events = new Collection<ProductEvent>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private Product()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the ID of the tenant of the product.
        /// </summary>
        public Guid TenantId { get; private set; }

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
        public Collection<ProductDetails> DetailsCollection { get; set; }
            = new Collection<ProductDetails>();

        /// <summary>
        /// Gets or sets product events.
        /// </summary>
        public virtual Collection<ProductEvent> Events { get; set; } = new Collection<ProductEvent>();

        /// <summary>
        /// Gets or sets product reports.
        /// </summary>
        public virtual ICollection<ReportReadModel> Reports { get; set; }

        /// <summary>
        /// Update the product with new details.
        /// </summary>
        /// <param name="details">The new product details.</param>
        public void Update(ProductDetails details)
        {
            this.DetailsCollection.Add(details);
        }

        /// <summary>
        /// Notify the product that it's infrastructure has been initialized.
        /// </summary>
        /// <param name="timestamp">The time it was initialized.</param>
        public void OnInitized(Instant timestamp)
        {
            var productEvent = new ProductEvent(ProductEventType.OneDriveInitialized, timestamp);
            this.Events.Add(productEvent);
        }

        /// <summary>
        /// Notify the product that it's infrastructure is not created.
        /// </summary>
        /// <param name="timestamp">The time the initialisation failed.</param>
        public void OnInitizationFailed(Instant timestamp)
        {
            var productEvent = new ProductEvent(ProductEventType.OneDriveInitializedFailed, timestamp);
            this.Events.Add(productEvent);
        }

        /// <summary>
        /// initialize the product newId.
        /// </summary>
        /// <param name="newId">The value to assign to newId.</param>
        public void InitializeNewId(Guid newId)
        {
            if (this.Id == default)
            {
                this.Id = newId;
            }
        }

        /// <summary>
        /// Gets the first ever alias set of this tenant.
        /// This is useful for backward compatibility because before we have string Ids and some events use this.
        /// </summary>
        /// <returns>The initial alias.</returns>
        public string GetStringId()
        {
            return this.History.Last().Alias;
        }
    }
}
