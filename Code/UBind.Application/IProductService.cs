// <copyright file="IProductService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Application service for handling product-related functionality.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Create a new product and queue its initialization.
        /// </summary>
        /// <param name="productAlias">The alias of the product.</param>
        /// <param name="tenantId">The string ID of the tenant.</param>
        /// <param name="name">A descriptive name for the product.</param>
        /// <returns>The new product.</returns>
        Task<Domain.Product.Product> CreateProduct(Guid tenantId, string productAlias, string name, Guid? productId = null);

        /// <summary>
        /// Set up required infrastructure for new product inlcluding folders, workbook etc.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="createComponentFiles">If true, a sample workbook and asset files will be created for the quote
        /// and claim components.</param>
        /// <returns>A task that can be awaited.</returns>
        Task InitializeProduct(Guid tenantId, Guid productId, bool createComponentFiles = false);

        /// <summary>
        /// Update a product.
        /// </summary>
        /// <param name="tenantId">The guid ID of the product's tenant.</param>
        /// <param name="productId">The guid ID of the product.</param>
        /// <param name="name">The new name for the product.</param>
        /// <param name="alias">The new alias for the product.</param>
        /// <param name="disabled">The product status.</param>
        /// <param name="deleted">If the product is deleted.</param>
        /// <param name="productQuoteExpirySetting">sets the quote expiry days settings.</param>
        /// <param name="updateType">sets the update type for the product quote expiry settings.</param>
        /// <returns>The updated product.</returns>
        Task<Domain.Product.Product> UpdateProduct(
            Guid tenantId,
            Guid productId,
            string name,
            string alias,
            bool disabled,
            bool deleted,
            CancellationToken cancellationToken,
            QuoteExpirySettings? productQuoteExpirySetting = null,
            ProductQuoteExpirySettingUpdateType updateType = ProductQuoteExpirySettingUpdateType.UpdateNone);

        /// <summary>
        /// Update the deployment settings of a product.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="deploymentSettings">The deployment settings.</param>
        /// <returns>The updated product.</returns>
        Domain.Product.Product UpdateDeploymentSettings(Guid tenantId, Guid productId, ProductDeploymentSetting deploymentSettings);

        /// <summary>
        /// Update the quote expiry settings of a product.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="expirySettings">The expiry settings.</param>
        /// <param name="updateType">sets the update type for the product quote expiry settings.</param>
        /// <returns>The updated product.</returns>
        Domain.Product.Product UpdateQuoteExpirySettings(
            Guid tenantId,
            Guid productId,
            QuoteExpirySettings expirySettings,
            ProductQuoteExpirySettingUpdateType updateType,
            CancellationToken cancellationToken);

        /// <summary>
        /// retrieve the deployment settings of a product.
        /// </summary>
        /// <param name="tenantId">The ID of the product's tenant.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The deployment settings.</returns>
        ProductDeploymentSetting GetDeploymentSettings(Guid tenantId, Guid productId);

        /// <summary>
        /// Update the product if exists, otherwise create a new product.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="productId">A unique identifier for the product.</param>
        /// <param name="name">A descriptive name for the product.</param>
        /// <param name="disabled">The product status.</param>
        /// <param name="deleted">If the product is deleted.</param>
        /// <returns>The new or updated roduct.</returns>
        Task<Domain.Product.Product> CreateOrUpdateProduct(Guid tenantId, string productId, string name, bool disabled, bool deleted);

        /// <summary>
        /// Seeds files to the product tenant.
        /// </summary>
        /// <param name="tenantId">tenantId.</param>
        /// <param name="productId">productid.</param>
        /// <param name="environment">environment.</param>
        /// <param name="file">file.</param>
        /// <param name="folder">folder to place to files to.</param>
        /// <returns>OK.</returns>
        Task SeedFilesAsync(Guid tenantId, Guid productId, string environment, FileModel file, string folder);

        /// <summary>
        /// Retrieves a product filtered by tenant ID.
        /// </summary>
        /// <param name="tenantId">The Tenant Id.</param>
        /// <param name="productId">The Product Id.</param>
        /// <returns>An instance of product.</returns>
        Domain.Product.Product GetProductById(Guid tenantId, Guid productId);
    }
}
