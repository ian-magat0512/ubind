// <copyright file="PasswordResetRecord.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using NodaTime;

    /// <summary>
    /// Represents event that occured on a password reset attempt.
    /// </summary>
    public class PasswordResetRecord : EmailRequestRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetRecord"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="email">The email to track.</param>
        /// <param name="clientIpAddress">The client IP address. </param>
        /// <param name="timestamp">The timestamp.</param>
        public PasswordResetRecord(
            Guid tenantId, Guid organisationId, string email, string clientIpAddress, Instant timestamp)
            : base(tenantId, organisationId, email, clientIpAddress, timestamp)
        {
        }

        // Parameterless constructor for EF.
        private PasswordResetRecord()
        {
        }
    }
}
