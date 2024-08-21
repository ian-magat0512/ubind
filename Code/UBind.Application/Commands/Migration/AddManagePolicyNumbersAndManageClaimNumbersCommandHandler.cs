// <copyright file="AddManagePolicyNumbersAndManageClaimNumbersCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
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

    public class AddManagePolicyNumbersAndManageClaimNumbersCommandHandler
        : ICommandHandler<AddManagePolicyNumbersAndManageClaimNumbersCommand, Unit>
    {
        private readonly IUserSessionDeletionService userSessionDeletionService;
        private readonly ILogger<AddManagePolicyNumbersAndManageClaimNumbersCommandHandler> logger;
        private readonly ITenantRepository tenantRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IClock clock;

        /// <summary>
        /// These permissions are the only permissions to add if the role is missing it.
        /// </summary>
        private readonly List<Permission> permissionsToAdd = new List<Permission>
        {
            Permission.ManagePolicyNumbers,
            Permission.ManageClaimNumbers,
        };

        private readonly string[] targetRoleNames = new[]
        {
            "Tenant Admin",
            "Organisation Admin",
            "Underwriter",
            "Broker",
            "Product Developer",
            "Claims Agent",
        };

        public AddManagePolicyNumbersAndManageClaimNumbersCommandHandler(
                IUserSessionDeletionService userSessionDeletionService,
                IRoleRepository roleRepository,
                ITenantRepository tenantRepository,
                ILogger<AddManagePolicyNumbersAndManageClaimNumbersCommandHandler> logger,
                IClock clock)
        {
            this.userSessionDeletionService = userSessionDeletionService;
            this.tenantRepository = tenantRepository;
            this.roleRepository = roleRepository;
            this.logger = logger;
            this.clock = clock;
        }

        public async Task<Unit> Handle(AddManagePolicyNumbersAndManageClaimNumbersCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var tenants = this.tenantRepository.GetTenants();

            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Starting migration for assigning new permissions to roles of Tenant {tenant.Id}.");
                var roles = this.roleRepository.GetRoles(tenant.Id, new RoleReadModelFilters());
                foreach (var role in roles.Where(r =>
                    r.Type == RoleType.Client && this.targetRoleNames.Contains(r.Name)))
                {
                    try
                    {
                        var hasNewPermissionsAdded = false;
                        var defaultRole = defaultRoleNameRegistry.GetDefaultRoleForRoleName(role.Name, role.Type);
                        var defaultRolePermissions = defaultRolePermissionsRegistry.GetPermissionsForDefaultRole(defaultRole).ToList();

                        var currentPermissions = role.Permissions;

                        // include all new permissions from default roles to the default roles already created.
                        var newPermissions = defaultRolePermissions.Except(currentPermissions).Distinct().ToList();
                        foreach (var newPermissionToAdd in newPermissions)
                        {
                            if (this.permissionsToAdd.Any(p => p == newPermissionToAdd))
                            {
                                this.logger.LogInformation(
                                       $"Added Permission '{newPermissionToAdd.Humanize()}' to Role '{defaultRole.Humanize()}' for Tenant {tenant.Id}");
                                role.AddPermission(newPermissionToAdd, this.clock.Now());
                                hasNewPermissionsAdded = true;
                            }
                        }

                        if (hasNewPermissionsAdded)
                        {
                            role.RemoveDuplicatePermissions();
                            this.roleRepository.SaveChanges();
                            this.userSessionDeletionService.DeleteByRoleId(role.TenantId, role.Id, cancellationToken);
                            await Task.Delay(200, cancellationToken);
                        }
                    }

                    // the role is not a default role and will cause argument exception, this is fine, we skip this role.
                    catch (ArgumentException e)
                    {
                        this.logger.LogError(
                            $"ERROR: A non-default role {role.Name} was encountered in Tenant {tenant.Id}, while updating the permissions of default roles," +
                            $"errorMessage: {e.Message}-{e.InnerException?.Message}");
                        var oo = e.Message;
                        continue;
                    }

                    this.logger.LogInformation($"Finished migration for assigning new permissions to the default roles of Tenant {tenant.Id}.");
                }
            }

            return Unit.Value;
        }
    }
}
