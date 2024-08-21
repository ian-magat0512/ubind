// <copyright file="InvoiceNumberRepositoryTests.cs" company="uBind">
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
    using UBind.Domain.Repositories;
    using UBind.Persistence.Infrastructure;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class InvoiceNumberRepositoryTests
    {
        private readonly IUBindDbContext dbContext;
        private Guid tenantId = Guid.NewGuid();
        private Guid productId = Guid.NewGuid();

        public InvoiceNumberRepositoryTests()
        {
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
        }

        [Fact]
        public void ConsumeInvoiceNumberForProduct_Throws_WhenNoInvoiceNumbersAreAvailableForProductInAnyEnviroment()
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
        public void ConsumeInvoiceNumberForProduct__Throws_WhenNoInvoiceNumbersAreAvailableForProductInThatEnvironment()
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
        public void ConsumeInvoiceNumberForProduct_ReturnsInvoiceNumber_WhenInvoiceNumbersIsAvailableForProductInThatEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var invoiceNumber = "EEEEEEE";
            var numbers = new List<string> { invoiceNumber };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);

            // Act
            var consumed = sut.ConsumeForProduct(this.tenantId, this.productId, DeploymentEnvironment.Development);

            // Assert
            Assert.Equal(invoiceNumber, consumed);
        }

        [Fact]
        public async Task ConsumeInvoiceNumberForProduct_Throws_WhenAllInvoiceNumbersForThatProductAndEnvironmentHaveAlreadyBeenAssigned()
        {
            using (var unitOfWork = new UnitOfWork(this.dbContext))
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
        public void LoadInvoiceNumbersForProduct_Ignore_WhenDuplicateInvoiceNumberExistsForProductAndEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var invoiceNumber = "AAAAAA";
            var newInvoiceNumber = "BBBBBB";
            var numbers = new List<string> { invoiceNumber };
            var newInvoiceNumbers = new List<string> { invoiceNumber, newInvoiceNumber };
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
                DeploymentEnvironment.Development,
                newInvoiceNumbers);

            // Assert
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            dbContext.InvoiceNumbers
                .Where(i => i.TenantId == this.tenantId
                    && i.ProductId == this.productId
                    && i.Environment == DeploymentEnvironment.Development)
                .Select(policy => policy.Number).Should().HaveCount(newInvoiceNumbers.Count);
        }

        [Fact]
        public void LoadInvoiceNumbersForProduct_DoesNotThrow_WhenDuplicateInvoiceNumberOnlyExistsInDifferenEnvironment()
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
        public void DeleteInvoiceNumbersForProduct_DeletesAllNumbers_WhenAllUnassigned()
        {
            // Arrange
            var sut = this.CreateSut();
            var invoiceNumber = "AAAAAAJ";
            var numbers = new List<string> { invoiceNumber };
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
            var invoiceNumbers = dbContext.InvoiceNumbers.Where(x => x.Number == invoiceNumber).ToList();
            invoiceNumbers.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task DeleteInvoiceNumbersForProduct_OnlyDeletesUnassignedNumbers_WhenSomeAreAssigned()
        {
            using (var unitOfWork = new UnitOfWork(this.dbContext))
            {
                // Arrange
                var sut = this.CreateSut();
                var environment = DeploymentEnvironment.Staging;
                var invoiceNumber1 = "AAAAAA";
                var invoiceNumber2 = "BBBBBB";
                var numbers = new List<string> { invoiceNumber1, invoiceNumber2 };
                sut.LoadForProduct(
                    this.tenantId,
                    this.productId,
                    environment,
                    numbers);
                sut = this.CreateSut();
                var consumedNumber = sut.ConsumeForProduct(this.tenantId, this.productId, environment);
                await unitOfWork.Commit();

                // Act
                sut.DeleteForProduct(this.tenantId, this.productId, environment, numbers);
                await unitOfWork.Commit();

                // Assert
                var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
                var remainingRecord = dbContext.InvoiceNumbers.First(x => x.Number == invoiceNumber1);
                Assert.True(remainingRecord.Number == invoiceNumber1);
                Assert.True(remainingRecord.IsAssigned);
            }
        }

        [Fact]
        public async Task DeleteInvoiceNumbersForProduct_OnlyDeletesUnassignedNumbersInList_WhenOnlySubsetSpecified()
        {
            using (var unitOfWork = new UnitOfWork(this.dbContext))
            {
                // Arrange
                var sut = this.CreateSut();
                var environment = DeploymentEnvironment.Staging;
                var invoiceNUmber1 = "AAAAAA";
                var invoiceNumber2 = "BBBBBB";
                var invoiceNumber3 = "CCCCCC";
                var invoiceNumber4 = "DDDDDD";
                var invoiceNumber5 = "EEEEEE";
                var numbers = new List<string> { invoiceNUmber1, invoiceNumber2, invoiceNumber3, invoiceNumber4, invoiceNumber5 };
                sut.LoadForProduct(
                    this.tenantId,
                    this.productId,
                    environment,
                    numbers);
                sut = this.CreateSut();
                var consumedNumber = sut.ConsumeForProduct(this.tenantId, this.productId, environment);
                await unitOfWork.Commit();
                var numbersToDelete = new List<string> { invoiceNumber2, invoiceNumber4 };
                sut = this.CreateSut();

                // Act
                sut.DeleteForProduct(this.tenantId, this.productId, environment, numbersToDelete);
                await unitOfWork.Commit();

                // Assert
                var remainingUnAssignedNumbers = this.CreateSut().GetAvailableForProduct(this.tenantId, this.productId, environment);
                Assert.True(remainingUnAssignedNumbers.Count == 2);
                Assert.Contains(invoiceNumber3, remainingUnAssignedNumbers);
                Assert.Contains(invoiceNumber5, remainingUnAssignedNumbers);
            }
        }

        [Fact]
        public void DeleteInvoiceNumbersForProduct_OnlyDeletesNumbersForCorrectProductAndEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var environment1 = DeploymentEnvironment.Staging;
            var productId2 = Guid.NewGuid();
            var environment2 = DeploymentEnvironment.Development;
            var invoiceNumber = "AAAAAA";
            var numbers = new List<string> { invoiceNumber };
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
            Assert.True(sut.GetAvailableForProduct(this.tenantId, this.productId, environment2).Single() == invoiceNumber);
            Assert.True(sut.GetAvailableForProduct(this.tenantId, productId2, environment1).Single() == invoiceNumber);
            Assert.True(sut.GetAvailableForProduct(this.tenantId, productId2, environment2).Single() == invoiceNumber);
        }

        [Fact]
        public void PurgeInvoiceNumbersForProduct_PurgesAssignedAndUnassignedNumbers_ForProductAndEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var environment = DeploymentEnvironment.Staging;
            var invoiceNUmber1 = "AAAAAA";
            var invoiceNumber2 = "BBBBBB";
            var invoiceNumber3 = "CCCCCC";
            var invoiceNumber4 = "DDDDDD";
            var invoiceNumber5 = "EEEEEE";
            var numbers = new List<string> { invoiceNUmber1, invoiceNumber2, invoiceNumber3, invoiceNumber4, invoiceNumber5 };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                environment,
                numbers);
            sut = this.CreateSut();
            var consumedNumber = sut.ConsumeForProduct(this.tenantId, this.productId, environment);
            var numbersToDelete = new List<string> { invoiceNumber2, invoiceNumber4 };
            sut = this.CreateSut();

            // Act
            sut.PurgeForProduct(this.tenantId, this.productId, environment);

            // Assert
            var remainingUnAssignedNumbers = this.CreateSut().GetAvailableForProduct(this.tenantId, this.productId, environment);
            Assert.False(remainingUnAssignedNumbers.Any());
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            dbContext.InvoiceNumbers
                .Where(i => i.TenantId == this.tenantId
                    && i.ProductId == this.productId
                    && i.Environment == environment)
                .Should().BeEmpty();
        }

        [Fact]
        public void LoadForProduct_RecordsHasNewIds_WhenCreatingNewInstances()
        {
            // Prepare
            var stub = this.CreateSut();
            var referenceNumber = "ZZZZZZZ";
            var numbers = new List<string> { referenceNumber };

            // Act
            stub.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);

            // Assert
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var invoiceNumber = dbContext.InvoiceNumbers
                .Where(c => c.TenantId == this.tenantId
                && c.ProductId == this.productId
                && c.Environment == DeploymentEnvironment.Development)
                .FirstOrDefault();
            invoiceNumber.Should().NotBeNull();
            invoiceNumber.ProductId.Should().Be(this.productId);
            invoiceNumber.TenantId.Should().Be(this.tenantId);
        }

        private InvoiceNumberRepository CreateSut()
        {
            IConnectionConfiguration connectionConfiguration = new ConnectionStrings();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            connectionConfiguration.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);
            var stub = new InvoiceNumberRepository(this.dbContext, connectionConfiguration, SystemClock.Instance);
            return stub;
        }
    }
}
