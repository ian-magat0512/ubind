// <copyright file="DeftErrorResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using Newtonsoft.Json;

    /// <summary>
    /// For parsing DEFT error response.
    /// </summary>
    public class DeftErrorResponse
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        [JsonProperty]
        public string ErrorCode { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        [JsonProperty]
        public string Message { get; private set; }

        /// <summary>
        /// Gets the error details.
        /// </summary>
        [JsonProperty]
        public string Details { get; private set; }

        /// <summary>
        /// Gets the error type.
        /// </summary>
        [JsonProperty]
        public string Type { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.Message}: {this.Details}";
        }
    }
}
