// <copyright file="GenerateReportCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Report
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for generating report.
    /// </summary>
    public class GenerateReportCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateReportCommand"/> class.
        /// </summary>
        public GenerateReportCommand(Guid tenantId, Guid reportId, DeploymentEnvironment environment, Instant fromTimestamp, Instant toTimestamp, string timeZone, bool includeTestData)
        {
            this.TenantId = tenantId;
            this.ReportId = reportId;
            this.Environment = environment;
            this.From = fromTimestamp;
            this.To = toTimestamp;
            this.TimeZoneId = timeZone;
            this.IncludeTestData = includeTestData;
        }

        public Instant From { get; set; }

        public Instant To { get; set; }

        public bool IncludeTestData { get; set; }

        public DeploymentEnvironment Environment { get; set; }

        public string TimeZoneId { get; set; }

        public Guid TenantId { get; set; }

        public Guid ReportId { get; set; }
    }
}
