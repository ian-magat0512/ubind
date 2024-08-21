// <copyright file="SystemEventEntityProvider.cs" company="uBind">
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
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching systemEvent.
    /// This provider support the following searches:
    /// 1. Search by SystemEvent Id.
    /// </summary>
    public class SystemEventEntityProvider : StaticEntityProvider
    {
        private readonly ISystemEventRepository systemEventRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEventEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The systemEvent id.</param>
        /// <param name="systemEventRepository">The systemEvent read model repository.</param>
        /// <param name="serialisedEntityFactory">The factory for the serialised entities.</param>
        public SystemEventEntityProvider(
            IProvider<Data<string>>? id,
            ISystemEventRepository systemEventRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "event")
        {
            this.systemEventRepository = systemEventRepository;
        }

        /// <summary>
        /// Method for retrieving systemEvent entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The systemEvent entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "eventId",
                    this.SchemaReferenceKey));
            }

            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var includedProperties = this.GetPropertiesToInclude(typeof(Domain.SerialisedEntitySchemaObject.Event));
            var systemEventDetails = default(ISystemEventWithRelatedEntities);
            var entityReference = this.resolvedEntityId;
            string entityReferenceType = "eventId";
            if (Guid.TryParse(this.resolvedEntityId, out Guid systemEventId))
            {
                systemEventDetails = this.systemEventRepository.GetSystemEventWithRelatedEntities(
                    tenantId, systemEventId, includedProperties);
            }

            if (systemEventDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("eventId", this.resolvedEntityId);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.Event.ToString(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<SystemEvent>)await this.SerialisedEntityFactory.Create(systemEventDetails, includedProperties));
        }
    }
}
