// <copyright file="PasswordHashingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    /// <inheritdoc />
    public class PasswordHashingService : IPasswordHashingService
    {
        private const int BCryptWorkFactor = 12;

        /// <inheritdoc/>
        public string SaltAndHash(string plaintextPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plaintextPassword, BCryptWorkFactor);
        }

        /// <inheritdoc/>
        public bool Verify(string plaintextPassword, string saltedHashedPasswordWithAlgorithmAndSalt)
        {
            return BCrypt.Net.BCrypt.Verify(plaintextPassword, saltedHashedPasswordWithAlgorithmAndSalt);
        }
    }
}
