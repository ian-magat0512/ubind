// <copyright file="IPasswordHashingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    /// <summary>
    /// Service for salting and hashing passwords.
    /// </summary>
    public interface IPasswordHashingService
    {
        /// <summary>
        /// Salt and hash a plaintext password.
        /// </summary>
        /// <param name="plainTextPassword">The plaintext password.</param>
        /// <returns>A string containing the salted, hashed password (along with algorithm and salt used).</returns>
        string SaltAndHash(string plainTextPassword);

        /// <summary>
        /// Verify that a plaintext password hashed to the same value as a given hashed password.
        /// </summary>
        /// <param name="plaintextPassword">The plaintext password to test.</param>
        /// <param name="saltedHashedPasswordWithAlgorithmAndSalt">A string containing the expected hased value (along with algorithm and salt used).</param>
        /// <returns><c>true</c> if the plaintext password hashes to the expected value, otherwise <c>false</c>.</returns>
        bool Verify(string plaintextPassword, string saltedHashedPasswordWithAlgorithmAndSalt);
    }
}
