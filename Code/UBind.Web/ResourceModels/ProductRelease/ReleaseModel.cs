// <copyright file="ReleaseModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.ProductRelease
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using NodaTime.Text;
    using UBind.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// A view model for a product release.
    /// </summary>
    public class ReleaseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseModel"/> class.
        /// </summary>
        /// <param name="release">The release.</param>
        /// <param name="product">The parent product for the release.</param>
        public ReleaseModel(Release release, Product product = null)
        {
            if (release != null)
            {
                this.Id = release.Id;
                this.TenantId = release.TenantId;
                this.ProductId = release.ProductId;
                this.ProductAlias = product != null ? product.Details.Alias : string.Empty;
                this.ProductName = product != null ? product?.Details?.Name ?? string.Empty : string.Empty;
                this.Number = release.Number + "." + release.MinorNumber;
                this.CreatedDateTime = InstantPattern.ExtendedIso.Format(release.CreatedTimestamp);
                this.Description = release?.Description ?? string.Empty;
                this.Type = release.Type;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for JSON deserializer.
        /// .</remarks>
        public ReleaseModel()
        {
        }

        /// <summary>
        /// Gets or sets the release ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product alias.
        /// </summary>
        public string ProductAlias { get; set; }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        public string ProductName { get; private set; }

        /// <summary>
        /// Gets or sets the number of the release.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the time the release was created in ISO 8601 format.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the release label.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [Required(ErrorMessage = "Release description is required.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the release type.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [Required(ErrorMessage = "Release type is required.")]
        public ReleaseType Type { get; set; }
    }
}
