// <copyright file="PolicyTransactionEntityProvider.cs" company="uBind">
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
    /// This class is needed because we need to have a provider that we can use for searching policy transaction.
    /// This provider support the following searches:
    /// 1. Search by Policy Transaction Id.
    /// </summary>
    public class PolicyTransactionEntityProvider : StaticEntityProvider
    {
        private readonly IPolicyTransactionReadModelRepository policyTransactionReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransactionEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The quote id.</param>
        /// <param name="policyTransactionReadModelRepository">The quote read model repository.</param>
        public PolicyTransactionEntityProvider(
            IProvider<Data<string>>? id,
            IPolicyTransactionReadModelRepository policyTransactionReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "policyTransaction")
        {
            this.policyTransactionReadModelRepository = policyTransactionReadModelRepository;
        }

        /// <summary>
        /// Method for retrieving policy transaction entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The policy transaction entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            var policyTransactionDetails = default(IPolicyTransactionReadModelWithRelatedEntities);
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var includedProperties = this.GetPropertiesToInclude(typeof(PolicyTransaction));
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var environment = providerContext.AutomationData.System.Environment;

            if (string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "policyTransactionId",
                    this.SchemaReferenceKey));
            }

            if (Guid.TryParse(this.resolvedEntityId, out Guid policyTransactionId))
            {
                policyTransactionDetails =
                    this.policyTransactionReadModelRepository.GetPolicyTransactionWithRelatedEntities(
                        tenantId, environment, policyTransactionId, includedProperties);
            }

            if (policyTransactionDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("policyTransactionId", this.resolvedEntityId);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.PolicyTransaction.Humanize(), "policyTransactionId", this.resolvedEntityId, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<Domain.ReadModel.Policy.PolicyTransaction>)(await this.SerialisedEntityFactory.Create(policyTransactionDetails, includedProperties)));
        }
    }
}
