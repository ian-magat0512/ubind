// <copyright file="ProductPortalSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class ProductPortalSettingRepository : IProductPortalSettingRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPortalSettingRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="clock">The system clock.</param>
        public ProductPortalSettingRepository(IUBindDbContext dbContext, IClock clock)
        {
            this.dbContext = dbContext;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public void CreateSettingsForAllDefaultOrganisationPortals(Guid tenantId, Guid productId)
        {
            var tenant = this.dbContext.Tenants.IncludeAllProperties().Where(t => t.Id == tenantId).SingleOrDefault();
            var portals = this.dbContext.PortalReadModels.Where(p => p.TenantId == tenantId).ToList();
            foreach (var portal in portals)
            {
                bool isDefaultOrganisation = tenant.Details.DefaultOrganisationId == portal.OrganisationId;

                if (isDefaultOrganisation)
                {
                    // create product portal settings if they don't exist.
                    this.AddOrUpdateProductSetting(tenantId, portal.Id, productId, true);
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ProductPortalSettingModel> GetProductPortalSettings(Guid tenantId, Guid portalId)
        {
            var tenant = this.dbContext.Tenants.IncludeAllProperties().Where(t => t.Id == tenantId).SingleOrDefault();
            var tenantProducts = this.GetTenantProducts(tenantId).ToList();
            var portal = this.dbContext.PortalReadModels.Where(p => p.TenantId == tenantId && p.Id == portalId).SingleOrDefault();
            bool isDefaultOrganisation = tenant.Details.DefaultOrganisationId == portal.OrganisationId;

            if (isDefaultOrganisation)
            {
                var tenantProductsPortalSettings = tenantProducts.GroupJoin(
                this.dbContext.ProductPortalSettings.Where(p => p.TenantId == tenantId && p.PortalId == portalId),
                product => product.Id,
                pps => pps.ProductId,
                (product, pps) => new
                {
                    product,
                    pps,
                }).ToList();

                var settings = tenantProductsPortalSettings.Select(
                    s => new ProductPortalSettingModel(
                        s.product.Details.Name,
                        portalId,
                        s.product.Id,
                        s.pps.Any() ? s.pps.FirstOrDefault().IsNewQuotesAllowed : true,
                        0));

                return settings.ToList();
            }
            else
            {
                var productOrganisationSettings = this.dbContext.ProductOrganisationSettings.Where(p => p.TenantId == tenantId && p.OrganisationId == portal.OrganisationId && p.IsNewQuotesAllowed).ToList();
                var organisationAvailableProducts = productOrganisationSettings.Join(
                    tenantProducts,
                    organisationProducts => organisationProducts.ProductId,
                    products => products.Id,
                    (organisationProducts, product) => new { OrganisationProduct = organisationProducts, Product = product }).ToList();

                var productOrganisationPortalSettings = organisationAvailableProducts.GroupJoin(
                this.dbContext.ProductPortalSettings.Where(p => p.TenantId == tenantId && p.PortalId == portalId),
                prd => prd.Product.Id,
                pps => pps.ProductId,
                (prd, pps) => new
                {
                    prd.Product,
                    pps,
                }).ToList();

                var settings = productOrganisationPortalSettings.Select(s => new ProductPortalSettingModel(
                    s.Product.Details.Name,
                    portalId,
                    s.Product.Id,
                    s.pps.Any() && s.pps.FirstOrDefault().IsNewQuotesAllowed,
                    0));

                return settings.ToList();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ProductPortalSettingModel> GetProductPortalSettings(Guid tenantId, string portalAlias)
        {
            var portalId = this.dbContext.PortalReadModels.Select(p => new { p.Id })
                .FirstOrDefault().Id;
            return this.GetProductPortalSettings(tenantId, portalId);
        }

        /// <inheritdoc/>
        public ProductPortalSetting AddOrUpdateProductSetting(
            Guid tenantId, Guid portalId, Guid productId, bool isNewQuotesAllowed)
        {
            var setting = this.dbContext.ProductPortalSettings
                .FirstOrDefault(s => s.PortalId == portalId && s.ProductId == productId);

            if (setting == null)
            {
                setting = new ProductPortalSetting(tenantId, portalId, productId, isNewQuotesAllowed, this.clock.Now());
                this.dbContext.ProductPortalSettings.Add(setting);
            }
            else
            {
                setting.SetNewQuotesAllowed(isNewQuotesAllowed);
            }

            this.dbContext.SaveChanges();

            return setting;
        }

        private IQueryable<Product> GetTenantProducts(Guid tenantId)
        {
            return this.dbContext.Products.IncludeAllProperties()
                .Where(p => p.TenantId == tenantId).AsEnumerable()
                .Where(p => !p.Details.Deleted && !p.Details.Disabled).AsQueryable();
        }
    }
}
