// <copyright file="ReportDeletedEvent.cs" company="uBind">
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
    /// Aggregate for report.
    /// </summary>
    public partial class ReportAggregate
    {
        /// <summary>
        /// The report has been deleted.
        /// </summary>
        public class ReportDeletedEvent : Event<ReportAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReportDeletedEvent"/> class.
            /// </summary>
            /// <param name="reportId">The id of the report.</param>
            /// <param name="performingUserId">The userId who deletes the report.</param>
            /// <param name="timestamp">The timestamp.</param>
            public ReportDeletedEvent(Guid tenantId, Guid reportId, Guid? performingUserId, Instant timestamp)
                : base(tenantId, reportId, performingUserId, timestamp)
            {
                this.IsDeleted = true;
            }

            [JsonConstructor]
            private ReportDeletedEvent()
            {
            }

            /// <summary>
            /// Gets or sets a value indicating whether the report is deleted.
            /// </summary>
            public bool IsDeleted { get; set; }
        }
    }
}
