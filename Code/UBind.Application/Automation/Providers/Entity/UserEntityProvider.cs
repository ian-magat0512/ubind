// <copyright file="UserEntityProvider.cs" company="uBind">
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
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching user.
    /// This provider support the following searches:
    /// 1. Search by User Id.
    /// 2. Search by User Email.
    /// </summary>
    public class UserEntityProvider : StaticEntityProvider
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IUserReadModelRepository userReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The user id.</param>
        /// <param name="userReadModelRepository">The user read model repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public UserEntityProvider(
            IProvider<Data<string>>? id,
            IUserReadModelRepository userReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory,
            ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "user")
        {
            this.cachingResolver = cachingResolver;
            this.userReadModelRepository = userReadModelRepository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The user id.</param>
        /// <param name="userAccountEmail">The user account email.</param>
        /// <param name="userReadModelRepository">The user read model repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public UserEntityProvider(
            IProvider<Data<string>>? id,
            IProvider<Data<string>>? userAccountEmail,
            IUserReadModelRepository userReadModelRepository,
            ISerialisedEntityFactory serialisedEntityFactory,
            ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "user")
        {
            this.cachingResolver = cachingResolver;
            this.UserAccountEmail = userAccountEmail;
            this.userReadModelRepository = userReadModelRepository;
            this.userReadModelRepository = userReadModelRepository;
        }

        /// <summary>
        /// Gets or sets the user account email.
        /// </summary>
        private IProvider<Data<string>>? UserAccountEmail { get; set; }

        /// <summary>
        /// Method for retrieving user entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The user entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            var email = (await this.UserAccountEmail.ResolveValueIfNotNull(providerContext))?.DataValue;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;

            var includedProperties = this.GetPropertiesToInclude(typeof(User));
            var userDetails = default(IUserReadModelWithRelatedEntities);

            string entityReference;
            string entityReferenceType;
            if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                entityReference = this.resolvedEntityId;
                entityReferenceType = "userId";
                if (Guid.TryParse(this.resolvedEntityId, out Guid userId))
                {
                    userDetails = this.userReadModelRepository.GetUserWithRelatedEntities(
                        tenantId, userId, includedProperties);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                        "email",
                        this.SchemaReferenceKey));
                }

                entityReference = email;
                entityReferenceType = "userAccountEmail";
                userDetails = this.userReadModelRepository.GetUserWithRelatedEntities(
                    tenantId, email, includedProperties);
            }

            if (userDetails == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                if (!string.IsNullOrWhiteSpace(this.resolvedEntityId))
                {
                    errorData.Add("userId", this.resolvedEntityId);
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    errorData.Add("userAccountEmail", email);
                }

                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.User.Humanize(), entityReferenceType, entityReference, errorData));
            }

            return ProviderResult<Data<IEntity>>.Success(
                (BaseEntity<UserReadModel>)(await this.SerialisedEntityFactory.Create(userDetails, includedProperties)));
        }
    }
}
