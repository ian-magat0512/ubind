// <copyright file="GetAssignableRolesMatchingFiltersQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Role
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Query handler for getting the roles which the performing user can assign.
    /// </summary>
    /// <remarks>
    /// - The customer role will not be included
    /// - If the organisation is not the default, the Tenant Admin role will not be included
    /// - If the performing user is not a Tenant Admin, the Tenant Admin role will not be included
    /// - If the performing user is not a Tenant Admin or Organisation Admin, the Organisation Admin role will not be included.
    /// </remarks>
    public class GetAssignableRolesMatchingFiltersQueryHandler : IQueryHandler<GetAssignableRolesMatchingFiltersQuery,
        IReadOnlyList<Role>>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAssignableRolesMatchingFiltersQueryHandler"/> class.
        /// </summary>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="userReadModelRepository">The user repository.</param>
        public GetAssignableRolesMatchingFiltersQueryHandler(
            IRoleRepository roleRepository,
            IUserReadModelRepository userReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.userReadModelRepository = userReadModelRepository;
            this.roleRepository = roleRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Role>> Handle(
            GetAssignableRolesMatchingFiltersQuery request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenant = await this.cachingResolver.GetTenantOrThrow(new Domain.Helpers.GuidOrAlias(request.Filters.TenantId));
            bool canManageClientAdmins = false;
            bool canManageOrganisationAdmins = false;
            var claimsPrincipal = this.httpContextPropertiesResolver.PerformingUser;
            if (claimsPrincipal != null)
            {
                var user = this.userReadModelRepository.GetUser(
                    claimsPrincipal.GetTenantId(),
                    claimsPrincipal.GetId().GetValueOrDefault());
                if (user != null)
                {
                    canManageClientAdmins = user.HasPermission(Permission.ManageTenantAdminUsers);
                    canManageOrganisationAdmins =
                        user.HasPermission(Permission.ManageOrganisationAdminUsers);
                }
            }

            Guid defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            bool isDefaultOrganisation = request.Filters.OrganisationIds == null
                || request.Filters.OrganisationIds.Contains(defaultOrganisationId);

            IReadOnlyList<Role> roles = this.roleRepository.GetRoles(tenant.Id, request.Filters)
                .Where(r =>
                {
                    if (r.Type == RoleType.Customer)
                    {
                        // you can't assign the customer role, it's automatic.
                        return false;
                    }

                    if (!isDefaultOrganisation && r.Permissions.Contains(Permission.ManageTenantAdminUsers))
                    {
                        // you can't assign Tenant Admin to someone not in the default org.
                        return false;
                    }

                    if (r.Permissions.Contains(Permission.ManageTenantAdminUsers) && !canManageClientAdmins)
                    {
                        // only client admin can assign a role with the Manage Client Admins permission
                        return false;
                    }

                    if (r.Permissions.Contains(Permission.ManageOrganisationAdminUsers)
                        && !canManageOrganisationAdmins && !canManageClientAdmins)
                    {
                        // only org admins or client admins can assign the org admins permission
                        return false;
                    }

                    return true;
                }).ToList();

            return roles;
        }
    }
}
