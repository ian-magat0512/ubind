// <copyright file="IMailClientFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    /// <summary>
    /// Factory for SMTP clients.
    /// </summary>
    public interface IMailClientFactory
    {
        /// <summary>
        /// Create a new mail client for connecting to an SMTP server.
        /// </summary>
        /// <param name="host">The host of the SMTP server.</param>
        /// <param name="username">The username of the SMTP server.</param>
        /// <param name="password">The password of the SMTP server.</param>
        /// <param name="port">The port of the SMTP server.</param>
        /// <returns>A new mail client.</returns>
        IMailClient Invoke(string host, string username, string password, int port = 25);
    }
}
