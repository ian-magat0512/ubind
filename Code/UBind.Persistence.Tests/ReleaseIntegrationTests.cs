// <copyright file="ReleaseIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ReleaseIntegrationTests
    {
        [Fact]
        public void Insert_DoesNotThrow_ForUniqueProductIdAndNumber()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var majorNumber = 0;
            var minorNumber = 0;
            var now = SystemClock.Instance.GetCurrentInstant();
            var release = new Release(tenantId, productId, majorNumber, minorNumber, "Test", ReleaseType.Major, now);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var repo = new ReleaseRepository(dbContext);
                repo.Insert(release);
                repo.SaveChanges();
            }

            // Act
            var releaseWithDuplicateNumber =
                new Release(tenantId, productId, majorNumber + 1, minorNumber, "Non-Dupe", ReleaseType.Major, now);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var repo = new ReleaseRepository(dbContext);
                repo.Insert(releaseWithDuplicateNumber);
                repo.SaveChanges();
            }

            // Assert
            // No exception indicates success.
        }

        [Fact]
        public void Insert_Allows_GoodFormat_Check_Database()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var majorNumber = 19;
            var minorNumber = 20;
            var now = SystemClock.Instance.GetCurrentInstant();
            var release = new Release(
                tenantId,
                productId,
                majorNumber,
                minorNumber,
                "Test",
                ReleaseType.Major,
                now.Plus(Duration.FromMinutes(1)));

            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var repo = new ReleaseRepository(dbContext);
                repo.Insert(release);
                repo.SaveChanges();
            }

            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var repo = new ReleaseRepository(dbContext);
                var numbers = repo.GetHighestReleaseNumberForProduct(
                    tenantId, productId);
                var rel = repo
                    .GetReleasesForProduct(
                        tenantId, productId, new EntityListFilters())
                    .FirstOrDefault();
                Assert.Equal(majorNumber, rel.Number);
                Assert.Equal(minorNumber, rel.MinorNumber);
            }

            // Assert
            // No exception indicates success.
        }

        [Fact]
        public void Insert_ThrowsDuplicateReleaseNumberException_ForNonUniqueProductIdAndNumber()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var majorNumber = 0;
            var minorNumber = 0;
            var now = SystemClock.Instance.GetCurrentInstant();
            var release = new Release(tenantId, productId, majorNumber, minorNumber, "Test", ReleaseType.Major, now);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var repo = new ReleaseRepository(dbContext);
                repo.Insert(release);
                repo.SaveChanges();
            }

            var releaseWithDuplicateNumber =
                new Release(tenantId, productId, majorNumber, minorNumber, "Dupe", ReleaseType.Major, now);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var repo = new ReleaseRepository(dbContext);

                // Act
                repo.Insert(releaseWithDuplicateNumber);
                Action act = () => repo.SaveChanges();

                // Assert
                act.Should().Throw<DuplicateReleaseNumberException>();
            }
        }

        [Fact]
        public void GetReleaseById_RecordsHasNewIds_WhenCreatingRecord()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var majorReleaseNumber = 0;
            var minorReleaseNumber = 1;
            var now = SystemClock.Instance.GetCurrentInstant();
            var newRelease = new Release(tenantId, productId, majorReleaseNumber, minorReleaseNumber, "Test", ReleaseType.Major, now);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var repo = new ReleaseRepository(dbContext);
                repo.Insert(newRelease);
                repo.SaveChanges();

                // Act
                var initialiazedRelease = repo.GetReleaseByIdWithFileContents(tenantId, newRelease.Id);

                // Assert
                initialiazedRelease.Should().NotBeNull();
                initialiazedRelease.TenantId.Should().Be(tenantId);
                initialiazedRelease.ProductId.Should().Be(productId);
            }
        }
    }
}
