// <copyright file="RoleEntityProvider.cs" company="uBind">
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
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// This class is needed because we need to have a provider that we can use for searching role.
    /// This provider support the following searches:
    /// 1. Search by Role Id.
    /// </summary>
    public class RoleEntityProvider : StaticEntityProvider
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IRoleRepository roleRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleEntityProvider"/> class.
        /// </summary>
        /// <param name="id">The role id.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public RoleEntityProvider(
            IProvider<Data<string>>? id,
            IRoleRepository roleRepository,
            ISerialisedEntityFactory serialisedEntityFactory,
            ICachingResolver cachingResolver)
            : base(id, serialisedEntityFactory, "role")
        {
            this.cachingResolver = cachingResolver;
            this.roleRepository = roleRepository;
        }

        /// <summary>
        /// Method for retrieving role entity.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The role entity.</returns>
        public override async ITask<IProviderResult<Data<IEntity>>> Resolve(IProviderContext providerContext)
        {
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            this.resolvedEntityId = this.resolvedEntityId ?? (await this.EntityId.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (string.IsNullOrWhiteSpace(this.resolvedEntityId))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "roleId",
                    this.SchemaReferenceKey));
            }

            var roleDetail = Guid.TryParse(this.resolvedEntityId, out Guid roleId) ?
                this.roleRepository.GetRoleById(tenantId, roleId) : default;

            if (roleDetail == null)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.EntityType, this.SchemaReferenceKey.Titleize());
                errorData.Add("roleId", this.resolvedEntityId);
                throw new ErrorException(Errors.Automation.Provider.Entity.NotFound(
                    EntityType.Role.Humanize(), "roleId", this.resolvedEntityId, errorData));
            }

            var roleEntity = new Role(roleDetail);
            return ProviderResult<Data<IEntity>>.Success(new Data<IEntity>(roleEntity));
        }
    }
}
