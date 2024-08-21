// <copyright file="IEncryptionConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Encryption
{
    public interface IEncryptionConfiguration
    {
        /// <summary>
        /// Gets or sets the private key to be used for decryption between ubind systems.
        /// </summary>
        string RsaPrivateKey { get; set; }

        /// <summary>
        /// Gets or sets the public key to be used for encryption/identification between ubind systems.
        /// </summary>
        string RsaPublicKey { get; set; }
    }
}
