// <copyright file="QuoteEntityProvider.cs" company="uBind">
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
    /// This class is needed because we need to have a provider that we can use for searching quote.
    /// This provider support the following searches:
    /// 1. Search by Quote Id.
    /// 2. Search by Quote Reference.
    /// </summary>
    public class QuoteEntityProvider : StaticEntityProvider
    {
        private readonly IQuoteReadModelRepository quoteReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The quote id.</param>
        /// <param name="quoteReadModelRepository">The quote read model repository.</param>
        public QuoteEntityProvider(
            IProvider<Data<string>>? id,
            IQuoteReadModelRepository quoteReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "quote")
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The quote id.</param>
        /// <param name="quoteReference">The quote reference.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="quoteReadModelRepository">The quote read model repository.</param>
        public QuoteEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? quoteReference,
            IProvider<Data<string>>? environment,
            IQuoteReadModelRepository quoteReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "quote")
        {
            this.QuoteReference = quoteReference;
            this.Environment = environment;
            this.quoteReadModelRepository = quoteReadModelRepository;
        }

        /// <summary>
        /// Gets or sets the quote reference.
        /// </summary>
        private IProvider<Data<string>>? QuoteReference { get; set; }

        /// <summary>
        /// Gets or sets the quote environment.
        /// </summary>
        private IProvider<Data<string>>? Environment { get; set; }

        /// <summary>
        /// Method for retrieving quote entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The quote entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var reference = (await this.QuoteReference.ResolveValueIfNotNull(providerContext))?.DataValue;
            var environmentParam = (await this.Environment.ResolveValueIfNotNull(providerContext))?.DataValue;
            DeploymentEnvironment? environment = this.EntityId == null
                ? this.GetEnvironment(environmentParam, providerContext) : null;

            var includedProperties = this.GetPropertiesToInclude(typeof(Quote));
            var quoteDetails = default(IQuoteReadModelWithRelatedEntities);
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;

            string entityReference;
            string entityReferenceType;
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReference = this.resolvedEntityId;
                entityReferenceType = "quoteId";
                if (Guid.TryParse(this.resolvedEntityId, out Guid quoteId))
                {
                    quoteDetails = this.quoteReadModelRepository.GetQuoteWithRelatedEntities(
                        tenantId, environment, quoteId, includedProperties);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(reference))
                {
                    throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                        "quoteReference",
                        this.SchemaReferenceKey));
                }

                entityReference = reference;
                entityReferenceType = "quoteNumber";
                quoteDetails = this.quoteReadModelRepository
                    .GetQuoteWithRelatedEntities(tenantId, reference, environment, includedProperties);
            }

            if (quoteDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("quoteId", this.resolvedEntityId);
                }

                if (!string.IsNullOrWhiteSpace(reference))
                {
                    errorData.Add("quoteNumber", reference);
                }

                if (!string.IsNullOrWhiteSpace(environmentParam))
                {
                    errorData.Add(ErrorDataKey.EntityEnvironment, environmentParam);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.Quote.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<NewQuoteReadModel>)(await this.SerialisedEntityFactory.Create(quoteDetails, includedProperties)));
        }
    }
}
