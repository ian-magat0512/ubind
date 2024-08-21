// <copyright file="ReportAggregateIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Report
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Extensions;
    using UBind.Domain.Report;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ReportAggregateIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;
        private Guid tenantId;
        private Guid productId;
        private Guid productId2;

        public ReportAggregateIntegrationTests()
        {
            this.productId = Guid.NewGuid();
            this.productId2 = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public async Task Save_ReportAggregateIsPersisted()
        {
            this.CreateTenantAndProduct(this.tenantId, this.productId, this.productId2);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                List<Guid> productIds = stack.ProductRepository.GetActiveProducts()
                     .Where(p => p.TenantId == this.tenantId)
                     .Select(p => p.Id).ToList();

                var report = new ReportAddModel
                {
                    Name = "Report Name",
                    Description = "Report Description",
                    ProductIds = productIds,
                    SourceData = "New Business, Renewal",
                    MimeType = "text/plain",
                    Filename = "filename.txt",
                    Body = "Body",
                };

                var reportAggregate = ReportAggregate.CreateReport(this.tenantId, default, report, this.performingUserId, this.clock.Now());
                await stack.ReportAggregateRepository.Save(reportAggregate);

                // Act
                var retrievedReport = stack.ReportAggregateRepository.GetById(this.tenantId, reportAggregate.Id);

                // Assert
                retrievedReport.Name.Should().Be(report.Name);
                retrievedReport.Description.Should().Be(report.Description);
                retrievedReport.SourceData.Should().Be(report.SourceData);
                retrievedReport.MimeType.Should().Be(report.MimeType);
                retrievedReport.Filename.Should().Be(report.Filename);
                retrievedReport.Body.Should().Be(report.Body);
            }
        }

        [Fact]
        public async Task Save_ReportAggregateIsDeletedIsPersisted_WhenDeleteEventIsTriggered()
        {
            this.CreateTenantAndProduct(this.tenantId, this.productId, this.productId2);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                List<Guid> productIds = stack.ProductRepository.GetActiveProducts()
                     .Where(p => p.TenantId == this.tenantId)
                     .Select(p => p.Id).ToList();

                var report = new ReportAddModel
                {
                    Name = "Report Name",
                    Description = "Report Description",
                    ProductIds = productIds,
                    SourceData = "New Business, Renewal",
                    MimeType = "text/plain",
                    Filename = "filename.txt",
                    Body = "Body",
                };

                var reportAggregate = ReportAggregate.CreateReport(this.tenantId, default, report, this.performingUserId, this.clock.Now());
                reportAggregate.DeleteReport(this.performingUserId, this.clock.Now());
                await stack.ReportAggregateRepository.Save(reportAggregate);

                // Act
                var retrievedReport = stack.ReportAggregateRepository.GetById(this.tenantId, reportAggregate.Id);

                // Assert
                retrievedReport.IsDeleted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Save_ReportAggregate_SourceDataSynched_Between_RetrievedReport_And_ReportReadModel()
        {
            this.CreateTenantAndProduct(this.tenantId, this.productId, this.productId2);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                List<Guid> productIds = stack.ProductRepository.GetActiveProducts()
                     .Where(p => p.TenantId == this.tenantId)
                     .Select(p => p.Id).ToList();

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

                var reportAggregate = ReportAggregate.CreateReport(this.tenantId, default, report, this.performingUserId, this.clock.Now());
                await stack.ReportAggregateRepository.Save(reportAggregate);

                // Act
                var retrievedReport = stack.ReportAggregateRepository.GetById(this.tenantId, reportAggregate.Id);
                var reportDetails = stack.ReportReadModelRepository.GetDetails(this.tenantId, retrievedReport.Id);

                // Assert
                retrievedReport.SourceData.Should().Be(reportDetails.Report.SourceData);
            }
        }

        [Fact]
        public async Task RecordOrganisationMigration_ReportAggregate_ShouldHaveOrganisation()
        {
            this.CreateTenantAndProduct(this.tenantId, this.productId, this.productId2);
            ReportAggregate reportAggregate;
            Guid newOrganisationId;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack1.TenantRepository.GetTenantById(this.tenantId);
                var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    this.clock.GetCurrentInstant());
                newOrganisationId = organisation.Id;
                var currentInstant = this.clock.GetCurrentInstant();
                tenant.SetDefaultOrganisation(newOrganisationId, currentInstant.Plus(Duration.FromMinutes(1)));
                await stack1.OrganisationAggregateRepository.Save(organisation);
                stack1.TenantRepository.SaveChanges();

                List<Guid> productIds = stack1.ProductRepository.GetActiveProducts()
                     .Where(p => p.TenantId == this.tenantId)
                     .Select(p => p.Id).ToList();

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

                var now = this.clock.Now();
                reportAggregate = ReportAggregate.CreateReport(this.tenantId, default, report, this.performingUserId, now);
                reportAggregate.RecordOrganisationMigration(newOrganisationId, this.performingUserId, now);
                await stack1.ReportAggregateRepository.Save(reportAggregate);
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var retrievedReport = stack2.ReportAggregateRepository.GetById(this.tenantId, reportAggregate.Id);

                // Assert
                retrievedReport.OrganisationId.Should().NotBeEmpty();
                retrievedReport.OrganisationId.Should().Be(newOrganisationId);
            }
        }

        private void CreateTenantAndProduct(Guid tenantId, Guid productId, Guid productId2)
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(tenantId);
                var product1 = ProductFactory.Create(tenantId, productId);
                var product2 = ProductFactory.Create(tenantId, productId2);

                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.Insert(product1);
                stack.ProductRepository.Insert(product2);
                stack.DbContext.SaveChanges();
            }
        }
    }
}
