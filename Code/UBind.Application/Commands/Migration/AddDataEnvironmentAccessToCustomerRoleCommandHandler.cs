// <copyright file="AddDataEnvironmentAccessToCustomerRoleCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Repositories;

    public class AddDataEnvironmentAccessToCustomerRoleCommandHandler
        : ICommandHandler<AddDataEnvironmentAccessToCustomerRoleCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IClock clock;

        public AddDataEnvironmentAccessToCustomerRoleCommandHandler(
            ITenantRepository tenantRepository,
            IRoleRepository roleRepository,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.roleRepository = roleRepository;
            this.clock = clock;
        }

        public Task<Unit> Handle(AddDataEnvironmentAccessToCustomerRoleCommand request, CancellationToken cancellationToken)
        {
            var now = this.clock.Now();
            var tenants = this.tenantRepository.GetTenants();
            foreach (var tenant in tenants)
            {
                if (tenant.Id == Tenant.MasterTenantId)
                {
                    continue;
                }

                var role = this.roleRepository.GetCustomerRoleForTenant(tenant.Id);
                if (!role.Permissions.Any(p => p == Permission.AccessProductionData))
                {
                    role.AddPermission(Permission.AccessDevelopmentData, now);
                    role.AddPermission(Permission.AccessStagingData, now);
                    role.AddPermission(Permission.AccessProductionData, now);
                }

                this.roleRepository.SaveChanges();
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
