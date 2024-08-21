// <copyright file="IClaimData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using NodaTime;

    /// <summary>
    /// Interface for accessing claim data.
    /// </summary>
    public interface IClaimData
    {
        /// <summary>
        /// Gets the amount the claim is for if known, otherwise null.
        /// </summary>
        decimal? Amount { get; }

        /// <summary>
        /// Gets the description of the claim if known, otherwise null.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the date of the incident the claim relates to if known, otherwise null.
        /// </summary>
        Instant? IncidentTimestamp { get; }

        /// <summary>
        ///  Gets the time zone which the incident date pertains to.
        /// </summary>
        DateTimeZone TimeZone { get; }
    }
}
