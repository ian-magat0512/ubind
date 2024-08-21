// <copyright file="IPasswordResetTrackingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System;

    /// <summary>
    /// Service for reset password attempts.
    /// </summary>
    public interface IPasswordResetTrackingService
    {
        /// <summary>
        /// Record reset password processed (not blocked) for a given email in a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="email">The email.</param>
        /// <param name="clientIpAddress">The IP of the client who logged in.</param>
        void Record(Guid tenantId, Guid organisationId, string email, string clientIpAddress);

        /// <summary>
        /// Check if has password reset request for an email should be blocked due to too many requests in a given period.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="email">The email.</param>
        /// <param name="requestPerPeriodBlockingTrigger">The number of attempts in the given period that will trigger blocking.</param>
        /// <param name="periodSizeInMinutes">Period size in minutes.</param>
        /// <returns>true if it the request should be blocked, otherwise false..</returns>
        bool ShouldBlockRequest(
            Guid tenantId,
            Guid organisationId,
            string email,
            int requestPerPeriodBlockingTrigger,
            int periodSizeInMinutes);
    }
}
