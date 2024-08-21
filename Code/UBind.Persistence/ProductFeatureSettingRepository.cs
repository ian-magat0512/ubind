// <copyright file="ProductFeatureSettingRepository.cs" company="uBind">
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
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;

    /// <summary>
    /// The product feature repository.
    /// </summary>
    public class ProductFeatureSettingRepository : IProductFeatureSettingRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFeatureSettingRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public ProductFeatureSettingRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public ProductFeatureSetting AddProductFeatureSetting(ProductFeatureSetting productFeature)
        {
            var createdProductFeature = this.dbContext.ProductFeatureSetting.Add(productFeature);
            this.dbContext.SaveChanges();
            return createdProductFeature;
        }

        /// <inheritdoc/>
        public ProductFeatureSetting EnableProductFeature(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureType)
        {
            var productFeature = this.GetProductFeatureSetting(tenantId, productId);
            productFeature.Enable(productFeatureType);
            this.dbContext.SaveChanges();
            return productFeature;
        }

        /// <inheritdoc/>
        public ProductFeatureSetting? GetProductFeatureSetting(Guid tenantId, Guid productId)
        {
            using (MiniProfiler.Current.Step($"{nameof(ProductFeatureSettingRepository)}.{nameof(this.GetProductFeatureSetting)}"))
            {
                ProductFeatureSetting productFeature = this.dbContext.ProductFeatureSetting.Where(p =>
                p.TenantId == tenantId &&
                p.ProductId == productId).FirstOrDefault();
                return productFeature;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ProductFeatureSetting> GetProductFeatureByTenantId(Guid tenantId)
        {
            List<ProductFeatureSetting> productFeatures = this.dbContext.ProductFeatureSetting.Where(p =>
                p.TenantId == tenantId).ToList();
            return productFeatures;
        }

        /// <inheritdoc/>
        public List<ProductFeatureSetting> GetDeployedProductFeatureSettings(
            Guid tenantId,
            DeploymentEnvironment environment)
        {
            using (MiniProfiler.Current.Step($"{nameof(ProductFeatureSettingRepository)}.{nameof(this.GetDeployedProductFeatureSettings)}"))
            {
                var query = this.dbContext.ProductFeatureSetting.Where(pfs => pfs.TenantId == tenantId)
                    .Join(
                    this.dbContext.Products
                        .Where(e => e.TenantId == tenantId)
                        .Select(p => new
                        {
                            Product = p,
                            ProductDetails = p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                            .FirstOrDefault(),
                        })
                        .Select(ppd => ppd.Product.Id),
                    pfs => pfs.ProductId,
                    productId => productId,
                    (pfs, productId) => pfs);

                if (environment == DeploymentEnvironment.Development)
                {
                    query = query.Join(
                            this.dbContext.DevReleases
                                .GroupBy(dr => dr.ProductId, (key, g) => g.OrderBy(r => r.CreatedTicksSinceEpoch)
                                .FirstOrDefault()),
                            pfs => pfs.ProductId,
                            dr => dr.ProductId,
                            (pfs, dr) => pfs);
                }
                else
                {
                    query = query.Join(
                            this.dbContext.Deployments
                                .Where(d => d.Environment == environment)
                                .GroupBy(dp => dp.ProductId, (key, g) => g.OrderBy(r => r.CreatedTicksSinceEpoch)
                                .FirstOrDefault()),
                            pfs => pfs.ProductId,
                            d => d.ProductId,
                            (pfs, d) => pfs);
                }

                return query.ToList();
            }
        }

        /// <inheritdoc/>
        public ProductFeatureSetting DisableProductFeature(
            Guid tenantId,
            Guid productId,
            ProductFeatureSettingItem productFeatureType)
        {
            var productFeature = this.GetProductFeatureSetting(tenantId, productId);
            productFeature.Disable(productFeatureType);
            this.dbContext.SaveChanges();
            return productFeature;
        }

        /// <inheritdoc/>
        public void UpdateProductFeatureRenewalSetting(
            Guid tenantId,
            Guid productId,
            bool allowRenewalAfterExpiry,
            Duration expiredPolicyRenewalDuration)
        {
            var productFeature = this.GetProductFeatureSetting(tenantId, productId);
            productFeature.UpdateProductFeatureRenewalSetting(allowRenewalAfterExpiry, expiredPolicyRenewalDuration);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void UpdateRefundSettings(
        Guid tenantId,
        Guid productId,
        RefundRule refundRule,
        PolicyPeriodCategory? cancellationPeriod,
        int? lastNumberOfYearsWhichNoClaimsMade)
        {
            var productFeature = this.GetProductFeatureSetting(tenantId, productId);
            productFeature.UpdateCancellationSetting(refundRule, cancellationPeriod, lastNumberOfYearsWhichNoClaimsMade);
            this.dbContext.SaveChanges();
        }
    }
}
