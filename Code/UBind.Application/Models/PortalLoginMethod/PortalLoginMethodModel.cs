// <copyright file="PortalLoginMethodModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Models.PortalLoginMethod
{
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;

    /// <summary>
    /// Represents the data needed for a portal to display an authentication method.
    /// This is used on portal login pages to both render a login button and/or
    /// redirect the user to the external idenity provider's login page.
    /// It contains some data from a PortalSignInMethodModel and an AuthenticationMethod.
    /// This needed because some AuthenticationMethod data is private and should not be
    /// sent publicly to the portal, and the portal needs some information from both models.
    /// </summary>
    public abstract class PortalLoginMethodModel
    {
        protected PortalLoginMethodModel(
            PortalSignInMethodReadModel portalSignInMethodReadModel,
            AuthenticationMethodReadModelSummary authenticationMethodReadModelSummary)
        {
            this.SortOrder = portalSignInMethodReadModel.SortOrder;
            this.Name = portalSignInMethodReadModel.Name;
            this.TypeName = portalSignInMethodReadModel.TypeName;
            this.AuthenticationMethodId = portalSignInMethodReadModel.AuthenticationMethodId;
            this.IncludeSignInButtonOnPortalLoginPage = authenticationMethodReadModelSummary.IncludeSignInButtonOnPortalLoginPage;
            this.SignInButtonBackgroundColor = authenticationMethodReadModelSummary.SignInButtonBackgroundColor;
            this.SignInButtonIconUrl = authenticationMethodReadModelSummary.SignInButtonIconUrl;
            this.SignInButtonLabel = authenticationMethodReadModelSummary.SignInButtonLabel;
        }

        public int SortOrder { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public Guid AuthenticationMethodId { get; set; }

        public bool IncludeSignInButtonOnPortalLoginPage { get; set; }

        public string? SignInButtonBackgroundColor { get; set; }

        public string? SignInButtonIconUrl { get; set; }

        public string? SignInButtonLabel { get; set; }
    }
}
