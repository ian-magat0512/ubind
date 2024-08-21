// <copyright file="DeploymentRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="DeploymentRepositoryTests"/>.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class DeploymentRepositoryTests
    {
        private Mock<DbSet<Release>> mockReleasesSet = new Mock<DbSet<Release>>();

        private Mock<DbSet<Deployment>> mockDeploymentsSet = new Mock<DbSet<Deployment>>();
        private List<Release> modelReleases;
        private List<Deployment> modelDeployments;

        public DeploymentRepositoryTests()
        {
            this.modelReleases = new List<Release>();

            var currentTimeStamp = Instant.FromDateTimeUtc(DateTime.SpecifyKind(new DateTime(2020, 5, 1), DateTimeKind.Utc));
            var moreThanOneMonthAgoTimeStamp = Instant.FromDateTimeUtc(DateTime.SpecifyKind(new DateTime(2020, 3, 30), DateTimeKind.Utc));
            var moreThanTwoMonthsAgoTimeStamp = Instant.FromDateTimeUtc(DateTime.SpecifyKind(new DateTime(2020, 2, 27), DateTimeKind.Utc));
            this.modelReleases.Add(new Release(TenantFactory.DefaultId, ProductFactory.DefaultId, 1002, 1002, "testRelease3", ReleaseType.Major, currentTimeStamp));
            this.modelReleases.Add(new Release(TenantFactory.DefaultId, ProductFactory.DefaultId, 1001, 1001, "testRelease2", ReleaseType.Major, moreThanOneMonthAgoTimeStamp));
            this.modelReleases.Add(new Release(TenantFactory.DefaultId, ProductFactory.DefaultId, 1000, 1000, "testRelease1", ReleaseType.Major, moreThanTwoMonthsAgoTimeStamp));

            this.modelDeployments = new List<Deployment>
            {
                new Deployment(TenantFactory.DefaultId, ProductFactory.DefaultId,  DeploymentEnvironment.Production, this.modelReleases[0], currentTimeStamp),
                new Deployment(TenantFactory.DefaultId, ProductFactory.DefaultId,  DeploymentEnvironment.Production,  this.modelReleases[1], moreThanOneMonthAgoTimeStamp),
                new Deployment(TenantFactory.DefaultId, ProductFactory.DefaultId, DeploymentEnvironment.Production,  this.modelReleases[2], moreThanTwoMonthsAgoTimeStamp),
            };

            this.mockReleasesSet.As<IQueryable<Release>>().Setup(m => m.Provider).Returns(this.modelReleases.AsQueryable().Provider);
            this.mockReleasesSet.As<IQueryable<Release>>().Setup(m => m.Expression).Returns(this.modelReleases.AsQueryable().Expression);
            this.mockReleasesSet.As<IQueryable<Release>>().Setup(m => m.ElementType).Returns(this.modelReleases.AsQueryable().ElementType);
            this.mockReleasesSet.As<IQueryable<Release>>().Setup(m => m.GetEnumerator()).Returns(this.modelReleases.AsQueryable().GetEnumerator());

            this.mockDeploymentsSet.As<IQueryable<Deployment>>().Setup(m => m.Provider).Returns(this.modelDeployments.AsQueryable().Provider);
            this.mockDeploymentsSet.As<IQueryable<Deployment>>().Setup(m => m.Expression).Returns(this.modelDeployments.AsQueryable().Expression);
            this.mockDeploymentsSet.As<IQueryable<Deployment>>().Setup(m => m.ElementType).Returns(this.modelDeployments.AsQueryable().ElementType);
            this.mockDeploymentsSet.As<IQueryable<Deployment>>().Setup(m => m.GetEnumerator()).Returns(this.modelDeployments.AsQueryable().GetEnumerator());
        }

        [Fact]
        public void PurgeDeployments_DeletesOnlyOldRecords_WhenPageRetentionIsSixty()
        {
            // Arrange
            var mockContext = new Mock<UBindDbContext>();
            mockContext.Setup(c => c.Releases).Returns(this.mockReleasesSet.Object);
            mockContext.Setup(c => c.Deployments).Returns(this.mockDeploymentsSet.Object);

            // Act
            var sut = new DeploymentRepository(mockContext.Object);
            var deletedDeployments = sut.PurgeDeployments(
                TenantFactory.DefaultId,
                60,
                ProductFactory.DefaultId);

            // Assert
            Assert.Equal(this.modelReleases[2].MinorNumber, deletedDeployments.ToList()[0].Release.MinorNumber);
            Assert.Equal(this.modelDeployments[2].Id, deletedDeployments.ToList()[0].Id);
            Assert.Single(deletedDeployments);
        }

        [Fact]
        public void PurgeDeployments_ShouldThrowAnException_WhenPageRetentionLessThanThirty()
        {
            // Arrange
            var mockContext = new Mock<UBindDbContext>();
            mockContext.Setup(c => c.Releases).Returns(this.mockReleasesSet.Object);
            mockContext.Setup(c => c.Deployments).Returns(this.mockDeploymentsSet.Object);

            // Act
            var sut = new DeploymentRepository(mockContext.Object);
            Func<IEnumerable<Deployment>> act = () => sut.PurgeDeployments(
                TenantFactory.DefaultId,
                29,
                ProductFactory.DefaultId);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
