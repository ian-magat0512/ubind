// <copyright file="ReportOrganisationMigratedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Report
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for reports.
    /// </summary>
    public partial class ReportAggregate
    {
        /// <summary>
        /// Event raised when a report has been modified due to the added organisation ID property.
        /// </summary>
        public class ReportOrganisationMigratedEvent
            : Event<ReportAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReportOrganisationMigratedEvent"/> class.
            /// </summary>
            /// <param name="organisationId">The ID of the organisation the quote belongs to.</param>
            /// <param name="reportId">The ID of the report.</param>
            /// <param name="performingUserId">The userId who performed the migration for the quote.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ReportOrganisationMigratedEvent(
                Guid tenantId, Guid organisationId, Guid reportId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, reportId, performingUserId, createdTimestamp)
            {
                this.OrganisationId = organisationId;
            }

            [JsonConstructor]
            private ReportOrganisationMigratedEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the organisation the report is for.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
