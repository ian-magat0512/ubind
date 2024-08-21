// <copyright file="OrganisationEntityProvider.cs" company="uBind">
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
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching organisation.
    /// This provider support the following searches:
    /// 1. Search by Organisation Id.
    /// 2. Search by Organisation Alias.
    /// </summary>
    public class OrganisationEntityProvider : StaticEntityProvider
    {
        private readonly IOrganisationReadModelRepository organisationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The organisation id.</param>
        /// <param name="organisationRepository">The organisation repository.</param>
        public OrganisationEntityProvider(
            IProvider<Data<string>>? id,
            IOrganisationReadModelRepository organisationRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "organisation")
        {
            this.organisationRepository = organisationRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The organisation id.</param>
        /// <param name="organisationAlias">The organisation alias.</param>
        /// <param name="organisationRepository">The organisation repository.</param>
        public OrganisationEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? organisationAlias,
            IOrganisationReadModelRepository organisationRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "organisation")
        {
            this.OrganisationAlias = organisationAlias;
            this.organisationRepository = organisationRepository;
        }

        /// <summary>
        /// Gets or sets the organisation alias.
        /// </summary>
        private IProvider<Data<string>>? OrganisationAlias { get; set; }

        /// <summary>
        /// Method for retrieving organisation entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The organisation entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var organisationAlias = (await this.OrganisationAlias.ResolveValueIfNotNull(providerContext))?.DataValue;
            string? entityReference = this.resolvedEntityId ?? organisationAlias;
            if (entityReference == null)
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "organisationAlias",
                    this.SchemaReferenceKey));
            }

            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var includedProperties = this.GetPropertiesToInclude(typeof(Organisation));
            var organisationDetails = default(IOrganisationReadModelWithRelatedEntities);
            string entityReferenceType;
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReferenceType = "organisationId";
                if (Guid.TryParse(entityReference, out Guid organisationId))
                {
                    organisationDetails = this.organisationRepository.GetOrganisationWithRelatedEntities(
                        tenantId, organisationId, includedProperties);
                }
            }
            else
            {
                entityReferenceType = "organisationAlias";
                organisationDetails = this.organisationRepository.GetOrganisationWithRelatedEntities(
                    tenantId, entityReference, includedProperties);
            }

            if (organisationDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("organisationId", entityReference);
                }

                if (!string.IsNullOrWhiteSpace(organisationAlias))
                {
                    errorData.Add("organisationAlias", entityReference);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.Organisation.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<OrganisationReadModel>)(await this.SerialisedEntityFactory.Create(organisationDetails, includedProperties)));
        }
    }
}
