// <copyright file="RoleAuthorisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authorisation;

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UBind.Application.ExtensionMethods;
using UBind.Application.Queries.Organisation;
using UBind.Application.Queries.Principal;
using UBind.Application.Queries.Role;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Permissions;
using UBind.Domain.ReadModel;

public class RoleAuthorisationService : IRoleAuthorisationService
{
    private readonly ICqrsMediator mediator;
    private readonly ICachingResolver cachingResolver;

    private readonly Permission[] viewPermissions = new Permission[] {
            Permission.ViewUsers,
            Permission.ViewUsersFromOtherOrganisations,
            Permission.ManageUsers,
            Permission.ManageUsersForOtherOrganisations,
            Permission.ViewRoles,
            Permission.ManageRoles,
            Permission.ViewRolesFromAllOrganisations,
            Permission.ManageRolesForAllOrganisations,
        };

    private readonly Permission[] managePermissions = new Permission[] {
            Permission.ManageRoles,
            Permission.ManageRolesForAllOrganisations,
        };

    public RoleAuthorisationService(ICqrsMediator mediator, ICachingResolver cachingResolver)
    {
        this.mediator = mediator;
        this.cachingResolver = cachingResolver;
    }

    public async Task ThrowIfUserCannotCreate(Guid tenantId, Guid organisationId, ClaimsPrincipal performingUser)
    {
        if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
        {
            throw new ErrorException(
                Domain.Errors.Authorisation.PermissionRequiredToCreateResource(this.managePermissions, "role"));
        }

        if (!await this.CanUserManageRolesForAllOrganisations(performingUser))
        {
            var allowedOrganisationIds
                = await this.GetIdsOfOrganisationsWhichPerformingUserCanManageRolesIn(performingUser);
            if (!allowedOrganisationIds.Contains(organisationId))
            {
                throw new ErrorException(
                    UBind.Domain.Errors.Authorisation.CannotCreateResourceInThatOrganisation("role"));
            }
        }
    }

    public async Task ThrowIfUserCannotDelete(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
    {
        if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
        {
            throw new ErrorException(
                Domain.Errors.Authorisation.PermissionRequiredToDeleteResource(this.managePermissions, "role"));
        }

        if (!await this.CanUserManageRolesForAllOrganisations(performingUser))
        {
            var allowedOrganisationIds
                = await this.GetIdsOfOrganisationsWhichPerformingUserCanManageRolesIn(performingUser);
            var role = await this.mediator.Send(new GetRoleByIdQuery(tenantId, entityId));
            if (!allowedOrganisationIds.Contains(role.OrganisationId))
            {
                throw new ErrorException(
                    UBind.Domain.Errors.Authorisation.CannotDeleteResourceFromThatOrganisation("role"));
            }
        }
    }

    public async Task ThrowIfUserCannotModify(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
    {
        if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
        {
            throw new ErrorException(
                Domain.Errors.Authorisation.PermissionRequiredToModifyResource(this.managePermissions, "role"));
        }

        if (!await this.CanUserManageRolesForAllOrganisations(performingUser))
        {
            var allowedOrganisationIds
                = await this.GetIdsOfOrganisationsWhichPerformingUserCanManageRolesIn(performingUser);
            var role = await this.mediator.Send(new GetRoleByIdQuery(tenantId, entityId));
            if (!allowedOrganisationIds.Contains(role.OrganisationId))
            {
                throw new ErrorException(
                    UBind.Domain.Errors.Authorisation.CannotModifyResourceFromThatOrganisation("role"));
            }
        }
    }

    public async Task ThrowIfUserCannotView(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
    {
        // permissions check
        await this.ThrowIfUserCannotViewAny(performingUser);

        if (!await this.CanUserViewRolesFromAllOrganisations(performingUser))
        {
            var allowedOrganisationIds
                = await this.GetIdsOfOrganisationsWhichPerformingUserCanViewRolesFrom(performingUser);
            var role = await this.mediator.Send(new GetRoleByIdQuery(tenantId, entityId));
            if (!allowedOrganisationIds.Contains(role.OrganisationId))
            {
                throw new ErrorException(
                    UBind.Domain.Errors.Authorisation.CannotViewResourceFromThatOrganisation("role"));
            }
        }
    }

    public async Task ThrowIfUserCannotViewAny(ClaimsPrincipal performingUser)
    {
        if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.viewPermissions)))
        {
            throw new ErrorException(
                Domain.Errors.Authorisation.PermissionRequiredToViewResource(
                    this.viewPermissions, "role"));
        }
    }

