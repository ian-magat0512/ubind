// <copyright file="ProductEntityProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using Humanizer;
    using MorseCode.ITask;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching product.
    /// This provider support the following searches:
    /// 1. Search by Product Id.
    /// 2. Search by Product Alias.
    /// </summary>
    public class ProductEntityProvider : StaticEntityProvider
    {
        private readonly IProductRepository productRepository;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The product id.</param>
        /// <param name="productRepository">The product repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public ProductEntityProvider(
            IProvider<Data<string>>? id,
            IProductRepository productRepository,
            ISerialisedEntityFactory serialisedEntityFactory,
            ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "product")
        {
            this.cachingResolver = cachingResolver;
            this.productRepository = productRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The product id.</param>
        /// <param name="productAlias">The product alias.</param>
        /// <param name="productRepository">The product repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public ProductEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? productAlias,
            IProductRepository productRepository,
            ISerialisedEntityFactory serialisedEntityFactory,
            ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "product")
        {
            this.cachingResolver = cachingResolver;
            this.ProductAlias = productAlias;
            this.productRepository = productRepository;
        }

        /// <summary>
        /// Gets or sets the product alias.
        /// </summary>
        private IProvider<Data<string>>? ProductAlias { get; set; }

        /// <summary>
        /// Method for retrieving product entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The product entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var productAlias = (await this.ProductAlias.ResolveValueIfNotNull(providerContext))?.DataValue;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;

            var includedProperties = this.GetPropertiesToInclude(typeof(Domain.SerialisedEntitySchemaObject.Product));
            string entityReference;
            string entityReferenceType;
            Domain.Product.Product? product;
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReference = this.resolvedEntityId;
                entityReferenceType = "productId";
                product = await this.cachingResolver.GetProductOrNull(tenantId, new GuidOrAlias(this.resolvedEntityId));
            }
            else
            {
                if (string.IsNullOrEmpty(productAlias))
                {
                    throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                        "productAlias",
                        this.SchemaReferenceKey));
                }

                entityReference = productAlias;
                entityReferenceType = "productAlias";
                product = await this.cachingResolver.GetProductByAliasOrNull(tenantId, productAlias);
            }

            if (product == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("entityProductId", this.resolvedEntityId);
                }

                if (!string.IsNullOrWhiteSpace(productAlias))
                {
                    errorData.Add("entityProductAlias", productAlias);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.Product.Humanize(), entityReferenceType, entityReference, errorData));
            }

            var productDetails = this.productRepository.GetProductWithRelatedEntities(product.TenantId, product.Id, includedProperties);
            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<Domain.Product.Product>)(await this.SerialisedEntityFactory.Create(productDetails, includedProperties)));
        }
    }
}
