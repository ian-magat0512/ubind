// <copyright file="LoginSamlUserCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading;
    using System.Threading.Tasks;
    using ComponentSpace.Saml2;
    using LinqKit;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Commands.Organisation;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Mappers;
    using UBind.Application.Models.Sso;
    using UBind.Application.Models.User;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Redis;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using User = UBind.Application.User;

    public class LoginSamlUserCommandHandler : ICommandHandler<LoginSamlUserCommand, UserLoginResult>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly ICqrsMediator mediator;
        private readonly ILogger<LoginSamlUserCommandHandler> logger;
        private readonly IOrganisationService organisationService;
        private readonly IAccessTokenService accessTokenService;
        private readonly IRoleRepository roleRepository;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IClock clock;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;
        private readonly User.IUserService userService;
        private PortalReadModel? portal;
        private RelayState? relayState;
        private string? userExternalId;
        private string? userEmailAddress;
        private Guid? userOrganisationId;
        private OrganisationReadModel? userOrganisation;

        public LoginSamlUserCommandHandler(
            ICachingResolver cachingResolver,
            IUserReadModelRepository userReadModelRepository,
            IUserAggregateRepository userAggregateRepository,
            IOrganisationReadModelRepository organisationReadModelRepository,
            IOrganisationAggregateRepository organisationAggregateRepository,
            ICqrsMediator mediator,
            ILogger<LoginSamlUserCommandHandler> logger,
            IOrganisationService organisationService,
            IAccessTokenService accessTokenService,
            IRoleRepository roleRepository,
            IUserLoginEmailRepository userLoginEmailRepository,
            IClock clock,
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository,
            User.IUserService userService)
        {
            this.cachingResolver = cachingResolver;
            this.userReadModelRepository = userReadModelRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.mediator = mediator;
            this.logger = logger;
            this.organisationService = organisationService;
            this.accessTokenService = accessTokenService;
            this.roleRepository = roleRepository;
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.clock = clock;
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
            this.userService = userService;
        }

        public async Task<UserLoginResult> Handle(LoginSamlUserCommand command, CancellationToken cancellationToken)
        {
            if (command.AuthenticationMethod.Disabled)
            {
                throw new ErrorException(Errors.Authentication.MethodDisabled(command.AuthenticationMethod.Name));
            }

            // validate the authentication method's organisation
            var authenticationMethodOrganisation
                = await this.cachingResolver.GetOrganisationOrThrow(command.Tenant.Id, command.AuthenticationMethod.OrganisationId);
            this.organisationService
                .ValidateOrganisationIsActive(authenticationMethodOrganisation, command.AuthenticationMethod.OrganisationId);

            // get the user's ID from the SAML response
            string userExternalId = this.GetUserExternalId(command);

            // find the user by their ID in the user directory they are linked to
            var user = this.userReadModelRepository.GetLinkedUser(
                command.Tenant.Id,
                command.AuthenticationMethod.Id,
                userExternalId);

            PortalUserType? userType = null;
            if (user == null
                && ((command.AuthenticationMethod.ShouldLinkExistingAgentWithSameEmailAddress ?? false)
                    || (command.AuthenticationMethod.ShouldLinkExistingCustomerWithSameEmailAddress ?? false)))
            {
                // if the user is not found by external ID, we might be able to find someone with the same email address
                // in the specified organisation
                userType = await this.DetermineUserTypeFromSamlResponse(command);
                user = await this.TryFindUserWithMatchingEmailAddress(command, userType);

                if (user != null)
                {
                    this.ThrowIfUserDisabled(command, user);
                    this.ThrowIfUserHasALinkedIdentityWithExclusiveRoleManagement(command, user);
                    await this.ThrowIfUsersOrganisationHasLinkedIdentityToADifferentOrganisation(command, user);
                }

                // We apply the linked identity here if they can't be auto updated, because if they CAN be auto updated, we
                // we apply the linked identity during the update process.
                if (user != null && !this.CanUserBeAutoUpdated(command, user))
                {
                    // store the linked identity
                    await this.ApplyLinkedIdentity(command, user);
                }
            }

            if (user != null)
            {
                this.ThrowIfUserDisabled(command, user);
                await this.ThrowIfUsersOrganisationHasLinkedIdentityToADifferentOrganisation(command, user);

                // validate the user's organisation is still active
                this.userOrganisation = await this.cachingResolver.GetOrganisationOrThrow(command.Tenant.Id, user.OrganisationId);
                this.organisationService
                    .ValidateOrganisationIsActive(this.userOrganisation, command.AuthenticationMethod.OrganisationId);

                await this.UpdateOrganisationNameWhenAllowed(command, this.userOrganisation);

                if (this.CanUserBeAutoUpdated(command, user))
                {
                    await this.UpdateUserDetails(command, user);
                }

                if (!user.HasBeenActivated)
                {
                    var userAggregate = this.userAggregateRepository.GetById(command.Tenant.Id, user.Id);
                    await this.userService.ActivateUser(userAggregate);
                }
            }
            else
            {
                if (!command.AuthenticationMethod.CanAgentAccountsBeAutoProvisioned
                    && !command.AuthenticationMethod.CanCustomerAccountsBeAutoProvisioned)
                {
                    throw new ErrorException(
                        Errors.Authentication.Saml.NoUserAccountExistsAndAutoProvisioningIsDisabledException(
                            command.AuthenticationMethod.Name, userExternalId));
                }

                userType = userType ?? await this.DetermineUserTypeFromSamlResponse(command);
                if (userType == null
                    && command.AuthenticationMethod.CanAgentAccountsBeAutoProvisioned
                    && command.AuthenticationMethod.CanCustomerAccountsBeAutoProvisioned)
                {
                    throw new ErrorException(
                        Errors.Authentication.Saml.NoUserAccountExistsAndCouldNotBeCreatedBecauseUserTypeCouldNotBeDeterminedException(
                            command.AuthenticationMethod.Name, userExternalId));
                }

                if (userType == PortalUserType.Customer
                    && !command.AuthenticationMethod.CanCustomerAccountsBeAutoProvisioned)
                {
                    throw new ErrorException(Errors.Authentication.Saml.CustomerAccountAutoProvisioningDisabled(
                        command.AuthenticationMethod.Name, userExternalId));
                }
                else if (userType == PortalUserType.Agent
                    && !command.AuthenticationMethod.CanAgentAccountsBeAutoProvisioned)
                {
                    throw new ErrorException(Errors.Authentication.Saml.AgentAccountAutoProvisioningDisabled(
                        command.AuthenticationMethod.Name, userExternalId));
                }

                // determine organisation to create the user in
                this.userOrganisation = await this.DetermineOrganisationFromSamlResponse(command);

                // create the user
                user = await this.CreateUser(command, userType.GetValueOrDefault(), this.userOrganisation);
            }

            string returnUrl = await this.GenerateReturnUrl(command, user, this.userOrganisation);
            var result = new UserLoginResult
            {
                User = user,
                JwtToken = await this.CreateUserSessionAndAccessToken(command, user),
                ReturnUrl = returnUrl,
            };

            return result;
        }

        private async Task<JwtSecurityToken> CreateUserSessionAndAccessToken(LoginSamlUserCommand command, UserReadModel user)
        {
            var permissions = await this.userService.GetEffectivePermissions(user, this.userOrganisation);
            var userSessionModel = new UserSessionModel(user, permissions, this.clock.Now());
            userSessionModel.SamlSessionData = command.SamlSessionData;
            userSessionModel.AuthenticationMethodId = command.AuthenticationMethod.Id;
            userSessionModel.AuthenticationMethodType = AuthenticationMethodType.Saml;
            return await this.accessTokenService.CreateAccessToken(userSessionModel);
        }

        /// <summary>
        /// Gets the user's external ID from the SAML response.
        /// This is their unique identifier within the SAML identity provider.
        /// The value is cached so it can be freely called repeatedly.
        /// </summary>
        private string GetUserExternalId(LoginSamlUserCommand command)
        {
            if (this.userExternalId != null)
            {
                return this.userExternalId;
            }

            if (command.AuthenticationMethod.UseNameIdAsUniqueIdentifier)
            {
                this.userExternalId = command.SsoResult.UserID;
                return this.userExternalId;
            }

            if (command.AuthenticationMethod.UniqueIdentifierAttributeName == null)
            {
                throw new ErrorException(Errors.Authentication.Saml.UniqueIdentifierAttributeNameNotSet(command.AuthenticationMethod.Name));
            }

            this.userExternalId = command.SsoResult.GetAttributeValueOrThrow(command.AuthenticationMethod.UniqueIdentifierAttributeName);
            return this.userExternalId;
        }

        private async Task<PortalUserType?> DetermineUserTypeFromSamlResponse(LoginSamlUserCommand command)
        {
            PortalUserType? userType = null;
            if (command.AuthenticationMethod.CanAgentsSignIn && command.AuthenticationMethod.CanCustomersSignIn)
            {
                var portal = await this.GetPortal(command);
                if (portal != null)
                {
                    userType = portal.UserType;
                }
                else if (command.AuthenticationMethod.UserTypeAttributeName.IsNotNullOrEmpty())
                {
                    string userTypeAttributeValue = command.SsoResult.GetAttributeValueOrThrow(
                        command.AuthenticationMethod.UserTypeAttributeName);
                    if (userTypeAttributeValue.IsNotNullOrEmpty())
                    {
                        userType = userTypeAttributeValue.ToEnumOrNull<PortalUserType>();
                    }
                }
            }
            else if (command.AuthenticationMethod.CanAgentsSignIn)
            {
                userType = PortalUserType.Agent;
            }
            else if (command.AuthenticationMethod.CanCustomersSignIn)
            {
                userType = PortalUserType.Customer;
            }

            if (userType == null
                && command.AuthenticationMethod.CanAgentAccountsBeAutoProvisioned
                && !command.AuthenticationMethod.CanCustomerAccountsBeAutoProvisioned)
            {
                userType = PortalUserType.Agent;
            }
            else if (userType == null
                && command.AuthenticationMethod.CanCustomerAccountsBeAutoProvisioned
                && !command.AuthenticationMethod.CanAgentAccountsBeAutoProvisioned)
            {
                userType = PortalUserType.Customer;
            }

            return userType;
        }

        private async Task<UserReadModel> CreateUser(LoginSamlUserCommand command, PortalUserType userType, OrganisationReadModel userOrganisation)
        {
            string? emailAddress = this.GetUsersEmailAddressFromSamlResponse(command);
            var userExternalId = this.GetUserExternalId(command);

            var userSignupModel = new User.UserSignupModel
            {
                Environment = this.GetRelayState(command.SsoResult)?.Environment ?? DeploymentEnvironment.Production,
                UserType = userType == PortalUserType.Agent ? UserType.Client : UserType.Customer,
                TenantId = command.Tenant.Id,
                OrganisationId = userOrganisation.Id,
                FirstName = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.FirstNameAttributeName),
                LastName = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.LastNameAttributeName),
                SendActivationInvitation = false,
                Email = emailAddress,
                WorkPhoneNumber = userType == PortalUserType.Agent
                    ? command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.PhoneNumberAttributeName)
                    : null,
                HomePhoneNumber = userType == PortalUserType.Customer
                    ? command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.PhoneNumberAttributeName)
                    : null,
                MobilePhoneNumber = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.MobileNumberAttributeName),
            };

            if (userType == PortalUserType.Customer)
            {
                // add the customer role
                var role = this.roleRepository.GetCustomerRoleForTenant(command.Tenant.Id);
                userSignupModel.InitialRoles = new[] { role.Id };
            }
            else
            {
                var roleIds = this.GetRoleIdsFromSamlResponse(command);
                if (command.AuthenticationMethod.DefaultAgentRoleId != null)
                {
                    roleIds.Add(command.AuthenticationMethod.DefaultAgentRoleId.Value);
                }

                if (roleIds.Any())
                {
                    userSignupModel.InitialRoles = roleIds;
                }
            }

            var userAggregate = await this.userService.CreateUser(
                userSignupModel,
                command.AuthenticationMethod.Id,
                userExternalId);
            return userAggregate.LatestProjectedReadModel;
        }

        private async Task<OrganisationReadModel> DetermineOrganisationFromSamlResponse(LoginSamlUserCommand command)
        {
            var organisationId = await this.GetIdOfOrganisationInferredFromSamlResponse(command);
            if (this.userOrganisation?.Id == organisationId)
            {
                return this.userOrganisation;
            }

            // check that the organisation is active and not deleted
            OrganisationReadModel organisation
                = await this.cachingResolver.GetOrganisationOrThrow(command.Tenant.Id, organisationId);
            this.organisationService.ValidateOrganisationIsActive(organisation, organisationId);

            return organisation;
        }

        /// <summary>
        /// Retrieves the ID of the organisation eluded to in the SAML response,
        /// auto-provisioning or updating the organisation if appropriate and enabled,
        /// and auto-linking if enabled.
        /// Cached, so can be called repeatedly at no cost.
        /// </summary>
        private async Task<Guid> GetIdOfOrganisationInferredFromSamlResponse(LoginSamlUserCommand command)
        {
            if (this.userOrganisationId != null)
            {
                return this.userOrganisationId.Value;
            }

            if (command.AuthenticationMethod.OrganisationUniqueIdentifierAttributeName.IsNullOrEmpty())
            {
                // they're not specifying one, so we'll assume to use the organisation that owns
                // the authentication method
                this.userOrganisationId = command.AuthenticationMethod.OrganisationId;
                return this.userOrganisationId.Value;
            }

            string? externalOrganisationId
                = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.OrganisationUniqueIdentifierAttributeName);
            if (string.IsNullOrEmpty(externalOrganisationId))
            {
                throw new ErrorException(
                    Errors.Authentication.Saml.OrganisationUniqueIdentifierNotProvided(
                        command.AuthenticationMethod.Name, command.AuthenticationMethod.OrganisationUniqueIdentifierAttributeName));
            }

            var organisation = this.GetLinkedOrganisation(command, externalOrganisationId);
            if (organisation != null)
            {
                await this.UpdateOrganisationNameWhenAllowed(command, organisation);
                this.userOrganisationId = organisation.Id;
                return this.userOrganisationId.Value;
            }

            bool canAutoProvisionOrganisation = command.AuthenticationMethod.CanOrganisationsBeAutoProvisioned ?? false;
            if (!canAutoProvisionOrganisation)
            {
                throw new ErrorException(
                    Errors.Authentication.Saml.NoOrganisationExistsAndAutoProvisioningIsDisabledException(
                    command.AuthenticationMethod.Name, externalOrganisationId));
            }

            // if we got this far then we can auto provision, so let's do it
            string organisationName
                = command.SsoResult.GetAttributeValueOrThrow(command.AuthenticationMethod.OrganisationNameAttributeName);

            if (organisationName.IsNullOrWhitespace())
            {
                throw new ErrorException(
                    Errors.Authentication.Saml.OrganisationNameNotProvided(
                        command.AuthenticationMethod.Name, command.AuthenticationMethod.OrganisationNameAttributeName));
            }

            string? organisationAlias
                = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.OrganisationAliasAttributeName);
            var organisationLinkedIdentity = new UBind.Domain.Aggregates.Organisation.LinkedIdentity
            {
                AuthenticationMethodId = command.AuthenticationMethod.Id,
                UniqueId = externalOrganisationId,
            };

            var organisationAggregate = await this.organisationService.CreateOrganisation(
                command.Tenant.Id,
                organisationAlias,
                organisationName,
                command.AuthenticationMethod.OrganisationId,
                null,
                new List<UBind.Domain.Aggregates.Organisation.LinkedIdentity> { organisationLinkedIdentity });
            this.userOrganisationId = organisationAggregate.Id;
            this.userOrganisation = organisationAggregate.LatestProjectedReadModel;
            return this.userOrganisationId.Value;
        }

        private async Task UpdateOrganisationNameWhenAllowed(LoginSamlUserCommand command, OrganisationReadModel? userOrganisation)
        {
            string? newOrganisationName = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.OrganisationNameAttributeName);
            bool shouldUpdateOrganisationDetails = newOrganisationName != null && userOrganisation.Name != newOrganisationName;
            if (shouldUpdateOrganisationDetails && command.AuthenticationMethod.CanOrganisationDetailsBeAutoUpdated == true)
            {
                await this.organisationService.UpdateOrganisation(
                    userOrganisation.TenantId,
                   userOrganisation.Id,
                   userOrganisation.Alias,
                   newOrganisationName);
            }
        }

        private List<Guid> GetRoleIdsFromSamlResponse(LoginSamlUserCommand command)
        {
            var roleIds = new List<Guid>();
            if (command.AuthenticationMethod.RoleAttributeName.IsNullOrEmpty())
            {
                return roleIds;
            }

            var roleAttributes = command.SsoResult.Attributes.Where(a => a.Name == command.AuthenticationMethod.RoleAttributeName);
            var externalRoleIds = new List<string>();
            foreach (var roleAttribute in roleAttributes)
            {
                roleAttribute.AttributeValues.ForEach(val =>
                {
                    string roleStringValue = val.ToString();
                    if (roleStringValue.IsNotNullOrWhitespace())
                    {
                        if (command.AuthenticationMethod.RoleAttributeValueDelimiter != null)
                        {
                            var splitRoleIds = roleStringValue.Split(command.AuthenticationMethod.RoleAttributeValueDelimiter);
                            externalRoleIds.AddRange(splitRoleIds);
                        }
                        else
                        {
                            externalRoleIds.Add(roleStringValue);
                        }
                    }
                });
            }

            if (externalRoleIds.Count == 0)
            {
                return roleIds;
            }

            var roleMap = command.AuthenticationMethod.RoleMap;
            foreach (var externalRoleId in externalRoleIds)
            {
                if (roleMap.TryGetValue(externalRoleId, out Guid roleId))
                {
                    roleIds.Add(roleId);
                }
                else
                {
                    this.logger.LogWarning(
                        "SAML response contained role ID {0} which is not mapped to a role in the authentication method {1}",
                        externalRoleId,
                        command.AuthenticationMethod.Name);
                }
            }

            return roleIds;
        }

        private async Task<PortalReadModel?> GetPortal(LoginSamlUserCommand command)
        {
            if (this.portal == null)
            {
                var relayState = this.GetRelayState(command.SsoResult);
                if (relayState != null && relayState.PortalId != null)
                {
                    this.portal = await this.cachingResolver
                        .GetPortalOrNull(command.Tenant.Id, relayState.PortalId.Value);
                }
            }

            return this.portal;
        }

        private RelayState? GetRelayState(ISpSsoResult ssoResult)
        {
            if (this.relayState == null)
            {
                this.relayState = ssoResult.RelayState?.FromJson<RelayState?>();
            }

            return this.relayState;
        }

        private async Task<string> GenerateReturnUrl(
            LoginSamlUserCommand command,
            UserReadModel user,
            OrganisationReadModel? userOrganisation)
        {
            var relayState = this.GetRelayState(command.SsoResult);
            Guid? portalId = relayState?.PortalId ?? user.PortalId;
            if (portalId == null)
            {
                portalId = await this.mediator.Send(new GetDefaultPortalIdQuery(
                    user.TenantId, user.OrganisationId, user.PortalUserType));

                if (portalId == null)
                {
                    // we couldn't resolve a portal for this user to login to
                    userOrganisation = userOrganisation
                        ?? await this.cachingResolver.GetOrganisationOrThrow(user.TenantId, user.OrganisationId);
                    throw new ErrorException(Errors.Portal.NoDefaultPortalExists(userOrganisation.Name, user.PortalUserType));
                }
            }

            string path = relayState?.Path ?? this.GetPortalHomePath(user.UserType.ToEnumOrThrow<UserType>());
            return await this.mediator.Send(new GetPortalUrlQuery(
                user.TenantId,
                user.OrganisationId,
                portalId,
                relayState?.Environment ?? DeploymentEnvironment.Production,
                path));
        }

        private string GetPortalHomePath(UserType userType)
        {
            switch (userType)
            {
                case UserType.Master:
                    return "master-home";
                case UserType.Client:
                    return "home";
                case UserType.Customer:
                    return "my-home";
                default:
                    throw new ArgumentOutOfRangeException(nameof(userType), userType, null);
            }
        }

        /// <summary>
        /// Gets the users email address from the SAML response.
        /// The result is cached so it can be called repeatedly without cost.
        /// </summary>
        private string GetUsersEmailAddressFromSamlResponse(LoginSamlUserCommand command)
        {
            if (this.userEmailAddress != null)
            {
                return this.userEmailAddress;
            }

            var userExternalId = this.GetUserExternalId(command);

            // get the user's email address from the SAML response
            if (command.AuthenticationMethod.UseNameIdAsEmailAddress ?? false)
            {
                this.userEmailAddress = userExternalId;
            }
            else if (command.AuthenticationMethod.EmailAddressAttributeName.IsNullOrEmpty())
            {
                throw new ErrorException(
                    Errors.Authentication.Saml.NoEmailAddressSourceConfigured(command.AuthenticationMethod.Name));
            }
            else
            {
                this.userEmailAddress = command.SsoResult.GetAttributeValueOrThrow(command.AuthenticationMethod.EmailAddressAttributeName);
            }

            return this.userEmailAddress;
        }

        /// <summary>
        /// Attempts to find a user with a matching email address in the Authentication Method's organisation,
        /// or within an organisation that it manages.
        /// </summary>
        private async Task<UserReadModel?> TryFindUserWithMatchingEmailAddress(
            LoginSamlUserCommand command,
            PortalUserType? userType)
        {
            string emailAddress = this.GetUsersEmailAddressFromSamlResponse(command);
            Guid organisationId = await this.GetIdOfOrganisationInferredFromSamlResponse(command);
            UserLoginEmail? userLoginEmail = this.userLoginEmailRepository.GetUserLoginByEmail(command.Tenant.Id, organisationId, emailAddress);
            if (userLoginEmail != null)
            {
                var user = this.userReadModelRepository.GetUser(command.Tenant.Id, userLoginEmail.Id);
                if (userType == null || (userType != null && user.PortalUserType == userType))
                {
                    return user;
                }
            }

            return null;
        }

        private bool CanUserBeAutoUpdated(LoginSamlUserCommand command, UserReadModel user)
        {
            return (user.PortalUserType == PortalUserType.Customer
                    && (command.AuthenticationMethod.CanCustomerDetailsBeAutoUpdated ?? false))
                || (user.PortalUserType == PortalUserType.Agent
                    && (command.AuthenticationMethod.CanAgentDetailsBeAutoUpdated ?? false));
        }

        private async Task ApplyLinkedIdentity(LoginSamlUserCommand command, UserReadModel user)
        {
            var userAggregate = this.userAggregateRepository.GetById(user.TenantId, user.Id);
            userAggregate.LinkIdentity(
                command.AuthenticationMethod.Id,
                this.GetUserExternalId(command),
                null, // there is no performing user as the person is logging in themselves.
                this.clock.Now());
            await this.userAggregateRepository.ApplyChangesToDbContext(userAggregate);

            // let's also make sure the organisation is linked
            string? externalOrganisationId
                = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.OrganisationUniqueIdentifierAttributeName);
            if (externalOrganisationId != null)
            {
                var organisation = this.organisationReadModelRepository.GetLinkedOrganisation(
                    command.Tenant.Id,
                    command.AuthenticationMethod.Id,
                    externalOrganisationId);
                if (organisation == null)
                {
                    var organisationAggregate = this.organisationAggregateRepository.GetById(user.TenantId, user.OrganisationId);
                    this.OrganisationLinkIdentity(organisationAggregate, externalOrganisationId, command.AuthenticationMethod.Id);
                    await this.UpdateOrganisationNameWhenAllowed(command, organisationAggregate.LatestProjectedReadModel);
                }
            }
        }

        private async Task UpdateUserDetails(
            LoginSamlUserCommand command,
            UserReadModel user)
        {
            var userMapper = new UserMapper();
            User.UserUpdateModel userUpdateModel = userMapper.UserReadModelToUserUpdateModel(user);
            userUpdateModel.AuthenticationMethodId = command.AuthenticationMethod.Id;
            userUpdateModel.ExternalUserId = this.GetUserExternalId(command);
            userUpdateModel.FirstName
                = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.FirstNameAttributeName)
                    ?? userUpdateModel.FirstName;
            userUpdateModel.LastName
                = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.LastNameAttributeName)
                    ?? userUpdateModel.LastName;
            userUpdateModel.Email = this.GetUsersEmailAddressFromSamlResponse(command);
            userUpdateModel.WorkPhoneNumber = user.PortalUserType == PortalUserType.Agent
                ? command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.PhoneNumberAttributeName)
                    ?? userUpdateModel.WorkPhoneNumber
                : userUpdateModel.WorkPhoneNumber;
            userUpdateModel.HomePhoneNumber = user.PortalUserType == PortalUserType.Customer
                ? command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.PhoneNumberAttributeName)
                    ?? userUpdateModel.HomePhoneNumber
                : userUpdateModel.HomePhoneNumber;
            userUpdateModel.MobilePhoneNumber
                = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.MobileNumberAttributeName)
                    ?? userUpdateModel.MobilePhoneNumber;

            // determine the configured roles for the user
            var roleIds = this.GetRoleIdsFromSamlResponse(command);
            if (command.AuthenticationMethod.DefaultAgentRoleId != null)
            {
                roleIds.Add(command.AuthenticationMethod.DefaultAgentRoleId.Value);
            }

            if (command.AuthenticationMethod.AreRolesManagedExclusivelyByThisIdentityProvider)
            {
                userUpdateModel.RoleIds = roleIds;
            }
            else
            {
                // merge with existing user roles
                var uniqueRoleIds = new HashSet<Guid>(user.Roles.Select(r => r.Id));
                roleIds.ForEach(r => uniqueRoleIds.Add(r));
                userUpdateModel.RoleIds = uniqueRoleIds.ToList();
            }

            await this.userService.Update(command.Tenant.Id, user.Id, userUpdateModel);
        }

        private void ThrowIfUserHasALinkedIdentityWithExclusiveRoleManagement(
            LoginSamlUserCommand command,
            UserReadModel user)
        {
            foreach (var linkedIdentity in user.LinkedIdentities)
            {
                var authenticationMethod = this.authenticationMethodReadModelRepository.Get(
                    command.Tenant.Id,
                    linkedIdentity.AuthenticationMethodId);
                if (authenticationMethod != null
                    && !authenticationMethod.Disabled
                    && authenticationMethod is SamlAuthenticationMethodReadModel samlMethod)
                {
                    if (samlMethod.AreRolesManagedExclusivelyByThisIdentityProvider)
                    {
                        throw new ErrorException(Errors.Authentication.Saml.UserHasALinkedIdentityWithExclusiveRoleManagement(
                            user.DisplayName,
                            samlMethod.Name));
                    }
                }
            }
        }

        private async Task ThrowIfUsersOrganisationHasLinkedIdentityToADifferentOrganisation(
            LoginSamlUserCommand command,
            UserReadModel user)
        {
            // get the organisation unique ID assertion
            var assertedOrganisationId = await this.GetIdOfOrganisationInferredFromSamlResponse(command);
            if (user.OrganisationId != assertedOrganisationId)
            {
                var assertedOrganisation
                    = await this.cachingResolver.GetOrganisationOrThrow(command.Tenant.Id, assertedOrganisationId);
                var userOrganisation
                    = await this.cachingResolver.GetOrganisationOrThrow(command.Tenant.Id, user.OrganisationId);
                throw new ErrorException(Errors.Authentication.Saml.UsersOrganisationHasLinkedIdentityToADifferentOrganisation(
                    user.DisplayName,
                    assertedOrganisation.Name,
                    userOrganisation.Name,
                    command.AuthenticationMethod.Name));
            }
        }

        private void ThrowIfUserDisabled(
            LoginSamlUserCommand command,
            UserReadModel user)
        {
            if (user.IsDisabled)
            {
                throw new ErrorException(Errors.User.Login.AccountDisabled());
            }
        }

        /// <summary>
        /// Gets the organisation with the matching external ID, or if we're allowed to, with a matching alias.
        /// </summary>
        private OrganisationReadModel? GetLinkedOrganisation(
            LoginSamlUserCommand command,
            string externalOrganisationId)
        {
            var organisation = this.organisationReadModelRepository.GetLinkedOrganisation(
                command.Tenant.Id,
                command.AuthenticationMethod.Id,
                externalOrganisationId);

            bool shouldLinkOrganisation = command.AuthenticationMethod.ShouldLinkExistingOrganisationWithSameAlias ?? false;
            if (organisation != null || !shouldLinkOrganisation)
            {
                return organisation;
            }

            // try to find an organisation with the same alias
            var organisationAlias = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.OrganisationAliasAttributeName);
            if (organisationAlias.IsNullOrWhitespace())
            {
                var organisationName = command.SsoResult.GetAttributeValueOrNull(command.AuthenticationMethod.OrganisationNameAttributeName);
                if (organisationName.IsNullOrWhitespace())
                {
                    // we can't link to an existing organisation without a name or alias.
                    return null;
                }

                organisationAlias = Converter.NameToAlias(organisationName);
            }

            organisation = this.organisationReadModelRepository.GetByAlias(command.Tenant.Id, organisationAlias);
            if (organisation == null)
            {
                return null;
            }

            var organisationAggregate = this.organisationAggregateRepository.GetById(command.Tenant.Id, organisation.Id);
            this.OrganisationLinkIdentity(organisationAggregate, externalOrganisationId, command.AuthenticationMethod.Id);
            return organisation;
        }

        private void OrganisationLinkIdentity(
           Organisation organisationAggregate,
           string externalOrganisationId,
           Guid authenticationMethodId)
        {
            organisationAggregate.LinkIdentity(
                    authenticationMethodId,
                    externalOrganisationId,
                    null,
                    this.clock.Now());
            this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
        }
    }
}
