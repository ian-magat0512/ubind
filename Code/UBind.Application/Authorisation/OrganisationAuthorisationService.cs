// <copyright file="OrganisationAuthorisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authorisation
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Organisation;
    using UBind.Application.Queries.Principal;
    using UBind.Application.Queries.User;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;

    public class OrganisationAuthorisationService : IOrganisationAuthorisationService
    {
        private readonly ICqrsMediator mediator;

        private readonly Permission[] viewPermissions = new Permission[] {
            Permission.ViewOrganisations,
            Permission.ViewAllOrganisations,
            Permission.ManageOrganisations,
            Permission.ManageAllOrganisations,
        };

        private readonly Permission[] managePermissions = new Permission[] {
            Permission.ManageOrganisations,
            Permission.ManageAllOrganisations,
        };

        public OrganisationAuthorisationService(
            ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task ApplyRestrictionsToFilters(ClaimsPrincipal performingUser, EntityListFilters filters)
        {
            var isUserFromDefaultOrganisation = await this.IsUserFromDefaultOrganisation(performingUser);
            if (isUserFromDefaultOrganisation)
            {
                // if the user is from the default organisation, then we don't need to apply any restrictions
                return;
            }

            // Even if the user has the ViewAllOrganisations permission, we still need to restrict the list of organisation ids
            // to only those that the user's organisation is a managing organisation of
            List<Guid> restrictToOrganisationIds = new List<Guid>();
            var performingUserOrganisationid = performingUser.GetOrganisationId();

            // get the list of organisation ids that the user's organisation is a managing organisation of
            restrictToOrganisationIds.AddRange(await this.mediator.Send(
                new GetIdsOfOrganisationsManagedByQuery(
                    performingUser.GetTenantId(), performingUserOrganisationid)));
            restrictToOrganisationIds.Add(performingUserOrganisationid);

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

        public async Task ThrowIfUserCannotCreate(Guid tenantId, Guid organisationId, ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
            {
                throw new ErrorException(
                    Domain.Errors.Authorisation.PermissionRequiredToCreateResource(this.managePermissions, "organisation"));
            }

            if (!await this.CanUserManageAllOrganisations(performingUser))
            {
                if (tenantId != performingUser.GetTenantId())
                {
                    throw new ErrorException(
                        UBind.Domain.Errors.Authorisation.CannotCreateResourceInThatOrganisation("organisation"));
                }
            }
        }

        public async Task ThrowIfUserCannotDelete(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
            {
                throw new ErrorException(
                    Domain.Errors.Authorisation.PermissionRequiredToDeleteResource(this.managePermissions, "organisation"));
            }

            if (performingUser.GetOrganisationId() == entityId)
            {
                throw new ErrorException(Errors.Authorisation.CannotDeleteOwnOrganisation());
            }

            if (!await this.CanUserManageAllOrganisations(performingUser))
            {
                var allowedOrganisationIds = await this.mediator.Send(new GetIdsOfOrganisationsManagedByQuery(
                        performingUser.GetTenantId(), performingUser.GetOrganisationId()));
                if (!allowedOrganisationIds.Contains(entityId))
                {
                    throw new ErrorException(
                        UBind.Domain.Errors.Authorisation.CannotDeleteResourceFromThatOrganisation("organisation"));
                }
            }
        }

        public async Task ThrowIfUserCannotModify(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
            {
                throw new ErrorException(
                    Errors.Authorisation.PermissionRequiredToModifyResource(this.managePermissions, "organisation"));
            }

            if (!await this.CanUserManageAllOrganisations(performingUser))
            {
                var allowedOrganisationIds = await this.mediator.Send(new GetIdsOfOrganisationsManagedByQuery(
                        performingUser.GetTenantId(), performingUser.GetOrganisationId()));
                allowedOrganisationIds.Add(performingUser.GetOrganisationId());
                if (!allowedOrganisationIds.Contains(entityId))
                {
                    throw new ErrorException(
                        Errors.Authorisation.CannotModifyResourceFromThatOrganisation("organisation"));
                }
            }
        }

        public async Task ThrowIfUserCannotView(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
        {
            // permissions check
            await this.ThrowIfUserCannotViewAny(performingUser);

            if (!await this.CanUserManageAllOrganisations(performingUser))
            {
                var allowedOrganisationIds = await this.mediator.Send(new GetIdsOfOrganisationsManagedByQuery(
                        performingUser.GetTenantId(), performingUser.GetOrganisationId()));
                allowedOrganisationIds.Add(performingUser.GetOrganisationId());
                if (!allowedOrganisationIds.Contains(entityId))
                {
                    throw new ErrorException(
                        Errors.Authorisation.CannotViewResourceFromThatOrganisation("organisation"));
                }
            }
        }

        public async Task ThrowIfUserCannotViewAny(ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.viewPermissions)))
            {
                throw new ErrorException(
                    Errors.Authorisation.PermissionRequiredToViewResource(
                        this.viewPermissions, "organisation"));
            }
        }

        private async Task<bool> CanUserViewAllOrganisations(ClaimsPrincipal performingUser)
        {
            return await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(
                performingUser,
                Permission.ViewAllOrganisations,
                Permission.ManageAllOrganisations));
        }

        private async Task<bool> CanUserManageAllOrganisations(ClaimsPrincipal performingUser)
        {
            return await this.mediator.Send(new PrincipalHasPermissionQuery(
                performingUser,
                Permission.ManageAllOrganisations));
        }

        private async Task<bool> IsUserFromDefaultOrganisation(ClaimsPrincipal performingUser)
        {
            return await this.mediator.Send(new IsUserFromDefaultOrganisationQuery(performingUser));
        }
    }
}
