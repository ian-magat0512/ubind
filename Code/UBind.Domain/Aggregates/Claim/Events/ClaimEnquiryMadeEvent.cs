// <copyright file="ClaimEnquiryMadeEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Claim.Entities;

    /// <summary>
    ///  Aggregate for claims.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Event raised when an enquiry was made for a claim.
        /// </summary>
        public class ClaimEnquiryMadeEvent : ClaimSnapshotEvent<ClaimEnquiryMadeEvent>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimEnquiryMadeEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the claim aggregate.</param>
            /// <param name="claim">The claim.</param>
            /// <param name="performingUserId">The user Id who made the claim enquiry.</param>
            /// <param name="createdTimestamp">The created timestamp.</param>
            public ClaimEnquiryMadeEvent(Guid tenantId, Guid aggregateId, Claim claim, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, claim, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private ClaimEnquiryMadeEvent()
            {
            }
        }
    }
}
