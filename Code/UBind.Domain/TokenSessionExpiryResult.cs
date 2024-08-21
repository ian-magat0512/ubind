// <copyright file="TokenSessionExpiryResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents the token session expiration result.
    /// </summary>
    public class TokenSessionExpiryResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenSessionExpiryResult"/> class.
        /// </summary>
        /// <param name="message">the message.</param>
        /// <param name="resultType">The type of this result.</param>
        /// <param name="periodType">The period type of the session.</param>
        /// <param name="period">the length of the session in period types.</param>
        /// <param name="success">the success of the request.</param>
        public TokenSessionExpiryResult(string message, string resultType, string periodType, long period, bool success)
        {
            this.Message = message;
            this.ResultType = resultType;
            this.PeriodType = periodType;
            this.Period = period;
            this.Success = success;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenSessionExpiryResult"/> class.
        /// </summary>
        /// <param name="success">the success of the request.</param>
        public TokenSessionExpiryResult(bool success)
        {
            this.Success = success;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the result type.
        /// </summary>
        public string ResultType { get; set; }

        /// <summary>
        /// Gets or sets the period type.
        /// </summary>
        public string PeriodType { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        public long Period { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request is successful or not.
        /// </summary>
        public bool Success { get; set; }
    }
}
