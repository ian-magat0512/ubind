// <copyright file="PolicyNumberRepositoryTests.cs" company="uBind">
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
    public class PolicyNumberRepositoryTests
    {
        private readonly IUBindDbContext dbContext;
        private Guid tenantId = Guid.NewGuid();
        private Guid productId = Guid.NewGuid();

        public PolicyNumberRepositoryTests()
        {
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
        }

        [Fact]
        public void ConsumePolicyNumberForProduct_Throws_WhenNoAreAvailableForProductInAnyEnvironment()
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
        public void ConsumePolicyNumberForProduct_Throws_WhenNoAreAvailableForProductInThatEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var numbers = new List<string>
            {
                "AAAAAA",
                "AAAAAB",
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
        public void ConsumePolicyNumberForProduct_ReturnsPolicyNumber_WhenIsAvailableForProductInThatEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var policyNumber = "AAAAAA";
            var numbers = new List<string> { policyNumber };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);

            // Act
            var consumed = sut.ConsumeForProduct(this.tenantId, this.productId, DeploymentEnvironment.Development);

            // Assert
            Assert.Equal(policyNumber, consumed);
        }

        [Fact]
        public async Task ConsumePolicyNumberForProduct_Throws_WhenAllForThatProductAndEnvironmentHaveAlreadyBeenAssigned()
        {
            using (var unitOfWork = new UnitOfWork(this.dbContext))
            {
                // Arrange
                var sut = this.CreateSut();
                var policyNumber = "AAAAAA";
                var numbers = new List<string> { policyNumber };
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
                Action act = () => sut.ConsumeForProduct(this.tenantId, this.productId, DeploymentEnvironment.Development);

                // Assert
                act.Should().Throw<ReferenceNumberUnavailableException>();
            }
        }

        [Fact]
        public void LoadForProduct_IgnoreDuplicateNumbers_WhenContainsDuplicate()
        {
            // Arrange
            var sut = this.CreateSut();
            var policyNumber = "AAAAAA";
            var duplicatePolicyNumber = "AAAAAA";
            var newPolicyNumber = "BBBBBB";
            var numbers = new List<string> { policyNumber };
            var withDuplicatenumbers = new List<string> { duplicatePolicyNumber, newPolicyNumber };
            var newPolicyNumbers = new List<string> { policyNumber, newPolicyNumber };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);

            // Act
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                newPolicyNumbers);
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            dbContext.PolicyNumbers
                .Where(pn => pn.TenantId == this.tenantId
                    && pn.ProductId == this.productId
                    && pn.Environment == DeploymentEnvironment.Development)
                .Select(policy => policy.Number)
                .Should().HaveCount(newPolicyNumbers.Count);
        }

        [Fact]
        public void LoadForProduct_DoesNotThrow_WhenDuplicatePolicyNumberOnlyExistsInDifferenEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var policyNumber = "AAAAAA";
            var numbers = new List<string> { policyNumber };
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
        public void DeleteForProduct_DeletesAllNumbers_WhenAllUnassigned()
        {
            // Arrange
            var sut = this.CreateSut();
            var policyNumber = "AAAAAA";
            var numbers = new List<string> { policyNumber };
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
            dbContext.PolicyNumbers
                .Where(pn => pn.TenantId == this.tenantId
                    && pn.ProductId == this.productId
                    && pn.Environment == DeploymentEnvironment.Staging)
                .Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteForProduct_OnlyDeletesUnassignedNumbers_WhenSomeAreAssigned()
        {
            using (var unitOfWork = new UnitOfWork(this.dbContext))
            {
                // Arrange
                var sut = this.CreateSut();
                var environment = DeploymentEnvironment.Staging;
                var policyNumber1 = "AAAAAA";
                var policyNumber2 = "BBBBBB";
                var numbers = new List<string> { policyNumber1, policyNumber2 };
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

                // Assert
                var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
                var remainingRecord = dbContext.PolicyNumbers
                    .Where(pn => pn.TenantId == this.tenantId
                        && pn.ProductId == this.productId
                        && pn.Environment == DeploymentEnvironment.Staging)
                    .Single();
                Assert.True(remainingRecord.Number == policyNumber1);
                Assert.True(remainingRecord.IsAssigned);
            }
        }

        [Fact]
        public void DeleteForProduct_OnlyDeletesUnassignedNumbersInList_WhenOnlySubsetSpecified()
        {
            // Arrange
            var sut = this.CreateSut();
            var environment = DeploymentEnvironment.Staging;
            var policyNumber1 = "AAAAAA";
            var policyNumber2 = "BBBBBB";
            var policyNumber3 = "CCCCCC";
            var policyNumber4 = "DDDDDD";
            var policyNumber5 = "EEEEEE";
            var numbers = new List<string> { policyNumber1, policyNumber2, policyNumber3, policyNumber4, policyNumber5 };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                environment,
                numbers);
            sut = this.CreateSut();
            var consumedNumber = sut.ConsumeForProduct(this.tenantId, this.productId, environment);
            var numbersToDelete = new List<string> { policyNumber2, policyNumber4 };
            sut = this.CreateSut();

            // Act
            sut.DeleteForProduct(this.tenantId, this.productId, environment, numbersToDelete);

            // Assert
            var remainingUnAssignedNumbers = this.CreateSut().GetAvailableForProduct(
                this.tenantId,
                this.productId,
                environment);
            Assert.True(remainingUnAssignedNumbers.Count == 2);
            Assert.Contains(policyNumber3, remainingUnAssignedNumbers);
            Assert.Contains(policyNumber5, remainingUnAssignedNumbers);
        }

        [Fact]
        public void DeleteForProduct_OnlyDeletesNumbersForCorrectProductAndEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var environment1 = DeploymentEnvironment.Staging;
            var productId2 = Guid.NewGuid();
            var environment2 = DeploymentEnvironment.Development;
            var policyNumber1 = "AAAAAA";
            var numbers = new List<string> { policyNumber1 };
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
            Assert.True(sut.GetAvailableForProduct(this.tenantId, this.productId, environment2).Single() == policyNumber1);
            Assert.True(sut.GetAvailableForProduct(this.tenantId, productId2, environment1).Single() == policyNumber1);
            Assert.True(sut.GetAvailableForProduct(this.tenantId, productId2, environment2).Single() == policyNumber1);
        }

        [Fact]
        public void PurgeForProduct_PurgesAssignedAndUnassignedNumbers_ForProductAndEnvironment()
        {
            // Arrange
            var sut = this.CreateSut();
            var environment = DeploymentEnvironment.Staging;
            var policyNumber1 = "AAAAAA";
            var policyNumber2 = "BBBBBB";
            var policyNumber3 = "CCCCCC";
            var policyNumber4 = "DDDDDD";
            var policyNumber5 = "EEEEEE";
            var numbers = new List<string> { policyNumber1, policyNumber2, policyNumber3, policyNumber4, policyNumber5 };
            sut.LoadForProduct(
                this.tenantId,
                this.productId,
                environment,
                numbers);
            sut = this.CreateSut();
            var consumedNumber = sut.ConsumeForProduct(this.tenantId, this.productId, environment);
            var numbersToDelete = new List<string> { policyNumber2, policyNumber4 };
            sut = this.CreateSut();

            // Act
            sut.PurgeForProduct(this.tenantId, this.productId, environment);

            // Assert
            var remainingUnAssignedNumbers = this.CreateSut().GetAvailableForProduct(this.tenantId, this.productId, environment);
            Assert.False(remainingUnAssignedNumbers.Any());
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            dbContext.PolicyNumbers
                .Where(pn => pn.TenantId == this.tenantId
                    && pn.ProductId == this.productId
                    && pn.Environment == DeploymentEnvironment.Staging)
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
            stub.LoadForProduct(
                this.tenantId,
                this.productId,
                DeploymentEnvironment.Development,
                numbers);

            // Assert
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var policyNumber = dbContext.PolicyNumbers.Where(
                c => c.TenantId == this.tenantId
                && c.ProductId == this.productId
                && c.Environment == DeploymentEnvironment.Development).FirstOrDefault();
            policyNumber.Should().NotBeNull();
            policyNumber.ProductId.Should().Be(this.productId);
            policyNumber.TenantId.Should().Be(this.tenantId);
        }

        private PolicyNumberRepository CreateSut()
        {
            IConnectionConfiguration connectionConfiguration = new ConnectionStrings();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();
            connectionConfiguration.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);
            var sut = new PolicyNumberRepository(this.dbContext, connectionConfiguration, SystemClock.Instance);
            return sut;
        }
    }
}
