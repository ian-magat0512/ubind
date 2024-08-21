// <copyright file="ReleaseUpsertModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.ProductRelease
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Resource model for creating/updating a release.
    /// </summary>
    public class ReleaseUpsertModel
    {
        /// <summary>
        /// Gets or sets the Id or Alias of the product.
        /// </summary>
        [Required]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the Id or Alias of the tenant.
        /// </summary>
        [Required]
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets disabled.
        /// </summary>
        [Required(ErrorMessage = "Release description is required.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets deleted.
        /// </summary>
        [Required(ErrorMessage = "Release type is required.")]
        public ReleaseType Type { get; set; }
    }
}
