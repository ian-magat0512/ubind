// <copyright file="PolicyEntityProvider.cs" company="uBind">
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
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using Policy = UBind.Domain.SerialisedEntitySchemaObject.Policy;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching policy.
    /// This provider support the following searches:
    /// 1. Search by Policy Id.
    /// 2. Search by Policy Number.
    /// </summary>
    public class PolicyEntityProvider : StaticEntityProvider
    {
        private readonly IPolicyReadModelRepository policyReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The policy id.</param>
        /// <param name="policyReadModelRepository">The policy read model repository.</param>
        public PolicyEntityProvider(
            IProvider<Data<string>>? id,
            IPolicyReadModelRepository policyReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "policy")
        {
            this.policyReadModelRepository = policyReadModelRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The policy id.</param>
        /// <param name="policyNumber">The policy number.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="policyReadModelRepository">The policy read model repository.</param>
        public PolicyEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? policyNumber,
            IProvider<Data<string>>? environment,
            IPolicyReadModelRepository policyReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "policy")
        {
            this.PolicyNumber = policyNumber;
            this.Environment = environment;
            this.policyReadModelRepository = policyReadModelRepository;
        }

        /// <summary>
        /// Gets or sets the policy number.
        /// </summary>
        private IProvider<Data<string>>? PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the policy environment.
        /// </summary>
        private IProvider<Data<string>>? Environment { get; set; }

        /// <summary>
        /// Method for retrieving policy entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The policy entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var policyNumber = (await this.PolicyNumber.ResolveValueIfNotNull(providerContext))?.DataValue;
            var environmentParam = (await this.Environment.ResolveValueIfNotNull(providerContext))?.DataValue;
            DeploymentEnvironment? environment = this.EntityId == null
                ? this.GetEnvironment(environmentParam, providerContext) : null;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;

            var includedProperties = this.GetPropertiesToInclude(typeof(Policy));
            var policyDetails = default(IPolicyReadModelWithRelatedEntities);

            string entityReference;
            string entityReferenceType;
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReference = this.resolvedEntityId;
                entityReferenceType = "policyId";
                if (Guid.TryParse(this.resolvedEntityId, out Guid policyId))
                {
                    policyDetails = this.policyReadModelRepository
                        .GetPolicyWithRelatedEntities(tenantId, environment, policyId, includedProperties);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(policyNumber))
                {
                    throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                        "policyNumber",
                        this.SchemaReferenceKey));
                }

                entityReference = policyNumber;
                entityReferenceType = "policyNumber";
                policyDetails = this.policyReadModelRepository.GetPolicyWithRelatedEntities(
                    providerContext.AutomationData.ContextManager.Tenant.Id,
                    providerContext.AutomationData.ContextManager.Product.Id,
                    environment,
                    policyNumber,
                    includedProperties);
            }

            if (policyDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("policyId", this.resolvedEntityId);
                }

                if (!string.IsNullOrWhiteSpace(policyNumber))
                {
                    errorData.Add("policyNumber", policyNumber);
                }

                if (!string.IsNullOrWhiteSpace(environmentParam))
                {
                    errorData.Add(ErrorDataKey.EntityEnvironment, environmentParam);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.Policy.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<PolicyReadModel>)(await this.SerialisedEntityFactory.Create(policyDetails, includedProperties)));
        }
    }
}
