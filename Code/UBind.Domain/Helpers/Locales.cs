// <copyright file="Locales.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    /// <summary>
    /// For exposing locale codes as properties, to avoid litterring code with typo-prone string literals.
    /// </summary>
    public static class Locales
    {
#pragma warning disable SA1303 // Const field names should begin with upper-case letter
#pragma warning disable SA1310 // Field names should not contain underscore

        /// <summary>
        /// Gets the locale code for English (Australia).
        /// </summary>
        public const string en_AU = "en-AU";

        /// <summary>
        /// Gets the locale code for English (United Kingdom).
        /// </summary>
        public const string en_GB = "en-GB";

        /// <summary>
        /// Gets the locale code for English (United States).
        /// </summary>
        public const string en_US = "en-US";

        /// <summary>
        /// Gets the locale code for English (New Zealand).
        /// </summary>
        public const string en_NZ = "en-NZ";

#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore SA1303 // Const field names should begin with upper-case letter
    }
}
