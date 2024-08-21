// <copyright file="ClaimData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using FluentAssertions;
    using NodaTime;

    /// <summary>
    /// Exposes claim and calculation data from a specific calculation result.
    /// </summary>
    public class ClaimData : IClaimData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimData"/> class.
        /// </summary>
        /// <param name="amount">The amount the claim is for if known, otherwise null.</param>
        /// <param name="description">A description of the claim if known, otherwise null.</param>
        /// <param name="incidentTimestamp">The date of the incident the claim relates to if known, otherwise null.</param>
        public ClaimData(
            decimal? amount,
            string description,
            Instant? incidentTimestamp,
            DateTimeZone timeZone)
        {
            timeZone.Should().NotBeNull();
            this.Amount = amount;
            this.Description = description;
            this.IncidentTimestamp = incidentTimestamp;
            this.TimeZone = timeZone;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimData"/> class.
        /// </summary>
        public ClaimData()
        {
            // set the default time zone.
            this.TimeZone = Timezones.AET;
        }

        /// <summary>
        /// Gets or sets the amount the claim is for if known, otherwise null.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the description of the claim if known, otherwise null.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date of the incident the claim relates to if known, otherwise null.
        /// </summary>
        public Instant? IncidentTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the timezone which the incident timestamp pertains to.
        /// </summary>
        public DateTimeZone TimeZone { get; set; }

        /// <summary>
        /// Clone the claim data.
        /// </summary>
        /// <returns>A new instance of <see cref="ClaimData"/> with data copied from this instance.</returns>
        public ClaimData Clone()
        {
            return new ClaimData(this.Amount, this.Description, this.IncidentTimestamp, this.TimeZone);
        }
    }
}
