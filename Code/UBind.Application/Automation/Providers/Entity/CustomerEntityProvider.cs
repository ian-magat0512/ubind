// <copyright file="CustomerEntityProvider.cs" company="uBind">
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
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching customer.
    /// This provider support the following searches:
    /// 1. Search by Customer Id.
    /// 2. Search by Customer Email Address.
    /// </summary>
    public class CustomerEntityProvider : StaticEntityProvider
    {
        private readonly ICustomerReadModelRepository customerReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The customer id.</param>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        public CustomerEntityProvider(
            IProvider<Data<string>>? id,
            ICustomerReadModelRepository customerReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "customer")
        {
            this.customerReadModelRepository = customerReadModelRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The customer id.</param>
        /// <param name="customerAccountEmail">The customer account email.</param>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        public CustomerEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? customerAccountEmail,
            ICustomerReadModelRepository customerReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "customer")
        {
            this.CustomerAccountEmail = customerAccountEmail;
            this.customerReadModelRepository = customerReadModelRepository;
        }

        /// <summary>
        /// Gets or sets the customer account email.
        /// </summary>
        protected IProvider<Data<string>>? CustomerAccountEmail { get; set; }

        /// <summary>
        /// Method for retrieving customer entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The customer entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var email = (await this.CustomerAccountEmail.ResolveValueIfNotNull(providerContext))?.DataValue;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            DeploymentEnvironment? environment = this.EntityId == null
                ? providerContext.AutomationData.System.Environment : null;

            var includedProperties = this.GetPropertiesToInclude(typeof(Customer));

            string? entityReference = this.resolvedEntityId ?? email;
            if (entityReference == null)
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "email",
                    this.SchemaReferenceKey));
            }

            var entityReferenceType = string.Empty;
            var customerDetails = default(ICustomerReadModelWithRelatedEntities);
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReferenceType = "customerId";
                if (Guid.TryParse(entityReference, out Guid customerId))
                {
                    customerDetails = this.customerReadModelRepository
                        .GetCustomerWithRelatedEntities(tenantId, environment, customerId, includedProperties);
                }
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                var organisationIdStr = await providerContext.AutomationData.GetValue<string>("/context/organisation/id", providerContext);
                if (organisationIdStr != null)
                {
                    var organisationId = Guid.Parse(organisationIdStr);
                    entityReference = email;
                    customerDetails = this.customerReadModelRepository
                        .GetCustomerWithRelatedEntities(tenantId, environment, organisationId, email, includedProperties);
                }
            }

            if (customerDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey);
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("customerId", entityReference);
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    errorData.Add("customerAccountEmail", entityReference);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.Customer.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<CustomerReadModel>)(await this.SerialisedEntityFactory.Create(customerDetails, includedProperties)));
        }
    }
}
