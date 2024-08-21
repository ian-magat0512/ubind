// <copyright file="IMicrosoftGraphConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    /// <summary>
    /// Provides configuration for accessing Microsoft Graph.
    /// </summary>
    public interface IMicrosoftGraphConfiguration
    {
        /// <summary>
        /// Gets the ID of the application registered with Azure portal.
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// Gets the OAUth client ID.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Gets the maximum retry count to use for API requests.
        /// </summary>
        int MaxRetryAttempts { get; }

        /// <summary>
        /// Gets the username of the One Drive account used for reading and writing files.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password for the One Drive account used for reading and writing files.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the name of the UBind folder in the user's root directory.
        /// </summary>
        string UBindFolderName { get; }

        /// <summary>
        /// Gets the lifetime of access tokens.
        /// </summary>
        int AccessTokenLifeTimeInMinutes { get; }
    }
}
