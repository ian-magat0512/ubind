// <copyright file="PolicyTransactionDashboardSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;

    public class PolicyTransactionDashboardSummaryModel : IDashboardSummaryModel
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public Instant CreatedTimestamp => Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);

        public long CreatedTicksSinceEpoch { get; set; }

        public Instant LastModifiedTimestamp => Instant.FromUnixTimeTicks(this.LastModifiedInTicksSinceEpoch);

        public long LastModifiedInTicksSinceEpoch { get; set; }

        public float Amount { get; set; }

        public Instant Timestamp => this.CreatedTimestamp;
    }
}
