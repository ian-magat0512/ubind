// <copyright file="RoleService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Infrastructure;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using Entities = UBind.Domain.Entities;

    /// <inheritdoc/>
    public class RoleService : IRoleService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IRoleRepository roleRepository;
        private readonly IClock clock;
        private readonly IUBindDbContext dbContext;
        private readonly IUserSessionDeletionService userSessionDeletionService;
        private CancellationToken cancellationToken = ApplicationLifetimeManager.ApplicationShutdownToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleService"/> class.
        /// </summary>
        /// <param name="roleRepository">The release repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        public RoleService(
            IRoleRepository roleRepository,
            ICachingResolver cachingResolver,
            IClock clock,
            IUserSessionDeletionService userSessionDeletionService,
            IUBindDbContext dbContext)
        {
            this.cachingResolver = cachingResolver;
            this.roleRepository = roleRepository;
            this.clock = clock;
            this.dbContext = dbContext;
            this.userSessionDeletionService = userSessionDeletionService;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Entities.Role> GetRoles(Guid tenantId, RoleReadModelFilters filters)
        {
            return this.roleRepository.GetRoles(tenantId, filters);
        }

        /// <inheritdoc/>
        public Entities.Role GetRole(Guid tenantId, Guid roleId)
        {
            var role = this.roleRepository.GetRoleById(tenantId, roleId);
            if (role == null)
            {
                throw new NotFoundException(Errors.General.NotFound("role", roleId));
            }

            return role;
        }

        /// <inheritdoc/>
        public async Task<Entities.Role> CreateRole(Guid tenantId, Guid organisationId, string name, RoleType roleType, string description)
        {
            if (this.roleRepository.IsNameInUse(tenantId, name))
            {
                throw new ErrorException(Errors.Role.NameInUse(name));
            }

            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            if (tenant == null)
            {
                throw new ErrorException(Errors.Tenant.NotFound(tenant.Details.Alias));
            }

            var now = this.clock.GetCurrentInstant();
            var role = new Entities.Role(tenant.Id, organisationId, roleType, name, description, now, false);
            this.roleRepository.Insert(role);
            this.roleRepository.SaveChanges();
            return role;
        }

        /// <inheritdoc/>
        public void CreateDefaultRolesForTenant(Tenant tenant)
        {
            if (tenant == null)
            {
                throw new ArgumentNullException(
                    nameof(tenant), "A null parameter was passed in CreateDefaultRolesForTenant.");
            }

            var now = this.clock.GetCurrentInstant();
            var defaultTenancyRoles = new List<DefaultRole>();
            defaultTenancyRoles.AddRange(
                ((DefaultRole[])Enum.GetValues(typeof(DefaultRole)))
                    .Where(c => c.GetAttributeOfType<RoleInformationAttribute>().RoleType == RoleType.Client));
            defaultTenancyRoles.AddRange(
                ((DefaultRole[])Enum.GetValues(typeof(DefaultRole)))
                    .Where(c => c.GetAttributeOfType<RoleInformationAttribute>().RoleType == RoleType.Customer));
            foreach (var role in defaultTenancyRoles)
            {
                var newRole = new Entities.Role(tenant.Id, tenant.Details.DefaultOrganisationId, role, now);

                // if role is already existing (when we re-enable a disabled tenant).
                if (!this.roleRepository.IsNameInUse(tenant.Id, newRole.Name))
                {
                    this.roleRepository.Insert(newRole);
                }
            }
        }

        /// <inheritdoc/>
        public Entities.Role UpdateRole(Guid tenantId, Guid roleId, string name, string description)
        {
            if (this.roleRepository.IsNameInUse(tenantId, name, roleId))
            {
                throw new ErrorException(Errors.Role.NameInUse(name));
            }

            var role = this.roleRepository.GetRoleById(tenantId, roleId);
            role.Update(name, description, this.clock.Now());
            this.roleRepository.SaveChanges();
            return role;
        }

        /// <inheritdoc/>
        public bool DeleteRole(Guid tenantId, Guid roleId)
        {
            var role = this.roleRepository.GetRoleById(tenantId, roleId);
            if (role.IsPermanent())
            {
                throw new ErrorException(Errors.Role.CannotDeletePermanentRole(role.Name));
            }

            if (role.Users.Any())
            {
                throw new ErrorException(Errors.Role.CannotDeleteRoleInUse(role.Name));
            }

            return this.roleRepository.Delete(role);
        }

        /// <inheritdoc/>
        public Permission AddPermissionToRole(Guid tenantId, Guid roleId, Permission permissionType)
        {
            this.cancellationToken.ThrowIfCancellationRequested();

            var role = this.roleRepository.GetRoleById(tenantId, roleId);
            role.AddPermission(permissionType, this.clock.Now());
            this.userSessionDeletionService.DeleteByRoleId(tenantId, roleId, this.cancellationToken);
            this.dbContext.SaveChanges();
            return permissionType;
        }

        /// <inheritdoc/>
        public Permission UpdatePermissionOfARole(
            Guid tenantId,
            Guid roleId,
            Permission previousPermissionType,
            Permission newPermissionType)
        {
            this.RemovePermissionFromRole(tenantId, roleId, previousPermissionType);
            this.AddPermissionToRole(tenantId, roleId, newPermissionType);
            return newPermissionType;
        }

        /// <inheritdoc/>
        public bool RemovePermissionFromRole(Guid tenantId, Guid roleId, Permission permissionType)
        {
            this.cancellationToken.ThrowIfCancellationRequested();

            var role = this.roleRepository.GetRoleById(tenantId, roleId);
            role.RemovePermission(permissionType, this.clock.Now());
            this.userSessionDeletionService.DeleteByRoleId(tenantId, roleId, this.cancellationToken);
            this.dbContext.SaveChanges();
            return true;
        }
    }
}
