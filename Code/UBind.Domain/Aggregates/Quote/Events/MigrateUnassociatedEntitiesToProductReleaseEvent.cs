// <copyright file="MigrateUnassociatedEntitiesToProductReleaseEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote;

using Newtonsoft.Json;
using NodaTime;

public partial class QuoteAggregate
{
    /// <summary>
    /// Represents an event within the QuoteAggregate that facilitates the migration of previously unassociated quotes
    /// and policy transactions, associating them to a new product release (newProductReleaseId).
    /// </summary>
    public class MigrateUnassociatedEntitiesToProductReleaseEvent : Event<QuoteAggregate, Guid>
    {
        public MigrateUnassociatedEntitiesToProductReleaseEvent(
            Guid tenantId, Guid productId, Guid aggregateId, Guid newProductReleaseId, Guid? performingUserId, Instant createdTimestamp)
            : base(tenantId, aggregateId, performingUserId, createdTimestamp)
        {
            this.ProductId = productId;
            this.NewProductReleaseId = newProductReleaseId;
        }

        [JsonConstructor]
        private MigrateUnassociatedEntitiesToProductReleaseEvent()
        {
        }

        [JsonProperty]
        public Guid ProductId { get; private set; }

        [JsonProperty]
        public Guid NewProductReleaseId { get; private set; }
    }
}
