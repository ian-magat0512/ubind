// <copyright file="ProductOrganisationSettingRepository.cs" company="uBind">
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
    using UBind.Domain.Aggregates;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class ProductOrganisationSettingRepository : IProductOrganisationSettingRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;
        private readonly IOrganisationSystemEventEmitter organisationSystemEventEmitter;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductOrganisationSettingRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="clock">The system clock.</param>
        public ProductOrganisationSettingRepository(
            IUBindDbContext dbContext,
            IClock clock,
            IOrganisationSystemEventEmitter organisationSystemEventEmitter,
            IHttpContextPropertiesResolver httpContextPropertiesResolver)
        {
            this.dbContext = dbContext;
            this.clock = clock;
            this.organisationSystemEventEmitter = organisationSystemEventEmitter;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        /// <inheritdoc/>
        public IEnumerable<ProductOrganisationSettingModel> GetProductOrganisationSettings(
            Guid tenantId, Guid organisationId)
        {
            var tenantProducts = this.GetTenantProducts(tenantId).ToList();

            var data = tenantProducts.GroupJoin(
                this.dbContext.ProductOrganisationSettings.Where(p => p.OrganisationId == organisationId),
                prd => prd.Id,
                pps => pps.ProductId,
                (prd, pps) => new
                {
                    prd,
                    pps,
                }).ToList();

            var settings = data.Select(s => new ProductOrganisationSettingModel(
                s.prd.Details.Name,
                organisationId,
                s.prd.Id,
                s.pps.Any() && s.pps.FirstOrDefault().IsNewQuotesAllowed,
                0));

            return settings.ToList();
        }

        /// <inheritdoc/>
        public ProductOrganisationSetting? GetProductOrganisationSetting(Guid tenantId, Guid organisationId, Guid productId)
        {
            return this.dbContext.ProductOrganisationSettings
                .Where(pos => pos.TenantId == tenantId && pos.OrganisationId == organisationId && pos.ProductId == productId)
                .SingleOrDefault();
        }

        /// <inheritdoc/>
        public async Task<ProductOrganisationSetting> UpdateProductSetting(
            Guid tenantId, Guid organisationId, Guid productId, bool isNewQuotesAllowed)
        {
            var setting =
                this.dbContext.ProductOrganisationSettings.FirstOrDefault(
                    s => s.OrganisationId == organisationId && s.ProductId == productId);
            if (setting == null)
            {
                setting = new ProductOrganisationSetting(
                    tenantId, organisationId, productId, isNewQuotesAllowed, this.clock.Now());
                this.dbContext.ProductOrganisationSettings.Add(setting);
            }
            else
            {
                setting.SetNewQuotesAllowed(isNewQuotesAllowed);
            }

            this.dbContext.SaveChanges();
            await this.organisationSystemEventEmitter.CreateAndEmitModifiedSystemEvent(
                tenantId,
                organisationId);
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
