// <copyright file="PortalSamlLoginMethodModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Models.PortalLoginMethod
{
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;

    public class PortalSamlLoginMethodModel : PortalLoginMethodModel
    {
        public PortalSamlLoginMethodModel(
            PortalSignInMethodReadModel portalSignInMethodReadModel,
            SamlAuthenticationMethodReadModel authenticationMethodReadModel,
            PortalReadModel portalReadModel)
            : base(portalSignInMethodReadModel, authenticationMethodReadModel)
        {
            this.IdentityProviderSingleSignOnServiceUrl = authenticationMethodReadModel.IdentityProviderSingleSignOnServiceUrl;
        }

        /// <summary>
        /// Gets or sets the IdP URL where uBind sends SAML Authentication Request messages to when a user needs to be
        /// authenticated.
        /// </summary>
        public string IdentityProviderSingleSignOnServiceUrl { get; set; }
    }
}
