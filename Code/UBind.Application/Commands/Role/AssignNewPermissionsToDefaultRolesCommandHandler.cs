// <copyright file="AssignNewPermissionsToDefaultRolesCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Role
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <summary>
    /// As implementation for UB-4685, new permissions will now be applied to default roles.
    /// </summary>
    public class AssignNewPermissionsToDefaultRolesCommandHandler
        : ICommandHandler<AssignNewPermissionsToDefaultRolesCommand, Unit>
    {
        private readonly IUserSessionDeletionService userSessionDeletionService;
        private readonly ILogger<AssignNewPermissionsToDefaultRolesCommandHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IClock clock;

        /// <summary>
        /// These permissions are the only permissions to add if the role is missing it.
        /// </summary>
        private readonly List<Permission> permissionsToAdd = new List<Permission>
        {
            Permission.ViewTenants,
            Permission.ViewAllClaims,
            Permission.ViewAllClaimsFromAllOrganisations,
            Permission.ManageAllClaims,
            Permission.ManageAllClaimsForAllOrganisations,
            Permission.ViewAllQuotes,
            Permission.ViewAllQuotesFromAllOrganisations,
            Permission.ManageAllQuotes,
            Permission.ManageAllQuotesForAllOrganisations,
            Permission.ViewAllPolicies,
            Permission.ViewAllPoliciesFromAllOrganisations,
            Permission.ManageAllPolicies,
            Permission.ManageAllPoliciesForAllOrganisations,
            Permission.ViewAllCustomers,
            Permission.ManageAllCustomers,
            Permission.ViewUsersFromOtherOrganisations,
            Permission.ManageUsersForOtherOrganisations,
            Permission.ViewRolesFromAllOrganisations,
            Permission.ManageRolesForAllOrganisations,
            Permission.ViewAllMessages,
            Permission.ManageAllMessages,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrganisationAdminRoleForAllTenantsCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The tenant repo.</param>
        /// <param name="roleRepository">The role repo.</param>
        /// <param name="logger">The logger to idenfity issues.</param>
        /// <param name="clock">Clock for obtaining current time.</param>
        public AssignNewPermissionsToDefaultRolesCommandHandler(
            ITenantRepository tenantRepository,
            IRoleRepository roleRepository,
            IUserSessionService userSessionService,
            ILogger<AssignNewPermissionsToDefaultRolesCommandHandler> logger,
            IClock clock,
            IUserSessionDeletionService userSessionDeletionService)
        {
            this.userSessionDeletionService = userSessionDeletionService;
            this.logger = logger;
            this.tenantRepository = tenantRepository;
            this.roleRepository = roleRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            AssignNewPermissionsToDefaultRolesCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var tenants = this.tenantRepository.GetTenants();

            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Starting Migration for Assigning New Permissions To Roles Of Tenant {tenant.Id}.");
                var roles = this.roleRepository.GetRoles(tenant.Id, new RoleReadModelFilters());
                foreach (var role in roles)
                {
                    try
                    {
                        var save = false;
                        var persistenceRole = this.roleRepository.GetRoleById(tenant.Id, role.Id);
                        var defaultRole = defaultRoleNameRegistry.GetDefaultRoleForRoleName(persistenceRole.Name, persistenceRole.Type);
                        var defaultRolePermissions = defaultRolePermissionsRegistry.GetPermissionsForDefaultRole(defaultRole).ToList();

                        var currentPermissions = persistenceRole.Permissions;

                        // remove roles in currentPermissions that are not in default role permissions.
                        var extraPermissionsToRemove = currentPermissions.Except(defaultRolePermissions).Distinct().ToList();
                        foreach (var extraPermissionToRemove in extraPermissionsToRemove)
                        {
                            this.logger.LogInformation(
                                $"Removed Permission '{extraPermissionsToRemove.Humanize()}' to Role '{defaultRole.Humanize()}' for Tenant {tenant.Id}");
                            persistenceRole.RemovePermission(extraPermissionToRemove, this.clock.Now());
                            save = true;
                        }

                        // include all new permissions from default roles to the default roles already created.
                        var newPermissionsToAdd = defaultRolePermissions.Except(currentPermissions).Distinct().ToList();
                        foreach (var newPermissionToAdd in newPermissionsToAdd)
                        {
                            if (this.permissionsToAdd.Any(p => p == newPermissionToAdd))
                            {
                                this.logger.LogInformation(
                                       $"Added Permission '{newPermissionToAdd.Humanize()}' to Role '{defaultRole.Humanize()}' for Tenant {tenant.Id}");
                                persistenceRole.AddPermission(newPermissionToAdd, this.clock.Now());
                                save = true;
                            }
                        }

                        if (save)
                        {
                            persistenceRole.RemoveDuplicatePermissions();
                            this.roleRepository.SaveChanges();
                            this.userSessionDeletionService.DeleteByRoleId(role.TenantId, role.Id, cancellationToken);
                            await Task.Delay(200, cancellationToken);
                        }
                    }

                    // the role is not a default role and will cause argument exception, this is fine, we skip this role.
                    catch (ArgumentException e)
                    {
                        var oo = e.Message;
                        continue;
                    }
                }
            }

            return Unit.Value;
        }
    }
}
