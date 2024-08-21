// <copyright file="ProductCreateRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Web.Validation;

    /// <summary>
    /// Resource model for creating a product.
    /// </summary>
    public class ProductCreateRequestModel
    {
        /// <summary>
        /// Gets or sets the Id of the product.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [EntityName]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alias of the product.
        /// </summary>
        [Required]
        [Alias]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the Id or Alias of the tenant the product is for.
        /// </summary>
        [Required]
        public string Tenant { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets additional property values.
        /// </summary>
        [JsonProperty(PropertyName = "properties")]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }
    }
}
