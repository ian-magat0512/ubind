// <copyright file="ClaimAggregateIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Claim
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Integration tests for claims aggregate.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class ClaimAggregateIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        /// <summary>
        /// Unit test to confirm claim versions are persisted.
        /// </summary>
        /// <returns>None.</returns>
        [Fact]
        public async Task ClaimVersionIsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var connectionConfig = new ConnectionStrings();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            connectionConfig.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quote = QuoteFactory.CreateNewPolicy(tenantId, productId, DeploymentEnvironment.Staging);
            var claimAggregate = ClaimAggregate.CreateForPolicy(
                "TEST123",
                quote,
                Guid.NewGuid(),
                "John Doe",
                "John",
                this.performingUserId,
                SystemClock.Instance.Now());
            claimAggregate.CreateVersion(this.performingUserId, SystemClock.Instance.Now());
            await stack.ClaimAggregateRepository.Save(claimAggregate);

            using (var newContext = new UBindDbContext(stack.DbContext.Database.Connection.ConnectionString))
            {
                var eventRecordRepository = new EventRecordRepository(newContext, connectionConfig);
                var repo = new ClaimAggregateRepository(
                    newContext,
                    eventRecordRepository,
                    new Mock<IClaimEventObserver>().Object,
                    new Mock<IAggregateSnapshotService<ClaimAggregate>>().Object,
                    SystemClock.Instance,
                    NullLogger<ClaimAggregateRepository>.Instance,
                    new Mock<IServiceProvider>().AddLoggers().Object);

                // Act
                var retrievedClaim = repo.GetById(tenantId, claimAggregate.Id);

                // Assert
                Assert.Equal(1, retrievedClaim.Claim.VersionNumber);
            }
        }
    }
}
