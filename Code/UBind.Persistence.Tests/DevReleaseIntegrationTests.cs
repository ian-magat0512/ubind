// <copyright file="DevReleaseIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Repositories;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class DevReleaseIntegrationTests
    {
        private readonly IUBindDbContext dbContext;
        private readonly DevReleaseRepository devReleaseRepository;
        private readonly FileContentRepository fileContentRepository;
        private Guid tenantId = Guid.NewGuid();
        private Guid productId = Guid.NewGuid();

        public DevReleaseIntegrationTests()
        {
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            this.fileContentRepository = new FileContentRepository(this.dbContext);
            this.devReleaseRepository = new DevReleaseRepository(this.dbContext, this.fileContentRepository);
        }

        [Fact]
        public void GetDevReleaseById_IncludeDetails_WhenReleaseIsInitialized()
        {
            // Arrange
            var devRelease = new DevRelease(this.tenantId, this.productId, SystemClock.Instance.GetCurrentInstant());
            this.devReleaseRepository.Insert(devRelease);
            this.devReleaseRepository.SaveChanges();
            var releaseDetails = new ReleaseDetails(
                WebFormAppType.Quote,
                "{ \"formConfiguration\": \"json\" }",
                "{ \"workflow\": \"json\" }",
                "{ \"exports\": \"json\" }",
                "{ \"automations\": \"json\" }",
                "{ \"outbound-email-servers\": \"json\" }",
                "{ \"payment\": \"json\" }",
                "{ \"funding\": \"json\" }",
                "{ \"product\": \"json\" }",
                Enumerable.Empty<Asset>(),
                Enumerable.Empty<Asset>(),
                new byte[] { 00, 00, 00, 00 },
                SystemClock.Instance.GetCurrentInstant());
            devRelease.QuoteDetails = releaseDetails;
            this.devReleaseRepository.SaveChanges();

            // Act
            DevRelease devReleaseFromRepo = this.devReleaseRepository.GetDevReleaseByIdWithFileContents(this.tenantId, devRelease.Id);

            // Assert
            Assert.NotNull(devReleaseFromRepo.QuoteDetails);
        }

        /// <summary>
        /// This test is here to help debug performance of dev release loading.
        /// It is primarily used to easily run code and inspect data etc.
        /// rather than being especially useful as part of an automated test run.
        /// </summary>
        [Fact(Skip = "Only used for debugging.")]
        public void GetLatestDevReleaseForProductIncludeAllProperties_OnlyLoadsSingleReleasesData()
        {
            // Arrange
            for (var i = 0; i < 100; ++i)
            {
                this.SeedDevRelease();
            }

            // Act
            DevRelease? devRelease = null;
            long elapsedTimeMs = 0;
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var fileContentRepository = new FileContentRepository(dbContext);
                var devReleaseRepository = new DevReleaseRepository(dbContext, fileContentRepository);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                devRelease = devReleaseRepository.GetDevReleaseForProductWithoutAssetFileContents(
                    TenantFactory.DefaultId,
                    ProductFactory.DefaultId);
                stopwatch.Stop();
                elapsedTimeMs = stopwatch.ElapsedMilliseconds;
            }

            // Assert
            Assert.True(elapsedTimeMs < 1000);
        }

        [Fact]
        public void GetDevReleaseById_RecordsHasNewIds_WhenCreatingNewInstances()
        {
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                // Arrange
                var now = SystemClock.Instance.GetCurrentInstant();
                var fileContentRepository = new FileContentRepository(dbContext);
                var devReleaseRepository = new DevReleaseRepository(dbContext, fileContentRepository);
                var devRelease = new DevRelease(this.tenantId, this.productId, now);
                devReleaseRepository.Insert(devRelease);
                devReleaseRepository.SaveChanges();

                // Act
                DevRelease retrievedDevRelease = devReleaseRepository.GetDevReleaseByIdWithFileContents(this.tenantId, devRelease.Id);

                // Assert
                retrievedDevRelease.Should().NotBeNull();
                retrievedDevRelease.TenantId.Should().Be(this.tenantId);
                retrievedDevRelease.ProductId.Should().Be(this.productId);
            }
        }

        private void SeedDevRelease()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var fileContentRepository = new FileContentRepository(dbContext);
                var devReleaseRepository = new DevReleaseRepository(dbContext, fileContentRepository);
                var devRelease = new DevRelease(this.tenantId, this.productId, now);
                var fileContent = FileContent.CreateFromBytes(this.tenantId, Guid.NewGuid(), Encoding.ASCII.GetBytes(new string('x', 100000)));
                var releaseDetails = new ReleaseDetails(
                    WebFormAppType.Quote,
                    "{ \"formConfiguration\": \"json\" }",
                    "{ \"workflow\": \"json\" }",
                    "{ \"exports\": \"json\" }",
                    "{ \"automations\": \"json\" }",
                    "{ \"outbound-email-servers\": \"json\" }",
                    "{ \"payment\": \"json\" }",
                    "{ \"funding\": \"json\" }",
                    "{ \"product\": \"json\" }",
                    new List<Asset> { new Asset(this.tenantId, "foo.txt", now, fileContent, now) },
                    new List<Asset> { new Asset(this.tenantId, "dummy.txt", now, fileContent, now) },
                    new byte[] { 00, 00, 00, 00 },
                    now);
                this.devReleaseRepository.Insert(devRelease);
                this.devReleaseRepository.SaveChanges();
            }
        }
    }
}
