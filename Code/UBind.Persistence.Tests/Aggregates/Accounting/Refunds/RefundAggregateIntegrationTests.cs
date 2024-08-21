// <copyright file="RefundAggregateIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Accounting.Refund
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates.Accounting.Refund;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection("Database collection")]
    public class RefundAggregateIntegrationTests
    {
        private readonly Money amount = new Money(313.33);
        private readonly Guid fakePerformingUserId = Guid.NewGuid();

        [Fact]
        public async Task CreateNewRefund_SavesCorrectData_WhenPersisted()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenantId = Guid.NewGuid();
                var productId = Guid.NewGuid();

                var referenceNumber = this.GetReferenceNumber(tenantId, stack);

                // Arrange
                Instant transactionTime = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(3));
                Instant createdTimestamp = stack.Clock.GetCurrentInstant();
                TransactionParties transactionPartyModel = new TransactionParties(Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Customer);

                // Act
                var aggregate = RefundAggregate.CreateNewRefund(tenantId, this.amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, this.fakePerformingUserId);
                await stack.RefundAggregateRepository.Save(aggregate);

                // Assert
                var retrievedRefund = stack.RefundReadModelRepository.GetById(aggregate.Id);
                Assert.Equal(retrievedRefund.ReferenceNumber, referenceNumber);
                Assert.Equal(retrievedRefund.PayerId, transactionPartyModel.PayerId);
                Assert.Equal(retrievedRefund.PayerType, transactionPartyModel.PayerType);
                Assert.Equal(retrievedRefund.Amount, this.amount);
                Assert.Equal(retrievedRefund.TransactionTimestamp, transactionTime);
                Assert.Equal(retrievedRefund.CreatedTimestamp, createdTimestamp);
            }
        }

        [Fact]
        public async Task Delete_SetsRefundIsDeletedToTrue_WhenCalled()
        {
            Guid id;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenantId = Guid.NewGuid();
                var productId = Guid.NewGuid();

                var referenceNumber = this.GetReferenceNumber(tenantId, stack);

                // Arrange
                Instant transactionTime = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(3));
                Instant createdTimestamp = stack.Clock.GetCurrentInstant();
                TransactionParties transactionPartyModel = new TransactionParties(Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Customer);

                var aggregate = RefundAggregate.CreateNewRefund(tenantId, this.amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, this.fakePerformingUserId);
                id = aggregate.Id;

                // Act
                aggregate.Delete(createdTimestamp, this.fakePerformingUserId);
                await stack.RefundAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var retrievedRefund = stack.RefundReadModelRepository.GetById(id);
                Assert.True(retrievedRefund.IsDeleted);
            }
        }

        [Fact]
        public async Task AssignParticipants_CorrectlyUpdatesRefundData_WhenCalled()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenantId = Guid.NewGuid();
                var productId = Guid.NewGuid();
                var referenceNumber = this.GetReferenceNumber(tenantId, stack);

                // Arrange
                Instant transactionTime = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(3));
                Instant createdTimestamp = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(1));
                TransactionParties transactionPartyModel = new TransactionParties(Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Customer, Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Organisation);

                var aggregate = RefundAggregate.CreateNewRefund(tenantId, this.amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, this.fakePerformingUserId);

                // Act
                aggregate.AssignParticipants(transactionPartyModel, createdTimestamp, this.fakePerformingUserId);
                await stack.RefundAggregateRepository.Save(aggregate);

                // Assert
                var retrievedRefund = stack.RefundReadModelRepository.GetById(aggregate.Id);
                Assert.Equal(retrievedRefund.ReferenceNumber, referenceNumber);
                Assert.Equal(retrievedRefund.PayerId, transactionPartyModel.PayerId);
                Assert.Equal(retrievedRefund.PayerType, transactionPartyModel.PayerType);
                Assert.Equal(retrievedRefund.PayeeId, transactionPartyModel.PayeeId);
                Assert.Equal(retrievedRefund.PayeeType, transactionPartyModel.PayeeType);
                Assert.Equal(retrievedRefund.Amount, this.amount);
                Assert.Equal(retrievedRefund.TransactionTimestamp, transactionTime);
                Assert.Equal(retrievedRefund.CreatedTimestamp, createdTimestamp);
            }
        }

        [Fact]
        public async Task AllocateToCreditNotes_RefundAggregateIsPersisted()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenantId = TenantFactory.DefaultId;
                var product1Id = Guid.NewGuid();
                var product2Id = Guid.NewGuid();
                this.CreateTenantAndProduct(tenantId, product1Id, product2Id);

                var referenceNumber = this.GetReferenceNumber(tenantId, stack);

                // Arrange
                Instant transactionTime = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(3));
                Instant createdTimestamp = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(1));
                Instant dueDateTime = stack.Clock.GetCurrentInstant().Plus(Duration.FromDays(7));

                var transactionPartyModel = new TransactionParties(Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Customer, Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Organisation);
                var aggregate = RefundAggregate.CreateNewRefund(tenantId, this.amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, this.fakePerformingUserId);

                aggregate.AssignParticipants(transactionPartyModel, createdTimestamp, this.fakePerformingUserId);

                var creditNote1 = new CreditNote(
                   tenantId,
                   createdTimestamp,
                   new CreditNoteNumber(tenantId, product1Id, DeploymentEnvironment.Development, "AAA123", createdTimestamp),
                   dueDateTime,
                   Guid.NewGuid());

                // Act
                aggregate.AllocateToCommercialDocument(transactionPartyModel, creditNote1, createdTimestamp, this.fakePerformingUserId);
                await stack.RefundAggregateRepository.Save(aggregate);

                // Assert
                var retrievedRefund = stack.RefundReadModelRepository.GetById(aggregate.Id);
                Assert.Equal(retrievedRefund.ReferenceNumber, referenceNumber);
                Assert.Equal(retrievedRefund.PayerId, transactionPartyModel.PayerId);
                Assert.Equal(retrievedRefund.PayerType, transactionPartyModel.PayerType);
                Assert.Equal(retrievedRefund.PayeeId, transactionPartyModel.PayeeId);
                Assert.Equal(retrievedRefund.PayeeType, transactionPartyModel.PayeeType);
                Assert.Equal(retrievedRefund.Amount, this.amount);
                Assert.Equal(retrievedRefund.TransactionTimestamp, transactionTime);
                Assert.Equal(retrievedRefund.CreatedTimestamp, createdTimestamp);

                retrievedRefund.Allocations.Count.Should().Be(1);
                retrievedRefund.Allocations.ToList()[0].CommercialDocument.Id.Should().NotBeEmpty();
                retrievedRefund.Allocations.ToList()[0].FinancialTransaction.Id.Should().Be(aggregate.Id);
                retrievedRefund.Allocations.ToList()[0].CreatedTimestamp.Should().Be(createdTimestamp);
                retrievedRefund.Allocations.ToList()[0].IsDeleted.Should().Be(false);
            }
        }

        private string GetReferenceNumber(Guid tenantId, ApplicationStack stack)
        {
            return stack.RefundReferenceNumberGenerator.GenerateRefundReferenceNumber(tenantId, DeploymentEnvironment.Development).ToString();
        }

        private void CreateTenantAndProduct(Guid tenantId, Guid productId1, Guid productId2)
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(tenantId);
                var product1 = ProductFactory.Create(tenantId, productId1);
                var product2 = ProductFactory.Create(tenantId, productId2);

                if (stack.TenantRepository.GetTenantById(tenant.Id) == null)
                {
                    stack.TenantRepository.Insert(tenant);
                    stack.TenantRepository.SaveChanges();
                }

                stack.ProductRepository.Insert(product1);
                stack.ProductRepository.Insert(product2);
                stack.DbContext.SaveChanges();
            }
        }
    }
}
