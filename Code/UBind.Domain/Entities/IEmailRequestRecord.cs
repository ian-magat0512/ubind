// <copyright file="IEmailRequestRecord.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;

    /// <summary>
    /// interface for class used for recording email requests.
    /// </summary>
    public interface IEmailRequestRecord
    {
        /// <summary>
        /// Gets the organisation Id the attempt was for.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the email used in the attempt.
        /// </summary>
        string EmailAddress { get; }

        /// <summary>
        /// Gets the IP address the attempt was made from.
        /// </summary>
        string ClientIpAddress { get; }

        /// <summary>
        /// Gets the entity's unique identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the entity created time (in ticks since Epoch).
        /// </summary>
        /// <remarks> Primitive typed property for EF to store created time.</remarks>
        long CreatedTicksSinceEpoch { get; }
    }
}
