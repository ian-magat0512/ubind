// <copyright file="ClaimVersionCreatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Claim.Entities;
    using ClaimFileAttachment = UBind.Domain.Entities.ClaimFileAttachment;

    /// <summary>
    /// Aggregate for claim version.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Event raised when a claim version has been created.
        /// </summary>
        public class ClaimVersionCreatedEvent : ClaimSnapshotEvent<ClaimVersionCreatedEvent>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimVersionCreatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="claim">The claim aggregate.</param>
            /// <param name="performingUserId">The userId who created the claim version.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            /// <param name="attachments">The attachments.</param>
            public ClaimVersionCreatedEvent(Guid tenantId, Guid aggregateId, Claim claim, Guid? performingUserId, Instant createdTimestamp, IList<ClaimFileAttachment> attachments)
                : base(tenantId, aggregateId, claim, performingUserId, createdTimestamp)
            {
                this.VersionId = Guid.NewGuid();
                this.VersionNumber = claim.VersionNumber + 1;
                this.Attachments = attachments;
            }

            [JsonConstructor]
            private ClaimVersionCreatedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the claim version.
            /// </summary>
            [JsonProperty]
            public Guid VersionId { get; private set; }

            /// <summary>
            /// Gets the version number.
            /// </summary>
            [JsonProperty]
            public int VersionNumber { get; private set; }

            /// <summary>
            /// Gets the claim file attachment of the claim version.
            /// </summary>
            [JsonProperty]
            public IList<ClaimFileAttachment> Attachments { get; private set; }
        }
    }
}
