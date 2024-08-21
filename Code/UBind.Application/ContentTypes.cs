// <copyright file="ContentTypes.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    /// <summary>
    /// Class specifying constants for content types.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// Json.
        /// </summary>
        public const string Json = "application/json";

        /// <summary>
        /// Plain text.
        /// </summary>
        public const string PlainText = "text/plain";

        /// <summary>
        /// HTML.
        /// </summary>
        public const string Html = "text/html";

        /// <summary>
        /// Url encoded.
        /// </summary>
        public const string Urlencoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// Stream.
        /// </summary>
        public const string Stream = "application/octet-stream";
    }
}
