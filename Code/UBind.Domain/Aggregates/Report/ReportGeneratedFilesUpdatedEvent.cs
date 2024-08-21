// <copyright file="ReportGeneratedFilesUpdatedEvent.cs" company="uBind">
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
        /// The report file has been added.
        /// </summary>
        public class ReportGeneratedFileAddedEvent : Event<ReportAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReportGeneratedFileAddedEvent"/> class.
            /// </summary>
            /// <param name="reportId">The id of the report.</param>
            /// <param name="reportFileId">The report file id.</param>
            /// <param name="performingUserId">The userId who generated the report file.</param>
            /// <param name="timestamp"> the timestamp.</param>
            public ReportGeneratedFileAddedEvent(Guid tenantId, Guid reportId, Guid reportFileId, Guid? performingUserId, Instant timestamp)
                 : base(tenantId, reportId, performingUserId, timestamp)
            {
                this.ReportFileId = reportFileId;
            }

            [JsonConstructor]
            private ReportGeneratedFileAddedEvent()
            {
            }

            /// <summary>
            /// Gets or sets filename of an ReportFile.
            /// </summary>
            [JsonProperty]
            public Guid ReportFileId { get; set; }
        }
    }
}