    public async Task ApplyRestrictionsToFilters(ClaimsPrincipal performingUser, EntityListFilters filters)
    {
        if (!await this.CanUserViewRolesFromAllOrganisations(performingUser))
        {
            // users can see roles from the default org, their own org, or organisations they manage
            List<Guid> restrictToOrganisationIds
                = await this.GetIdsOfOrganisationsWhichPerformingUserCanViewRolesFrom(performingUser);

            // apply the restricted organisation ids to the filters
            if (filters.OrganisationIds != null && filters.OrganisationIds.Any())
            {
                // if the user has specified a list of organisation ids to filter by, then we need to restrict
                // the list of organisation ids to only those that the user has permission to view
                filters.OrganisationIds = restrictToOrganisationIds.Intersect(filters.OrganisationIds).ToArray();
            }
            else
            {
                // if the user has not specified a list of organisation ids to filter by, then we need to restrict
                // the list of organisation ids to only those that the user has permission to view
                filters.OrganisationIds = restrictToOrganisationIds;
            }
        }
    }

    public async Task<List<Guid>> GetIdsOfOrganisationsWhichPerformingUserCanViewRolesFrom(ClaimsPrincipal performingUser)
    {
        List<Guid> organisationIds = new List<Guid>();
        if (await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, Permission.ViewRolesFromAllOrganisations, Permission.ManageRolesForAllOrganisations, Permission.ViewUsersFromOtherOrganisations, Permission.ManageUsersForOtherOrganisations)))
        {

            // get the list of organisation ids that the user's organisation is a managing organisation of
            organisationIds.AddRange(await this.mediator.Send(
                new GetIdsOfOrganisationsManagedByQuery(
                    performingUser.GetTenantId(), performingUser.GetOrganisationId())));
            organisationIds.Add(performingUser.GetOrganisationId());
        }
        else if (await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(
            performingUser, Permission.ViewRoles, Permission.ManageRoles, Permission.ViewUsers, Permission.ManageUsers)))
        {
            // user can only view users from their own organisation
            organisationIds.Add(performingUser.GetOrganisationId());
        }

        // add the default organisation id
        var tenant = await this.cachingResolver.GetTenantOrThrow(performingUser.GetTenantId());
        organisationIds.Add(tenant.Details.DefaultOrganisationId);

        return organisationIds;
    }

    public async Task<List<Guid>> GetIdsOfOrganisationsWhichPerformingUserCanManageRolesIn(ClaimsPrincipal performingUser)
    {
        List<Guid> organisationIds = new List<Guid>();
        if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageRolesForAllOrganisations)))
        {
            // get the list of organisation ids that the user's organisation is a managing organisation of
            organisationIds.AddRange(await this.mediator.Send(
                new GetIdsOfOrganisationsManagedByQuery(
                    performingUser.GetTenantId(), performingUser.GetOrganisationId())));
            organisationIds.Add(performingUser.GetOrganisationId());
        }
        else if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageUsers)))
        {
            // user can only manage users from their own organisation
            organisationIds.Add(performingUser.GetOrganisationId());
        }

        return organisationIds;
    }

    private async Task<bool> CanUserViewRolesFromAllOrganisations(ClaimsPrincipal performingUser)
    {
        return performingUser.IsMasterUser()
            && await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(
                performingUser,
                Permission.ViewRolesFromAllOrganisations,
                Permission.ManageRolesForAllOrganisations));
    }

    private async Task<bool> CanUserManageRolesForAllOrganisations(ClaimsPrincipal performingUser)
    {
        return performingUser.IsMasterUser()
            && await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(
                performingUser,
                Permission.ManageRolesForAllOrganisations));
    }
}
