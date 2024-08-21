// <copyright file="ProductRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Product;
    using UBind.Domain.Repositories;
    using UBind.Persistence.ReadModels;

    /// <inheritdoc/>
    public class ProductRepository : IProductRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public ProductRepository(
            IUBindDbContext dbContext)
        {
            Contract.Assert(dbContext != null);
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets an expression for instantiating product summaries from products and tenants for use in EF projections.
        /// </summary>
        public Expression<Func<Product, Tenant, IProductSummary>> SummarySelector =>
            (p, t) => new ProductSummary
            {
                Id = p.Id,
                CreatedTicksSinceEpoch = p.CreatedTicksSinceEpoch,
                DetailsCollection = p.DetailsCollection,
                Events = p.Events,
                TenantId = p.TenantId,
                TenantName = t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name,
                TenantDisabled = t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Disabled,
            };

        /// <inheritdoc/>
        public IEnumerable<Product> GetActiveProducts()
        {
            return this.dbContext.Products
               .IncludeAllProperties()
               .Where(p => !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted)
               .OrderBy(p => p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name);
        }

        /// <inheritdoc/>
        public IEnumerable<Product> GetAllProducts()
        {
            return this.dbContext.Products
               .IncludeAllProperties()
               .OrderBy(p => p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name)
               .ToList();
        }

        /// <inheritdoc/>
        public Product? GetProductById(Guid tenantId, Guid productId, bool includeDeleted = false)
        {
            var query = this.dbContext.Products
                .IncludeAllProperties()
                .Where(p => p.TenantId == tenantId
                    && p.Id == productId);

            if (!includeDeleted)
            {
                return query
                    .Where(p => !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted)
                    .FirstOrDefault();
            }

            return query.FirstOrDefault();
        }

        public async Task<string?> GetProductAliasById(Guid tenantId, Guid productId)
        {
            var query = this.dbContext.Products
               .IncludeAllProperties()
               .Where(p => p.TenantId == tenantId
                   && p.Id == productId);

            return await query.Select(p => p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Alias)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public Product? GetProductByAlias(Guid tenantId, string productAlias)
        {
            return this.dbContext.Products
                .IncludeAllProperties()
                .Where(p => p.TenantId == tenantId
                    && p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Alias == productAlias
                    && !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public Product GetProductByStringId(Guid tenantId, string productStringId)
        {
            // This function is rarely called, so we're leaving the .ToList() in here because we don't know if we can remove it
            // and safely know if it still works.
            return this.dbContext.Products
                .Where(x => x.TenantId == tenantId)
               .IncludeAllProperties()
               .ToList()
               .Where(p =>
               string.Equals(p.GetStringId(), productStringId, StringComparison.OrdinalIgnoreCase))
               .FirstOrDefault();
        }

        /// <inheritdoc/>
        public IEnumerable<IProductSummary> GetAllProductSummariesForTenant(Guid tenantId)
        {
            var query = this.dbContext.Products.IncludeAllProperties()
                .Where(p => p.TenantId == tenantId)
                .Join(
                    this.dbContext.Tenants.IncludeAllProperties(),
                    p => p.TenantId,
                    t => t.Id,
                    this.SummarySelector)
                .ToList();
            return query;
        }

        /// <inheritdoc/>
        public IEnumerable<Product> GetAllProductsForTenant(Guid tenantId, bool includeDeleted = false)
        {
            var query = this.dbContext.Products
               .Where(p => p.TenantId == tenantId);

            if (!includeDeleted)
            {
                return query
                    .Where(p => !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted)
                    .IncludeAllProperties();
            }

            return query.IncludeAllProperties();
        }

        /// <inheritdoc/>
        public IEnumerable<IProductSummary> GetAllActiveProductSummariesForTenant(Guid tenantId)
        {
            return this.GetAllProductSummariesForTenant(tenantId)
                .Where(p => !p.DetailsCollection
                    .OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted);
        }

        /// <inheritdoc/>
        public IEnumerable<IProductSummary> GetDeployedActiveProducts(Guid tenantId, DeploymentEnvironment environment)
        {
            var query = this.dbContext.Products.IncludeAllProperties()
                .Where(p => p.TenantId == tenantId)
                .Join(
                    this.dbContext.Tenants.IncludeAllProperties(),
                    p => p.TenantId,
                    t => t.Id,
                    this.SummarySelector);

            if (environment == DeploymentEnvironment.Development)
            {
                query = query.Join(
                        this.dbContext.DevReleases
                            .GroupBy(dr => dr.ProductId, (key, g) => g.OrderBy(r => r.CreatedTicksSinceEpoch)
                            .FirstOrDefault()),
                        p => p.Id,
                        dr => dr.ProductId,
                        (p, dr) => p);
            }
            else
            {
                query = query.Join(
                        this.dbContext.Deployments
                            .Where(d => d.Environment == environment)
                            .GroupBy(dp => dp.ProductId, (key, g) => g.OrderBy(r => r.CreatedTicksSinceEpoch)
                            .FirstOrDefault()),
                        p => p.Id,
                        d => d.ProductId,
                        (p, d) => p);
            }

            return query.Where(p => !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted
                && !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Disabled).ToList();
        }

        public IEnumerable<IProductSummary> GetProductSummaries(Guid tenantId, ProductReadModelFilters filters)
        {
            return this.GetProductSummariesQuery(tenantId, filters).AsEnumerable();
        }

        /// <inheritdoc/>
        public IQueryable<IProductSummary> GetProductSummariesQuery(Guid tenantId, ProductReadModelFilters filters)
        {
            var activeTenants = this.dbContext.Tenants.IncludeAllProperties()
                .Where(t => !t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted);
            var query = this.dbContext.Products.IncludeAllProperties()
                .Join(
                    this.dbContext.Tenants.IncludeAllProperties(), // the source table of the inner join
                    product => product.TenantId,        // Select the primary key (the first part of the "on" clause in an sql "join" statement)
                    tenant => tenant.Id,   // Select the foreign key (the second part of the "on" clause)
                    this.SummarySelector) // selection
                .Where(x => x.TenantId == tenantId)
                .Where(p => !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted)
                .Where(p => activeTenants.Any(t => t.Id == p.TenantId));

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<IProductSummary>(false);
                foreach (var status in filters.Statuses)
                {
                    statusPredicate = statusPredicate.Or(this.GetStatusExpression(status));
                }

                query = query.Where(statusPredicate);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<IProductSummary>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(p =>
                        p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                            .FirstOrDefault().Name.Contains(searchTerm)
                        || p.Id.ToString().Contains(searchTerm)
                        || p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                            .FirstOrDefault().Alias.Contains(searchTerm)
                        || p.TenantId.ToString().Contains(searchTerm)
                        || p.TenantId.ToString().Contains(searchTerm));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(Product.CreatedTicksSinceEpoch))
                {
                    query = query.Where(t => t.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
                else
                {
                    query = query.Where(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                        .FirstOrDefault().CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(Product.CreatedTicksSinceEpoch))
                {
                    query = query.Where(p => p.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
                else
                {
                    query = query.Where(p => p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                        .FirstOrDefault().CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
            }

            if (filters.HasFeatureSettingsEnabled != null && filters.HasFeatureSettingsEnabled.Any())
            {
                var productFeatures = this.dbContext.ProductFeatureSetting;
                foreach (var featureSettingItem in filters.HasFeatureSettingsEnabled)
                {
                    switch (featureSettingItem)
                    {
                        case ProductFeatureSettingItem.NewBusinessQuotes:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.AreNewBusinessQuotesEnabled));
                            break;
                        case ProductFeatureSettingItem.AdjustmentQuotes:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.AreAdjustmentQuotesEnabled));
                            break;
                        case ProductFeatureSettingItem.RenewalQuotes:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.AreRenewalQuotesEnabled));
                            break;
                        case ProductFeatureSettingItem.CancellationQuotes:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.AreCancellationQuotesEnabled));
                            break;
                        case ProductFeatureSettingItem.Claims:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.IsClaimsEnabled));
                            break;
                        case ProductFeatureSettingItem.NewBusinessPolicyTransactions:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.AreNewBusinessPolicyTransactionsEnabled));
                            break;
                        case ProductFeatureSettingItem.AdjustmentPolicyTransactions:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.AreAdjustmentPolicyTransactionsEnabled));
                            break;
                        case ProductFeatureSettingItem.RenewalPolicyTransactions:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.AreRenewalPolicyTransactionsEnabled));
                            break;
                        case ProductFeatureSettingItem.CancellationPolicyTransactions:
                            query = query.Where(p => productFeatures.Any(t => t.ProductId == p.Id && t.TenantId == p.TenantId && t.AreCancellationPolicyTransactionsEnabled));
                            break;
                    }
                }
            }

            if (filters.Environment != null)
            {
                if (filters.Environment == DeploymentEnvironment.Development)
                {
                    if (filters.HasComponentTypes != null && filters.HasComponentTypes.Any())
                    {
                        var componentTypeExpression = PredicateBuilder.New<DevRelease>(false);
                        foreach (var componentType in filters.HasComponentTypes)
                        {
                            switch (componentType)
                            {
                                case WebFormAppType.Quote:
                                    componentTypeExpression.Or(r => r.QuoteDetails != null);
                                    break;
                                case WebFormAppType.Claim:
                                    componentTypeExpression.Or(r => r.ClaimDetails != null);
                                    break;
                                default:
                                    throw new ErrorException(Errors.General.UnexpectedEnumValue(componentType, typeof(WebFormAppType)));
                            }
                        }

                        query = query.Join(
                            this.dbContext.DevReleases
                               .Where(dr => dr.TenantId == tenantId)
                               .Where(componentTypeExpression)
                               .GroupBy(dr => dr.ProductId, (key, g) => g.OrderBy(r => r.CreatedTicksSinceEpoch)
                               .FirstOrDefault()),
                            product => product.Id,
                            dr => dr.ProductId,
                            (product, dr) => product);
                    }
                    else
                    {
                        query = query.Join(
                           this.dbContext.DevReleases
                               .Where(dr => dr.TenantId == tenantId)
                               .GroupBy(dr => dr.ProductId, (key, g) => g.OrderBy(r => r.CreatedTicksSinceEpoch)
                               .FirstOrDefault()),
                           product => product.Id,
                           dr => dr.ProductId,
                           (product, dr) => product);
                    }
                }

                if (filters.Environment == DeploymentEnvironment.Staging || filters.Environment == DeploymentEnvironment.Production)
                {
                    if (filters.HasComponentTypes != null && filters.HasComponentTypes.Any())
                    {

                        var componentTypeExpression = PredicateBuilder.New<Deployment>(false);
                        foreach (var componentType in filters.HasComponentTypes)
                        {
                            switch (componentType)
                            {
                                case WebFormAppType.Quote:
                                    componentTypeExpression.Or(d => d.Release.QuoteDetails != null);
                                    break;
                                case WebFormAppType.Claim:
                                    componentTypeExpression.Or(d => d.Release.ClaimDetails != null);
                                    break;
                                default:
                                    throw new ErrorException(Errors.General.UnexpectedEnumValue(componentType, typeof(WebFormAppType)));
                            }
                        }

                        query = query.Join(
                            this.dbContext.Deployments
                               .Where(d => d.TenantId == tenantId && d.Environment == filters.Environment)
                               .Where(componentTypeExpression)
                               .GroupBy(dr => dr.ProductId, (key, g) => g.OrderBy(r => r.CreatedTicksSinceEpoch)
                               .FirstOrDefault()),
                            product => product.Id,
                            dr => dr.ProductId,
                            (product, dr) => product);
                    }
                    else
                    {
                        query = query.Join(
                           this.dbContext.Deployments
                               .Where(d => d.TenantId == tenantId)
                               .Where(d => d.Environment == filters.Environment)
                               .GroupBy(dr => dr.ProductId, (key, g) => g.OrderBy(r => r.CreatedTicksSinceEpoch)
                               .FirstOrDefault()),
                           product => product.Id,
                           dr => dr.ProductId,
                           (product, dr) => product);
                    }
                }
            }

            if (filters.PortalAlias != null)
            {
                var portalId =
                    this.dbContext.PortalReadModels
                        .FirstOrDefault(p => p.TenantId == tenantId && p.Alias == filters.PortalAlias).Id;

                var productPortalSettings = this.dbContext.ProductPortalSettings
                    .Where(s => s.TenantId == tenantId && s.PortalId == portalId && s.IsNewQuotesAllowed)
                    .Select(p => p.ProductId);
                query = query.Where(p => productPortalSettings.Any(s => s == p.Id));
            }

            bool hasOrganisationFilters = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilters)
            {
                var productOrgSettings =
                    this.dbContext.ProductOrganisationSettings
                    .Where(s => filters.OrganisationIds.Contains(s.OrganisationId) && s.IsNewQuotesAllowed);
                query = query.Where(p => productOrgSettings.Any(s => s.ProductId == p.Id));
            }

            // because someone can sort by "Details.Name", we need to go .ToList() now, since Details is not a database column.
            // This will be resolved when we make Tenant and Product have a read model.
            query = query.ToList().AsQueryable();

            return query.Order(filters.SortBy, filters.SortOrder);
        }

        /// <inheritdoc/>
        public void Insert(Product product)
        {
            this.dbContext.Products.Add(product);
        }

        /// <inheritdoc/>
        public bool ProductIdIsAvailableInTenant(Guid tenantId, Guid productId)
        {
            var product = this.dbContext.Products
                .Where(p => p.TenantId == tenantId
                    && p.Id != productId
                    && !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted)
                .IncludeAllProperties()
                .FirstOrDefault();

            return product == null;
        }

        /// <inheritdoc/>
        public bool ProductNameIsAvailableInTenant(Guid tenantId, string name, Guid productId)
        {
            var product = this.dbContext.Products
                .Where(p => p.TenantId == tenantId
                    && p.Id != productId
                    && !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted)
                .IncludeAllProperties()
                .ToList()
                .Where(p => string.Equals(p.Details.Name, name, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            return product == null;
        }

        /// <inheritdoc/>
        public bool ProductAliasIsAvailableInTenant(Guid tenantId, string alias, Guid productId)
        {
            var product = this.dbContext.Products
                .Where(p =>
                    p.TenantId == tenantId
                    && p.Id != productId
                    && !p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Deleted)
                .IncludeAllProperties()
                .ToList()
                .Where(p =>
                    string.Equals(p.Details.Alias, alias, StringComparison.OrdinalIgnoreCase)
                    || p.Details.Alias.Equals(alias, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault();

            return product == null;
        }

        /// <inheritdoc/>
        public bool ProductIdWasDeletedInTenant(Guid tenantId, Guid productId)
        {
            var product = this.dbContext.Products
                .Where(p => p.TenantId == tenantId && p.Id == productId)
                .IncludeAllProperties()
                .FirstOrDefault();

            return product != null ? product.Details.Deleted : false;
        }

        /// <inheritdoc/>
        public IProductWithRelatedEntities GetProductWithRelatedEntities(Guid tenantId, Guid productId, IEnumerable<string> relatedEntities)
        {
            var productWithRelatedEntities = this.CreateQueryForProductDetailsWithRelatedEntities(tenantId, relatedEntities)
                .FirstOrDefault(x => x.Product.Id == productId);
            return productWithRelatedEntities ?? default;
        }

        /// <inheritdoc/>
        public IQueryable<IProductWithRelatedEntities> CreateQueryForProductDetailsWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities)
        {
            var query = from product in this.dbContext.Products.IncludeAllProperties()
                        where product.TenantId == tenantId
                        select new ProductWithRelatedEntities()
                        {
                            Tenant = default,
                            TenantDetails = default,
                            Product = product,
                            Details = product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault(),
                            TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                            StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                        };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Product.Tenant))))
            {
                query = query.Join(
                    this.dbContext.Tenants.IncludeAllProperties(),
                    e => e.Product.TenantId,
                    t => t.Id,
                    (e, tenant) => new ProductWithRelatedEntities
                    {
                        Tenant = tenant,
                        TenantDetails = tenant.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault(),
                        Product = e.Product,
                        Details = e.Details,
                        TextAdditionalPropertiesValues = e.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = e.StructuredDataAdditionalPropertyValues,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Product.AdditionalProperties))))
            {
                query = query.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    p => p.Product.Id,
                    apv => apv.EntityId,
                    (p, apv) =>
                new ProductWithRelatedEntities
                {
                    Tenant = p.Tenant,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    Details = p.Details,
                    TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                        .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                })
                .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    p => p.Product.Id,
                    apv => apv.EntityId,
                    (p, apv) =>
                new ProductWithRelatedEntities
                {
                    Tenant = p.Tenant,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    Details = p.Details,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv
                        .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                });
            }

            return query;
        }

        public IEnumerable<IProductWithRelatedEntities> GetProductsWithRelatedEntities(
            Guid tenantId, ProductReadModelFilters filters, IEnumerable<string> relatedEntities)
        {
            var products = this.GetProductSummariesQuery(tenantId, filters)
                .Select(p => new ProductWithRelatedEntities
                {
                    Tenant = default,
                    TenantDetails = default,
                    Product = new Product(
                        p.TenantId,
                        p.Id,
                        p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name,
                        p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Alias,
                        p.CreatedTimestamp),
                    Details = p.Details,
                    TextAdditionalPropertiesValues = default,
                    StructuredDataAdditionalPropertyValues = default
                });

            return this.CreateQueryForProductDetailsWithRelatedEntities(products, relatedEntities);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public List<Guid> GetProductIdsByTenant(Guid tenantId, int skip, int pageSize)
        {
            var queryable = this.dbContext.Products.Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedTicksSinceEpoch).Skip(skip).Take(pageSize);
            return queryable.Select(p => p.Id).ToList();
        }

        private IEnumerable<ProductWithRelatedEntities> CreateQueryForProductDetailsWithRelatedEntities(
            IEnumerable<ProductWithRelatedEntities> query, IEnumerable<string> relatedEntities)
        {
            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Product.Tenant))))
            {
                query = query.Join(
                    this.dbContext.Tenants.IncludeAllProperties(),
                    e => e.Product.TenantId,
                    t => t.Id,
                    (e, tenant) => new ProductWithRelatedEntities
                    {
                        Tenant = tenant,
                        TenantDetails = tenant.Details,
                        Product = e.Product,
                        Details = e.Details,
                        TextAdditionalPropertiesValues = e.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = e.StructuredDataAdditionalPropertyValues,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Product.AdditionalProperties))))
            {
                query = query.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    p => p.Product.Id,
                    apv => apv.EntityId,
                    (p, apv) =>
                new ProductWithRelatedEntities
                {
                    Tenant = p.Tenant,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    Details = p.Details,
                    TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                        .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                })
                .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    p => p.Product.Id,
                    apv => apv.EntityId,
                    (p, apv) =>
                new ProductWithRelatedEntities
                {
                    Tenant = p.Tenant,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    Details = p.Details,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv
                        .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                });
            }

            return query;
        }

        private Expression<Func<IProductSummary, bool>> GetStatusExpression(string status)
        {
            if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return product => !product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Disabled;
            }

            if (status.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            {
                return product => product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Disabled;
            }

            throw new InvalidOperationException("Unknown product status " + status.ToString());
        }
    }
}
