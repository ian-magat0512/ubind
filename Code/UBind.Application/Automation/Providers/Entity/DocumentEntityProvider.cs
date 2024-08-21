// <copyright file="DocumentEntityProvider.cs" company="uBind">
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
    /// This class is needed because we need to have a provider that we can use for searching document.
    /// This provider support the following searches:
    /// 1. Search by Document Id.
    /// </summary>
    public class DocumentEntityProvider : StaticEntityProvider
    {
        private readonly IQuoteDocumentReadModelRepository documentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The document id.</param>
        /// <param name="documentRepository">The quote document read model repository.</param>
        public DocumentEntityProvider(
            IProvider<Data<string>>? id,
            IQuoteDocumentReadModelRepository documentRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "document")
        {
            this.documentRepository = documentRepository;
        }

        /// <summary>
        /// Method for retrieving document entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The document entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            var includedProperties = this.GetPropertiesToInclude(typeof(Document));
            var document = default(IDocumentReadModelWithRelatedEntities);
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "documentId",
                    this.SchemaReferenceKey));
            }

            if (Guid.TryParse(this.resolvedEntityId, out var documentId))
            {
                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                document = this.documentRepository.GetDocumentWithRelatedEntities(
                    tenantId, providerContext.AutomationData.System.Environment, documentId, includedProperties);
            }

            if (document == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("documentId", this.resolvedEntityId);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.Document.Humanize(), "documentId", this.resolvedEntityId, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<DocumentEntity>)(await this.SerialisedEntityFactory.Create(document, includedProperties)));
        }
    }
}
