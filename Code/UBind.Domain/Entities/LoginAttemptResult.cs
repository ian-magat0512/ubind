// <copyright file="LoginAttemptResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using NodaTime;

    /// <summary>
    /// Represents event that occured on a login attempt.
    /// </summary>
    public class LoginAttemptResult : EmailRequestRecord
    {
        private LoginAttemptResult(
            Guid tenantId,
            Guid organisationId,
            string email,
            string clientIpAddress,
            bool succeeded,
            Instant timestamp,
            string error = null)
            : base(tenantId, organisationId, email, clientIpAddress, timestamp)
        {
            this.Succeeded = succeeded;
            this.Error = error;
        }

        // Parameterless constructor for EF.
        private LoginAttemptResult()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the attempt succeeded.
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Gets the error for failed attempts.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginAttemptResult"/> class for successful attempt.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant that the attempt was for.</param>
        /// <param name="organisationId">The Id of the organisation that the attempt was for.</param>
        /// <param name="email">The email address.</param>
        /// <param name="clientIpAddress">The IP address of the client.</param>
        /// <param name="timestamp">The time of the attempt.</param>
        /// <returns>A new instance of the <see cref="LoginAttemptResult"/> class for successful attempt.</returns>
        public static LoginAttemptResult CreateSuccess(
            Guid tenantId, Guid organisationId, string email, string clientIpAddress, Instant timestamp)
        {
            return new LoginAttemptResult(tenantId, organisationId, email, clientIpAddress, true, timestamp);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginAttemptResult"/> class for failed attempt.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant that the attempt was for.</param>
        /// <param name="organisationId">The Id of the organisation that the attempt was for.</param>
        /// <param name="email">The email address.</param>
        /// <param name="clientIpAddress">The IP address of the client.</param>
        /// <param name="timestamp">The time of the attempt.</param>
        /// <param name="reason">The reason for the failure.</param>
        /// <returns>A new instance of the <see cref="LoginAttemptResult"/> class for failed atempt.</returns>
        public static LoginAttemptResult CreateFailure(
            Guid tenantId,
            Guid organisationId,
            string email,
            string clientIpAddress,
            Instant timestamp,
            string reason)
        {
            return new LoginAttemptResult(tenantId, organisationId, email, clientIpAddress, false, timestamp, reason);
        }
    }
}
