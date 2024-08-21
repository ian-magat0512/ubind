// <copyright file="ProductSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Web.Validation;

    /// <summary>
    /// Resource model for product details.
    /// </summary>
    public class ProductSetModel : IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductSetModel"/> class.
        /// </summary>
        /// <param name="product">The product.</param>
        public ProductSetModel(IProductSummary product)
        {
            if (product != null)
            {
                this.Id = product.Id;
                this.Alias = product.Details.Alias;
                this.Name = product.Details.Name;
                this.TenantName = product.TenantName;
                this.Status = product.GetStatus();
                this.Deleted = product.Details.Deleted;
                this.Disabled = product.TenantDisabled ? true : product.Details.Disabled;
                this.CreatedDateTime = product.CreatedTimestamp.ToExtendedIso8601String();
                this.CreatedTicksSinceEpoch = product.CreatedTimestamp.ToUnixTimeTicks();
                this.LastModifiedDateTime = product.Details.CreatedTimestamp.ToExtendedIso8601String();
                this.LastModifiedTicksSinceEpoch = product.Details.CreatedTimestamp.ToUnixTimeTicks();
                this.TenantId = product.TenantId;
                this.QuoteExpirySettings = product.Details.QuoteExpirySetting;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductSetModel"/> class.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="tenant">The tenant.</param>
        public ProductSetModel(Product product, Tenant tenant)
        {
            if (product != null)
            {
                this.Id = product.Id;
                this.Alias = product.Details.Alias;
                this.Name = product.Details.Name;
                this.TenantName = tenant.Details.Name;
                this.Status = product.GetStatus();
                this.Deleted = product.Details.Deleted;
                this.Disabled = tenant.Details.Disabled ? true : product.Details.Disabled;
                this.CreatedDateTime = product.CreatedTimestamp.ToExtendedIso8601String();
                this.CreatedTicksSinceEpoch = product.CreatedTimestamp.ToUnixTimeTicks();
                this.LastModifiedDateTime = product.Details.CreatedTimestamp.ToExtendedIso8601String();
                this.LastModifiedTicksSinceEpoch = product.Details.CreatedTimestamp.ToUnixTimeTicks();
                this.TenantId = product.TenantId;
                this.QuoteExpirySettings = product.Details.QuoteExpirySetting;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductSetModel"/> class.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="tenant">The tenant.</param>
        /// <param name="additionalPropertyValueDtos">List of <see cref="AdditionalPropertyValueDto"/>.</param>
        public ProductSetModel(
            Product product, Tenant tenant, List<AdditionalPropertyValueDto> additionalPropertyValueDtos)
            : this(product, tenant)
        {
            if (additionalPropertyValueDtos != null && additionalPropertyValueDtos.Any())
            {
                this.AdditionalPropertyValues = additionalPropertyValueDtos.Select(
                    apv => new AdditionalPropertyValueModel(apv)).ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductSetModel"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for JSON deserializer.</remarks>
        public ProductSetModel()
        {
        }

        /// <summary>
        /// Gets the product Id.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [Required(ErrorMessage = "Product ID is required.")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        [Required(ErrorMessage = "Product name is required.")]
        [RegularExpression(".*[a-zA-Z0-9].*", ErrorMessage = "Product Name must contain at least one alphanumeric character.")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the product status.
        /// </summary>
        /// <remarks>Public setter required for JSON serialization.</remarks>
        [JsonProperty]
        public string Status { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the portal is deleted.
        /// </summary>
        /// <remarks>Public setter required for JSON serialization.</remarks>
        [JsonProperty]
        public bool Deleted { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether a claim component exists or not.
        /// </summary>
        public bool HasClaimComponent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a claim component exists or not.
        /// </summary>
        public bool HasQuoteComponent { get; set; }

        /// <summary>
        /// Gets a value indicating whether the portal is disabled.
        /// </summary>
        [JsonProperty]
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets the time the product was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the time the product was created in ticks since epoch.
        /// </summary>
        [JsonProperty]
        public long CreatedTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the time the product was last modified.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets the time the product was last modified in ticks since epoch.
        /// </summary>
        [JsonProperty]
        public long LastModifiedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the ID of the product's tenant.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the quote expiry settings of the product.
        /// </summary>
        [JsonProperty]
        public QuoteExpirySettings QuoteExpirySettings { get; private set; }

        /// <summary>
        /// Gets the name of the product's tenant.
        /// </summary>
        [JsonProperty]
        public string TenantName { get; private set; }

        /// <summary>
        /// Gets the alias of the product.
        /// </summary>
        [JsonProperty]
        [RegularExpression(
            @ValidationExpressions.Alias,
            ErrorMessage = "Alias must only contain lowercase, alphabetic characters and hyphens. It must not end or begin with a hyphen.")]
        public string Alias { get; private set; }

        /// <inheritdoc/>
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; }
    }
}
