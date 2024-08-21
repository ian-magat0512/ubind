// <copyright file="AuthConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Configuration
{
    using System.Linq;
    using UBind.Application.Authentication;

    /// <inheritdoc/>
    public class AuthConfiguration : IAuthConfiguration
    {
        /// <summary>
        /// Gets or sets the domain the server is hosted at.
        /// </summary>
        public string ServerDomain { get; set; }

        /// <summary>
        /// Gets or sets permitted CORS origins as a comma-separated string.
        /// </summary>
        public string PermittedCorsOrigins { get; set; }

        /// <inheritdoc/>
        string[] IAuthConfiguration.PermittedCorsOrigins => this.PermittedCorsOrigins
            .Split(',')
            .Select(item => item.Trim())
            .ToArray();

        /// <inheritdoc/>
        public string TokenIssuer => this.ServerSchemeAndDomain;

        /// <inheritdoc/>
        public string TokenAudience => this.ServerSchemeAndDomain;

        private string ServerSchemeAndDomain => "https://" + this.ServerDomain;
    }
}
