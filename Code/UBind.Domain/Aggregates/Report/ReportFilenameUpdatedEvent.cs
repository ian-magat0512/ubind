// <copyright file="ReportFilenameUpdatedEvent.cs" company="uBind">
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
        /// Report filename has been updated.
        /// </summary>
        public class ReportFilenameUpdatedEvent : Event<ReportAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReportFilenameUpdatedEvent"/> class.
            /// </summary>
            /// <param name="reportId">The id of the report.</param>
            /// <param name="filename">The filename of the report.</param>
            /// <param name="performingUserId">The user authentication data.</param>
            /// <param name="timestamp">The timestamp.</param>
            public ReportFilenameUpdatedEvent(Guid tenantId, Guid reportId, string filename, Guid? performingUserId, Instant timestamp)
                : base(tenantId, reportId, performingUserId, timestamp)
            {
                this.Filename = filename;
            }

            [JsonConstructor]
            private ReportFilenameUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the filename of the report.
            /// </summary>
            [JsonProperty]
            public string Filename { get; private set; }
        }
    }
}
