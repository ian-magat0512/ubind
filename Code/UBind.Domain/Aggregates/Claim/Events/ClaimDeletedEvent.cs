// <copyright file="ClaimDeletedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim;

using Newtonsoft.Json;
using NodaTime;

/// <summary>
/// Aggregate for deleting claim.
/// </summary>
public partial class ClaimAggregate
{
    /// <summary>
    /// Delete claim.
    /// </summary>
    public class ClaimDeletedEvent : Event<ClaimAggregate, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimDeletedEvent"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="claimId">The claim Id.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">The time stamp when the event was created.</param>
        public ClaimDeletedEvent(
            Guid tenantId,
            Guid claimId,
            Guid? performingUserId,
            Instant timestamp)
            : base(tenantId, claimId, performingUserId, timestamp)
        {
        }

        [JsonConstructor]
        private ClaimDeletedEvent()
        : base(default, default(Guid), default(Guid), default(Instant))
        {
        }
    }
}
