// <copyright file="Authentication.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Net;

    public static partial class Errors
    {
        public static class Authentication
        {
            public static Error ConfigurationNotFound(Guid tenantId, Guid authenticationMethodId) => new Error(
                "authentication.method.not.found",
                "An authentication method configuration was not found",
                $"No Authentication Method configuration was found for the tenant with ID {tenantId} and "
                + $"authentication method with ID {authenticationMethodId}.",
                HttpStatusCode.BadRequest);

            public static Error MethodDisabled(string authenticationMethodName) => new Error(
                "authentication.method.disabled",
                "You can't sign in with this authentication method",
                $"The authentication method \"{authenticationMethodName}\" is disabled. "
                + "Please get in touch with your systems administrator or customer support.",
                HttpStatusCode.Unauthorized);

            public static class Saml
            {
                public static Error AuthenticationMethodNotSaml(Guid authenticationMethodId) => new Error(
                    "authentication.method.not.saml",
                    "We were expecting a SAML configuration",
                    $"The authentication method with id {authenticationMethodId} is not a SAML configuration. "
                    + "Please check that you are using the correct authentication method ID.",
                    HttpStatusCode.BadRequest);

                public static Error ServiceProviderCertificateNotLoaded() => new Error(
                    "saml.service.provider.certificate.not.loaded",
                    "The service provider certificate has not been loaded",
                    "The SAML service provider certificate has not been loaded from the certificate store. "
                    + "The certificate ServiceProviderCertificateSubjectName must be specified in appsettings.json, "
                    + "and the certificate must be loaded into the store (e.g. Windows Certificate Store), with a "
                    + "StoreLocation of LocalMachine.",
                    HttpStatusCode.InternalServerError);

                public static Error AssertionReturnUrlNotProvided() => new Error(
                    "saml.assertion.return.url.not.provided",
                    "The SAML return URL was not provided",
                    "When receiving an assertion from the identity provider, the return URL was not provided. "
                    + "The return URL is need to know which portal URL to return the user to after sign-in. "
                    + "The return URL is passed to the identity provider as part of the SAML SSO initiation, "
                    + "and we require the identity provider to pass it back.",
                    HttpStatusCode.BadRequest);

                public static Error SamlLogoutReturnUrlNotProvided() => new Error(
                    "saml.logout.return.url.not.provided",
                    "The SAML return URL was not provided",
                    "When receiving a single logout request or response from the identity provider, the return URL was "
                    + "not provided. "
                    + "The return URL is need to know which portal URL to return the user to after logging out. "
                    + "The return URL is passed to the identity provider as part of the logout request. "
                    + "and we require the identity provider to pass it back.",
                    HttpStatusCode.BadRequest);

                public static Error AttributeNotFound(string attributeName) => new Error(
                    "saml.attribute.not.found",
                    "The SAML attribute was not found",
                    $"The SAML attribute {attributeName} was not found in the assertion.",
                    HttpStatusCode.BadRequest);

                public static Error UniqueIdentifierAttributeNameNotSet(string authenticationMethodName) => new Error(
                    "saml.unique.identifier.attribute.name.not.set",
                    "The SAML unique identifier attribute name was not set",
                    $"The SAML unique identifier attribute name was not set for the authentication method "
                    + $"{authenticationMethodName}.",
                    HttpStatusCode.InternalServerError);

                public static Error NoUserAccountExistsAndAutoProvisioningIsDisabledException(
                    string authenticationMethodName, string userExternalId) => new Error(
                    "saml.no.user.account.exists.and.auto.provisioning.is.disabled",
                    "No user account exists and auto-provisioning is disabled",
                    $"No user account exists for the user with external ID {userExternalId} and the authentication method "
                        + $"{authenticationMethodName}. Auto-provisioning is disabled for this authentication method.",
                    HttpStatusCode.Unauthorized);

                public static Error CustomerAccountAutoProvisioningDisabled(
                    string authenticationMethodName, string userExternalId) => new Error(
                    "saml.customer.account.auto.provisioning.is.disabled",
                    "Customer account auto-provisioning is disabled",
                    $"No user account exists for the user with external ID {userExternalId} and the authentication method "
                    + $"{authenticationMethodName}. Auto-provisioning of customer accounts is disabled for this "
                    + "authentication method.",
                    HttpStatusCode.Unauthorized);

                public static Error AgentAccountAutoProvisioningDisabled(
                    string authenticationMethodName, string userExternalId) => new Error(
                    "saml.agent.account.auto.provisioning.is.disabled",
                    "Agent account auto-provisioning is disabled",
                    $"No user account exists for the user with external ID {userExternalId} and the authentication method "
                    + $"{authenticationMethodName}. Auto-provisioning of agent accounts is disabled for this "
                    + "authentication method.",
                    HttpStatusCode.Unauthorized);

                public static Error NoUserAccountExistsAndCouldNotBeCreatedBecauseUserTypeCouldNotBeDeterminedException(
                    string authenticationMethodName, string userExternalId) => new Error(
                        "saml.no.user.account.exists.and.could.not.be.created.because.user.type.could.not.be.determined",
                        "No user account exists and could not be created because user type could not be determined",
                        $"No user account exists for the user with external ID {userExternalId} and the "
                        + $"authentication method {authenticationMethodName}. Auto provisioning is enabled for both "
                        + "of the user types (agents and customers), however the user type could not be "
                        + "determined, so the user account could not be created. "
                        + "If you want to suport Identity Provider Initiated SSO, and auto provisioning of both "
                        + "customers and agents, you'll need to specify and provide a SAML user type attribute.",
                        HttpStatusCode.Unauthorized);

                public static Error OrganisationUniqueIdentifierNotProvided(
                    string authenticationMethodName, string organisationUniqueIdentifierAttributeName) => new Error(
                        "saml.organisation.unique.identifier.not.provided",
                        "The organisation unique identifier was not provided",
                        $"The organisation unique identifier was not provided for the authentication method "
                        + $"{authenticationMethodName}. When receiving the SAML assertion, we were expecting the "
                        + $"organisation unique identifier in the attribute \"{organisationUniqueIdentifierAttributeName}\".",
                        HttpStatusCode.Unauthorized);

                public static Error OrganisationNameNotProvided(
                    string authenticationMethodName, string organisationNameAttributeName) => new Error(
                        "saml.organisation.name.not.provided",
                        "The organisation name was not provided",
                        $"The organisation name was not provided for the authentication method {authenticationMethodName}. "
                        + $"When receiving the SAML assertion, we were expecting the organisation name in the attribute "
                        + $"\"{organisationNameAttributeName}\", but it was empty.",
                        HttpStatusCode.Unauthorized);

                public static Error NoOrganisationExistsAndAutoProvisioningIsDisabledException(
                    string authenticationMethodName, string organisationUniqueIdentifier) => new Error(
                        "saml.no.organisation.exists.and.auto.provisioning.is.disabled",
                        "No organisation exists and auto-provisioning is disabled",
                        $"No organisation exists for the organisation with unique identifier {organisationUniqueIdentifier} "
                        + $"and the authentication method {authenticationMethodName} does not allow auto-provisioning.",
                        HttpStatusCode.Unauthorized);

                public static Error NoEmailAddressSourceConfigured(string authenticationMethodName) => new Error(
                    "saml.no.email.address.source.configured",
                    "No email address source configured",
                    $"No email address source is configured for the authentication method {authenticationMethodName}. "
                    + "Please set the email address attribute name, or configure the NameId as the email address.",
                    HttpStatusCode.Unauthorized);

                public static Error UserHasALinkedIdentityWithExclusiveRoleManagement(
                    string userDisplayName, string authenticationMethodName) => new Error(
                        "saml.user.has.a.linked.identity.with.exclusive.role.management",
                        "We couldn't provision your user account",
                        "Your user account is already linked with another identity provider, and that identity "
                        + "provider has been configured to exclusively manage your roles. We therefore cannot "
                        + "provision your account with this new identity provider, as the two would end up "
                        + "overwriting your roles, leading to an unpredictable situation with unexpected results.",
                        HttpStatusCode.Conflict);

                public static Error UsersOrganisationHasLinkedIdentityToADifferentOrganisation(
                    string userDisplayName,
                    string assertedOrganisationName,
                    string userOrganisationName,
                    string authenticationMethodName) => new Error(
                        "saml.users.organisation.has.linked.identity.to.a.different.organisation",
                        "Your user account is linked with a different organisation",
                        $"When trying to login with the SAML authentication method \"{authenticationMethodName}\", "
                        + $"the user account for \"{userDisplayName}\" was found to be under the organisation "
                        + $"\"{userOrganisationName}\", however the identity provider asserted that it was under the "
                        + $"organisation \"{assertedOrganisationName}\". "
                        + "If the user has been moved to a different organisation in the identity provider, it must "
                        + "also be moved to that organisation in uBind such that the organisation unique identifier "
                        + "in uBind now matches that in the identity provider.",
                        HttpStatusCode.Conflict);

                public static Error SingleLogoutNotSupported(string authenticationMethodName) => new Error(
                    "saml.single.logout.not.supported",
                    "Single logout not supported",
                    $"Single logout is not supported for the authentication method {authenticationMethodName}. "
                    + "No single logout service URL has been configured.",
                    HttpStatusCode.BadRequest);
            }
        }
    }
}
