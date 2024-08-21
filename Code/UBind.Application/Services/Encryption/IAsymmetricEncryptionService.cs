// <copyright file="IAsymmetricEncryptionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Encryption
{
    public interface IAsymmetricEncryptionService
    {
        /// <summary>
        /// Encrypts the given input text using a certain passkey.
        /// </summary>
        /// <param name="clearText">The input text.</param>
        /// <returns>The encrypted value in string format.</returns>
        string Encrypt(string clearText);

        /// <summary>
        /// Decrypts the given cipher text using a secret key.
        /// </summary>
        /// <param name="cipherText">The cipher text to decrypt.</param>
        /// <returns>The decrypted value in string format.</returns>
        string Decrypt(string cipherText);

        /// <summary>
        /// Generate new RSA keys for encryption and decryption.
        /// </summary>
        /// <returns>
        /// An object key pair wherein:
        /// PrivateKey is the private key in serialized xml format.
        /// PublicKey is the public key in PEM format.
        /// </returns>
        dynamic GenerateRSAKeys();
    }
}
