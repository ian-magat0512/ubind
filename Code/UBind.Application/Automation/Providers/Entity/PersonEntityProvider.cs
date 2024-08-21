// <copyright file="PersonEntityProvider.cs" company="uBind">
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
    /// This class is needed because we need to have a provider that we can use for searching person.
    /// This provider support the following searches:
    /// 1. Search by person Id.
    /// 2. Search by person Email Address.
    /// </summary>
    public class PersonEntityProvider : StaticEntityProvider
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IPersonReadModelRepository personReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The person id.</param>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public PersonEntityProvider(
            IProvider<Data<string>>? id,
            IPersonReadModelRepository personReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory,
            ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "person")
        {
            this.cachingResolver = cachingResolver;
            this.personReadModelRepository = personReadModelRepository;
        }

        /// <inheritdoc/>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var includedProperties = this.GetPropertiesToInclude(typeof(Person));
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            string? entityReference = this.resolvedEntityId;
            if (string.IsNullOrWhiteSpace(entityReference))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "personId",
                    this.SchemaReferenceKey));
            }

            var personDetails = default(IPersonReadModelWithRelatedEntities);
            if (Guid.TryParse(entityReference, out Guid personId))
            {
                personDetails = this.personReadModelRepository.GetPersonWithRelatedEntities(tenantId, personId, includedProperties);
            }

            if (personDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("personId", entityReference);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.Person.Humanize(), "personId", entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<PersonReadModel>)(await this.SerialisedEntityFactory.Create(personDetails, includedProperties)));
        }
    }
}
