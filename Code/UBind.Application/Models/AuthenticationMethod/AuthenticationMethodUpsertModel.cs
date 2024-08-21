// <copyright file="AuthenticationMethodUpsertModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Model.AuthenticationMethod
{
    public class AuthenticationMethodUpsertModel
    {
        /// <summary>
        /// Gets or sets the ID or alias of the Tenant.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the ID or alias of the Organisation.
        /// </summary>
        public string Organisation { get; set; }

        /// <summary>
        /// Gets or sets the name of the authentication method.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type name of the authentication method.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can sign in using the authentication method.
        /// </summary>
        public bool CanCustomersSignIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether agents can sign in using the authentication method.
        /// </summary>
        public bool CanAgentsSignIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the sign in button on the portal login page.
        /// </summary>
        public bool IncludeSignInButtonOnPortalLoginPage { get; set; }

        /// <summary>
        /// Gets or sets the sign in button background color.
        /// </summary>
        public string? SignInButtonBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the sign in button icon url.
        /// </summary>
        public string? SignInButtonIconUrl { get; set; }

        /// <summary>
        /// Gets or sets the sign in button label.
        /// </summary>
        public string? SignInButtonLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the authentication method is disabled.
        /// </summary>
        public bool Disabled { get; set; }
    }
}
