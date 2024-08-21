// <copyright file="PortalEntityProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using Humanizer;
    using MorseCode.ITask;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using Portal = UBind.Domain.SerialisedEntitySchemaObject.Portal;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching portal.
    /// This provider support the following searches:
    /// 1. Search by Portal Id.
    /// 2. Search by Portal Alias.
    /// </summary>
    public class PortalEntityProvider : StaticEntityProvider
    {
        private readonly IPortalReadModelRepository portalReadModelRepository;

        public PortalEntityProvider(
            IProvider<Data<string>>? id,
            IPortalReadModelRepository portalReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "portal")
        {
            this.portalReadModelRepository = portalReadModelRepository;
        }

        public PortalEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? portalAlias,
            IPortalReadModelRepository portalReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "portal")
        {
            this.PortalAlias = portalAlias;
            this.portalReadModelRepository = portalReadModelRepository;
        }

        /// <summary>
        /// Gets or sets the portal alias.
        /// </summary>
        private IProvider<Data<string>>? PortalAlias { get; set; }

        /// <summary>
        /// Method for retrieving portal entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The portal entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var portalAlias = (await this.PortalAlias.ResolveValueIfNotNull(providerContext))?.DataValue;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;

            var includedProperties = this.GetPropertiesToInclude(typeof(Portal));
            var portalDetails = default(IPortalWithRelatedEntities);

            string entityReference;
            string entityReferenceType;
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReference = this.resolvedEntityId;
                entityReferenceType = "portalId";
                if (Guid.TryParse(this.resolvedEntityId, out Guid portalId))
                {
                    portalDetails =
                        this.portalReadModelRepository.GetPortalWithRelatedEntities(tenantId, portalId, includedProperties);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(portalAlias))
                {
                    throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                        "portalAlias",
                        this.SchemaReferenceKey));
                }

                entityReference = portalAlias;
                entityReferenceType = "portalAlias";
                portalDetails =
                    this.portalReadModelRepository.GetPortalWithRelatedEntitiesByAlias(tenantId, portalAlias, includedProperties);
            }

            if (portalDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("portalId", this.resolvedEntityId);
                }

                if (!string.IsNullOrWhiteSpace(portalAlias))
                {
                    errorData.Add("portalAlias", portalAlias);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.Portal.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<PortalReadModel>)(await this.SerialisedEntityFactory.Create(portalDetails, includedProperties)));
        }
    }
}
