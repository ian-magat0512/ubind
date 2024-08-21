// <copyright file="ClaimIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ClaimIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public async Task IncidentDate_IsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               quoteAggregate,
               Guid.NewGuid(),
               "John Smith",
               "Jonboy",
               this.performingUserId,
               SystemClock.Instance.Now());
            await stack.ClaimAggregateRepository.Save(claimAggregate);

            // Assert
            var claimReadModel = stack.ClaimReadModelUpdateRepository.GetById(claimAggregate.TenantId, claimAggregate.Id);

            // Assert
            Assert.Equal("AAAAAA", claimReadModel.ClaimReference);
        }

        [Fact]
        public async Task ClaimReference_IsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               quoteAggregate,
               Guid.NewGuid(),
               "John Smith",
               "Jonboy",
               this.performingUserId,
               SystemClock.Instance.Now());
            await stack.ClaimAggregateRepository.Save(claimAggregate);

            // Assert
            var claimReadModel = stack.ClaimReadModelUpdateRepository.GetById(claimAggregate.TenantId, claimAggregate.Id);

            // Assert
            Assert.Equal("AAAAAA", claimReadModel.ClaimReference);
        }

        [Fact]
        public async Task Status_IsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               quoteAggregate,
               Guid.NewGuid(),
               "John Smith",
               "Jonboy",
               this.performingUserId,
               SystemClock.Instance.Now());
            var workflow = await new DefaultClaimWorkflowProvider().GetConfigurableClaimWorkflow(It.IsAny<ReleaseContext>());
            claimAggregate.ChangeClaimState(ClaimActions.Notify, this.performingUserId, SystemClock.Instance.Now(), workflow);
            await stack.ClaimAggregateRepository.Save(claimAggregate);

            // Assert
            var claimReadModel = stack.ClaimReadModelUpdateRepository.GetById(claimAggregate.TenantId, claimAggregate.Id);

            // Assert
            Assert.Equal("Notified", claimReadModel.Status);
        }

        [Fact]
        public async Task ChangeState_WhenClaim_ExpectsIncomplete_IsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var userId = Guid.NewGuid();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               quoteAggregate,
               userId,
               "John Smith",
               "Jonboy",
               this.performingUserId,
               SystemClock.Instance.Now());
            var workflow = await new DefaultClaimWorkflowProvider().GetConfigurableClaimWorkflow(It.IsAny<ReleaseContext>());
            claimAggregate.ChangeClaimState(ClaimActions.Actualise, userId, SystemClock.Instance.Now(), workflow);
            await stack.ClaimAggregateRepository.Save(claimAggregate);

            // Assert
            var claimReadModel = stack.ClaimReadModelUpdateRepository.GetById(claimAggregate.TenantId, claimAggregate.Id);

            // Assert
            Assert.Equal("Incomplete", claimReadModel.Status);
            Assert.Equal("Incomplete", claimAggregate.Claim.ClaimStatus);
        }

        [Fact]
        public async Task ChangeState_WhenApproval_ExpectsApproved_IsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var userId = Guid.NewGuid();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               quoteAggregate,
               userId,
               "John Smith",
               "Jonboy",
               this.performingUserId,
               SystemClock.Instance.Now());
            claimAggregate.RecordCalculationResult(FormDataJsonFactory.Sample, CalculationResultJsonFactory.Create(), this.performingUserId, SystemClock.Instance.Now());
            var workflow = await new DefaultClaimWorkflowProvider().GetConfigurableClaimWorkflow(It.IsAny<ReleaseContext>());
            claimAggregate.ChangeClaimState(ClaimActions.AutoApproval, userId, SystemClock.Instance.Now(), workflow);

            await stack.ClaimAggregateRepository.Save(claimAggregate);

            // Assert
            var claimReadModel = stack.ClaimReadModelUpdateRepository.GetById(claimAggregate.TenantId, claimAggregate.Id);

            // Assert
            Assert.Equal(ClaimState.Approved, claimReadModel.Status);
            Assert.Equal(ClaimState.Approved, claimAggregate.Claim.ClaimStatus);
        }

        [Fact]
        public async Task ChangeState_WhenAcknowledge_ExpectsAcknowledged_IsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var userId = Guid.NewGuid();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               quoteAggregate,
               userId,
               "John Smith",
               "Jonboy",
               this.performingUserId,
               SystemClock.Instance.Now());
            claimAggregate.RecordCalculationResult(FormDataJsonFactory.Sample, CalculationResultJsonFactory.Create(), this.performingUserId, SystemClock.Instance.Now());

            var workflow = await new DefaultClaimWorkflowProvider().GetConfigurableClaimWorkflow(It.IsAny<ReleaseContext>());
            claimAggregate.ChangeClaimState(ClaimActions.Acknowledge, userId, SystemClock.Instance.Now(), workflow);

            await stack.ClaimAggregateRepository.Save(claimAggregate);

            // Assert
            var claimReadModel = stack.ClaimReadModelUpdateRepository.GetById(claimAggregate.TenantId, claimAggregate.Id);

            // Assert
            Assert.Equal(ClaimState.Acknowledged, claimReadModel.Status);
            Assert.Equal(ClaimState.Acknowledged, claimAggregate.Claim.ClaimStatus);
        }

        [Fact]
        public async Task ClaimNumber_IsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var userId = Guid.NewGuid();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               quoteAggregate,
               userId,
               "John Smith",
               "Jonboy",
               this.performingUserId,
               SystemClock.Instance.Now());
            claimAggregate.AssignClaimNumber("QQQQQQ", userId, SystemClock.Instance.Now());

            await stack.ClaimAggregateRepository.Save(claimAggregate);

            // Assert
            var claimReadModel = stack.ClaimReadModelUpdateRepository.GetById(claimAggregate.TenantId, claimAggregate.Id);

            // Assert
            Assert.Equal("QQQQQQ", claimReadModel.ClaimNumber);
            Assert.Equal("AAAAAA", claimReadModel.ClaimReference);
        }

        [Fact]
        public async Task CalculationResult_IsPersisted()
        {
            // Arrange
            var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var quoteAggregate = QuoteFactory.CreateNewPolicy();
            var userId = Guid.NewGuid();
            var claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               quoteAggregate,
               userId,
               "John Smith",
               "Jonboy",
               this.performingUserId,
               SystemClock.Instance.Now());
            claimAggregate.RecordCalculationResult(FormDataJsonFactory.Sample, CalculationResultJsonFactory.Create(), this.performingUserId, SystemClock.Instance.Now());

            await stack.ClaimAggregateRepository.Save(claimAggregate);

            // Assert
            var claimReadModel = stack.ClaimReadModelUpdateRepository.GetById(claimAggregate.TenantId, claimAggregate.Id);

            // Assert
            Assert.NotNull(claimReadModel.LatestCalculationResult);
        }
    }
}
