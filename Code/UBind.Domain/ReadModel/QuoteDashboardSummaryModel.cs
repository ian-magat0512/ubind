// <copyright file="QuoteDashboardSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.ValueTypes;

    public class QuoteDashboardSummaryModel : IDashboardSummaryModel
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public Instant CreatedTimestamp => Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);

        public long CreatedTicksSinceEpoch { get; set; }

        public Instant LastModifiedTimestamp => Instant.FromUnixTimeTicks(this.LastModifiedTimeInTicksSinceEpoch);

        public long LastModifiedTimeInTicksSinceEpoch { get; set; }

        public string QuoteState { get; set; }

        public bool IsAbandoned =>
            (this.QuoteState.Equals(StandardQuoteStates.Incomplete, StringComparison.InvariantCultureIgnoreCase)
            && Period.Between(
                this.LastModifiedTimestamp.ToLocalDateTimeInAet(),
                Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateTimeInAet(),
                PeriodUnits.Days).Days > 7)
            || this.QuoteState.Equals(StandardQuoteStates.Expired, StringComparison.InvariantCultureIgnoreCase);

        public bool IsCompleted => this.QuoteState.Equals(StandardQuoteStates.Complete, StringComparison.InvariantCultureIgnoreCase);

        public float Amount { get; set; }

        public Instant Timestamp => this.CreatedTimestamp;
    }
}
