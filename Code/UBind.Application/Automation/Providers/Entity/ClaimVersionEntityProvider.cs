// <copyright file="ClaimVersionEntityProvider.cs" company="uBind">
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
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching claim version.
    /// This provider support the following searches:
    /// 1. Search by Claim Version Id.
    /// 2. Search by Claim Id and Version Number.
    /// 2. Search by Claim Number and Version Number.
    /// 3. Search by Claim Reference and Version Number.
    /// </summary>
    public class ClaimVersionEntityProvider : StaticEntityProvider
    {
        private readonly IClaimVersionReadModelRepository claimVersionReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimVersionEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The claim id.</param>
        /// <param name="claimVersionReadModelRepository">The claim version read model repository.</param>
        public ClaimVersionEntityProvider(
            IProvider<Data<string>> id,
            IClaimVersionReadModelRepository claimVersionReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "claimVersion")
        {
            this.claimVersionReadModelRepository = claimVersionReadModelRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimVersionEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The claim version id.</param>
        /// <param name="claimId">The claim id.</param>
        /// <param name="claimReference">The claim reference.</param>
        /// <param name="claimNumber">The claim number.</param>
        /// <param name="versionNumber">The claim version number.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="claimVersionReadModelRepository">The claim version read model repository.</param>
        public ClaimVersionEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? claimId,
            IProvider<Data<string>>? claimReference,
            IProvider<Data<string>>? claimNumber,
            IProvider<Data<long>>? versionNumber,
            IProvider<Data<string>>? environment,
            IClaimVersionReadModelRepository claimVersionReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "claimVersion")
        {
            this.ClaimId = claimId;
            this.ClaimReference = claimReference;
            this.ClaimNumber = claimNumber;
            this.VersionNumber = versionNumber;
            this.Environment = environment;
            this.claimVersionReadModelRepository = claimVersionReadModelRepository;
        }

        /// <summary>
        /// Gets or sets the claim id.
        /// </summary>
        private IProvider<Data<string>>? ClaimId { get; set; }

        /// <summary>
        /// Gets or sets the claim reference.
        /// </summary>
        private IProvider<Data<string>>? ClaimReference { get; set; }

        /// <summary>
        /// Gets or sets the claim number.
        /// </summary>
        private IProvider<Data<string>>? ClaimNumber { get; set; }

        /// <summary>
        /// Gets or sets the claim environment.
        /// </summary>
        private IProvider<Data<string>>? Environment { get; set; }

        /// <summary>
        /// Gets or sets the claim version number.
        /// </summary>
        private IProvider<Data<long>>? VersionNumber { get; set; }

        /// <summary>
        /// Method for retrieving claim version entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The claim version entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var claimId = (await this.ClaimId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var claimNumber = (await this.ClaimNumber.ResolveValueIfNotNull(providerContext))?.DataValue;
            var reference = (await this.ClaimReference.ResolveValueIfNotNull(providerContext))?.DataValue;
            var versionNumber = (await this.VersionNumber.ResolveValueIfNotNull(providerContext))?.DataValue ?? 0;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;

            var environmentParam = (await this.Environment.ResolveValueIfNotNull(providerContext))?.DataValue;
            DeploymentEnvironment? environment = this.EntityId == null
                ? this.GetEnvironment(environmentParam, providerContext) : null;

            var includedProperties = this.GetPropertiesToInclude(typeof(ClaimVersion));

            var entityReference = string.Empty;
            var entityReferenceType = string.Empty;
            var claimVersionDetails = default(IClaimVersionReadModelWithRelatedEntities);
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReference = this.resolvedEntityId;
                entityReferenceType = "claimVersionId";
                if (Guid.TryParse(this.resolvedEntityId, out Guid claimVersionId))
                {
                    claimVersionDetails = this.claimVersionReadModelRepository
                        .GetClaimVersionWithRelatedEntities(tenantId, environment, claimVersionId, includedProperties);
                }
            }
            else if (!string.IsNullOrWhiteSpace(claimId))
            {
                entityReference = $"{claimId}-{versionNumber}";
                entityReferenceType = "claimId";
                claimVersionDetails = this.claimVersionReadModelRepository
                    .GetClaimVersionWithRelatedEntities(tenantId, environment, new Guid(claimId), (int)versionNumber, includedProperties);
            }
            else if (!string.IsNullOrWhiteSpace(claimNumber))
            {
                entityReference = $"{claimNumber}-{versionNumber}";
                entityReferenceType = "claimNumber";
                claimVersionDetails = this.claimVersionReadModelRepository.GetClaimVersionWithRelatedEntitiesByNumber(
                    tenantId, claimNumber, environment, (int)versionNumber, includedProperties);
            }
            else if (!string.IsNullOrWhiteSpace(reference))
            {
                entityReference = $"{reference}-{versionNumber}";
                claimVersionDetails =
                    this.claimVersionReadModelRepository.GetClaimVersionWithRelatedEntitiesByReference(
                        tenantId, reference, environment, (int)versionNumber, includedProperties);
            }

            if (claimVersionDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("claimVersionId", this.resolvedEntityId);
                }

                if (!string.IsNullOrWhiteSpace(claimId))
                {
                    errorData.Add("claimId", claimId);
                }

                if (!string.IsNullOrWhiteSpace(claimNumber))
                {
                    errorData.Add("claimNumber", claimNumber);
                }

                if (!string.IsNullOrWhiteSpace(reference))
                {
                    errorData.Add("claimReference", reference);
                }

                errorData.Add("versionNumber", versionNumber);

                if (!string.IsNullOrWhiteSpace(environmentParam))
                {
                    errorData.Add(ErrorDataKey.EntityEnvironment, environmentParam);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.ClaimVersion.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<ClaimVersionReadModel>)(await this.SerialisedEntityFactory.Create(claimVersionDetails, includedProperties)));
        }
    }
}
