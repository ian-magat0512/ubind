// <copyright file="SmsMessageEntityProvider.cs" company="uBind">
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
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// The entity provider for sms.
    /// </summary>
    public class SmsMessageEntityProvider : StaticEntityProvider
    {
        private readonly ISmsRepository smsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsMessageEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The email id.</param>
        /// <param name="smsRepository">The email repository.</param>
        public SmsMessageEntityProvider(
            IProvider<Data<string>>? id,
            ISmsRepository smsRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "sms")
        {
            this.smsRepository = smsRepository;
        }

        /// <summary>
        /// Method for retrieving sms entity.
        /// </summary>
        /// <param name="providerContext">The provider context.</param>
        /// <returns>The sms entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "smsId",
                    this.SchemaReferenceKey));
            }

            var includedProperties = this.GetPropertiesToInclude(typeof(SmsMessage));
            var sms = default(ISmsReadModelWithRelatedEntities);
            if (Guid.TryParse(this.resolvedEntityId, out Guid smsId))
            {
                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                sms = this.smsRepository.GetSmsWithRelatedEntities(tenantId, smsId, includedProperties);
            }

            if (sms == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("messageId", this.resolvedEntityId);
                    errorData.Add("messageType", "sms");
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.Message.Humanize(), "id", this.resolvedEntityId, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<Domain.Entities.Message>)(await this.SerialisedEntityFactory.Create(sms, includedProperties)));
        }
    }
}
