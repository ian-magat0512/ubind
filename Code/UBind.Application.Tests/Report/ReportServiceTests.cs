// <copyright file="ReportServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Report
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Extensions;
    using UBind.Domain.Report;
    using Xunit;

    public class ReportServiceTests
    {
        [Fact]
        public void ReportAggregateGenerated_ShouldSaveTheSameDataSource_FromReportAddModel()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            Guid? performingUserId = Guid.NewGuid();
            ICollection<Guid> productIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

            var report = new ReportAddModel
            {
                Name = "Report Name",
                Description = "Report Description",
                ProductIds = productIds,
                SourceData = "New Business,Renewal,Adjustment,Cancellation,Quote,System Email,Product Email,Claim",
                MimeType = "text/plain",
                Filename = "filename.txt",
                Body = "Body",
            };

            // Act
            var reportAggregate = ReportAggregate.CreateReport(tenantId, default, report, performingUserId, SystemClock.Instance.Now());

            // Assert
            reportAggregate.UnsavedEvents.Should().NotBeNullOrEmpty();
            reportAggregate.UnsavedEvents.Should().OnlyHaveUniqueItems();
            report.SourceData.Should().Be(reportAggregate.SourceData);
        }
    }
}
