// <copyright file="EmailMessageEntityProvider.cs" company="uBind">
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
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching email.
    /// This provider support the following searches:
    /// 1. Search by Email Id.
    /// </summary>
    public class EmailMessageEntityProvider : StaticEntityProvider
    {
        private readonly IEmailRepository emailRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessageEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The email id.</param>
        /// <param name="emailRepository">The email repository.</param>
        public EmailMessageEntityProvider(IProvider<Data<string>>? id, IEmailRepository emailRepository, ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "email")
        {
            this.emailRepository = emailRepository;
        }

        /// <summary>
        /// Method for retrieving email entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The email entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            var includedProperties = this.GetPropertiesToInclude(typeof(EmailMessage));
            var email = default(IEmailReadModelWithRelatedEntities);
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var environment = providerContext.AutomationData.System.Environment;
            if (string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "emailId",
                    this.SchemaReferenceKey));
            }

            if (Guid.TryParse(this.resolvedEntityId, out Guid emailId))
            {
                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                email = this.emailRepository
                    .GetEmailWithRelatedEntities(tenantId, environment, emailId, includedProperties);
            }

            if (email == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("emailId", this.resolvedEntityId);
                    errorData.Add("messageType", "email");
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.Message.Humanize(), "emailId", this.resolvedEntityId, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<Domain.Entities.Message>)(await this.SerialisedEntityFactory.Create(email, includedProperties)));
        }
    }
}
