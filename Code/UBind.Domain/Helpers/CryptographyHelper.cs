// <copyright file="CryptographyHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Cryptography helper class.
    /// </summary>
    public class CryptographyHelper
    {
        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <param name="content">The content byte array.</param>
        /// <param name="hashAlgorithmName">The hash algorithm name.</param>
        /// <returns>The computed hash code.</returns>
        public static string ComputeHashString(byte[] content, HashAlgorithmName hashAlgorithmName)
        {
            using (var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName.Name))
            {
                var hash = hashAlgorithm.ComputeHash(content);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        /// <summary>
        /// Computes the hash value for the specified byte array using SHA256 as the default hash algorithm.
        /// </summary>
        /// <param name="content">The content byte array.</param>
        /// <returns>The computed hash code.</returns>
        public static string ComputeHashString(byte[] content)
        {
            return ComputeHashString(content, HashAlgorithmName.SHA256);
        }

        public static string MaskPrivateKey(string privateKey, int numberOfVisibleCharsAtStartAndEnd = 10)
        {
            string maskedKey = privateKey;

            if (string.IsNullOrEmpty(privateKey))
            {
                return string.Empty;
            }

            int visibleChars = numberOfVisibleCharsAtStartAndEnd;
            int maskingCharsLength = privateKey.Length - 2 * visibleChars;

            if (maskingCharsLength > 0)
            {
                string visibleStart = privateKey.Substring(0, visibleChars);
                string visibleEnd = privateKey.Substring(privateKey.Length - visibleChars);
                string maskingChars = new string('*', maskingCharsLength);

                maskedKey = visibleStart + maskingChars + visibleEnd;
            }

            return maskedKey;
        }
    }
}
