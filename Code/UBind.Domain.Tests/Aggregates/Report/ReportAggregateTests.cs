// <copyright file="ReportAggregateTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Report
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Report;
    using Xunit;

    public class ReportAggregateTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public void Initialization_ShouldUpdatesPropertiesCorrectly()
        {
            // Arrange
            var productIds = new List<Guid>();
            productIds.Add(Guid.NewGuid());
            productIds.Add(Guid.NewGuid());

            var report = new ReportAddModel
            {
                Name = "Report Name",
                Description = "Report Description",
                ProductIds = productIds,
                SourceData = "Source1, Source2",
                MimeType = "text/plain",
                Filename = "filename.txt",
                Body = "Body",
            };

            // Act
            var currentInstant = this.clock.GetCurrentInstant();
            var reportAggregate = ReportAggregate.CreateReport(Guid.NewGuid(), default, report, this.performingUserId, currentInstant);

            // Assert
            reportAggregate.Name.Should().Be(report.Name);
            reportAggregate.Description.Should().Be(report.Description);
            reportAggregate.ProductIds.OrderBy(id => id).SequenceEqual(report.ProductIds.OrderBy(id => id))
                .Should().BeTrue();
            reportAggregate.SourceData.Should().Be(report.SourceData);
            reportAggregate.MimeType.Should().Be(report.MimeType);
            reportAggregate.Filename.Should().Be(report.Filename);
            reportAggregate.Body.Should().Be(report.Body);
        }

        [Fact]
        public void Update_ShouldUpdatesPropertiesCorrectly()
        {
            // Arrange
            var productIds = new List<Guid>();
            productIds.Add(Guid.NewGuid());
            productIds.Add(Guid.NewGuid());
            Guid tenantId = Guid.NewGuid();
            var initReport = new ReportAddModel
            {
                Name = "Init Report Name",
                Description = "Init Report Description",
                ProductIds = productIds,
                SourceData = "Source1, Source2",
                MimeType = "text/plain",
                Filename = "initfilename.txt",
                Body = "InitBody",
            };

            var updatedReport = new ReportAddModel
            {
                Name = "Report Name",
                Description = "Report Description",
                ProductIds = productIds,
                SourceData = "Source1, Source2",
                MimeType = "text/plain",
                Filename = "filename.txt",
                Body = "Body",
            };

            var currentInstant = this.clock.GetCurrentInstant();
            var reportAggregate = ReportAggregate.CreateReport(tenantId, default, initReport, this.performingUserId, currentInstant);

            // Act
            reportAggregate.Update(tenantId, updatedReport, this.performingUserId, currentInstant);

            // Assert
            reportAggregate.Name.Should().BeSameAs(updatedReport.Name);
            reportAggregate.Description.Should().BeSameAs(updatedReport.Description);
            reportAggregate.ProductIds.OrderBy(id => id).SequenceEqual(updatedReport.ProductIds.OrderBy(id => id))
                .Should().BeTrue();
            reportAggregate.SourceData.Should().BeSameAs(updatedReport.SourceData);
            reportAggregate.MimeType.Should().BeSameAs(updatedReport.MimeType);
            reportAggregate.Filename.Should().BeSameAs(updatedReport.Filename);
            reportAggregate.Body.Should().BeSameAs(updatedReport.Body);
        }

        [Fact]
        public void Delete_SetsIsDeletedToTrue()
        {
            // Arrange
            var productIds = new List<Guid>();
            productIds.Add(Guid.NewGuid());
            productIds.Add(Guid.NewGuid());
            Guid tenantId = Guid.NewGuid();

            var report = new ReportAddModel
            {
                Name = "Report Name",
                Description = "Report Description",
                ProductIds = productIds,
                SourceData = "Source1, Source2",
                MimeType = "text/plain",
                Filename = "filename.txt",
                Body = "Body",
            };

            var currentInstant = this.clock.GetCurrentInstant();
            var reportAggregate = ReportAggregate.CreateReport(tenantId, default, report, this.performingUserId, currentInstant);

            // Act
            reportAggregate.Update(tenantId, report, this.performingUserId, currentInstant);
            reportAggregate.DeleteReport(this.performingUserId, currentInstant);

            // Assert
            Assert.True(reportAggregate.IsDeleted);
        }

        [Fact]
        public void AddGeneratedReportFile_UpdateReportFileIds()
        {
            // Arrange
            var reportFileId = Guid.NewGuid();
            var productIds = new List<Guid>();
            productIds.Add(Guid.NewGuid());
            productIds.Add(Guid.NewGuid());
            Guid tenantId = Guid.NewGuid();

            var report = new ReportAddModel
            {
                Name = "Report Name",
                Description = "Report Description",
                ProductIds = productIds,
                SourceData = "Source1, Source2",
                MimeType = "text/plain",
                Filename = "filename.txt",
                Body = "Body",
            };

            var currentInstant = this.clock.GetCurrentInstant();
            var reportAggregate = ReportAggregate.CreateReport(tenantId, default, report, this.performingUserId, currentInstant);

            // Act
            reportAggregate.AddGeneratedReportFile(reportFileId, this.performingUserId, currentInstant);

            // Assert
            Assert.True(reportAggregate.ReportFileIds.Contains(reportFileId));
        }
    }
}
