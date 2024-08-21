// <copyright file="IEwayConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Eway
{
    /// <summary>
    /// Configuration for an Eway account.
    /// </summary>
    public interface IEwayConfiguration
    {
        /// <summary>
        /// Gets the API key.
        /// </summary>
        string ApiKey { get; }

        /// <summary>
        /// Gets the client side encryption key.
        /// </summary>
        string ClientSideEncryptionKey { get; }

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        EwayEndpoint Endpoint { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Gets the encryption service url.
        /// </summary>
        string EncryptionUrl { get; }

        /// <summary>
        /// Gets the server encryption pass key.
        /// </summary>
        string ServerSideEncryptionKey { get; }
    }
}
