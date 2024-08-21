// <copyright file="ReportReadModelRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Report;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ReportReadModelRepositoryIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;
        private Guid tenantId;
        private Guid productId;
        private Guid productId2;

        public ReportReadModelRepositoryIntegrationTests()
        {
            this.productId = Guid.NewGuid();
            this.productId2 = Guid.NewGuid();
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public async Task GetReportsByTenantId_ShouldHaveEntries_AfterReportAggregateSave()
        {
            this.CreateTenantAndProduct(this.tenantId, this.productId, this.productId2);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = stack.TenantRepository.GetTenantById(this.tenantId);

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
                var retrievedReport = stack.ReportReadModelRepository.GetReportsAndDetailsByTenantIdAndOrganisationId(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    new EntityListFilters()
                    {
                        SortBy = nameof(ReportReadModel.CreatedTicksSinceEpoch),
                        SortOrder = Domain.Enums.SortDirection.Descending,
                    });

                // Assert
                Assert.True(retrievedReport.Any());
            }
        }

        [Fact]
        public async Task SingleOrDefaultIncludeAllProperties_ShouldIncludeProductsDetails()
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
                var retrievedReport = stack.ReportReadModelRepository.SingleOrDefaultIncludeAllProperties(reportAggregate.TenantId, reportAggregate.Id);

                // Assert
                Assert.True(retrievedReport.Products.Any());
            }
        }

        private void CreateTenantAndProduct(
            Guid tenantId,
            Guid productId,
            Guid productId2)
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
