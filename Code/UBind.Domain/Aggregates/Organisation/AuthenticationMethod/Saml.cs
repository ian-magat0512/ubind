﻿// <copyright file="Saml.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Organisation.AuthenticationMethod
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Flurl;
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// Security Assertion Markup Language (SAML) is a standard that allows identity providers (IdP) to pass
    /// authorization credentials to service providers (SP).
    /// As an example, uBind are a Service Provider, and our client IAG would be an Identity provider, as they
    /// provide the identities of their brokers to us so we can log them in.
    /// --
    /// The following information needs to be generated by uBind and given to the IdP so they can configure
    /// SAML on their end:
    /// - Assertion Consumer Service URL (ACS URL): an endpoint in uBind (SP) where the Identity Provider (IdP)
    /// sends its SAML responses/assertions.
    /// - Single Logout URL (SLO URL): an endpoint in uBind where the IdP sends logout requests.
    /// </summary>
    public class Saml : AuthenticationMethodBase
    {
        public override string TypeName => "SAML";

        /// <summary>
        /// Gets or sets a unique identifier for the service provider or identity provider.
        /// It's usually a URI, but it doesn't have to be a reachable location.
        /// </summary>
        public string IdentityProviderEntityIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the IdP URL where uBind sends SAML Authentication Request messages to when a user needs to be
        /// authenticated.
        /// </summary>
        [JsonConverter(typeof(UrlConverter))]
        public Url IdentityProviderSingleSignOnServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL which uBind will send a logout request to when the user logs out of uBind.
        /// This SLO URL is used to inform the IdP that the user has logged out of the SP (uBind), so that
        /// the IdP can invalidate the user's session and also notify other service providers to log them out.
        /// On the configuration screen we should also publish the uBind logout URL (SP SLO URL) so that the
        /// IdP can send us a notification that the user's session has been invalidated and we can log them out
        /// locally.
        /// </summary>
        [JsonConverter(typeof(UrlConverter))]
        public Url? IdentityProviderSingleLogoutServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL that is used to resolve a SAML artifact into the corresponding SAML assertion.
        /// Instead of sending the entire SAML assertion to the Service Provider (SP) directly (via the user's
        /// browser), the IdP sends a smaller reference called a SAML Artifact. This approach is beneficial
        /// when you want to avoid potentially sensitive data being exposed at the user's browser.
        /// The SP then uses a direct, server-to-server connection to the IdP's ArtifactResolutionServiceUrl
        /// to exchange the SAML Artifact for the full SAML Assertion.
        /// This process allows the SAML Assertion(which contains the user's authentication and/or authorization
        /// data) to be transmitted securely between the SP and the IdP, without passing through the user's browser.
        /// </summary>
        [JsonConverter(typeof(UrlConverter))]
        public Url? IdentityProviderArtifactResolutionServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the digital certificate used by the IdP to sign SAML responses/assertions.
        /// This certificate must contain IdP's public key (not the private key).
        /// uBind will use the public key to verify the signature of the SAML response/assertion.
        /// </summary>
        [JsonConverter(typeof(X509Certificate2Converter))]
        public X509Certificate2 IdentityProviderCertificate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to sign SAML requests sent to the IdP.
        /// </summary>
        public bool MustSignAuthenticationRequests { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to link the identity to an existing customer user account if one is
        /// found with the same email address.
        /// If this is set to false, and an existing user account exists with the same email address, an error will be
        /// generated and the user won't be able to log in.
        /// If this is set to true, during first time login, the user will be linked to the existing user account.
        /// This setting will only be available if customers can sign in using this authentication method.
        /// </summary>
        public bool? ShouldLinkExistingCustomerWithSameEmailAddress { get; set; }

        public bool CanCustomerAccountsBeAutoProvisioned { get; set; }

        public bool? CanCustomerDetailsBeAutoUpdated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to link the identity to an existing agent user account if one is
        /// found with the same email address.
        /// If this is set to false, and an existing user account exists with the same email address, an error will be
        /// generated and the user won't be able to log in.
        /// If this is set to true, during first time login, the user will be linked to the existing user account.
        /// This setting will only be available if agents can sign in using this authentication method.
        /// Note that even if the user is found via email address, their organisation must also match.
        /// If the organisation does not match, an error will be generated and the user won't be able to log in.
        /// For the organisation to match, there must be either no organisation attribute value (in which case the
        /// organisation associated with the defined authentication method will be checked), or the organisation
        /// attribute must match the alias of the organisation managed by the organisation with the defined
        /// authentication method.
        /// </summary>
        public bool? ShouldLinkExistingAgentWithSameEmailAddress { get; set; }

        public bool CanAgentAccountsBeAutoProvisioned { get; set; }

        public bool? CanAgentDetailsBeAutoUpdated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow users of organisations that are managed by this
        /// organisation to sign in using assertions from the Identity Provider.
        /// This would require that there is an appropriate attribute mapping for the user's organisation,
        /// and/or auto provisioning of the organisation is enabled.
        /// </summary>
        public bool CanUsersOfManagedOrganisationsSignIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether organisations can be auto provisioned.
        /// If this is set to false, and an organisation does not exist with a matching alias, an error will be
        /// generated, and the user will not be able to have their account autoprovisioned.
        /// </summary>
        public bool? CanOrganisationsBeAutoProvisioned { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to link the user to an existing organisation with the same alias.
        /// If this is set to false, and an existing organisation exists with the same alias during auto-provisioning,
        /// an error will be generated, and the user will not be able to have their account auto-provisioned.
        /// If it's set to true, during auto-provisioning, the user will be linked to the existing organisation.
        /// This setting is only relevant if both CanUsersOfManagedOrganisationsSignIn and CanOrganisationsBeAutoProvisioned
        /// are set to true.
        /// </summary>
        public bool? ShouldLinkExistingOrganisationWithSameAlias { get; set; }

        public bool? CanOrganisationDetailsBeAutoUpdated { get; set; }

        /// <summary>
        /// Gets or sets the format of the NameID that the IdP will include in the SAML response.
        /// This could be an email address, a transient identifier, a persistent identifier, etc.
        /// Example: "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress".
        /// </summary>
        public string NameIdFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the NameID as the unique identifier for the user.
        /// If set to false, the unique identifier must be set in the attribute mapping.
        /// </summary>
        public bool UseNameIdAsUniqueIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the NameID as the email address of the user.
        /// This is only relevant if the NameIDFormat is set to an emailAddress.
        /// If this is set, there is no need to use an attribute mapping to map the email address.
        /// </summary>
        public bool? UseNameIdAsEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute that contains the unique identifier for the user.
        /// This is only required when UseNameIdAsUniqueIdentifier is set to false.
        /// </summary>
        public string? UniqueIdentifierAttributeName { get; set; }

        public string? FirstNameAttributeName { get; set; }

        public string? LastNameAttributeName { get; set; }

        public string? EmailAddressAttributeName { get; set; }

        public string? PhoneNumberAttributeName { get; set; }

        public string? MobileNumberAttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute that contains the type of user account.
        /// This is needed to determine whether the user is a customer or an agent, and is only required
        /// when this authentication method allows both customers and users to sign-in using an IDP initiated
        /// sign in method with auto provisioning enabled for that user type.
        /// The reason this is required, is because if the user doesn't exist, we need to know which type of
        /// user to create. If the user initiated their own login from a portal, we know they are a customer or
        /// an agent because of the portal they initated their request from, because it would have been an agent portal
        /// or a customer portal. However if the IdP redirects the user to uBind's SAML endpoint, asking us to create a
        /// user, we don't know which portal, so we don't know the user type to create. Therefore we need to rely on this SAML
        /// attribute to help us.
        /// </summary>
        public string? UserTypeAttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute that contains the unique identifier for the user's organisation.
        /// Only required if CanUsersOfManagedOrganisationsSignIn is set to true.
        /// </summary>
        public string? OrganisationUniqueIdentifierAttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute that contains the name of the user's organisation.
        /// Only required if CanUsersOfManagedOrganisationsSignIn is set to true.
        /// </summary>
        public string? OrganisationNameAttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute that contains the alias of the user's organisation.
        /// If left blank it will be auto generated.
        /// </summary>
        public string? OrganisationAliasAttributeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute that contains a user's role or roles.
        /// The role attribute name allows us to know what roles in the IdP the user has.
        /// The way these multi-valued attributes are represented in the SAML assertion can vary. For example, they
        /// might be included as multiple &lt;saml:AttributeValue&gt; elements within a single &lt;saml:Attribute&gt;
        /// element, or they might be included as a single string with some form of delimiter (such as commas or
        /// semicolons) between the individual values.
        /// Once we have the role for a particualuser, we still might need to map the roles in the IdP to roles in
        /// uBind, defined against the organisation or tenant, so we'lkl have a separate role mapping for that.
        /// </summary>
        public string? RoleAttributeName { get; set; }

        /// <summary>
        /// Gets or sets the delimiter used to separate multiple values in the role AttributeValue.
        /// Leave empty if each role AttributeValue only contains a single value, and instead there are multiple
        /// instances of AttributeValue for the role Attribute.
        /// </summary>
        public string? RoleAttributeValueDelimiter { get; set; }

        /// <summary>
        /// Gets or sets a default role which all agents provisioned by this authentication method will be given.
        /// </summary>
        public Guid? DefaultAgentRoleId { get; set; }

        /// <summary>
        /// Gets or sets a mapping of Idp role names to uBind role IDs.
        /// </summary>
        public Dictionary<string, Guid>? RoleMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this identity provider is the authority on what roles
        /// users have.
        /// If this is set to true, then roles cannot be manually assigned to this user wtihin uBind, and
        /// each time the user signs in, their roles will be updated to match the roles specified in the
        /// identity provider.
        /// </summary>
        public bool AreRolesManagedExclusivelyByThisIdentityProvider { get; set; }
    }
}
