// <copyright file="CreateOrganisationAdminRoleForAllTenantsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Role
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for the startup job to create the default organisation admin role
    /// for all existing tenants.
    /// </summary>
    public class CreateOrganisationAdminRoleForAllTenantsCommandHandler
        : ICommandHandler<CreateOrganisationAdminRoleForAllTenantsCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrganisationAdminRoleForAllTenantsCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The tenant repo.</param>
        /// <param name="roleRepository">The role repo.</param>
        /// <param name="clock">The clock.</param>
        public CreateOrganisationAdminRoleForAllTenantsCommandHandler(
            ITenantRepository tenantRepository,
            IRoleRepository roleRepository,
            IClock clock)
        {
            this.tenantRepository = tenantRepository;
            this.roleRepository = roleRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public Task<Unit> Handle(
            CreateOrganisationAdminRoleForAllTenantsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = this.tenantRepository.GetTenants();
            foreach (var tenant in tenants)
            {
                // check if the role exists first, so this can be run idempotent.
                var roleInfo = DefaultRole.OrganisationAdmin.GetAttributeOfType<RoleInformationAttribute>();
                var maybeRole = this.roleRepository.TryGetRoleByName(tenant.Id, roleInfo.Name);
                if (maybeRole.HasValue)
                {
                    // skip, since the role already exists
                    continue;
                }

                // The role didn't already exist, so create it
                var role = new Role(
                    tenant.Id,
                    Guid.NewGuid(),
                    DefaultRole.OrganisationAdmin,
                    this.clock.GetCurrentInstant());
                this.roleRepository.Insert(role);
            }

            this.roleRepository.SaveChanges();
            return Task.FromResult(Unit.Value);
        }
    }
}
