// <copyright file="InvitationController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.User;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Models.User;
    using UBind.Application.Queries.Person;
    using UBind.Application.Queries.User;
    using UBind.Application.Services;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Person;

    /// <summary>
    /// Controller for invitations (activation and password reset).
    /// </summary>
    /// <remarks>
    /// The API should support both end points for backward compatibility.
    /// Old API will get the tenant's default organisation and pass it to the new service.
    /// Organisation Id is used over organisation alias because we only use alias for public URLs.
    /// </remarks>
    [Produces(ContentTypes.Json)]
    [Route("api/v1/{tenant}/invitation")]
    [Route("api/v1/tenant/{tenant}/organisation/{organisation}/invitation")]
    public class InvitationController : PortalBaseController
    {
        private readonly IAuthorisationService authorisationService;
        private readonly IUserActivationInvitationService activationService;
        private readonly IUserPasswordResetInvitationService passwordResetService;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IAccessTokenService accessTokenService;
        private readonly IOrganisationService organisationService;
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvitationController"/> class.
        /// </summary>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="activationService">The user activation service.</param>
        /// <param name="passwordResetService">The user password reset service.</param>
        /// <param name="userReadModelRepository">The user read model repository.</param>
        /// <param name="accessTokenService">The Access token service.</param>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="cachingResolver">The tenant and product resolver.</param>
        /// <param name="mediator">The mediator.</param>
        public InvitationController(
            IAuthorisationService authorisationService,
            IUserActivationInvitationService activationService,
            IUserPasswordResetInvitationService passwordResetService,
            IUserReadModelRepository userReadModelRepository,
            IAccessTokenService accessTokenService,
            IOrganisationService organisationService,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
            : base(cachingResolver)
        {
            this.authorisationService = authorisationService;
            this.activationService = activationService;
            this.passwordResetService = passwordResetService;
            this.userReadModelRepository = userReadModelRepository;
            this.accessTokenService = accessTokenService;
            this.organisationService = organisationService;
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
        }

        /// <summary>
        /// Check the validity of the activation invitation.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias where the request is coming from.</param>
        /// <param name="userId">The user ID of the requester.</param>
        /// <param name="invitationId">The invitation ID of the requester.</param>
        /// <remarks>
        /// If the organisation Id isn't specified, it will use the default organisation id from the tenant.
        /// </remarks>
        /// <returns>Returns OK action result with activation status.</returns>
        [HttpPost]
        [Route("validate-activation")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ValidateActivationInvitiation(
            string tenant, Guid userId, Guid invitationId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            this.activationService
                .CheckUserActivationInvitationStatus(tenantModel.Id, userId, invitationId);

            return this.Ok();
        }

        /// <summary>
        /// Sets the password from activation invitation.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias where the request is coming from.</param>
        /// <param name="userPasswordModel">The model for user password.</param>
        /// <remarks>
        /// If the organisation Id isn't specified, it will use the default organisation id from the tenant.
        /// </remarks>
        /// <returns>Returns OK action result with activation status.</returns>
        [HttpPost]
        [Route("set-password")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [ProducesResponseType(typeof(UserAuthorisationModel), StatusCodes.Status200OK)]
        public async Task<ActionResult> SetPasswordFromActivation(
            string tenant,
            [FromBody] UserPasswordRequestModel userPasswordModel)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            await this.activationService.SetPasswordFromActivation(
                tenantModel.Id,
                userPasswordModel.UserId,
                userPasswordModel.InvitationId,
                userPasswordModel.ClearTextPassword);
            var user = this.userReadModelRepository.GetUser(tenantModel.Id, userPasswordModel.UserId);
            var userSessionModel = await this.mediator.Send(new CreateUserSessionCommand(user));
            var accessToken = await this.accessTokenService.CreateAccessToken(userSessionModel);
            var serializedToken = new JwtSecurityTokenHandler().WriteToken(accessToken);
            var authModel = await this.authorisationService.GenerateUserAuthorisationModel(user, tenantModel, serializedToken);
            return this.Ok(authModel);
        }

        /// <summary>
        /// Validate the password reset by invitation Id.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias where the request is coming from.</param>
        /// <param name="userId">The user ID of the requester.</param>
        /// <param name="invitationId">The invitation ID of the requester.</param>
        /// <remarks>
        /// If the organisation Id isn't specified, it will use the default organisation id from the tenant.
        /// </remarks>
        /// <returns>Returns OK action result with activation status.</returns>
        [HttpPost]
        [Route("validate-reset-password")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ValidateResetPasswordInvitation(
            string tenant, Guid userId, Guid invitationId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            this.passwordResetService.CheckPasswordResetInvitationStatus(tenantModel.Id, userId, invitationId);
            return this.Ok();
        }

        /// <summary>
        /// Resets the password from invitation Id.
        /// </summary>
        /// <param name="userPasswordModel">The model for user password.</param>
        /// <remarks>
        /// If the organisation Id isn't specified, it will use the default organisation id from the tenant.
        /// </remarks>
        /// <returns>Returns OK action result with activation status.</returns>
        [HttpPost]
        [Route("reset-password")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [ProducesResponseType(typeof(UserAuthorisationModel), StatusCodes.Status200OK)]
        public async Task<ActionResult> ResetPassword(
            [FromBody] UserPasswordRequestModel userPasswordModel)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(userPasswordModel.Tenant));
            await this.mediator.Send(new ResetPasswordCommand(
                tenantModel.Id,
                userPasswordModel.UserId,
                userPasswordModel.InvitationId,
                userPasswordModel.ClearTextPassword));
            var user = this.userReadModelRepository.GetUser(tenantModel.Id, userPasswordModel.UserId);
            var userSessionModel = await this.mediator.Send(new CreateUserSessionCommand(user));
            var accessToken = await this.accessTokenService.CreateAccessToken(userSessionModel);
            var serializedToken = new JwtSecurityTokenHandler().WriteToken(accessToken);
            var authModel = await this.authorisationService.GenerateUserAuthorisationModel(user, tenantModel, serializedToken);
            return this.Ok(authModel);
        }

        /// <summary>
        /// Send reset password invitation to user by email.
        /// </summary>
        /// <param name="model">The password invitation resource model.</param>
        /// <remarks>
        /// If the organisation Id isn't specified, it will use the default organisation id from the tenant.
        /// </remarks>
        /// <returns>Returns OK action result with activation status.</returns>
        [HttpPost]
        [Route("request-reset-password")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SendResetPasswordInvitation(
            [FromBody] PasswordInvitationRequestModel model)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(model.Tenant));
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenantModel, new GuidOrAlias(model.Organisation));
            var userDetails = this.userReadModelRepository.GetUsersMatchingEmailAddressIncludingPlusAddressing(tenantModel.Id, Uri.UnescapeDataString(model.Email)).ToList().FirstOrDefault();
            if (userDetails != null)
            {
                var personSummary = new GetPersonSummaryByIdQuery(tenantModel.Id, userDetails.PersonId);
                var person = await this.mediator.Send(personSummary);
                if (!person.UserHasBeenActivated && person.UserHasBeenInvitedToActivate && person.UserId.HasValue)
                {
                    var activationEmailCommand = new QueueActivationEmailCommand(tenantModel.Id, person.UserId.Value, model.Environment);
                    await this.mediator.Send(activationEmailCommand);
                    return this.NoContent();
                }
            }

            await this.mediator.Send(new CreateAndSendResetPasswordInvitationCommand(
                tenantModel.Id,
                organisationModel.Id,
                Uri.UnescapeDataString(model.Email),
                model.Environment,
                null,
                model.IsPasswordExpired));

            return this.NoContent();
        }

        /// <summary>
        /// Sends a user activation email to the given email address.
        /// </summary>
        /// <param name="activationModel">The properties of the person who should be sent an activation invitation.</param>
        /// <returns>Returns OK action result with activation status, otherwise, returns not found.</returns>
        [HttpPost]
        [Route("send-person-activation")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [MustHavePermission(Permission.ManageUsers)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RequestPersonActivation([FromBody] SendPersonActivationModel activationModel)
        {
            string tenant = activationModel.Tenant ?? this.User.GetTenantId().ToString();
            Tenant tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            this.authorisationService.ThrowIfUserNotInTheSameOrMasterTenancy(tenantModel.Id, this.User);

            var getPersonQuery = new GetPersonSummaryByIdQuery(tenantModel.Id, activationModel.PersonId);
            var person = await this.mediator.Send(getPersonQuery);

            if (person.UserHasBeenInvitedToActivate)
            {
                var validationQuery = new ThrowIfEmailAddressInUseQuery(
                    tenantModel.Id, person.Email, person.OrganisationId);
                await this.mediator.Send(validationQuery);

                var activationEmailCommand = new QueueActivationEmailCommand(
                    tenantModel.Id,
                    person.UserId.Value,
                    activationModel.Environment);
                await this.mediator.Send(activationEmailCommand);
            }

            return this.Ok();
        }
    }
}
