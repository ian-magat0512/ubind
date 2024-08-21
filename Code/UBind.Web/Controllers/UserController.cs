// <copyright file="UserController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers
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
    using UBind.Application.Person;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Queries.User;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Organisation;

    /// <summary>
    /// Controller for user requests.
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("api/v1/user")]
    public class UserController : PortalBaseController
    {
        private readonly IOrganisationService organisationService;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly Application.User.IUserService userService;
        private readonly IUserProfilePictureRepository userProfilePictureRepository;
        private readonly IAuthorisationService authorisationService;
        private readonly IPersonService personService;
        private readonly ICqrsMediator mediator;
        private readonly IUserAuthorisationService userAuthorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">The user service of <see cref="Application.User"/>.</param>
        /// <param name="personService">The person service.</param>
        /// <param name="userProfilePictureRepository">The repo holding profile pictures.</param>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">Mediator.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        /// <param name="userAuthorisationService">The service for checking whether you're allowed.</param>
        public UserController(
            Application.User.IUserService userService,
            IPersonService personService,
            IUserProfilePictureRepository userProfilePictureRepository,
            IOrganisationService organisationService,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IAdditionalPropertyValueService additionalPropertyValueService,
            IUserAuthorisationService userAuthorisationService)
            : base(cachingResolver)
        {
            this.userService = userService;
            this.personService = personService;
            this.userProfilePictureRepository = userProfilePictureRepository;
            this.organisationService = organisationService;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.userAuthorisationService = userAuthorisationService;
        }

        /// <summary>
        /// Retrieves users.
        /// </summary>
        /// <param name="options">Additional filters to be used against the result set.</param>
        /// <returns>List of users.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(
            Permission.ViewUsers,
            Permission.ViewUsersFromOtherOrganisations,
            Permission.ManageUsers,
            Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(IEnumerable<UserResourceModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers([FromQuery] UserQueryOptionsModel options)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(options.Tenant, "list users from a different tenancy");
            options.Tenant = tenantId.ToString();
            options.Organisation = options.Organisation ?? this.User.GetOrganisationId().ToString();
            var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(options.Tenant));
            var filter = await options.ToFilters(tenantModel.Id, this.CachingResolver);
            await this.userAuthorisationService.ApplyRestrictionsToFilters(this.User, filter);
            var organisationModel = await this.CachingResolver.GetOrganisationOrThrow(tenantModel, new GuidOrAlias(options.OrganisationId));
            var users = await this.mediator.Send(new GetUsersQuery(tenantId, filter));
            return this.Ok(users.Select(u => new UserResourceModel(u)));
        }

        /// <summary>
        /// Retrieves users that are assignable as owner.
        /// </summary>
        /// <param name="organisation">The organisation.</param>
        /// <returns>List of users.</returns>
        [HttpGet("assignable-as-owner")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(
            Permission.ManageCustomers,
            Permission.ManageAllCustomers,
            Permission.ManageAllCustomersForAllOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(IEnumerable<UserResourceModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersAssignableAsOwner(string organisation)
        {
            var organisationModel = await this.CachingResolver.GetOrganisationOrThrow(this.User.GetTenantId(), new GuidOrAlias(organisation));
            var query = new GetUsersAssignableAsOwnerQuery(this.User.GetTenantId(), organisationModel.Id);
            var users = await this.mediator.Send(query);
            var result = users.Select(u => new UserResourceModel(u));
            return this.Ok(result);
        }

        /// <summary>
        /// Gets the available tenant roles for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tenant">The tenant ID or alias (optional).</param>
        /// <returns>List of available role summaries.</returns>
        [HttpGet("{userId}/available-roles")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(
            Permission.ViewUsers,
            Permission.ViewUsersFromOtherOrganisations,
            Permission.ManageUsers,
            Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(IEnumerable<RoleSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableTenantRolesForUser(Guid userId, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "list roles from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotView(tenantId, userId, this.User);
            var roles = this.userService.GetAvailableUserRoles(tenantId, userId);
            var roleSummaries = roles.Select(r => new RoleSummaryModel(r));
            return this.Ok(roleSummaries);
        }

        /// <summary>
        /// Gets the specific organisation for a user.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="organisation">The Id or Alias of the organisation.</param>
        /// <param name="tenant">The tenant ID or alias (optional).</param>
        /// <returns>The organisation record that matches the given Id.</returns>
        /// <exception cref="UnauthorizedException">An error occured for unauthorised user access.</exception>
        [HttpGet("{userId}/organisation/{organisation}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations)]
        [ProducesResponseType(typeof(OrganisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrganisationForUser(Guid userId, string organisation, [FromQuery] string? tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "access users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotView(tenantId, userId, this.User);
            var organisationModel = await this.CachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            var organisationSummary = await this.organisationService
                .GetOrganisationById(tenantId, organisationModel.Id);
            return this.Ok(new OrganisationModel(organisationSummary));
        }

        /// <summary>
        /// Retrieves a specific user registered in the database.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tenant">The ID of the tenant.</param>
        /// <param name="organisation">The ID of the organisation.</param>
        /// <returns>A user record.</returns>
        [HttpGet]
        [Route("{userId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations)]
        [ProducesResponseType(typeof(UserDetailResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUser(Guid userId, [FromQuery] string? tenant = null, [FromQuery] string? organisation = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "access users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotView(tenantId, userId, this.User);
            var user = await this.mediator.Send(new GetUserDetailByIdQuery(tenantId, userId));
            if (!string.IsNullOrEmpty(organisation))
            {
                var organisationId = new GuidOrAlias(organisation);
                if (organisationId.Guid.HasValue && user.OrganisationId != organisationId.Guid)
                {
                    return Errors.General.NotFound("user", organisation, "organisationId")
                        .ToProblemJsonResult();
                }
            }

            PortalReadModel? portal = null;
            if (user.PortalId.HasValue)
            {
                portal = await this.mediator.Send(
                    new GetPortalByIdQuery(user.TenantId, user.PortalId.Value));
            }

            var person = this.personService.Get(user.TenantId, user.PersonId);
            var additionalPropertyValuesDto = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
               user.TenantId, Domain.Enums.AdditionalPropertyEntityType.User, userId);

            return this.Ok(new UserDetailResourceModel(user, portal, person, additionalPropertyValuesDto));
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="model">User sign-up view model.</param>
        /// <returns>Instance of the created user.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [ValidateModel]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(UserResourceModel), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateUser([FromBody] SignupModel model)
        {
            var tenantModel = await this.GetContextTenantOrThrow(model.Tenant, "create users in a different tenancy");
            model.Environment = model.Environment == DeploymentEnvironment.None ? DeploymentEnvironment.Production : model.Environment;
            model.TenantId = tenantModel.Id;
            Guid organisationId = model.OrganisationId == default ? tenantModel.Details.DefaultOrganisationId : model.OrganisationId;
            foreach (var roleId in model.InitialRoles)
            {
                await this.authorisationService.ThrowIfRoleIsNotAssignableToUserUnderOrganisation(tenantModel.Id, roleId, organisationId);
            }

            await this.userAuthorisationService.ThrowIfUserCannotCreate(tenantModel.Id, organisationId, this.User);
            if (model.PortalId.HasValue)
            {
                var portal = await this.mediator.Send(
                    new GetPortalByIdQuery(tenantModel.Id, model.PortalId.Value));
                if (portal == null)
                {
                    return Errors.Portal.NotFound(tenantModel.Id, model.PortalId.Value.ToString())
                        .ToProblemJsonResult();
                }
            }

            var defaultUserType = this.User.GetTenantId() == Tenant.MasterTenantId && model.TenantId == Tenant.MasterTenantId
                ? UserType.Master
                : UserType.Client;
            UserType userType = (UserType)Enum.Parse(typeof(UserType), model.UserType ?? defaultUserType.ToString());
            UserSignupModel userSignupModel = new UserSignupModel
            {
                ActivationInvitationId = model.ActivationInvitationId,
                AlternativeEmail = model.AlternativeEmail,
                WorkPhoneNumber = model.WorkPhoneNumber,
                Email = model.Email,
                Environment = model.Environment,
                FullName = model.FullName,
                NamePrefix = model.NamePrefix,
                FirstName = model.FirstName,
                MiddleNames = model.MiddleNames,
                NameSuffix = model.NameSuffix,
                LastName = model.LastName,
                Company = model.Company,
                Title = model.Title,
                HomePhoneNumber = model.HomePhoneNumber,
                MobilePhoneNumber = model.MobilePhoneNumber,
                PreferredName = model.PreferredName,
                UserType = userType,
                SendActivationInvitation = model.SendActivationInvitation == null ? true : model.SendActivationInvitation,
                Password = model.Password == null ? string.Empty : model.Password,
                TenantId = tenantModel.Id,
                OrganisationId = organisationId,
                PortalId = model.PortalId,
                UserId = model.UserId,
                EmailAddresses = model.EmailAddresses,
                PhoneNumbers = model.PhoneNumbers,
                StreetAddresses = model.StreetAddresses,
                WebsiteAddresses = model.WebsiteAddresses,
                MessengerIds = model.MessengerIds,
                SocialMediaIds = model.SocialMediaIds,
                InitialRoles = model.InitialRoles,
                Properties = model.Properties.ToDomainAdditionalProperties(),
            };

            var userDto = await this.mediator.Send(new CreateUserCommand(userSignupModel));
            if ((bool)userSignupModel.SendActivationInvitation)
            {
                this.Response.Headers.Add("Status-Description", $"User account created. An account activation link has been sent to the email address {model.Email}");
            }
            else
            {
                this.Response.Headers.Add("Status-Description", "User account created. The user account was created with a password and no activation link will be sent.");
            }

            return this.Ok(new UserResourceModel(userDto));
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="model">The user update dto.</param>
        /// <returns>The response object result.</returns>
        [HttpPut("{userId}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(UserDetailResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] ResourceModels.UserUpdateModel model)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(model.Tenant, "update users in a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            var properties = model.Properties.ToDomainAdditionalProperties();
            await this.mediator.Send(new UpdateUserCommand(
                tenantId,
                userId,
                model.ToUserUpdateModel(),
                properties));
            var userReadModel = await this.mediator.Send(new GetUserDetailByIdQuery(tenantId, userId));
            PortalReadModel? portal = null;
            if (userReadModel.PortalId.HasValue)
            {
                portal = await this.mediator.Send(
                    new GetPortalByIdQuery(userReadModel.TenantId, userReadModel.PortalId.Value));
            }

            var result = new UserDetailResourceModel(userReadModel, portal);
            return this.Ok(result);
        }

        /// <summary>
        /// Enable an existing user by ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>The updated user model.</returns>
        [HttpPatch("{userId}/enable")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(UserDetailResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableUserById(Guid userId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "enable users in a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            var command = new EnableUserByIdCommand(tenantId, userId);
            var user = await this.mediator.Send(command);
            PortalReadModel? portal = null;
            if (user.PortalId.HasValue)
            {
                portal = await this.mediator.Send(
                    new GetPortalByIdQuery(user.TenantId, user.PortalId.Value));
            }

            var result = new UserDetailResourceModel(user, portal);
            return this.Ok(result);
        }

        /// <summary>
        /// Disable an existing user by ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <returns>The updated user model.</returns>
        [HttpPatch("{userId}/disable")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(UserDetailResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableUserById(Guid userId, [FromQuery] string tenant)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "disable users in a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            var command = new DisableUserByIdCommand(tenantId, userId);
            var user = await this.mediator.Send(command);
            PortalReadModel? portal = null;
            if (user.PortalId.HasValue)
            {
                portal = await this.mediator.Send(
                    new GetPortalByIdQuery(user.TenantId, user.PortalId.Value));
            }

            var result = new UserDetailResourceModel(user, portal);
            return this.Ok(result);
        }

        /// <summary>
        /// Gets the user picture based from user id.
        /// Note: It is not recommended to use this, since when the picture is updated the url won't change and
        /// the browser will cache it. We do want caching for speed,
        /// so please use /api/v1/picture/{pictureId} instead.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>The user picture data.</returns>
        [HttpGet]
        [Obsolete]
        [Route("{userId}/profile-picture")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPicture(Guid userId, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "view users in a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotView(tenantId, userId, this.User);

            // TODO: Change this so we use a profile picture Id that's different to the users Guid
            var profilePicture = this.userProfilePictureRepository.GetById(userId);
            if (profilePicture == null)
            {
                return Errors.General.NotFound("profile picture for user", userId).ToProblemJsonResult();
            }

            return this.File(profilePicture.PictureData, "image/png");
        }

        /// <summary>
        /// Enable user by matching exact user email and email with plus sign.
        /// When tenantId is null it will enable all users with thatemail address across all tenants.
        /// </summary>
        /// <param name="userStatusUpdateModel">The user status model.</param>
        /// <returns>The response object result.</returns>
        [HttpPatch("enable")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<UserStatusModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableUser([FromBody] ToggleUserModel userStatusUpdateModel)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "enable users across tenancies by email address");
            Guid? tenantId = null;
            if (!string.IsNullOrEmpty(userStatusUpdateModel.Tenant))
            {
                var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(userStatusUpdateModel.Tenant));
                tenantId = tenantModel.Id;
            }

            var query = new GetUsersMatchingEmailAddressIncludingPlusAddressingQuery(
               tenantId,
               null,
               userStatusUpdateModel.Email,
               blocked: true,
               this.User.GetId());
            var userModels = await this.mediator.Send(query);
            if (!userModels.Any())
            {
                return this.Ok(Enumerable.Empty<UserStatusModel>());
            }
            var command = new EnableOrDisableUsersCommand(
                userModels,
                blocked: false);
            var users = await this.mediator.Send(command);
            var userUpdatedModels = users.Select(r => new UserStatusModel(r));
            return this.Ok(userUpdatedModels);
        }

        /// <summary>
        /// Disable user by matching exact user email and email with plus sign.
        /// When tenantId is null it will disable all user matching with email across tenant.
        /// </summary>
        /// <param name="userStatusUpdateModel">The user status model.</param>
        /// <returns>The response object result.</returns>
        [HttpPatch("disable")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<UserStatusModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableUser([FromBody] ToggleUserModel userStatusUpdateModel)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "disable users across tenancies by email address");
            Guid? tenantId = null;
            if (!string.IsNullOrEmpty(userStatusUpdateModel.Tenant))
            {
                var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(userStatusUpdateModel.Tenant));
                tenantId = tenantModel.Id;
            }
            var query = new GetUsersMatchingEmailAddressIncludingPlusAddressingQuery(
               tenantId,
               null,
               userStatusUpdateModel.Email,
               blocked: false,
               this.User.GetId());
            var userModels = await this.mediator.Send(query);
            if (!userModels.Any())
            {
                return this.Ok(Enumerable.Empty<UserStatusModel>());
            }
            var command = new EnableOrDisableUsersCommand(
                userModels,
                blocked: true);
            var userReadmodels = await this.mediator.Send(command);
            var users = userReadmodels.Select(user => new UserStatusModel(user));
            return this.Ok(users);
        }

        /// <summary>
        /// Get user by email by matching exact email address and
        /// email with same username and domain name but with plus sign.
        /// </summary>
        /// <param name="userEmailModel">The user email model.</param>
        /// <returns>The response object result.</returns>
        [HttpGet("get-user-by-email")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<UserStatusModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserByEmail([FromQuery] UsersByEmailRequestModel userEmailModel)
        {
            await this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "list users across tenancies by email address");
            Guid? tenantId = null;
            if (!string.IsNullOrEmpty(userEmailModel.Tenant))
            {
                var tenantModel = await this.CachingResolver.GetTenantOrThrow(new GuidOrAlias(userEmailModel.Tenant));
                tenantId = tenantModel.Id;
            }

            var query = new GetUsersMatchingEmailAddressIncludingPlusAddressingQuery(
                tenantId,
                null,
                userEmailModel.Email,
                userEmailModel.Blocked,
                this.User.GetId());
            var userModels = await this.mediator.Send(query);
            var users = userModels.Select(r => new UserStatusModel(r));
            return this.Ok(users);
        }

        /// <summary>
        /// Updates the user account profile picture.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="tenant">The tenant Id or Alias.</param>
        /// <returns>The instance of the updated user account.</returns>
        [HttpPost]
        [Route("{userId}/profile-picture")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [ProducesResponseType(typeof(UserResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUserPicture(Guid userId, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update users in a different tenancy");

            // check that they're authorised.
            if (userId != this.User.GetId())
            {
                // check that they're authorised.
                await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            }

            var user = await this.mediator.Send(new GetUserByIdQuery(tenantId, userId));
            if (user == null)
            {
                return Errors.General.NotFound("user", userId).ToProblemJsonResult();
            }

            user.ProfilePictureId = await this.userService.SaveProfilePictureForUser(
                tenantId,
                this.Request.Form.Files[0],
                user);
            return this.Ok(new UserResourceModel(user));
        }

        /// <summary>
        /// List roles assigned to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>A response indicating success or errors.</returns>
        [HttpGet]
        [Route("{userId}/role")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<RoleSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRoles(Guid userId, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "view users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotView(tenantId, userId, this.User);
            var roles = this.userService.GetUserRoles(tenantId, userId);
            var roleSummaries = roles.Select(r => new RoleSummaryModel(r));
            return this.Ok(roleSummaries);
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>A response indicating success or errors.</returns>
        [HttpPost]
        [Route("{userId}/role/{roleId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignRoleToUser(Guid userId, Guid roleId, string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            await this.userService.AddRoleToUser(tenantId, userId, roleId);
            return this.Ok();
        }

        /// <summary>
        /// Unassign a role from a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roleId">The ID of the role.</param>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>A response indicating success or errors.</returns>
        [HttpDelete("{userId}/role/{roleId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId, string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            await this.userService.RemoveUserRole(tenantId, userId, roleId);
            return this.Ok();
        }

        /// <summary>
        /// Gets the effective permissions of a user.
        /// The effective permissions are the aggregate permissions from all of the assigned roles of the user,
        /// minus any permissions which may not be enabled for that user's organisation.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tenant">The Id or Alias of the tenant.</param>
        /// <returns>A response indicating success or errors.</returns>
        [HttpGet("{userId}/permission")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewUsers, Permission.ViewUsersFromOtherOrganisations)]
        [ProducesResponseType(typeof(IEnumerable<PermissionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEffectivePermissionsForUser(Guid userId, string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotView(tenantId, userId, this.User);
            List<Permission> permissions = await this.mediator.Send(
                new GetEffectivePermissionsForUserQuery(tenantId, userId));
            var permissionModels = permissions.Select(p => new PermissionModel(p));
            return this.Ok(permissionModels);
        }

        /// <summary>
        /// Transfer the user to a different organisation of the same tenancy.
        /// </summary>
        /// <param name="userId">The ID of the user to move.</param>
        /// <param name="organisation">The Id or Alias of the organisation on where the user will be moving.</param>
        /// <param name="includeCustomers">An option whether or not to include moving its customers. The default value is true.</param>
        /// <returns>The action result containing status codes.</returns>
        [HttpPatch("{userId}/organisation/{organisation}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHavePermission(Permission.ManageUsers)]
        [MustHavePermission(Permission.ManageUsersForOtherOrganisations)]
        [MustHaveOneOfPermissions(Permission.ManageOrganisations, Permission.ManageAllOrganisations)]
        [MustHavePermission(Permission.ManageTenantAdminUsers)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferUserOrganisation(
            Guid userId, string organisation, [FromQuery] bool includeCustomers = true, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            var organisationModel = await this.CachingResolver.GetOrganisationOrThrow(tenantId, new GuidOrAlias(organisation));
            var command = new TransferUserToOtherOrganisationCommand(tenantId, userId, organisationModel.Id, includeCustomers);
            await this.mediator.Send(command);
            return this.Ok($"Validation passed and now starting to transfer user '{userId}' to organisation '{organisation}'");
        }

        /// <summary>
        /// Retrieves a specific user with the given person id.
        /// </summary>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="tenant">The ID or alias of the tenant.</param>
        /// <returns>A user record.</returns>
        [HttpGet("person/{personId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewUsers)]
        [RequiresFeature(Feature.UserManagement)]
        [ProducesResponseType(typeof(UserResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserByPersonId(Guid personId, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "view users from a different tenancy");
            this.authorisationService.ThrowIfUserNotInTheSameOrMasterTenancy(tenantId, this.User);
            var command = new GetUserByPersonIdQuery(tenantId, personId);
            var user = await this.mediator.Send(command);
            await this.userAuthorisationService.ThrowIfUserCannotView(tenantId, user.Id, this.User);
            return this.Ok(new UserResourceModel(user));
        }

        /// <summary>
        /// Unlinks this users identity from the specified authentication method representing an identity provider.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="authenticationMethodId">The ID of the authentication method.</param>
        /// <param name="tenant">The tenant ID or aliase.</param>
        [HttpDelete]
        [Route("{userId}/linked-identity/{authenticationMethodId}")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations)]
        [ProducesResponseType(typeof(UserDetailResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UnlinkIdentity(Guid userId, Guid authenticationMethodId, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            var command = new UnlinkIdentityCommand(tenantId, userId, authenticationMethodId);
            await this.mediator.Send(command);
            var user = await this.mediator.Send(new GetUserDetailByIdQuery(tenantId, userId));
            PortalReadModel? portal = null;
            if (user.PortalId.HasValue)
            {
                portal = await this.mediator.Send(
                    new GetPortalByIdQuery(user.TenantId, user.PortalId.Value));
            }

            var person = this.personService.Get(user.TenantId, user.PersonId);
            var additionalPropertyValuesDto = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
               user.TenantId, Domain.Enums.AdditionalPropertyEntityType.User, userId);
            return this.Ok(new UserDetailResourceModel(user, portal, person, additionalPropertyValuesDto));
        }

        /// <summary>
        /// Sets portal for a user.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="portalId">The Id of the portal to put into user.</param>
        /// <returns>No content.</returns>
        [HttpPatch]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations, Permission.ManageOrganisationAdminUsers, Permission.ManageTenantAdminUsers)]
        [Route("{userId}/portal/{portalId}")]
        public async Task<IActionResult> AssignPortalToUser(Guid userId, Guid portalId, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            var assignPortalToUserCommand = new AssignPortalToUserCommand(tenantId, userId, portalId);
            await this.mediator.Send(assignPortalToUserCommand);
            return this.NoContent();
        }

        /// <summary>
        /// Remove the existing portal for a user.
        /// </summary>
        /// <param name="userId">The Id of the user.</param>
        /// <returns>No content.</returns>
        [HttpDelete]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [MustHaveOneOfPermissions(Permission.ManageUsers, Permission.ManageUsersForOtherOrganisations, Permission.ManageOrganisationAdminUsers, Permission.ManageTenantAdminUsers)]
        [Route("{userId}/portal")]
        public async Task<IActionResult> UnassignPortalForUser(Guid userId, [FromQuery] string? tenant = null)
        {
            var tenantId = await this.GetContextTenantIdOrThrow(tenant, "update users from a different tenancy");
            await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, userId, this.User);
            var unassignPortalFromUserCommand = new UnassignPortalFromUserCommand(tenantId, userId);
            await this.mediator.Send(unassignPortalFromUserCommand);
            return this.NoContent();
        }
    }
}
