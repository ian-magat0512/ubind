// <copyright file="ReportInitializedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Report
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for Report.
    /// </summary>
    public partial class ReportAggregate
    {
        /// <summary>
        /// The report has been initialized.
        /// </summary>
        public class ReportInitializedEvent : Event<ReportAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReportInitializedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The id of the tenant the report belongs to.</param>
            /// <param name="reportId">The id of the aggregate.</param>
            /// <param name="performingUserId">The userId who initialized the report.</param>
            /// <param name="organisationId">The ID of the organisation the report belongs to.</param>
            /// <param name="timestamp">The time event was created.</param>
            public ReportInitializedEvent(
                Guid tenantId,
                Guid reportId,
                Guid organisationId,
                Guid? performingUserId,
                Instant timestamp)
                : base(tenantId, reportId, performingUserId, timestamp)
            {
                this.OrganisationId = organisationId;
            }

            [JsonConstructor]
            private ReportInitializedEvent()
               : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the Reports's organisation.
            /// </summary>
            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
