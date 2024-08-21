// <copyright file="IProductRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Product;

    /// <summary>
    /// Repository for storing products.
    /// </summary>
    public interface IProductRepository : IRepository
    {
        /// <summary>
        /// Insert a new product into the repository.
        /// </summary>
        /// <param name="product">The product to insert.</param>
        void Insert(Product product);

        /// <summary>
        /// Retrieve a product from the repository by Alias.
        /// </summary>
        /// <param name="tenantId">Th guid ID of the product's tenant and part of product's compound keys.</param>
        /// <param name="productAlias">The string Alias of the product and part of product's compound keys.</param>
        /// <returns>The product with the given compound ID, or null if none found.</returns>
        Product? GetProductByAlias(Guid tenantId, string productAlias);

        /// <summary>
        /// Retrieves product using the old string id ( which was removed ).
        /// Note: Please dont use this outside events, for backward compatibility use only.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productStringId">The old product string Id.</param>
        /// <returns>The product.</returns>
        Product GetProductByStringId(Guid tenantId, string productStringId);

        /// <summary>
        /// Retrieves a product from the repository based on its compound ID.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the product's tenant, forming part of the compound keys.</param>
        /// <param name="productId">The unique identifier of the product, forming part of the compound keys.</param>
        /// <param name="includeDeleted"> A flag indicating whether to include deleted products
        /// in the search for the specified tenant and product IDs.</param>
        /// <returns>The product with the specified compound ID, or null if no matching product is found.</returns>
        Product? GetProductById(Guid tenantId, Guid productId, bool includeDeleted = false);

        /// <summary>
        /// Retrieves latest product alias from the repository based on product ID.
        /// </summary>
        /// <param name="productId">The unique identifier of the product, forming part of the compound keys.</param>
        /// <returns>The product with the specified compound ID, or null if no matching product is found.</returns>
        Task<string?> GetProductAliasById(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieve all active products from the repo that satisfy the given filters.
        /// </summary>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the products in the repo.</returns>
        IEnumerable<IProductSummary> GetProductSummaries(Guid tenantId, ProductReadModelFilters filters);

        /// <summary>
        /// Generate a query that retreives all active products from the repo that satisfy the given filters.
        /// </summary>
        /// <param name="filters">Filters to apply.</param>
        /// <returns>All the products in the repo.</returns>
        IQueryable<IProductSummary> GetProductSummariesQuery(Guid tenantId, ProductReadModelFilters filters);

        /// <summary>
        /// Retrieve all active/disabled products from the repo for a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>All the products in the repo.</returns>
        IEnumerable<IProductSummary> GetAllProductSummariesForTenant(Guid tenantId);

        /// <summary>
        /// Retrieves all products, including active or disabled ones, for a given tenant from the repository.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <param name="includeDeleted">A flag indicating whether to include deleted products in the search for the specified tenant.</param>
        /// <returns>An enumerable collection of products in the repository.</returns>
        /// <returns>All the products in the repo.</returns>
        IEnumerable<Product> GetAllProductsForTenant(Guid tenantId, bool includeDeleted = false);

        /// <summary>
        /// Retrieve all active products from the repo for a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>All active products in the repo.</returns>
        IEnumerable<IProductSummary> GetAllActiveProductSummariesForTenant(Guid tenantId);

        /// <summary>
        /// Retrieves all active products from the repo.
        /// </summary>
        /// <returns>All the active products in the repository.</returns>
        IEnumerable<Product> GetActiveProducts();

        /// <summary>
        /// Retrieves all products from the repo, including deleted ones.
        /// </summary>
        /// <returns>All products in the repository.</returns>
        IEnumerable<Product> GetAllProducts();

        /// <summary>
        /// Checks if a given product name is available in a given tenant.
        /// </summary>
        /// <param name="tenantId">The guid ID of the tenant to check in.</param>
        /// <param name="name">The name to check.</param>
        /// <param name="productId">The guid ID of the product to check in.</param>
        /// <returns><c>true</c> if the name is available, otherwise <c>false</c>.</returns>
        bool ProductNameIsAvailableInTenant(Guid tenantId, string name, Guid productId);

        /// <summary>
        /// Checks if a given product ID is available in a given tenant.
        /// </summary>
        /// <param name="tenantId">The guid ID of the tenant to check in.</param>
        /// <param name="productId">The guid ID of the product to check in.</param>
        /// <returns><c>true</c> if the name is available, otherwise <c>false</c>.</returns>
        bool ProductIdIsAvailableInTenant(Guid tenantId, Guid productId);

        /// <summary>
        /// Checks if a given product name is available in a given tenant.
        /// </summary>
        /// <param name="tenantId">The guid ID of the tenant to check in.</param>
        /// <param name="alias">The alias to check.</param>
        /// <param name="productId">The guid ID of the product to check in.</param>
        /// <returns><c>true</c> if the name is available, otherwise <c>false</c>.</returns>
        bool ProductAliasIsAvailableInTenant(Guid tenantId, string alias, Guid productId);

        /// <summary>
        /// Check if a given product ID was already deleted in a given tenantId.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to check in.</param>
        /// <param name="productId">The product ID to check.</param>
        /// <returns>The deleted status if the product exists, otherwise <c>false</c>.</returns>
        bool ProductIdWasDeletedInTenant(Guid tenantId, Guid productId);

        /// <summary>
        /// Gets the product with related entities by id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The product read model with related entities.</returns>
        IProductWithRelatedEntities GetProductWithRelatedEntities(Guid tenantId, Guid productId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Method for creating IQueryable method that retrieve products and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for products.</returns>
        IQueryable<IProductWithRelatedEntities> CreateQueryForProductDetailsWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Save any changes to products.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Gets the list of product ids.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="skip">The current page index.</param>
        /// <param name="pageSize">The set max count per page.</param>
        /// <returns>List of product ids.</returns>
        List<Guid> GetProductIdsByTenant(Guid tenantId, int skip, int pageSize);

        IEnumerable<IProductSummary> GetDeployedActiveProducts(Guid tenantId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets all products within the tenant, matching a given filter.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the request is for.</param>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the products within the tenant matching a given filter.</returns>
        IEnumerable<IProductWithRelatedEntities> GetProductsWithRelatedEntities(
            Guid tenantId, ProductReadModelFilters filters, IEnumerable<string> relatedEntities);
    }
}
