// <copyright file="ClaimEntityProvider.cs" company="uBind">
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
    /// This class is needed because we need to have a provider that we can use for searching claim.
    /// This provider support the following searches:
    /// 1. Search by Claim Id.
    /// 2. Search by Claim Number.
    /// 3. Search by Claim Reference.
    /// </summary>
    public class ClaimEntityProvider : StaticEntityProvider
    {
        private readonly IClaimReadModelRepository claimReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The claim id.</param>
        /// <param name="claimReadModelRepository">The claim read model repository.</param>
        public ClaimEntityProvider(
            IProvider<Data<string>> id,
            IClaimReadModelRepository claimReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "claim")
        {
            this.claimReadModelRepository = claimReadModelRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The claim id.</param>
        /// <param name="claimReference">The claim reference.</param>
        /// <param name="claimNumber">The claim number.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="claimReadModelRepository">The claim read model repository.</param>
        public ClaimEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? claimReference,
            IProvider<Data<string>>? claimNumber,
            IProvider<Data<string>>? environment,
            IClaimReadModelRepository claimReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "claim")
        {
            this.ClaimReference = claimReference;
            this.ClaimNumber = claimNumber;
            this.Environment = environment;
            this.claimReadModelRepository = claimReadModelRepository;
        }

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
        /// Method for retrieving claim entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The claim entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var claimNumber = (await this.ClaimNumber.ResolveValueIfNotNull(providerContext))?.DataValue;
            var reference = (await this.ClaimReference.ResolveValueIfNotNull(providerContext))?.DataValue;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;

            var environmentParam = (await this.Environment.ResolveValueIfNotNull(providerContext))?.DataValue;
            DeploymentEnvironment? environment = this.EntityId == null
                ? this.GetEnvironment(environmentParam, providerContext) : null;

            if (string.IsNullOrWhiteSpace(this.resolvedEntityId) && string.IsNullOrWhiteSpace(claimNumber) && string.IsNullOrWhiteSpace(reference))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "claimNumber",
                    this.SchemaReferenceKey));
            }

            var includedProperties = this.GetPropertiesToInclude(typeof(Claim));
            var entityReference = this.resolvedEntityId ?? reference ?? claimNumber!;
            var claimDetails = default(IClaimReadModelWithRelatedEntities);

            string entityReferenceType;
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReferenceType = "claimId";
                if (Guid.TryParse(entityReference, out Guid claimId))
                {
                    claimDetails = this.claimReadModelRepository.GetClaimWithRelatedEntities(
                        tenantId, environment, claimId, includedProperties);
                }
            }
            else if (!string.IsNullOrWhiteSpace(reference))
            {
                entityReferenceType = "claimReference";
                claimDetails = this.claimReadModelRepository
                    .GetClaimWithRelatedEntitiesByReference(tenantId, entityReference, environment, includedProperties);
            }
            else
            {
                entityReferenceType = "claimNumber";
                claimDetails = this.claimReadModelRepository
                    .GetClaimWithRelatedEntitiesByNumber(tenantId, entityReference, environment, includedProperties);
            }

            if (claimDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("claimId", entityReference);
                }

                if (!string.IsNullOrWhiteSpace(claimNumber))
                {
                    errorData.Add("claimNumber", entityReference);
                }

                if (!string.IsNullOrWhiteSpace(reference))
                {
                    errorData.Add("claimReference", entityReference);
                }

                if (!string.IsNullOrWhiteSpace(environmentParam))
                {
                    errorData.Add(ErrorDataKey.EntityEnvironment, environmentParam);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.Claim.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<ClaimReadModel>)(await this.SerialisedEntityFactory.Create(claimDetails, includedProperties)));
        }
    }
}
