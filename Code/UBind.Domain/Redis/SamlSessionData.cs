// <copyright file="SamlSessionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Redis
{
    public class SamlSessionData
    {
        /// <summary>
        /// Gets or sets the identifier of the identity provider.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets a unique value for the session within the IdP.
        /// </summary>
        public string? SessionIndex { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user.
        /// </summary>
        public string NameId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the IdP supports single logout.
        /// If this is set to true, the logout request will be sent to the IdP.
        /// </summary>
        public bool SupportsSingleLogout { get; set; }
    }
}
