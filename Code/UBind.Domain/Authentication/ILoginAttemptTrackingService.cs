// <copyright file="ILoginAttemptTrackingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System;
    using NodaTime;

    /// <summary>
    /// Service for tracking attempt failures and handling account locking.
    /// </summary>
    public interface ILoginAttemptTrackingService
    {
        /// <summary>
        /// Determine login attempt with a particular email is blocked in a given tenant, due to too many failed attempts.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="email">The email address.</param>
        /// <returns>true, if login with the given email is blocked, otherwise false.</returns>
        bool IsLoginAttemptEmailBlocked(Guid tenantId, Guid organisationId, string email);

        /// <summary>
        /// Record a successful login attempt with a given email in a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="email">The email.</param>
        /// <param name="clientIpAddress">The IP of the client who logged in.</param>
        /// <param name="timestamp">The time of the login.</param>
        void RecordLoginSuccess(
            Guid tenantId, Guid organisationId, string email, string clientIpAddress, Instant timestamp);

        /// <summary>
        /// Record a failed login attempt with a given email in a given tenant, and block the email if necessary.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="email">The email.</param>
        /// <param name="clientIpAddress">The IP of the client who logged in.</param>
        /// <param name="reason">The reason the login failed.</param>
        /// <param name="timestamp">The time of the login.</param>
        Task RecordLoginFailureAndBlockEmailIfNecessary(
            Guid tenantId,
            Guid organisationId,
            string email,
            string clientIpAddress,
            string reason,
            Instant timestamp);
    }
}
