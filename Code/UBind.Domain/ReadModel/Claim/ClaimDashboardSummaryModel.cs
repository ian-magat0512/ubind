// <copyright file="ClaimDashboardSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using NodaTime;
    using UBind.Domain.Extensions;

    public class ClaimDashboardSummaryModel : IDashboardSummaryModel
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public Instant CreatedTimestamp => Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);

        public long CreatedTicksSinceEpoch { get; set; }

        public Instant LastModifiedTimestamp => Instant.FromUnixTimeTicks(this.LastModifiedInTicksSinceEpoch);

        public long LastModifiedInTicksSinceEpoch { get; set; }

        public string ClaimState { get; set; }

        public float Amount { get; set; }

        public bool IsSettled => this.ClaimState == Domain.ClaimState.Complete;

        public bool IsDeclined => this.ClaimState == Domain.ClaimState.Declined;

        public long? SettledTicksSinceEpoch { get; set; }

        public Instant? SettledTimestamp => this.SettledTicksSinceEpoch.HasValue
            ? Instant.FromUnixTimeTicks(this.SettledTicksSinceEpoch.Value)
            : null;

        public long? DeclinedTicksSinceEpoch { get; set; }

        public Instant? DeclinedTimestamp => this.DeclinedTicksSinceEpoch.HasValue
            ? Instant.FromUnixTimeTicks(this.DeclinedTicksSinceEpoch.Value)
            : null;

        public long? LodgedTicksSinceEpoch { get; set; }

        public Instant? LodgedTimestamp => this.LodgedTicksSinceEpoch.HasValue
            ? Instant.FromUnixTimeTicks(this.LodgedTicksSinceEpoch.Value)
            : null;

        public float ProcessingTime
        {
            get
            {
                if (!this.IsSettled && !this.IsDeclined)
                {
                    return 0;
                }

                if (this.IsSettled && this.LodgedTimestamp.HasValue && this.SettledTimestamp.HasValue)
                {
                    var processingTime = this.LodgedTimestamp.Value.GetDaysDifference(this.SettledTimestamp.Value);
                    return Convert.ToSingle(Math.Round(processingTime, 1));
                }
                else if (this.IsDeclined && this.LodgedTimestamp.HasValue && this.DeclinedTimestamp.HasValue)
                {
                    var processingTime = this.LodgedTimestamp.Value.GetDaysDifference(this.DeclinedTimestamp.Value);
                    return Convert.ToSingle(Math.Round(processingTime, 1));
                }
                else
                {
                    return 0;
                }
            }
        }

        public float SettledAmount => this.IsSettled ? this.Amount : 0;

        public Instant Timestamp => this.IsSettled ? this.SettledTimestamp.Value : this.DeclinedTimestamp.Value;
    }
}
