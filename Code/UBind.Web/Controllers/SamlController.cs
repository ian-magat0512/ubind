// <copyright file="SamlController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using ComponentSpace.Saml2;
    using ComponentSpace.Saml2.Assertions;
    using ComponentSpace.Saml2.Exceptions;
    using ComponentSpace.Saml2.Metadata.Export;
    using ComponentSpace.Saml2.Protocols;
    using Microsoft.AspNetCore.Mvc;
    using ServiceStack;
    using UBind.Application.Commands.User;
    using UBind.Application.Models.Sso;
    using UBind.Application.Queries.AuthenicationMethod;
    using UBind.Application.Queries.Portal;
    using UBind.Domain;
    using UBind.Domain.Attributes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.Redis;

    /// <summary>
    /// Controller for handling SAML related requests.
    /// </summary>
    [Route("api/v1/tenant/{tenant}/saml")]
    public class SamlController : Controller
    {
        private readonly ILogger<SamlController> logger;
        private readonly ICachingResolver cachingResolver;
        private readonly IConfigurationToMetadata configurationToMetadata;
        private readonly ISamlServiceProvider samlServiceProvider;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SamlController"/> class.
        /// </summary>
        public SamlController(
            ICachingResolver cachingResolver,
            IConfigurationToMetadata configurationToMetadata,
            ISamlServiceProvider samlServiceProvider,
            ICqrsMediator mediator,
            ILogger<SamlController> logger)
        {
            this.logger = logger;
            this.cachingResolver = cachingResolver;
            this.configurationToMetadata = configurationToMetadata;
            this.samlServiceProvider = samlServiceProvider;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets the SAML Service Provider (SP) configuration (SAML Metadata) for a given authentication method.
        /// </summary>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <param name="authenticationMethodId">The authentication method ID.</param>
        /// <returns>The SAML metadata in XML format.</returns>
        [HttpGet]
        [Route("{authenticationMethodId}/metadata")]
        [Produces("application/xml")]
        public async Task<IActionResult> GetMetadata(string tenant, Guid authenticationMethodId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var configurationName = $"{tenantModel.Id}|{authenticationMethodId}";
            await this.samlServiceProvider.SetConfigurationNameAsync(configurationName);
            var entityDescriptor = await this.configurationToMetadata.ExportAsync(configurationName, "uBind");
            var xmlElement = entityDescriptor.ToXml();
            return this.Content(xmlElement.Format(), "application/xml");
        }

        /// <summary>
        /// Initiates a single sign-on (SSO) request for a given authentication method.
        /// </summary>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <param name="authenticationMethodId">The ID of the authentication method.</param>
        /// <param name="portalId">The ID of the portal that initiated the sign-on.
        /// This ensures we can generate a return URL that will redirect the user back to that portal.</param>
        /// <param name="path">The path within thhe portal the user should be redirected to after sign-in.</param>
        /// <param name="environment">The environment the user should be redirected to after sign-in.</param>
        [HttpGet]
        [HttpPost]
        [Route("{authenticationMethodId}/initiate-single-sign-on")]
        [RequestIntent(RequestIntent.ReadOnly)]
        public async Task<IActionResult> InitiateSingleSignOn(
            string tenant,
            Guid authenticationMethodId,
            [FromQuery] Guid? portalId = null,
            [FromQuery] string? path = null,
            [FromQuery] DeploymentEnvironment? environment = null)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var configurationName = $"{tenantModel.Id}|{authenticationMethodId}";
            var authenticationMethod
                = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantModel.Id, authenticationMethodId));
            EntityHelper.ThrowIfNotFound(tenantModel.Id, authenticationMethodId, authenticationMethod);
            var samlAuthenticationMethod = this.GetAsSamlAuthenticationMethodOrThrow(authenticationMethod);
            await this.samlServiceProvider.SetConfigurationNameAsync(configurationName);
            string? relayState = portalId != null || path != null || environment != null
                ? new RelayState { PortalId = portalId, Path = path, Environment = environment }.ToJson()
                : null;
            await this.samlServiceProvider.InitiateSsoAsync(samlAuthenticationMethod.IdentityProviderEntityIdentifier, relayState);
            return new EmptyResult();
        }

        /// <summary>
        /// Requests the user to sign out of the identity provider (IdP).
        /// </summary>
        /// <param name="tenant">The tenant ID or alias.</param>
        /// <param name="authenticationMethodId">The ID of the authentication method.</param>
        /// <param name="logoutReason">The reason the user was logged out, e.g. session timeout.</param>
        /// <param name="portalId">The ID of the portal that initiated the sign-on.
        /// This ensures we can generate a return URL that will redirect the user back to that portal.</param>
        /// <param name="path">The path within thhe portal the user should be redirected to after sign-in.</param>
        /// <param name="environment">The environment the user should be redirected to after sign-in.</param>
        [HttpGet]
        [HttpPost]
        [Route("{authenticationMethodId}/initiate-single-logout")]
        [RequestIntent(RequestIntent.ReadWrite)]
        public async Task<IActionResult> InitiateSingleLogout(
            string tenant,
            Guid authenticationMethodId,
            [FromQuery] string? logoutReason = null,
            [FromQuery] Guid? portalId = null,
            [FromQuery] string? path = null,
            [FromQuery] DeploymentEnvironment environment = DeploymentEnvironment.Production)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var authenticationMethod
                = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantModel.Id, authenticationMethodId));
            EntityHelper.ThrowIfNotFound(tenantModel.Id, authenticationMethodId, authenticationMethod);
            var configurationName = $"{tenantModel.Id}|{authenticationMethodId}";
            var samlAuthenticationMethod = this.GetAsSamlAuthenticationMethodOrThrow(authenticationMethod);
            if (string.IsNullOrEmpty(samlAuthenticationMethod.IdentityProviderSingleLogoutServiceUrl))
            {
                throw new ErrorException(
                    Errors.Authentication.Saml.SingleLogoutNotSupported(samlAuthenticationMethod.Name));
            }

            await this.samlServiceProvider.SetConfigurationNameAsync(configurationName);
            string? relayState = portalId != null || path != null || environment != null
                ? new RelayState { PortalId = portalId, Path = path, Environment = environment }.ToJson()
                : null;
            try
            {
                await this.samlServiceProvider.InitiateSloAsync(
                    samlAuthenticationMethod.IdentityProviderEntityIdentifier,
                    logoutReason,
                    relayState);
            }
            catch (SamlProtocolException sex) when (sex.Message.Contains("There is no SSO session to partner"))
            {
                // We can safely ignore this exception, as it means the user is already logged out on the Idp, and
                // what's likely happened is that someone has used their browser's stored token or back button and
                // then tried to log out again.

                // Instead, let's just log the user out locally and redirect them to the login page
                await this.mediator.Send(new LogoutCommand(this.User));
                string returnUrl = await this.mediator.Send(new GetPortalUrlQuery(
                    tenantModel.Id,
                    authenticationMethod.OrganisationId,
                    portalId,
                    environment,
                    path ?? "login"));
                return this.Redirect(returnUrl);
            }

            return new EmptyResult();
        }

        /// <summary>
        /// Receives and processes a SAML assertion, after the identity provider (IdP) has authenticated the user.
        /// </summary>
        [HttpGet]
        [HttpPost]
        [Route("{authenticationMethodId}/assertion-consumer-service")]
        [RequestIntent(RequestIntent.ReadWrite)]
        public async Task<IActionResult> AssertionConsumerService(
            string tenant,
            Guid authenticationMethodId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var authenticationMethod
                = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantModel.Id, authenticationMethodId));
            EntityHelper.ThrowIfNotFound(tenantModel.Id, authenticationMethodId, authenticationMethod);
            var samlAuthenticationMethod = this.GetAsSamlAuthenticationMethodOrThrow(authenticationMethod);
            await this.samlServiceProvider.SetConfigurationNameAsync($"{tenantModel.Id}|{authenticationMethodId}");

            // When the SAML assertion is received, we need to store the session index, nameID and issuer
            // so that upon logout (IdP initiated SLO), we can lookup the session ID from this data and terminate it.
            var samlSessionData = new SamlSessionData();
            samlSessionData.SupportsSingleLogout
                = !string.IsNullOrEmpty(samlAuthenticationMethod.IdentityProviderSingleLogoutServiceUrl);
            var samlAssertionHandler = (HttpContext httpContext, SamlAssertion samlAssertion) =>
            {
                samlSessionData.Issuer = samlAssertion.Issuer.Name;
                samlSessionData.NameId = samlAssertion.GetNameID();
                samlSessionData.SessionIndex = samlAssertion.GetAuthenticationStatements().First()?.SessionIndex;
            };
            this.samlServiceProvider.Events.OnSamlAssertionReceived += samlAssertionHandler;
            ISpSsoResult ssoResult;
            try
            {
                ssoResult = await this.samlServiceProvider.ReceiveSsoAsync();
            }
            catch (SamlErrorStatusException ex)
            {
                // Log the basic exception message
                this.logger.LogError($"SAML error occurred: {ex.Message}");

                // Extract additional details from the Status property
                var statusCode = ex.Status?.StatusCode.Code;
                var statusMessage = ex.Status?.StatusMessage.Message;

                // Log the additional details
                this.logger.LogError($"SAML status code: {statusCode}");
                if (!string.IsNullOrEmpty(statusMessage))
                {
                    this.logger.LogError($"SAML status message: {statusMessage}");
                }

                // Re-throw the exception with the detail so that it can be displayed to users
                throw new ErrorException(Errors.Saml.SamlErrorStatus(statusMessage, statusCode));
            }
            finally
            {
                this.samlServiceProvider.Events.OnSamlAssertionReceived -= samlAssertionHandler;
            }

            var userLoginResult = await this.mediator.Send(new LoginSamlUserCommand(
                tenantModel, samlAuthenticationMethod, ssoResult, samlSessionData));

            // make a "url safe" version of the JWT
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            string tokenString = tokenHandler.WriteToken(userLoginResult.JwtToken);
            string urlSafeToken = WebUtility.UrlEncode(tokenString);

            // redirect them to the return url, appending the jwt as a url fragment
            var url = userLoginResult.ReturnUrl + $"#token={urlSafeToken}";
            return this.Redirect(url);
        }

        /// <summary>
        /// Receive the single logout request or response.
        /// If a request is received then single logout is being initiated by the identity provider.
        /// If a response is received then this is in response to single logout having been initiated by the service provider.
        /// </summary>
        [HttpGet]
        [HttpPost]
        [Route("{authenticationMethodId}/single-logout-service")]
        [RequestIntent(RequestIntent.ReadWrite)]
        public async Task<IActionResult> SingleLogoutService(
            string tenant,
            Guid authenticationMethodId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var authenticationMethod
                = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantModel.Id, authenticationMethodId));
            EntityHelper.ThrowIfNotFound(tenantModel.Id, authenticationMethodId, authenticationMethod);
            await this.samlServiceProvider.SetConfigurationNameAsync($"{tenantModel.Id}|{authenticationMethodId}");

            // When the SAML assertion is received, we need to store the session index, nameID and issuer
            // so that upon logout (IdP initiated SLO), we can lookup the session ID from this data and terminate it.
            var samlSessions = new List<SamlSessionData>();
            var samlLogoutHandler = (HttpContext httpContext, LogoutRequest logoutRequest, string something) =>
            {
                string issuer = logoutRequest.Issuer.Name;
                string nameId = logoutRequest.NameID.Name;
                foreach (var sessionIndex in logoutRequest.SessionIndexes)
                {
                    samlSessions.Add(new SamlSessionData { Issuer = issuer, NameId = nameId, SessionIndex = sessionIndex.Index });
                }
            };
            try
            {
                this.samlServiceProvider.Events.OnLogoutRequestReceived += samlLogoutHandler;

                var sloResult = await this.samlServiceProvider.ReceiveSloAsync();
                var returnUrl = await this.mediator.Send(
                    new LogoutSamlUserCommand(tenantModel.Id, authenticationMethod.OrganisationId, sloResult, samlSessions));
                if (!sloResult.IsResponse)
                {
                    // This is a request from the IdP to logout.
                    // Respond to the IdP-initiated SLO request indicating successful logout.
                    await this.samlServiceProvider.SendSloAsync();
                }

                return this.Redirect(returnUrl);
            }
            finally
            {
                this.samlServiceProvider.Events.OnLogoutRequestReceived -= samlLogoutHandler;
            }
        }

        /// <summary>
        /// Resolve the HTTP artifact.
        /// This is only required if supporting the HTTP-Artifact binding.
        /// </summary>
        [HttpGet]
        [HttpPost]
        [Route("{authenticationMethodId}/artefact-resolution-service")]
        [RequestIntent(RequestIntent.ReadOnly)]
        public async Task<IActionResult> ArtifactResolutionService(
            string tenant,
            Guid authenticationMethodId)
        {
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var authenticationMethod
                = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantModel.Id, authenticationMethodId));
            EntityHelper.ThrowIfNotFound(tenantModel.Id, authenticationMethodId, authenticationMethod);
            await this.samlServiceProvider.SetConfigurationNameAsync($"{tenantModel.Id}|{authenticationMethodId}");
            await this.samlServiceProvider.ResolveArtifactAsync();
            return new EmptyResult();
        }

        private SamlAuthenticationMethodReadModel GetAsSamlAuthenticationMethodOrThrow(
            IAuthenticationMethodReadModelSummary? authenticationMethod)
        {
            if (authenticationMethod is SamlAuthenticationMethodReadModel samlAuthenticationMethod)
            {
                return samlAuthenticationMethod;
            }

            throw new ErrorException(Errors.Authentication.Saml.AuthenticationMethodNotSaml(authenticationMethod.Id));
        }
    }
}
