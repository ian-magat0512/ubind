// <copyright file="MessageEntityProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using System.Threading.Tasks;
    using Humanizer;
    using MorseCode.ITask;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;

    public class MessageEntityProvider : StaticEntityProvider
    {
        private readonly ISmsRepository smsRepository;
        private readonly IEmailRepository emailRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="smsRepository">The sms repository.</param>
        /// <param name="serialisedEntityFactory">The serialization manager.</param>
        public MessageEntityProvider(
            IProvider<Data<string>> id,
            ISmsRepository smsRepository,
            IEmailRepository emailRepository,
            ISerialisedEntityFactory serialisedEntityFactory)
            : base(id, serialisedEntityFactory, "message")
        {
            this.smsRepository = smsRepository;
            this.emailRepository = emailRepository;
        }

        /// <summary>
        /// Method for retrieving message entity.
        /// </summary>
        /// <param name="providerContext">The provider context.</param>
        /// <returns>The message entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            bool isSms = false;
            bool isEmail = false;
            var messageId = default(Guid);
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;

            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                if (Guid.TryParse(this.resolvedEntityId, out Guid resultId))
                {
                    messageId = resultId;
                    isSms = this.smsRepository.DoesSmsExists(providerContext.AutomationData.ContextManager.Tenant.Id, messageId);
                    isEmail = this.emailRepository.DoesEmailExists(providerContext.AutomationData.ContextManager.Tenant.Id, messageId);
                }
            }

            if (!isSms && !isEmail)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("messageId", this.resolvedEntityId);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(EntityType.Message.Humanize(), "id", this.resolvedEntityId, errorData));
            }

            if (isSms)
            {
                return ProviderResult<Data<IEntity>>.Success(await this.GetSmsMessageEntity(messageId, providerContext));
            }

            return ProviderResult<Data<IEntity>>.Success(await this.GetEmailMessageEntity(messageId, providerContext));
        }

        private async Task<Data<IEntity>> GetEmailMessageEntity(Guid entityId, IProviderContext providerContext)
        {
            var entityIdBuilder = new StaticBuilder<Data<string>>() { Value = entityId.ToString() };

            var emailMessageEntityProviderConfig = new EmailMessageEntityProviderConfigModel()
            {
                EmailId = entityIdBuilder,
            };

            var emailMessageEntityProvider = emailMessageEntityProviderConfig.Build(providerContext.AutomationData.ServiceProvider);
            emailMessageEntityProvider.IncludedProperties = this.IncludedProperties;
            var result = await emailMessageEntityProvider.Resolve(providerContext);
            return result.GetValueOrThrowIfFailed();
        }

        private async Task<Data<IEntity>> GetSmsMessageEntity(Guid entityId, IProviderContext providerContext)
        {
            var entityIdBuilder = new StaticBuilder<Data<string>>() { Value = entityId.ToString() };

            var smsMessageEntityProviderConfig = new SmsMessageEntityProviderConfigModel()
            {
                SmsId = entityIdBuilder,
            };

            var smsMessageEntityProvider = smsMessageEntityProviderConfig.Build(providerContext.AutomationData.ServiceProvider);
            smsMessageEntityProvider.IncludedProperties = this.IncludedProperties;
            var result = await smsMessageEntityProvider.Resolve(providerContext);
            return result.GetValueOrThrowIfFailed();
        }
    }
}
