// <copyright file="CreditNoteNumberRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Infrastructure;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class CreditNoteNumberRepositoryTests
    {
        private readonly IUBindDbContext dbContext;

        private Guid tenantId = Guid.NewGuid();
        private Guid productId = Guid.NewGuid();

        public CreditNoteNumberRepositoryTests()
        {
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
        }

        [Fact]
        public void ConsumeCreditNoteNumberForProduct_Throws_WhenNoCreditNoteNumbersAreAvailableForProductInAnyEnviroment()
        {
            // Arrange
            var sut = this.CreateSut();

            // Act
            Action act = () =>
                sut.ConsumeForProduct(this.tenantId, Guid.NewGuid(), DeploymentEnvironment.Development);

            // Assert
            act.Should().Throw<ReferenceNumberUnavailableException>();
        }

        [Fact]
        public void ConsumeCreditNoteNumberForProduct__Throws_WhenNoCreditNoteNumbersAreAvailableForProductInThatEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var numbers = new List<string>
            {
                "EEEEEEE",
                "CCCEESH",
            };

            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);

            // Act
            Action act = () =>
              sut.ConsumeForProduct(this.tenantId, this.productId, DeploymentEnvironment.Staging);

            // Assert
            act.Should().Throw<ReferenceNumberUnavailableException>();
        }

        [Fact]
        public void ConsumeCreditNoteNumberForProduct_ReturnsCreditNoteNumber_WhenCreditNoteNumbersIsAvailableForProductInThatEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var creditNote = "EEEEEEE";
            var numbers = new List<string> { creditNote };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);

            // Act
            var consumed = sut.ConsumeForProduct(this.tenantId, this.productId, DeploymentEnvironment.Development);

            // Assert
            Assert.Equal(creditNote, consumed);
        }

        [Fact]
        public async Task ConsumeCreditNoteNumberForProduct_Throws_WhenAllCreditNoteNumbersForThatProductAndEnvironmentHaveAlreadyBeenAssigned()
        {
            using (var unitOfWork = new UnitOfWork(this.dbContext))
            {
                // Arrange
                var sut = this.CreateSut();
                var creditNote = "AAAAAA";
                var numbers = new List<string> { creditNote };
                sut.LoadForProduct(
                    this.tenantId,
                    this.productId,
                    DeploymentEnvironment.Development,
                    numbers);
                sut = this.CreateSut();
                sut.ConsumeForProduct(this.tenantId, this.productId, DeploymentEnvironment.Development);
                await unitOfWork.Commit();
                sut = this.CreateSut();

                // Act
                Action act = () =>
                    sut.ConsumeForProduct(this.tenantId, this.productId, DeploymentEnvironment.Development);

                // Assert
                act.Should().Throw<ReferenceNumberUnavailableException>();
            }
        }

        [Fact]
        public void LoadCreditNoteNumbersForProduct_Ignore_WhenDuplicateCreditNoteNumberExistsForProductAndEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var creditNote = "AAAAAA";
            var duplicateInvoiceNumber = "AAAAAA";
            var newCreditNoteNumber = "BBBBBB";
            var numbers = new List<string> { creditNote };
            var withDuplicatenumbers = new List<string> { duplicateInvoiceNumber, newCreditNoteNumber };
            var newCreditNoteNumbers = new List<string> { creditNote, newCreditNoteNumber };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);
            sut = this.CreateSut();

            // Act + Assert
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                newCreditNoteNumbers);
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            dbContext.CreditNoteNumbers
                .Where(cnn => cnn.TenantId == this.tenantId
                    && cnn.ProductId == this.productId
                    && cnn.Environment == DeploymentEnvironment.Development)
                .Select(policy => policy.Number).Should().HaveCount(newCreditNoteNumbers.Count);
        }

        [Fact]
        public void LoadCreditNoteNumbersForProduct_DoesNotThrow_WhenDuplicateCreditNoteNumberOnlyExistsInDifferenEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var invoiceNumber = "AAAAAA";
            var numbers = new List<string> { invoiceNumber };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);
            sut = this.CreateSut();

            // Act
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Staging,
                numbers);

            // No exception indicates test success.
        }

        [Fact]
        public void DeleteCreditNoteNumbersForProduct_DeletesAllNumbers_WhenAllUnassigned()
        {
            // Arrange
            var sut = this.CreateSut();
            var creditNoteNumber = "AAAAAA";
            var numbers = new List<string> { creditNoteNumber };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Staging,
                numbers);
            sut = this.CreateSut();

            // Act
            sut.DeleteForProduct(this.tenantId, this.productId, DeploymentEnvironment.Staging, numbers);

            // Assert
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            dbContext.CreditNoteNumbers
                .Where(cnn => cnn.TenantId == this.tenantId
                    && cnn.ProductId == this.productId
                    && cnn.Environment == DeploymentEnvironment.Staging)
                .Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteCreditNoteNumbersForProduct_OnlyDeletesUnassignedNumbers_WhenSomeAreAssigned()
        {
            using (var unitOfWork = new UnitOfWork(this.dbContext))
            {
                // Arrange
                var sut = this.CreateSut();
                var environment = DeploymentEnvironment.Staging;
                var creditNoteNumber1 = "AAAAAA";
                var creditNoteNumber2 = "BBBBBB";
                var numbers = new List<string> { creditNoteNumber1, creditNoteNumber2 };
                sut.LoadForProduct(
                    this.tenantId,
                    this.productId,
                    environment,
                    numbers);
                sut = this.CreateSut();
                var consumedNumber = sut.ConsumeForProduct(this.tenantId, this.productId, environment);
                await unitOfWork.Commit();

                // Act
                var deletedNumbers = sut.DeleteForProduct(this.tenantId, this.productId, environment, numbers);
                await unitOfWork.Commit();

                // Assert
                deletedNumbers.Should().HaveCount(1);
                var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
                var remainingNumbers = dbContext.CreditNoteNumbers
                    .Where(cnn => cnn.TenantId == this.tenantId
                        && cnn.ProductId == this.productId
                        && cnn.Environment == environment);
                remainingNumbers.Should().HaveCount(1);
                var remainingRecord = remainingNumbers.Single();
                Assert.True(remainingRecord.Number == creditNoteNumber1);
                Assert.True(remainingRecord.IsAssigned);
            }
        }

        [Fact]
        public void DeleteCreditNoteNumbersForProduct_OnlyDeletesUnassignedNumbersInList_WhenOnlySubsetSpecified()
        {
            // Arrange
            var sut = this.CreateSut();
            var environment = DeploymentEnvironment.Staging;
            var creditNoteNUmber1 = "AAAAAA";
            var creditNoteNumber2 = "BBBBBB";
            var creditNoteNumber3 = "CCCCCC";
            var creditNoteNumber4 = "DDDDDD";
            var creditNoteNumber5 = "EEEEEE";
            var numbers = new List<string> { creditNoteNUmber1, creditNoteNumber2, creditNoteNumber3, creditNoteNumber4, creditNoteNumber5 };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                environment,
                numbers);
            sut = this.CreateSut();
            var consumedNumber = sut.ConsumeForProduct(this.tenantId, this.productId, environment);
            var numbersToDelete = new List<string> { creditNoteNumber2, creditNoteNumber4 };
            sut = this.CreateSut();

            // Act
            sut.DeleteForProduct(this.tenantId, this.productId, environment, numbersToDelete);

            // Assert
            var remainingUnAssignedNumbers = this.CreateSut().GetAvailableForProduct(this.tenantId, this.productId, environment);
            Assert.True(remainingUnAssignedNumbers.Count == 2);
            Assert.Contains(creditNoteNumber3, remainingUnAssignedNumbers);
            Assert.Contains(creditNoteNumber5, remainingUnAssignedNumbers);
        }

        [Fact]
        public void DeleteCreditNoteNumbersForProduct_OnlyDeletesNumbersForCorrectProductAndEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var environment1 = DeploymentEnvironment.Staging;
            var productId2 = Guid.NewGuid();
            var environment2 = DeploymentEnvironment.Development;
            var creditNoteNumber = "AAAAAA";
            var numbers = new List<string> { creditNoteNumber };
            sut.LoadForProduct(this.tenantId, this.productId, environment1, numbers);
            sut.LoadForProduct(this.tenantId, this.productId, environment2, numbers);
            sut.LoadForProduct(this.tenantId, productId2, environment1, numbers);
            sut.LoadForProduct(this.tenantId, productId2, environment2, numbers);
            sut = this.CreateSut();

            // Act
            sut.DeleteForProduct(this.tenantId, this.productId, environment1, numbers);

            // Assert
            sut = this.CreateSut();
            Assert.Empty(sut.GetAvailableForProduct(this.tenantId, this.productId, environment1));
            Assert.True(sut.GetAvailableForProduct(this.tenantId, this.productId, environment2).Single() == creditNoteNumber);
            Assert.True(sut.GetAvailableForProduct(this.tenantId, productId2, environment1).Single() == creditNoteNumber);
            Assert.True(sut.GetAvailableForProduct(this.tenantId, productId2, environment2).Single() == creditNoteNumber);
        }

        [Fact]
        public void PurgeCreditNoteNumbersForProduct_PurgesAssignedAndUnassignedNumbers_ForProductAndEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var environment = DeploymentEnvironment.Staging;
            var creditNoteNUmber1 = "AAAAAA";
            var creditNoteNumber2 = "BBBBBB";
            var creditNoteNumber3 = "CCCCCC";
            var creditNoteNumber4 = "DDDDDD";
            var creditNoteNumber5 = "EEEEEE";
            var numbers = new List<string> { creditNoteNUmber1, creditNoteNumber2, creditNoteNumber3, creditNoteNumber4, creditNoteNumber5 };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                environment,
                numbers);
            sut = this.CreateSut();
            var consumedNumber = sut.ConsumeForProduct(this.tenantId, this.productId, environment);
            var numbersToDelete = new List<string> { creditNoteNumber2, creditNoteNumber4 };
            sut = this.CreateSut();

            // Act
            sut.PurgeForProduct(this.tenantId, this.productId, environment);

            // Assert
            var remainingUnAssignedNumbers = this.CreateSut().GetAvailableForProduct(this.tenantId, this.productId, environment);
            Assert.False(remainingUnAssignedNumbers.Any());
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            dbContext.CreditNoteNumbers
                .Where(cnn => cnn.TenantId == this.tenantId
                    && cnn.ProductId == this.productId
                    && cnn.Environment == environment)
                .Should().BeEmpty();
        }

        [Fact]
        public void LoadForProduct_RecordsHasNewIds_WhenCreatingNewInstances()
        {
            // Prepare
            var stub = this.CreateSut();
            var referenceNumber = "ZZZZZZ";
            var numbers = new List<string> { referenceNumber };

            // Act
            stub.LoadForProduct(TenantFactory.DefaultId, ProductFactory.DefaultId, DeploymentEnvironment.Development, numbers);

            // Assert
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var creditNote = dbContext.CreditNoteNumbers.Where(c => c.TenantId == TenantFactory.DefaultId && c.ProductId == ProductFactory.DefaultId && c.Environment == DeploymentEnvironment.Development).FirstOrDefault();
            creditNote.Should().NotBeNull();
            creditNote.ProductId.Should().Be(ProductFactory.DefaultId);
            creditNote.TenantId.Should().Be(TenantFactory.DefaultId);
        }

        private CreditNoteNumberRepository CreateSut()
        {
            IConnectionConfiguration connectionConfiguration = new ConnectionStrings();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            connectionConfiguration.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);
            var stub = new CreditNoteNumberRepository(this.dbContext, connectionConfiguration, SystemClock.Instance);
            return stub;
        }
    }
}