// <copyright file="PaymentAggregateIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Accounting.Payment
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Accounting;
    using UBind.Domain.Aggregates.Accounting.Payment;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection("Database collection")]
    public class PaymentAggregateIntegrationTests
    {
        private readonly Money amount = new Money(1000);
        private readonly Guid fakePerformingUserId = Guid.NewGuid();

        [Fact]
        public async Task CreateNewPayment_SavesCorrectData_WhenPersisted()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionString))
            {
                var tenantId = Guid.NewGuid();
                var referenceNumber = this.GetReferenceNumber(tenantId, stack);

                // Arrange
                Instant transactionTime = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(3));
                Instant createdTimestamp = stack.Clock.GetCurrentInstant();
                TransactionParties transactionPartyModel = new TransactionParties(Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Customer);

                // Act
                var aggregate = PaymentAggregate.CreateNewPayment(TenantFactory.DefaultId, this.amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, this.fakePerformingUserId);
                await stack.PaymentAggregateRepository.Save(aggregate);

                // Assert
                var retrievedPayment = stack.PaymentReadModelRepository.GetById(aggregate.Id);
                Assert.Equal(retrievedPayment.ReferenceNumber, referenceNumber);
                Assert.Equal(retrievedPayment.PayerId, transactionPartyModel.PayerId);
                Assert.Equal(retrievedPayment.PayerType, transactionPartyModel.PayerType);
                Assert.Equal(retrievedPayment.Amount, this.amount);
                Assert.Equal(retrievedPayment.TransactionTimestamp, transactionTime);
                Assert.Equal(retrievedPayment.CreatedTimestamp, createdTimestamp);
            }
        }

        [Fact]
        public async Task Delete_SetsPaymentIsDeletedToTrue_WhenCalled()
        {
            Guid id;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionString))
            {
                var tenantId = Guid.NewGuid();
                var referenceNumber = this.GetReferenceNumber(tenantId, stack);

                // Arrange
                Instant transactionTime = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(3));
                Instant createdTimestamp = stack.Clock.GetCurrentInstant();
                TransactionParties transactionPartyModel = new TransactionParties(Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Customer);

                var aggregate = PaymentAggregate.CreateNewPayment(TenantFactory.DefaultId, this.amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, this.fakePerformingUserId);
                id = aggregate.Id;

                // Act
                aggregate.Delete(createdTimestamp, this.fakePerformingUserId);
                await stack.PaymentAggregateRepository.Save(aggregate);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var retrievedPayment = stack.PaymentReadModelRepository.GetById(id);
                Assert.True(retrievedPayment.IsDeleted);
            }
        }

        [Fact]
        public async Task AssignParticipants_CorrectlyUpdatesPaymentData_WhenCalled()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionString))
            {
                var tenantId = Guid.NewGuid();
                var referenceNumber = this.GetReferenceNumber(tenantId, stack);

                // Arrange
                Instant transactionTime = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(3));
                Instant createdTimestamp = stack.Clock.GetCurrentInstant().Minus(Duration.FromHours(1));

                TransactionParties transactionPartyModel = new TransactionParties(Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Customer, Guid.NewGuid(), Domain.ValueTypes.TransactionPartyType.Organisation);

                var aggregate = PaymentAggregate.CreateNewPayment(TenantFactory.DefaultId, this.amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, this.fakePerformingUserId);

                // Act
                aggregate.AssignParticipants(transactionPartyModel, createdTimestamp, this.fakePerformingUserId);
                await stack.PaymentAggregateRepository.Save(aggregate);

                // Assert
                var retrievedPayment = stack.PaymentReadModelRepository.GetById(aggregate.Id);
                Assert.Equal(retrievedPayment.ReferenceNumber, referenceNumber);
                Assert.Equal(retrievedPayment.PayerId, transactionPartyModel.PayerId);
                Assert.Equal(retrievedPayment.PayerType, transactionPartyModel.PayerType);

                Assert.Equal(retrievedPayment.PayeeId, transactionPartyModel.PayeeId);
                Assert.Equal(retrievedPayment.PayeeType, transactionPartyModel.PayeeType);

                Assert.Equal(retrievedPayment.Amount, this.amount);
                Assert.Equal(retrievedPayment.TransactionTimestamp, transactionTime);
                Assert.Equal(retrievedPayment.CreatedTimestamp, createdTimestamp);
            }
        }

        [Fact]
        public async Task AllocateToCommercialDocument_CorrectlySavesRefundAllocationToCreditNote_WhenCalled()
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionString))
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

                var aggregate = PaymentAggregate.CreateNewPayment(TenantFactory.DefaultId, this.amount, referenceNumber, transactionTime, createdTimestamp, transactionPartyModel, this.fakePerformingUserId);
                aggregate.AssignParticipants(transactionPartyModel, createdTimestamp, this.fakePerformingUserId);

                var invoice1 = new Invoice(
                   tenantId,
                   createdTimestamp,
                   new InvoiceNumber(tenantId, product1Id, DeploymentEnvironment.Development, "AAA123", createdTimestamp),
                   dueDateTime,
                   Guid.NewGuid());

                // Act
                aggregate.AllocateToCommercialDocument(transactionPartyModel, invoice1, createdTimestamp, this.fakePerformingUserId);
                await stack.PaymentAggregateRepository.Save(aggregate);

                // Assert
                var retrievedPayment = stack.PaymentReadModelRepository.GetById(aggregate.Id);
                Assert.Equal(retrievedPayment.ReferenceNumber, referenceNumber);
                Assert.Equal(retrievedPayment.PayerId, transactionPartyModel.PayerId);
                Assert.Equal(retrievedPayment.PayerType, transactionPartyModel.PayerType);

                Assert.Equal(retrievedPayment.PayeeId, transactionPartyModel.PayeeId);
                Assert.Equal(retrievedPayment.PayeeType, transactionPartyModel.PayeeType);

                Assert.Equal(retrievedPayment.Amount, this.amount);
                Assert.Equal(retrievedPayment.TransactionTimestamp, transactionTime);
                Assert.Equal(retrievedPayment.CreatedTimestamp, createdTimestamp);

                retrievedPayment.Allocations.Count.Should().Be(1);
                retrievedPayment.Allocations.ToList()[0].CommercialDocument.Id.Should().NotBeEmpty();
                retrievedPayment.Allocations.ToList()[0].FinancialTransaction.Id.Should().Be(aggregate.Id);
                retrievedPayment.Allocations.ToList()[0].CreatedTimestamp.Should().Be(createdTimestamp);
                retrievedPayment.Allocations.ToList()[0].IsDeleted.Should().Be(false);
            }
        }

        private string GetReferenceNumber(Guid tenantId, ApplicationStack stack)
        {
            var referenceNumber =
                stack.PaymentReferenceNumberGenerator.GeneratePaymentReferenceNumber(
                    tenantId, DeploymentEnvironment.Development).ToString();
            return referenceNumber;
        }

        private void CreateTenantAndProduct(Guid tenantId, Guid productId1, Guid productId2)
        {
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(tenantId);
                var product1 = ProductFactory.Create(tenant.Id, productId1);
                var product2 = ProductFactory.Create(tenant.Id, productId2);

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
