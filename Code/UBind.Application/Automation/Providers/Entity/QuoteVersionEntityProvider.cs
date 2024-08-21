// <copyright file="QuoteVersionEntityProvider.cs" company="uBind">
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
    /// This class is needed because we need to have a provider that we can use for searching quote version.
    /// This provider support the following searches:
    /// 1. Search by Quote Version Id.
    /// 2. Search by Quote Id and Version Number.
    /// 3. Search by Quote Reference and Version Number.
    /// </summary>
    public class QuoteVersionEntityProvider : StaticEntityProvider
    {
        private readonly IQuoteVersionReadModelRepository quoteVersionReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The quote version id.</param>
        /// <param name="quoteVersionReadModelRepository">The quote version read model repository.</param>
        public QuoteVersionEntityProvider(
            IProvider<Data<string>>? id,
            IQuoteVersionReadModelRepository quoteVersionReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "quoteVersion")
        {
            this.quoteVersionReadModelRepository = quoteVersionReadModelRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The quote version id.</param>
        /// <param name="quoteId">The quote id.</param>
        /// <param name="quoteReference">The quote reference.</param>
        /// <param name="versionNumber">The quote version number.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="quoteVersionReadModelRepository">The quote version read model repository.</param>
        public QuoteVersionEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? quoteId,
            IProvider<Data<string>>? quoteReference,
            IProvider<Data<long>>? versionNumber,
            IProvider<Data<string>>? environment,
            IQuoteVersionReadModelRepository quoteVersionReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "quoteVersion")
        {
            this.QuoteId = quoteId;
            this.QuoteReference = quoteReference;
            this.VersionNumber = versionNumber;
            this.Environment = environment;
            this.quoteVersionReadModelRepository = quoteVersionReadModelRepository;
        }

        /// <summary>
        /// Gets or sets the quote id.
        /// </summary>
        private IProvider<Data<string>>? QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the quote reference.
        /// </summary>
        private IProvider<Data<string>>? QuoteReference { get; set; }

        /// <summary>
        /// Gets or sets the quote environment.
        /// </summary>
        private IProvider<Data<string>>? Environment { get; set; }

        /// <summary>
        /// Gets or sets the quote version number.
        /// </summary>
        private IProvider<Data<long>>? VersionNumber { get; set; }

        /// <summary>
        /// Method for retrieving quote version entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The quote version entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var quoteId = (await this.QuoteId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var reference = (await this.QuoteReference.ResolveValueIfNotNull(providerContext))?.DataValue;

            long versionNumber = (await this.VersionNumber.ResolveValueIfNotNull(providerContext))?.DataValue ?? 0;
            var environmentParam = (await this.Environment.ResolveValueIfNotNull(providerContext))?.DataValue;
            DeploymentEnvironment? environment = this.EntityId == null
                ? this.GetEnvironment(environmentParam, providerContext) : null;

            var includedProperties = this.GetPropertiesToInclude(typeof(QuoteVersion));
            var quoteVersionDetails = default(IQuoteVersionReadModelWithRelatedEntities);

            string entityReference;
            string entityReferenceType;
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReference = this.resolvedEntityId;
                entityReferenceType = "quoteVersionId";
                if (Guid.TryParse(this.resolvedEntityId, out Guid quoteVersionId))
                {
                    quoteVersionDetails = this.quoteVersionReadModelRepository.GetQuoteVersionWithRelatedEntities(
                        tenantId, environment, quoteVersionId, includedProperties);
                }
            }
            else if (!string.IsNullOrWhiteSpace(quoteId))
            {
                entityReference = $"{quoteId}-{versionNumber}";
                entityReferenceType = "quoteId";
                quoteVersionDetails = this.quoteVersionReadModelRepository.GetQuoteVersionWithRelatedEntities(
                    tenantId, environment, new Guid(quoteId), (int)versionNumber, includedProperties);
            }
            else if (!string.IsNullOrWhiteSpace(reference))
            {
                entityReference = $"{reference}-{versionNumber}";
                entityReferenceType = "quoteNumber";
                quoteVersionDetails = this.quoteVersionReadModelRepository.GetQuoteVersionWithRelatedEntities(
                    tenantId, reference, environment, (int)versionNumber, includedProperties);
            }
            else
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "quoteVersionId",
                    this.SchemaReferenceKey));
            }

            if (quoteVersionDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("quoteVersionId", this.resolvedEntityId);
                }

                if (!string.IsNullOrWhiteSpace(quoteId))
                {
                    errorData.Add("quoteId", quoteId);
                }

                if (!string.IsNullOrWhiteSpace(reference))
                {
                    errorData.Add("quoteReference", reference);
                }

                if (!string.IsNullOrWhiteSpace(environmentParam))
                {
                    errorData.Add("entityEnvironment", environmentParam);
                }

                errorData.Add("versionNumber", versionNumber);
                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.QuoteVersion.Humanize(),
                    entityReferenceType,
                    entityReference,
                    errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<QuoteVersionReadModel>)(await this.SerialisedEntityFactory.Create(quoteVersionDetails, includedProperties)));
        }
    }
}
