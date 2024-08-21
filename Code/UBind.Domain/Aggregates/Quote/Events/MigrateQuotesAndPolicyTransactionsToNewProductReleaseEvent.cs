// <copyright file="MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent.cs" company="uBind">
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
    /// Migrates quotes and policy transactions within this QuoteAggregate which are using the originalProductReleaseId to use the newProductReleaseId
    /// </summary>
    public class MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent : Event<QuoteAggregate, Guid>
    {
        public MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent(
            Guid tenantId, Guid aggregateId, Guid orginalProductReleaseId, Guid newProductReleaseId, Guid? performingUserId, Instant createdTimestamp)
            : base(tenantId, aggregateId, performingUserId, createdTimestamp)
        {
            this.OrginalProductReleaseId = orginalProductReleaseId;
            this.NewProductReleaseId = newProductReleaseId;
        }

        [JsonConstructor]
        private MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent()
        {
        }

        [JsonProperty]
        public Guid OrginalProductReleaseId { get; private set; }

        [JsonProperty]
        public Guid NewProductReleaseId { get; private set; }
    }
}
