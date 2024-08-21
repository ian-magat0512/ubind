// <copyright file="Product.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// This class is needed because we need to generate json representation of product entity that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Product : EntitySupportingAdditionalProperties<Domain.Product.Product>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        /// <param name="id">The id of the entity.</param>
        public Product(Guid id)
            : base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        /// <param name="model">The product read model with related entities.</param>
        public Product(UBind.Domain.Product.Product model, string baseApiUrl, ICachingResolver cachingResolver)
            : base(model.Id, model.CreatedTicksSinceEpoch, model.Details?.CreatedTicksSinceEpoch)
        {
            this.Alias = model.Details.Alias;
            this.TenantId = model.TenantId.ToString();
            this.Name = model.Details?.Name;

            var tenantAlias = cachingResolver.GetTenantAliasOrThrow(model.TenantId);
            this.AssetUrl = $"{baseApiUrl}/assets/{tenantAlias}/{model.Details.Alias}";
            this.Disabled = model.Details?.Disabled ?? true;

            this.EntityDescriptor = this.Name;
            this.EntityReference = this.Name;
        }

        public Product(
            IProductWithRelatedEntities model,
            IEnumerable<string> includedProperties,
            string baseApiUrl,
            ICachingResolver cachingResolver)
            : this(model.Product, baseApiUrl, cachingResolver)
        {
            if (model.Tenant != null)
            {
                this.Tenant = new Tenant(model.Tenant);
            }

            this.PopulateAdditionalProperties(model, includedProperties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        [JsonConstructor]
        private Product()
        {
        }

        /// <summary>
        /// Gets or sets the alias of the product.
        /// </summary>
        [JsonProperty(PropertyName = "alias", Order = 3)]
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 4)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tenant id of the product.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the product.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 23)]
        public Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the URL of the assets folder for this product environment.
        /// </summary>
        [JsonProperty(PropertyName = "assetUrl", Order = 24)]
        public string AssetUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the flag if the product is still enable.
        /// </summary>
        [JsonProperty(PropertyName = "disabled", Order = 25)]
        [Required]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the additional properties for this product.
        /// </summary>
        [JsonProperty(PropertyName = "additionalProperties", Order = 26)]
        public override Dictionary<string, object> AdditionalProperties { get; set; }
    }
}
