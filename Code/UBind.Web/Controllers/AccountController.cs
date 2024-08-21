// <copyright file="AccountController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Authentication;
    using UBind.Application.Commands.User;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Models.User;
    using UBind.Application.Person;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Queries.User;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling user account.
    /// </summary>
    [Route("api/v1/account")]
    [Produces("application/json")]
    public class AccountController : Controller
    {
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly Application.User.IUserService userService;
        private readonly IAccessTokenService accessTokenService;
        private readonly ICqrsMediator mediator;
        private readonly IPersonService personService;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly ICachingResolver cachingResolver;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userReadModelRepository">The user read model repository.</param>
        /// <param name="userService">The user service of <see cref="Application.User"/>.</param>
        /// <param name="personService">The person service.</param>
        /// <param name="accessTokenService">The access token service.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        /// <param name="mediator">The mediator that encapsulates request/response and publishing interaction patterns.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public AccountController(
            IUserReadModelRepository userReadModelRepository,
            Application.User.IUserService userService,
            IPersonService personService,
            IAccessTokenService accessTokenService,
            IAdditionalPropertyValueService additionalPropertyValueService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IAuthorisationService authorisationService)
        {
            this.userReadModelRepository = userReadModelRepository;
            this.userService = userService;
            this.accessTokenService = accessTokenService;
            this.personService = personService;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Authenticate a user via username and password.
        /// </summary>
        /// <param name="loginModel">The model containing the user's login credentials.</param>
        /// <returns>The access and id tokens, if successful, otherwise a bad request response.</returns>
        [HttpPost("login")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [ProducesResponseType(typeof(UserAuthorisationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] AuthenticationModel loginModel)
        {
            string tenantIdOrAlias = string.IsNullOrEmpty(loginModel.Tenant) ? loginModel.TenantId : loginModel.Tenant;
            var tenant = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenantIdOrAlias));
            var organisationIdOrAlias = string.IsNullOrEmpty(loginModel.Organisation) ? loginModel.OrganisationId : loginModel.Organisation;
            var organisation = await this.cachingResolver.GetOrganisationOrNull(tenant.Id, new GuidOrAlias(organisationIdOrAlias));
            var authenticateCommand = new AuthenticateUserCommand(
                tenant.Id,
                organisation?.Id ?? Guid.Empty,
                loginModel.EmailAddress,
                loginModel.PlaintextPassword);
            var authenticatedUser = await this.mediator.Send(authenticateCommand);
            var userSessionModel = await this.mediator.Send(new CreateUserSessionCommand(authenticatedUser));
            var accessToken = await this.accessTokenService.CreateAccessToken(userSessionModel);
            var serializedToken = new JwtSecurityTokenHandler().WriteToken(accessToken);

            // The following line is to allow the user to access the Hangfire dashboard. And also insure that the cookie is only set if the user has access to the dashboard.
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(accessToken.Claims));
            bool hasHangfireDashboardAccess = await this.authorisationService.DoesUserHaveAccessToHangfireDashboard(claimsPrincipal);
            if (hasHangfireDashboardAccess)
            {
                this.HttpContext.Response.Cookies.Append(
                    WebPortal.HangfireDashboardCookie,
                    serializedToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                    });
            }

            var authModel = await this.authorisationService
                .GenerateUserAuthorisationModel(authenticatedUser, tenant, serializedToken);
            return this.Ok(authModel);
        }

        /// <summary>
        /// Gets key information about the currently logged in user and what they are authorised to access.
        /// </summary>
        /// <returns>The user information and associated organisation and portal information.</returns>
        [HttpGet]
        [Route("authorisation-model")]
        [MustBeLoggedIn]
        [ProducesResponseType(typeof(UserAuthorisationModel), StatusCodes.Status200OK)]
        [NoCache]
        public async Task<IActionResult> GetUserAuthorisationModel()
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(this.User.GetTenantId());
            var authModel = await this.authorisationService.GenerateUserAuthorisationModel(this.User, tenantModel);
            return this.Ok(authModel);
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>An Ok response.</returns>
        [HttpPost("logout")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ValidateModel]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            await this.mediator.Send(new LogoutCommand(this.User));
            this.HttpContext.Response.Cookies.Delete(WebPortal.HangfireDashboardCookie);
            return this.Ok();
        }

        /// <summary>
        /// Gets the current account details based on the authorisation token.
        /// </summary>
        /// <returns>The user data transfer object.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHavePermission(Permission.ViewMyAccount)]
        [ProducesResponseType(typeof(UserDetailResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccount()
        {
            var userId = this.User.GetId();
            if (!userId.HasValue)
            {
                return Errors.General.NotFound("User", this.User.ToString()).ToProblemJsonResult();
            }

            var user = this.userReadModelRepository.GetUserDetail(this.User.GetTenantId(), userId.Value);
            if (user == null)
            {
                return Errors.General.NotFound("User", userId).ToProblemJsonResult();
            }

            var additionalPropertyValues = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                user.TenantId,
                AdditionalPropertyEntityType.User,
                userId.Value);
            PortalReadModel portal = null;
            if (user.PortalId.HasValue)
            {
                portal = await this.mediator.Send(new GetPortalByIdQuery(user.TenantId, user.PortalId.Value));
            }

            var person = this.personService.Get(user.TenantId, user.PersonId);
            var resourceModel = new UserDetailResourceModel(user, portal, person, additionalPropertyValues);
            return this.Ok(resourceModel);
        }

        /// <summary>
        /// Updates the user account of the sender.
        /// </summary>
        /// <param name="model">The account update view model.</param>
        /// <returns>The instance of the updated user account.</returns>
        [HttpPut]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [MustHavePermission(Permission.EditMyAccount)]
        [ProducesResponseType(typeof(UserResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAccount([FromBody] AccountUpdateViewModel model)
        {
            var userId = this.User.GetId();
            var tenantId = this.User.GetTenantId();
            if (!userId.HasValue)
            {
                return Errors.General.NotFound("User", this.User.ToString()).ToProblemJsonResult();
            }

            var userReadModel = this.userService.GetUser(tenantId, userId.Value);
            Application.User.UserUpdateModel updateModel = new Application.User.UserUpdateModel
            {
                Email = model.Email,
                AlternativeEmail = model.AlternativeEmail,
                PreferredName = model.PreferredName,
                FullName = model.FullName,
                NamePrefix = model.NamePrefix,
                FirstName = model.FirstName,
                MiddleNames = model.MiddleNames,
                NameSuffix = model.NameSuffix,
                LastName = model.LastName,
                Company = model.Company,
                Title = model.Title,
                Blocked = false,
                EmailAddresses = model.EmailAddresses,
                PhoneNumbers = model.PhoneNumbers,
                StreetAddresses = model.StreetAddresses,
                WebsiteAddresses = model.WebsiteAddresses,
                MessengerIds = model.MessengerIds,
                SocialMediaIds = model.SocialMediaIds,
                PortalId = userReadModel.PortalId,
            };
            var properties = model.Properties.ToDomainAdditionalProperties();
            if (updateModel.Blocked)
            {
                throw new ErrorException(Errors.User.CannotDisableOwnAccount());
            }

            await this.mediator.Send(new UpdateUserCommand(tenantId, userId.Value, updateModel, properties));
            var additionalPropertyValues = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                tenantId,
                AdditionalPropertyEntityType.User,
                userId.Value);
            var personReadModel = this.personService.Get(userReadModel.TenantId, userReadModel.PersonId);
            return this.Ok(new UserResourceModel(userReadModel, additionalPropertyValues, personReadModel));
        }

        /// <summary>
        /// Updates the user account profile picture.
        /// </summary>
        /// <returns>The instance of the updated user account.</returns>
        [HttpPost]
        [Route("profile-picture")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHavePermission(Permission.EditMyAccount)]
        [ProducesResponseType(typeof(UserResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUserPicture()
        {
            var userId = this.User.GetId();
            if (!userId.HasValue)
            {
                return Errors.General.NotFound("User", this.User.ToString()).ToProblemJsonResult();
            }

            var user = this.userService.GetUser(this.User.GetTenantId(), userId.Value);
            if (user == null)
            {
                return Errors.General.NotFound("user", userId).ToProblemJsonResult();
            }

            var tenantId = this.User.GetTenantId();
            user.ProfilePictureId = await this.userService.SaveProfilePictureForUser(tenantId, this.Request.Form.Files[0], user);
            return this.Ok(new UserResourceModel(user, null));
        }

        /// <summary>
        /// Gets the portal base URL for the current user, so that we can redirect them there
        /// if they login to the wrong place.
        /// </summary>
        /// <param name="environment">The environment for the portal.</param>
        /// <returns>The base URL of the portal.</returns>
        [HttpGet]
        [Route("portal-url")]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(typeof(UserResourceModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPortalUrl(
            [Required][FromQuery] string tenant,
            [Required][FromQuery] DeploymentEnvironment environment)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var userId = this.User.GetId();
            var user = this.userReadModelRepository.GetUserDetail(tenantModel.Id, userId.Value);

            // Get the default portal for the user. We'll use this if the portal user type doesn't match
            // (e.g. if it's a customer trying to login to an agent portal)
            Guid? portalId = user.PortalId;
            if (portalId == null)
            {
                portalId = await this.mediator.Send(
                    new GetDefaultPortalIdQuery(user.TenantId, user.OrganisationId, user.PortalUserType));
                if (portalId == null)
                {
                    var organisation = await this.cachingResolver.GetOrganisationOrThrow(user.TenantId, user.OrganisationId);
                    throw new ErrorException(Errors.Portal.NoDefaultPortalExists(organisation.Name, user.PortalUserType));
                }
            }

            var portalUrl = await this.mediator.Send(new GetPortalUrlQuery(user.TenantId, null, portalId, environment));
            return this.Ok(portalUrl);
        }

        /// Gets the effective permissions of the logged in user.
        /// The effective permissions are the aggregate permissions from all of the assigned roles of
        /// the logged in user, minus any permissions which may not be enabled for that user's organisation.
        /// <returns>A response indicating success or errors.</returns>
        [HttpGet("permission")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [ProducesResponseType(typeof(IEnumerable<PermissionModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPermissionsOfTheLoggedInUser()
        {
            if (this.User.GetUserType() == UserType.Customer)
            {
                return Errors.General.NotAuthorized("Get user permissions").ToProblemJsonResult();
            }

            var userId = this.User.GetId().Value;
            var tenantId = this.User.GetTenantId();
            List<Permission> permissions = await this.mediator.Send(
                new GetEffectivePermissionsForUserQuery(tenantId, userId));
            var permissionModels = permissions.Select(p => new PermissionModel(p));
            return this.Ok(permissionModels);
        }

        /// <summary>
        /// List roles assigned to the logged in user..
        /// </summary>
        /// <returns>List of roles assigned to the logged in user</returns>
        [HttpGet("role")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [ProducesResponseType(typeof(IEnumerable<RoleSummaryModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolesOfTheLoggedInUser()
        {
            if (this.User.GetUserType() == UserType.Customer)
            {
                return Errors.General.NotAuthorized("Get user roles").ToProblemJsonResult();
            }

            var tenantId = this.User.GetTenantId();
            var userId = this.User.GetId().Value;
            var roles = await this.mediator.Send(new EffectiveRolesForUserQuery(tenantId, userId));
            var roleSummaries = roles.Select(r => new RoleSummaryModel(r));
            return this.Ok(roleSummaries);
        }
    }
}
