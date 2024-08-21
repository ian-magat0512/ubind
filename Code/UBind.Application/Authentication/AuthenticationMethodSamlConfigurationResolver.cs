// <copyright file="AuthenticationMethodSamlConfigurationResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authentication
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ComponentSpace.Saml2.Configuration;
    using ComponentSpace.Saml2.Configuration.Resolver;
    using Flurl;
    using UBind.Application.Configuration;
    using UBind.Application.Queries.AuthenicationMethod;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    public class AuthenticationMethodSamlConfigurationResolver : ISamlConfigurationResolver
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IBaseUrlResolver baseUrlResolver;
        private readonly ICqrsMediator mediator;
        private readonly ISamlConfiguration samlConfiguration;

        public AuthenticationMethodSamlConfigurationResolver(
            IBaseUrlResolver baseUrlResolver,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            ISamlConfiguration samlConfiguration)
        {
            this.baseUrlResolver = baseUrlResolver;
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.samlConfiguration = samlConfiguration;
        }

        public async Task<bool> IsLocalServiceProviderAsync(string configurationName)
        {
            (var tenantId, var authenticationMethodId) = this.GetTenantAndAuthenticationMethodId(configurationName);
            var authenticationMethod = await this.mediator.Send(new GetAuthenticationMethodQuery(tenantId, authenticationMethodId));
            return authenticationMethod != null;
        }

        /// <summary>
        /// Gets the SP configuration for the given tenant and authentication method.
        /// </summary>
        /// <param name="configurationName">A string in the format "{tenantId}|{authenticationMethodId}".</param>
        public async Task<LocalServiceProviderConfiguration> GetLocalServiceProviderConfigurationAsync(string configurationName)
        {
            (var tenantId, var authenticationMethodId) = this.GetTenantAndAuthenticationMethodId(configurationName);
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var authenticationMethod = await this.mediator.Send(new GetAuthenticationMethodQuery(tenant.Id, authenticationMethodId));
            EntityHelper.ThrowIfNotFound(tenant.Id, authenticationMethodId, authenticationMethod);
            var samlAuthenticationMethod = this.GetAsSamlAuthenticationMethodOrThrow(authenticationMethod);
            var baseUrl = this.baseUrlResolver.GetBaseUrl(tenant);
            var samlBaseUrl = Url.Combine(baseUrl, $"api/v1/tenant/{tenant.Details.Alias}/saml/{authenticationMethodId}");
            var localServiceProviderConfiguration = new LocalServiceProviderConfiguration()
            {
                Name = "https://ubind.insure",
                Description = "uBind",
                AssertionConsumerServiceUrl = Url.Combine(samlBaseUrl, "assertion-consumer-service"),
                SingleLogoutServiceUrl = Url.Combine(samlBaseUrl, "single-logout-service"),
                ArtifactResolutionServiceUrl = Url.Combine(samlBaseUrl, "artefact-resolution-service"),
            };

            // TODO: ask whether we can skip the certificate if we don't need to sign requests
            /*
            if (samlAuthenticationMethod.MustSignAuthenticationRequests)
            {
            */
            var certificate = this.samlConfiguration.ServiceProviderCertificate;
            if (certificate == null)
            {
                throw new ErrorException(Errors.Authentication.Saml.ServiceProviderCertificateNotLoaded());
            }

            localServiceProviderConfiguration.LocalCertificates = new List<Certificate>
            {
                new Certificate()
                {
                    String = this.samlConfiguration.ServiceProviderCertificateBase64,
                },
            };
            /*
            }
            */

            return localServiceProviderConfiguration;
        }

        public async Task<PartnerIdentityProviderConfiguration> GetPartnerIdentityProviderConfigurationAsync(string configurationName, string partnerName = null)
        {
            (var tenantId, var authenticationMethodId) = this.GetTenantAndAuthenticationMethodId(configurationName);
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            var authenticationMethod = await this.mediator.Send(new GetAuthenticationMethodQuery(tenant.Id, authenticationMethodId));
            EntityHelper.ThrowIfNotFound(tenant.Id, authenticationMethodId, authenticationMethod);
            var samlAuthenticationMethod = this.GetAsSamlAuthenticationMethodOrThrow(authenticationMethod);
            var partnerIdentityProviderConfiguration = new PartnerIdentityProviderConfiguration()
            {
                Name = samlAuthenticationMethod.IdentityProviderEntityIdentifier,
                Description = samlAuthenticationMethod.Name,
                SingleSignOnServiceUrl = samlAuthenticationMethod.IdentityProviderSingleSignOnServiceUrl,
                SingleLogoutServiceUrl = samlAuthenticationMethod.IdentityProviderSingleLogoutServiceUrl,
                ArtifactResolutionServiceUrl = samlAuthenticationMethod.IdentityProviderArtifactResolutionServiceUrl,
                PartnerCertificates = new List<Certificate>()
                {
                    new Certificate()
                    {
                        String = samlAuthenticationMethod.IdentityProviderCertificate,
                    },
                },
            };

            return partnerIdentityProviderConfiguration;
        }

        public Task<bool> IsLocalIdentityProviderAsync(string configurationName = null)
        {
            return Task.FromResult(false);
        }

        public Task<LocalIdentityProviderConfiguration> GetLocalIdentityProviderConfigurationAsync(string configurationName = null)
        {
            throw new NotImplementedException();
        }

        public Task<PartnerServiceProviderConfiguration> GetPartnerServiceProviderConfigurationAsync(string configurationName = null, string partnerName = null)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetPartnerIdentityProviderNamesAsync(string configurationName = null)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetPartnerServiceProviderNamesAsync(string configurationName = null)
        {
            throw new NotImplementedException();
        }

        private (Guid, Guid) GetTenantAndAuthenticationMethodId(string configurationName)
        {
            string[] parts = configurationName.Split('|');
            return (new Guid(parts[0]), new Guid(parts[1]));
        }

        private SamlAuthenticationMethodReadModel GetAsSamlAuthenticationMethodOrThrow(
            IAuthenticationMethodReadModelSummary authenticationMethod)
        {
            if (authenticationMethod is SamlAuthenticationMethodReadModel samlAuthenticationMethod)
            {
                return samlAuthenticationMethod;
            }

            throw new ErrorException(Errors.Authentication.Saml.AuthenticationMethodNotSaml(authenticationMethod.Id));
        }
    }
}
