// <copyright file="TenantEntityProvider.cs" company="uBind">
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
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching tenant.
    /// This provider support the following searches:
    /// 1. Search by Tenant Id.
    /// 2. Search by Tenant Alias.
    /// </summary>
    public class TenantEntityProvider : StaticEntityProvider
    {
        private readonly ITenantRepository tenantRepository;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The tenant id.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public TenantEntityProvider(
            IProvider<Data<string>>? id,
            ITenantRepository tenantRepository,
            ISerialisedEntityFactory serialisedEntityFactory,
            ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "tenant")
        {
            this.cachingResolver = cachingResolver;
            this.tenantRepository = tenantRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The tenant id.</param>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public TenantEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? tenantAlias,
            ITenantRepository tenantRepository,
            ISerialisedEntityFactory serialisedEntityFactory,
            ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "tenant")
        {
            this.cachingResolver = cachingResolver;
            this.TenantAlias = tenantAlias;
            this.tenantRepository = tenantRepository;
        }

        /// <summary>
        /// Gets or sets the tenant alias.
        /// </summary>
        private IProvider<Data<string>>? TenantAlias { get; set; }

        /// <summary>
        /// Method for retrieving tenant entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The tenant entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var tenantAlias = (await this.TenantAlias.ResolveValueIfNotNull(providerContext))?.DataValue;

            if (this.resolvedEntityId == null && tenantAlias == null)
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "tenantAlias",
                    this.SchemaReferenceKey));
            }

            var includedProperties = this.GetPropertiesToInclude(typeof(Domain.SerialisedEntitySchemaObject.Tenant));
            var entityReferenceType = this.resolvedEntityId == null ? "tenantAlias" : "tenantId";
            string entityReference = this.resolvedEntityId ?? tenantAlias!;
            var tenantDetails = default(ITenantWithRelatedEntities);
            Domain.Tenant? tenant = await this.cachingResolver.GetTenantOrNull(new GuidOrAlias(entityReference));
            if (tenant != null)
            {
                tenantDetails = this.tenantRepository.GetTenantWithRelatedEntitiesById(tenant.Id, includedProperties);
            }

            if (tenantDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("entityTenantId", this.resolvedEntityId);
                }

                if (!string.IsNullOrWhiteSpace(tenantAlias))
                {
                    errorData.Add("entityTenantAlias", tenantAlias);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.Tenant.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<Domain.Tenant>)(await this.SerialisedEntityFactory.Create(tenantDetails, includedProperties)));
        }
    }
}
