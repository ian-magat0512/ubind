// <copyright file="MicrosoftGraphConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Configuration
{
    using UBind.Application.MicrosoftGraph;

    /// <inheritdoc/>
    public class MicrosoftGraphConfiguration : IMicrosoftGraphConfiguration
    {
        /// <inheritdoc/>
        public string ApplicationId { get; set; }

        /// <inheritdoc/>
        public string ClientId { get; set; }

        /// <inheritdoc/>
        public int MaxRetryAttempts { get; set; }

        /// <inheritdoc/>
        public string Username { get; set; }

        /// <inheritdoc/>
        public string Password { get; set; }

        /// <inheritdoc/>
        public string UBindFolderName { get; set; }

        /// <inheritdoc/>
        public int AccessTokenLifeTimeInMinutes => 60;
    }
}
