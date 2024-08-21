// <copyright file="RoleController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.User;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Principal;
    using UBind.Application.Queries.Role;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for Roles and its permissions.
    /// </summary>
    /// <remarks>Tenant ID in route is not used, and is only there for making web server logs more informative.</remarks>
    [MustBeLoggedIn]
    [Produces("application/json")]
    [Route("api/v1/role")]
    public class RoleController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IRoleService roleService;
        private readonly IUserService userService;
        private readonly IRoleTypePermissionsRegistry roleTypePermissionsRegistry;
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;
        private readonly IUserAuthorisationService userAuthorisationService;
        private readonly IRoleAuthorisationService roleAuthorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleController"/> class.
        /// </summary>
        /// <param name="roleService">The setting repository.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="roleTypePermissionsRegistry">The role types permissions registry.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        /// <param name="userAuthorisationService">The user authorisation service.</param>
        public RoleController(
            IRoleService roleService,
            IUserService userService,
            IRoleTypePermissionsRegistry roleTypePermissionsRegistry,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IUserAuthorisationService userAuthorisationService,
            IRoleAuthorisationService roleAuthorisationService)
        {
            this.cachingResolver = cachingResolver;
            this.roleService = roleService;
            this.userService = userService;
            this.roleTypePermissionsRegistry = roleTypePermissionsRegistry;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
            this.userAuthorisationService = userAuthorisationService;
            this.roleAuthorisationService = roleAuthorisationService;
        }

        /// <summary>
        /// Gets all roles (for the user's tenant).
        /// </summary>
        /// <param name="options">Role query options.</param>
        /// <returns>The role list of roles.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(
            Permission.ManageUsers,
            Permission.ManageUsersForOtherOrganisations,
            Permission.ViewRoles,
            Permission.ViewRolesFromAllOrganisations,
            Permission.ManageRoles,
            Permission.ManageRolesForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(IEnumerable<RoleSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] RoleQueryOptionsModel options)
        {
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options, restrictToOwnOrganisation: false);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(options.Tenant));
            RoleReadModelFilters filters = await options.ToFilters(tenantModel.Id, this.cachingResolver);
            await this.roleAuthorisationService.ApplyRestrictionsToFilters(this.User, filters);
            await this.ApplyModifyUserOrViewRolePermissionFilters(filters);
            List<RoleSummaryModel> roles;
            if (options.Assignable.HasValue && options.Assignable.Value)
            {
                if (options.OrganisationId != null)
                {
                    filters.OrganisationIds = new List<Guid> { options.OrganisationId.Value };
                }

                roles = (await this.mediator.Send(new GetAssignableRolesMatchingFiltersQuery(filters)))
                    .Select(r => new RoleSummaryModel(r))
                    .ToList();
            }
            else
            {
                roles = this.roleService
                    .GetRoles(tenantModel.Id, filters)
                    .Select(r => new RoleSummaryModel(r))
                    .ToList();
            }

            return this.Ok(roles);
        }

        /// <summary>
        /// Gets a specific role by ID.
        /// </summary>
        /// <param name="roleId">The ID of the role to get.</param>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>The role including permissions.</returns>
        [HttpGet("{roleId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewRoles, Permission.ViewRolesFromAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(RoleDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoleById(Guid roleId, string tenant = null)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.roleAuthorisationService.ThrowIfUserCannotView(tenantModel.Id, roleId, this.User);
            var role = this.roleService.GetRole(tenantModel.Id, roleId);
            var roleModel = new RoleDetailModel(role);
            return this.Ok(roleModel);
        }

        /// <summary>
        /// Create a new role.
        /// </summary>
        /// <param name="model">Model with product details.</param>
        /// <returns>The newly created role.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageRoles, Permission.ManageRolesForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(RoleDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] RoleUpdateModel model)
        {
            await this.roleAuthorisationService.ThrowIfUserCannotCreate(this.User.GetTenantId(), this.User.GetOrganisationId(), this.User);
            RoleType roleType = this.User.GetTenantId() == Tenant.MasterTenantId ? RoleType.Master : RoleType.Client;
            var role = await this.roleService.CreateRole(
                this.User.GetTenantId(),
                this.User.GetOrganisationId(),
                model.Name,
                roleType,
                model.Description);
            return this.Ok(new RoleDetailModel(role));
        }

        /// <summary>
        /// Updates a role.
        /// </summary>
        /// <param name="roleId">The ID of the role to update.</param>
        /// <param name="model">Resource model for the role.</param>
        /// <returns>The updated role.</returns>
        [HttpPut("{roleId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageRoles, Permission.ManageRolesForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(RoleDetailModel), StatusCodes.Status200OK)]
        public IActionResult Put(Guid roleId, [FromBody] RoleUpdateModel model)
        {
            this.roleAuthorisationService.ThrowIfUserCannotModify(this.User.GetTenantId(), roleId, this.User);
            var role = this.roleService.UpdateRole(this.User.GetTenantId(), roleId, model.Name, model.Description);
            return this.Ok(new RoleDetailModel(role));
        }

        /// <summary>
        /// Deletes a Role.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>The newly created product.</returns>
        [HttpDelete("{roleId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageRoles, Permission.ManageRolesForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public IActionResult Delete(Guid roleId)
        {
            this.roleAuthorisationService.ThrowIfUserCannotDelete(this.User.GetTenantId(), roleId, this.User);
            var result = this.roleService.DeleteRole(this.User.GetTenantId(), roleId);
            return this.Ok(result);
        }

        /// <summary>
        /// Assigns a permission to a role.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="permission">The permission to assign.</param>
        /// <returns>A response indicating success or error.</returns>
        [HttpPost("{roleId}/permission/{permission}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageRoles, Permission.ManageRolesForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignPermission(Guid roleId, Permission permission)
        {
            await this.roleAuthorisationService.ThrowIfUserCannotModify(this.User.GetTenantId(), roleId, this.User);
            this.roleService.AddPermissionToRole(this.User.GetTenantId(), roleId, permission);
            return this.Ok();
        }

        /// <summary>
        /// Update a permission for the role.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="previousPermission">The previous permission to update.</param>
        /// <param name="newPermission">The permission to assign.</param>
        /// <returns>A response indicating success or error.</returns>
        [HttpPut("{roleId}/permission/{previousPermission}/{newPermission}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageRoles, Permission.ManageRolesForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePermission(Guid roleId, Permission previousPermission, Permission newPermission)
        {
            await this.roleAuthorisationService.ThrowIfUserCannotModify(this.User.GetTenantId(), roleId, this.User);
            this.roleService.UpdatePermissionOfARole(this.User.GetTenantId(), roleId, previousPermission, newPermission);
            return this.Ok();
        }

        /// <summary>
        /// Removes a permission from a role.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="permission">The permission to remove.</param>
        /// <returns>A response indicating success or error.</returns>
        [HttpDelete("{roleId}/permission/{permission}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageRoles, Permission.ManageRolesForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RetractPermission(Guid roleId, Permission permission)
        {
            await this.roleAuthorisationService.ThrowIfUserCannotModify(this.User.GetTenantId(), roleId, this.User);
            this.roleService.RemovePermissionFromRole(this.User.GetTenantId(), roleId, permission);
            return this.Ok();
        }

        /// <summary>
        /// Gets all permissions of the specified tenant.
        /// </summary>
        /// <param name="roleType">The applicable role type to filter by.</param>
        /// <returns>All permissions available for the specified tenant.</returns>
        [HttpGet("permissions")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ManageRoles, Permission.ManageRolesForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(IEnumerable<PermissionModel>), StatusCodes.Status200OK)]
        public IActionResult GetPermissions(RoleType? roleType)
        {
            IEnumerable<Permission> permissions;
            if (roleType.HasValue)
            {
                permissions = this.roleTypePermissionsRegistry.GetPermissionsForRoleType(roleType.Value);
            }
            else
            {
                permissions = Enum<Permission>.GetValues();
            }

            var permissionModels = permissions.Select(p => new PermissionModel(p));
            return this.Ok(permissionModels);
        }

        /// <summary>
        /// List roles assigned to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tenant">The Id or Alias of the tenant (optional).</param>
        /// <returns>A response indicating success or errors.</returns>
        [HttpGet("user/{userId}/roles")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(IEnumerable<RoleSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRoles(Guid userId, string tenant = null)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.userAuthorisationService.ThrowIfUserCannotView(tenantModel.Id, userId, this.User);
            var roles = this.userService.GetUserRoles(tenantModel.Id, userId);
            var roleSummaries = roles.Select(r => new RoleSummaryModel(r));
            return this.Ok(roleSummaries);
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="tenant">The Id or Alias of the tenant (optional).</param>
        /// <returns>A response indicating success or errors.</returns>
        [HttpPut("user/{userId}/roles/{roleId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignRoleToUser(Guid userId, Guid roleId, string tenant = null)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantModel.Id, userId, this.User);
            await this.mediator.Send(new AssignRoleToUserCommand(tenantModel.Id, userId, roleId));
            return this.Ok();
        }

        /// <summary>
        /// Retracts a role from a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="tenant">The Id or Alias of the tenant (optional).</param>
        /// <returns>A response indicating success or errors.</returns>
        [HttpDelete("user/{userId}/roles/{roleId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId, string tenant = null)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantModel.Id, userId, this.User);
            await this.mediator.Send(new UnassignRoleFromUserCommand(tenantModel.Id, userId, roleId));
            return this.Ok();
        }

        private async Task ApplyModifyUserOrViewRolePermissionFilters(RoleReadModelFilters filters)
        {
            if (await this.mediator.Send(new PrincipalHasPermissionQuery(this.User, Permission.ManageUsers))
               || await this.mediator.Send(new PrincipalHasPermissionQuery(this.User, Permission.ManageUsersForOtherOrganisations)))
            {
                // filter by manage users permission.
                await this.authorisationService.ApplyModifyUserRestrictionsToFilters(this.User, filters);
            }
            else
            {
                // filter by view role permission.
                await this.authorisationService.ApplyViewRoleRestrictionsToFilters(this.User, filters);
            }
        }
    }
}
