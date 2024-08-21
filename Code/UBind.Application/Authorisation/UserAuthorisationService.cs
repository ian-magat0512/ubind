// <copyright file="UserAuthorisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authorisation
{
    using System.Security.Claims;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Organisation;
    using UBind.Application.Queries.Principal;
    using UBind.Application.Queries.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using Errors = Domain.Errors;

    public class UserAuthorisationService : EntityAuthorisationService, IUserAuthorisationService
    {
        private readonly ICqrsMediator mediator;

        private readonly Permission[] viewPermissions = new Permission[] {
            Permission.ViewUsers,
            Permission.ViewUsersFromOtherOrganisations,
            Permission.ManageUsers,
            Permission.ManageUsersForOtherOrganisations,
        };

        private readonly Permission[] managePermissions = new Permission[] {
            Permission.ManageUsers,
            Permission.ManageUsersForOtherOrganisations,
        };

        public UserAuthorisationService(
            ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task ThrowIfUserCannotView(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
        {
            // permissions check
            await this.ThrowIfUserCannotViewAny(performingUser);

            if (!await this.CanUserViewUsersFromAllOrganisations(performingUser))
            {
                var user = await this.mediator.Send(new GetUserByIdQuery(tenantId, entityId));
                if (!await this.CanPerformingUserViewUsersFromOrganisation(performingUser, user.OrganisationId))
                {
                    throw new ErrorException(
                        Errors.Authorisation.CannotViewResourceFromThatOrganisation("user"));
                }
            }
        }

        public async Task ThrowIfUserCannotViewAny(ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.viewPermissions)))
            {
                throw new ErrorException(
                    Errors.Authorisation.PermissionRequiredToViewResource(
                        this.viewPermissions, "user"));
            }
        }

        public async Task ThrowIfUserCannotCreate(Guid tenantId, Guid organisationId, ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
            {
                throw new ErrorException(
                    Errors.Authorisation.PermissionRequiredToCreateResource(this.managePermissions, "user"));
            }

            if (!await this.CanUserManageUsersForAllOrganisations(performingUser))
            {
                if (!await this.CanPerformingUserManageUsersFromOrganisation(performingUser, organisationId))
                {
                    throw new ErrorException(
                        Errors.Authorisation.CannotCreateResourceInThatOrganisation("user"));
                }
            }
        }

        public async Task ThrowIfUserCannotModify(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
            {
                throw new ErrorException(
                    Errors.Authorisation.PermissionRequiredToModifyResource(this.managePermissions, "user"));
            }

            var user = await this.mediator.Send(new GetUserByIdQuery(tenantId, entityId));
            if (!await this.CanUserManageUsersForAllOrganisations(performingUser))
            {
                if (!await this.CanPerformingUserManageUsersFromOrganisation(performingUser, user.OrganisationId))
                {
                    throw new ErrorException(
                        Errors.Authorisation.CannotModifyResourceFromThatOrganisation("user"));
                }
            }

            await this.ThrowIfUserCannotModifyAdminUsers(performingUser, user);
        }

        public async Task ThrowIfUserCannotDelete(Guid tenantId, Guid entityId, ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(performingUser, this.managePermissions)))
            {
                throw new ErrorException(
                    Errors.Authorisation.PermissionRequiredToDeleteResource(this.managePermissions, "user"));
            }

            if (performingUser.GetId() == entityId)
            {
                throw new ErrorException(Errors.Authorisation.CannotDeleteOwnUserAccount());
            }

            var user = await this.mediator.Send(new GetUserByIdQuery(tenantId, entityId));
            if (!await this.CanUserManageUsersForAllOrganisations(performingUser))
            {
                if (!await this.CanPerformingUserManageUsersFromOrganisation(performingUser, user.OrganisationId))
                {
                    throw new ErrorException(
                        Errors.Authorisation.CannotDeleteResourceFromThatOrganisation("user"));
                }
            }

            await this.ThrowIfUserCannotModifyAdminUsers(performingUser, user);
        }

        public async Task ApplyRestrictionsToFilters(ClaimsPrincipal performingUser, EntityListFilters filters)
        {
            if (!await this.CanUserViewUsersFromAllOrganisations(performingUser))
            {
                List<Guid> restrictToOrganisationIds
                    = await this.GetIdsOfOrganisationsWhichPerformingUserCanViewUsersFrom(performingUser);

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

                // fail-safe: if we can't view all organisations but we don't have any restriction in the list, at
                // least restrict the list to the user's own organisation
                if (filters.OrganisationIds.None())
                {
                    filters.OrganisationIds = new List<Guid> { performingUser.GetOrganisationId() };
                }
            }
        }

        private async Task<bool> CanUserViewUsersFromAllOrganisations(ClaimsPrincipal performingUser)
        {
            return performingUser.IsMasterUser()
                && await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(
                    performingUser,
                    Permission.ViewUsersFromOtherOrganisations,
                    Permission.ManageUsersForOtherOrganisations));
        }

        private async Task<bool> CanUserManageUsersForAllOrganisations(ClaimsPrincipal performingUser)
        {
            return performingUser.IsMasterUser()
                && await this.mediator.Send(new PrincipalHasOneOfThePermissionsQuery(
                    performingUser,
                    Permission.ManageUsersForOtherOrganisations));
        }

        private async Task<bool> CanPerformingUserViewUsersFromOrganisation(
            ClaimsPrincipal performingUser, Guid organisationId)
        {
            return (await this.GetIdsOfOrganisationsWhichPerformingUserCanViewUsersFrom(
                performingUser)).Contains(organisationId);
        }

        private async Task<List<Guid>> GetIdsOfOrganisationsWhichPerformingUserCanViewUsersFrom(
            ClaimsPrincipal performingUser)
        {
            // if the user has the permission ManageUsersForOtherOrganisations OR ViewUsersFromOtherOrganisations,
            // then they can view users from organisations that their organisation is a managing organisation of
            List<Guid> organisationIds = new List<Guid>();
            if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewUsersFromOtherOrganisations))
                || await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageUsersForOtherOrganisations)))
            {
                // get the list of organisation ids that the user's organisation is managing
                organisationIds.AddRange(await this.GetIdsOfOrganisationsManageByPerformingUserOrganisation(performingUser));
                organisationIds.Add(performingUser.GetOrganisationId());
            }
            else if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageUsers))
                || await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewUsers)))
            {
                // user can only view users from their own organisation
                organisationIds.Add(performingUser.GetOrganisationId());
            }

            return organisationIds;
        }

        private async Task<bool> CanPerformingUserManageUsersFromOrganisation(
            ClaimsPrincipal performingUser, Guid organisationId)
        {
            return (await this.GetIdsOfOrganisationsWhichPerformingUserCanManageUsersIn(
                performingUser)).Contains(organisationId);
        }

        private async Task<List<Guid>> GetIdsOfOrganisationsWhichPerformingUserCanManageUsersIn(
            ClaimsPrincipal performingUser)
        {
            List<Guid> organisationIds = new List<Guid>();
            if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageUsersForOtherOrganisations)))
            {
                // get the list of organisation ids that the user's organisation is managing
                organisationIds.AddRange(await this.GetIdsOfOrganisationsManageByPerformingUserOrganisation(performingUser));
                organisationIds.Add(performingUser.GetOrganisationId());
            }
            else if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageUsers)))
            {
                // user can only manage users from their own organisation
                organisationIds.Add(performingUser.GetOrganisationId());
            }

            return organisationIds;
        }

        private async Task ThrowIfUserCannotModifyAdminUsers(ClaimsPrincipal performingUser, IUserReadModelSummary user)
        {
            if (user.HasPermission(Permission.ManageTenantAdminUsers) &&
                 !await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageTenantAdminUsers)))
            {
                // only another client admin can modify a client admin
                throw new ErrorException(
                    Errors.User.Authorisation.CannotModifyUserWithElevatedPermission(
                        user.FullName,
                        Permission.ManageTenantAdminUsers));
            }

            if (user.HasPermission(Permission.ManageOrganisationAdminUsers) &&
                !await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageOrganisationAdminUsers)))
            {
                // only another org admin can modify a org admin
                throw new ErrorException(
                    Errors.User.Authorisation.CannotModifyUserWithElevatedPermission(
                        user.FullName,
                        Permission.ManageOrganisationAdminUsers));
            }
        }

        private async Task<List<Guid>> GetIdsOfOrganisationsManageByPerformingUserOrganisation(ClaimsPrincipal performingUser)
        {
            var isDefaultOrganisation = await this.mediator.Send(new IsUserFromDefaultOrganisationQuery(performingUser));
            if (isDefaultOrganisation)
            {
                // if its the default organisation, then it can manage all sub-organisations
                return await this.mediator.Send(
                    new GetIdsOfOrganisationsManagedByQuery(
                        performingUser.GetTenantId(), null));
            }

            return await this.mediator.Send(
                new GetIdsOfOrganisationsManagedByQuery(
                    performingUser.GetTenantId(), performingUser.GetOrganisationId()));
        }
    }
}
