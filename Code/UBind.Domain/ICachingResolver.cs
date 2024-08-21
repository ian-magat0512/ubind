// <copyright file="ICachingResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;

    /// <summary>
    /// This service is needed to retreive the tenant/product/portal etc.
    /// using different identification ( id or aliases identifiers).
    /// </summary>
    public interface ICachingResolver
    {
        /// <summary>
        /// Removes the cached tenant. This should be called when the tenant is modified.
        /// </summary>
        /// <param name="tenantId">The tenant Id to be removed.</param>
        /// <param name="tenantAliases">The tenant aliases to remove.</param>
        void RemoveCachedTenants(Guid tenantId, List<string> tenantAliases);

        /// <summary>
        /// Removes the cached product. This should be called when the product is modified.
        /// </summary>
        /// <param name="tenantId">The tenant Id to be removed.</param>
        /// <param name="productId">The product Id to be removed.</param>
        /// <param name="productAliases">The product aliases to remove.</param>
        void RemoveCachedProducts(Guid tenantId, Guid productId, List<string> productAliases);

        void CacheOrganisation(Guid tenantId, Guid organisationId, OrganisationReadModel organisation);

        /// <summary>
        /// Manually adding the tenant to the cache.
        /// This is used so that when we create a tenant, observers for organisation and portal created
        /// can get the tenant from the cache even before the tenant is persisted in the db.
        /// </summary>
        void CacheTenant(Guid tenantId, Tenant tenant);

        /// <summary>
        /// Removes caching for the product, to be able to call fresh data from caching resolver.
        /// </summary>
        /// <param name="tenantId">The tenant Id to be removed.</param>
        /// <param name="portalId">The portal Id to be removed.</param>
        /// <param name="portalAliases">The portal aliases to remove.</param>
        void RemoveCachedPortals(Guid tenantId, Guid portalId, List<string> portalAliases);

        /// <summary>
        /// Removes the cached organisation. This should be called when the organisation is modified.
        /// </summary>
        /// <param name="tenantId">The tenant Id to be removed.</param>
        /// <param name="organisationId">The organisation Id to be removed.</param>
        /// <param name="organisationAliases">The organisation aliases to remove.</param>
        void RemoveCachedOrganisations(Guid tenantId, Guid organisationId, List<string> organisationAliases);

        /// <summary>
        /// Removes caching for the feature settings, to be able to call fresh data from caching resolver.
        /// </summary>
        /// <param name="tenantId">The tenant Id to be removed.</param>
        void RemoveCachedFeatureSettings(Guid tenantId);

        /// <summary>
        /// Removes caching for the product settings, to be able to call fresh data from caching resolver.
        /// </summary>
        /// <param name="tenantId">The tenant Id to be removed.</param>
        /// <param name="productId">The product Id to be removed.</param>
        void RemoveCachedProductSettings(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieve the tenant by its alias, Throws error if not found.
        /// </summary>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <returns>The tenant.</returns>
        Task<Tenant> GetTenantByAliasOrThrow(string tenantAlias);

        /// <summary>
        /// Retreive the tenant by its alias, return null if not found.
        /// </summary>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <returns>the tenant.</returns>
        Task<Tenant?> GetTenantByAliasOrNull(string tenantAlias);

        /// <summary>
        /// Retrieve the tenant, throw error if not found.
        /// </summary>
        /// <param name="tenantIdOrAlias">The tenantId that can be intrepreted either a guid or a string.</param>
        /// <returns>the tenant.</returns>
        Task<Tenant> GetTenantOrThrow(GuidOrAlias tenantIdOrAlias);

        /// <summary>
        /// Retrieve the tenant, return null if not found.
        /// </summary>
        /// <param name="tenantIdOrAlias">The tenantId that can be intrepreted either a guid or a string.</param>
        /// <returns>the tenant.</returns>
        Task<Tenant?> GetTenantOrNull(GuidOrAlias tenantIdOrAlias);

        Task<Guid?> GetTenantIdOrNull(GuidOrAlias tenantIdOrAlias);

        Task<Guid> GetTenantIdOrThrow(GuidOrAlias tenantIdOrAlias);

        /// <summary>
        /// Retrieve the tenant, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <returns>the tenant.</returns>
        Task<Tenant> GetTenantOrThrow(Guid? tenantId);

        /// <summary>
        /// Retrieve the tenant, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <returns>the tenant.</returns>
        Task<Tenant?> GetTenantOrNull(Guid? tenantId);

        /// <summary>
        /// Retrieve the tenant's alias, throw error if not found.
        /// </summary>
        Task<string> GetTenantAliasOrThrowAsync(Guid tenantId);

        string GetTenantAliasOrThrow(Guid tenantId);

        /// <summary>
        /// Retrieve the product by alias, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productAlias">The product alias.</param>
        /// <returns>The product.</returns>
        Task<Product.Product> GetProductByAliasOrThrow(Guid tenantId, string productAlias);

        /// <summary>
        /// Retrieve the product by alias, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productAlias">The product alias.</param>
        /// <returns>The product.</returns>
        Task<Product.Product?> GetProductByAliasOrNull(Guid tenantId, string productAlias);

        /// <summary>
        /// Retrieve the product by alias, throw error if not found.
        /// </summary>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <param name="productAlias">The product alias.</param>
        /// <returns>The product.</returns>
        Task<Product.Product> GetProductByAliasOThrow(string tenantAlias, string productAlias);

        /// <summary>
        /// Retrieve the product by alias, return null if not found.
        /// </summary>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <param name="productAlias">The product alias.</param>
        /// <returns>The product.</returns>
        Task<Product.Product?> GetProductByAliasOrNull(string tenantAlias, string productAlias);

        /// <summary>
        /// Retrieve the product, throw error if not found.
        /// </summary>
        /// <param name="tenantIdOrAlias">The tenantId that can be intrepreted either a guid or a string.</param>
        /// <param name="productIdOrAlias">The productId that can be intrepreted either a guid or a string.</param>
        /// <returns>The product.</returns>
        Task<Product.Product> GetProductOrThrow(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias);

        Task<Product.Product> GetProductOrThrow(Tenant tenantModel, GuidOrAlias productIdOrAlias);

        /// <summary>
        /// Retrieve the product, return null if not found.
        /// </summary>
        /// <param name="tenantIdOrAlias">The tenantId that can be intrepreted either a guid or a string.</param>
        /// <param name="productIdOrAlias">The productId that can be intrepreted either a guid or a string.</param>
        /// <returns>The product.</returns>
        Task<Product.Product?> GetProductOrNull(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias);

        Task<Guid?> GetProductIdOrNull(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias);

        Task<Guid?> GetProductIdOrNull(Guid tenantId, GuidOrAlias productIdOrAlias);

        Task<Guid> GetProductIdOrThrow(GuidOrAlias tenantIdOrAlias, GuidOrAlias productIdOrAlias);

        Task<Guid> GetProductIdOrThrow(Guid tenantId, GuidOrAlias productIdOrAlias);

        /// <summary>
        /// Retrieve the product, throw error if not found.
        /// Sometimes just used to retrieve the productId from a vague idOrAlias string.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="productIdOrAlias">The productId that can be intrepreted either a guid or a string.</param>
        /// <returns>The product.</returns>
        Task<Product.Product> GetProductOrThrow(Guid tenantId, GuidOrAlias productIdOrAlias);

        /// <summary>
        /// Retrieve the product, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="productIdOrAlias">The productId that can be intrepreted either a guid or a string.</param>
        /// <returns>The product.</returns>
        Task<Product.Product?> GetProductOrNull(Guid tenantId, GuidOrAlias productIdOrAlias);

        /// <summary>
        /// Retrieve the product, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="productId">The productId.</param>
        /// <returns>The product.</returns>
        Task<Product.Product> GetProductOrThrow(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieve the product, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="productId">The productId.</param>
        /// <returns>The product.</returns>
        Task<Product.Product?> GetProductOrNull(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieve the organisation by alias, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationAlias">The organisation alias.</param>
        /// <returns>The product.</returns>
        Task<OrganisationReadModel> GetOrganisationByAliasOrThrow(Guid tenantId, string organisationAlias);

        /// <summary>
        /// Retrieve the organisation by alias, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="organisationAlias">The organisation alias.</param>
        /// <returns>The product.</returns>
        Task<OrganisationReadModel?> GetOrganisationByAliasOrNull(Guid tenantId, string organisationAlias);

        /// <summary>
        /// Retrieve the organisation, throw error if not found.
        /// </summary>
        /// <param name="tenantIdOrAlias">The tenantId that can be intrepreted either a guid or a string.</param>
        /// <param name="organisationIdOrAlias">The organisation that can be intrepreted either a guid or a string.</param>
        /// <returns>The product.</returns>
        Task<OrganisationReadModel> GetOrganisationOrThrow(GuidOrAlias tenantIdOrAlias, GuidOrAlias organisationIdOrAlias);

        Task<OrganisationReadModel> GetOrganisationOrThrow(Tenant tenantModel, GuidOrAlias organisationIdOrAlias);

        /// <summary>
        /// Retrieve the organisation, return null if not found.
        /// </summary>
        /// <param name="tenantIdOrAlias">The tenantId that can be intrepreted either a guid or a string.</param>
        /// <param name="organisationIdOrAlias">The organisation that can be intrepreted either a guid or a string.</param>
        /// <returns>The product.</returns>
        Task<OrganisationReadModel?> GetOrganisationOrNull(GuidOrAlias tenantIdOrAlias, GuidOrAlias organisationIdOrAlias);

        /// <summary>
        /// Retrieve the organisation, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="organisationIdOrAlias">The organisation that can be intrepreted either a guid or a string.</param>
        /// <returns>The product.</returns>
        Task<OrganisationReadModel> GetOrganisationOrThrow(Guid tenantId, GuidOrAlias organisationIdOrAlias);

        /// <summary>
        /// Retrieve the organisation, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="organisationIdOrAlias">The organisation that can be intrepreted either a guid or a string.</param>
        /// <returns>The product.</returns>
        Task<OrganisationReadModel?> GetOrganisationOrNull(Guid tenantId, GuidOrAlias organisationIdOrAlias);

        /// <summary>
        /// Retrieve the product, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <returns>The product.</returns>
        Task<OrganisationReadModel> GetOrganisationOrThrow(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Retrieve the organisation, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <returns>The product.</returns>
        Task<OrganisationReadModel?> GetOrganisationOrNull(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Gets the products alias.
        /// </summary>
        Task<string> GetProductAliasOrThrowAsync(Guid tenantId, Guid productId);

        string GetProductAliasOrThrow(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieves the portal, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The guid tenant id.</param>
        /// <param name="portalId">The portal Id.</param>
        /// <returns>the portal.</returns>
        Task<PortalReadModel> GetPortalOrThrow(Guid tenantId, Guid portalId);

        /// <summary>
        /// Retrieves the portal, return null if not found.
        /// </summary>
        /// <param name="tenantId">The guid tenant id.</param>
        /// <param name="portalId">The portal Id.</param>
        /// <returns>the portal.</returns>
        Task<PortalReadModel?> GetPortalOrNull(Guid tenantId, Guid portalId);

        /// <summary>
        /// Retrieves the portal, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="portalIdOrAlias">The portalId that can be intrepreted either a guid or a string alias.</param>
        /// <returns>the portal.</returns>
        Task<PortalReadModel> GetPortalOrThrow(Guid tenantId, GuidOrAlias portalIdOrAlias);

        Task<PortalReadModel> GetPortalOrThrow(Tenant tenantModel, GuidOrAlias portalIdOrAlias);

        /// <summary>
        /// Retrieves the portal, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="portalIdOrAlias">The portalId that can be intrepreted either a guid or a string alias.</param>
        /// <returns>the portal.</returns>
        Task<PortalReadModel?> GetPortalOrNull(Guid tenantId, GuidOrAlias portalIdOrAlias);

        /// <summary>
        /// Retrieves the feature setting, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The guid tenant id.</param>
        /// <returns>the feature setting.</returns>
        IEnumerable<Setting> GetSettingsOrThrow(Guid tenantId);

        /// <summary>
        /// Retrieves the feature setting, return null if not found.
        /// </summary>
        /// <param name="tenantId">The guid tenant id.</param>
        /// <returns>the feature setting.</returns>
        IEnumerable<Setting>? GetSettingsOrNull(Guid tenantId);

        /// <summary>
        /// Retrieves the active feature setting, throw error if not found.
        /// </summary>
        /// <param name="tenantId">The guid tenant id.</param>
        /// <returns>the feature setting.</returns>
        IEnumerable<Setting> GetActiveSettingsOrThrow(Guid tenantId);

        /// <summary>
        /// Retrieves the active feature setting, return null if not found.
        /// </summary>
        /// <param name="tenantId">The guid tenant id.</param>
        /// <returns>the feature setting.</returns>
        IEnumerable<Setting>? GetActiveSettingsOrNull(Guid tenantId);

        /// <summary>
        /// Retrieves a list of product settings, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="environment">The environment the product is deployed to.</param>
        /// <returns>a list of product settings.</returns>
        List<ProductFeatureSetting>? GetDeployedProductSettingsOrNull(Guid tenantId, DeploymentEnvironment environment);

        /// <summary>
        /// Retrieves a list of product settings, throws an error if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="environment">The environment the product is deployed to.</param>
        /// <returns>a list of product settings.</returns>
        List<ProductFeatureSetting> GetDeployedProductSettingsOrThrow(Guid tenantId, DeploymentEnvironment environment);

        /// <summary>
        /// Retrieves a list of product settings, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The product id.</param>
        /// <returns>the product settings.</returns>
        ProductFeatureSetting? GetProductSettingOrNull(Guid tenantId, Guid productId);

        /// <summary>
        /// Retrieves a list of product settings, return null if not found.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="productId">The product id.</param>
        /// <returns>the product settings.</returns>
        ProductFeatureSetting GetProductSettingOrThrow(Guid tenantId, Guid productId);

        /// <summary>
        /// Gets the product release ID for the given product release ID or number.
        /// </summary>
        /// <returns></returns>
        Task<Guid> GetProductReleaseIdOrThrow(Guid tenantId, Guid productId, GuidOrAlias productReleaseIdOrNumber);

        Task<List<Product.Product>> GetActiveProducts();
    }
}
