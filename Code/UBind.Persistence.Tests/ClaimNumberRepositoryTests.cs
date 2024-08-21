// <copyright file="ClaimNumberRepositoryTests.cs" company="uBind">
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

    /// <summary>
    /// Integration tests for Claims Number Repository.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class ClaimNumberRepositoryTests
    {
        private readonly IUBindDbContext dbContext;
        private Guid tenantId = Guid.NewGuid();
        private Guid productId = Guid.NewGuid();

        public ClaimNumberRepositoryTests()
        {
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
        }

        [Fact]
        public void ConsumeClaimsNumberForProduct_Throws_WhenNoClaimNumbersAreAvailableForProductInAnyEnvironment()
        {
            // Arrange
            var stub = this.CreateStub();

            // Act
            Action act = () =>
                stub.ConsumeForProduct(this.tenantId, Guid.NewGuid(), DeploymentEnvironment.Development);

            // Assert
            act.Should().Throw<ReferenceNumberUnavailableException>();
        }

        [Fact]
        public void ConsumeForProduct_Throws_WhenNoClaimNumbersAreAvailableForProductInSpecifiedEnvironment()
        {
            var stub = this.CreateStub();
            var productId = Guid.NewGuid();
            var numbers = new List<string>
            {
                "AAAAAAAE",
                "EEEEEDDD",
            };

            stub.LoadForProduct(
                this.tenantId,
                productId,
                DeploymentEnvironment.Development,
                numbers);

            // Act
            Action act = () =>
                stub.ConsumeForProduct(this.tenantId, productId, DeploymentEnvironment.Staging);

            // Assert
            act.Should().Throw<ReferenceNumberUnavailableException>();
        }

        [Fact]
        public void ConsumeForProduct_ReturnsClaimNumber_WhenClaimNumberIsAvailableInProductForSpecificEnvironment()
        {
            var stub = this.CreateStub();
            var productId = Guid.NewGuid();
            var referenceNumber = "YYYYYYYY";
            var numbers = new List<string> { referenceNumber };
            stub.LoadForProduct(
                this.tenantId,
                productId,
                DeploymentEnvironment.Development,
                numbers);

            var consumed = stub.ConsumeForProduct(
                this.tenantId,
                productId,
                DeploymentEnvironment.Development);

            Assert.Equal(referenceNumber, consumed);
        }

        [Fact]
        public async Task ConsumeForProduct_Throws_WhenAllClaimNumbersForThatProductAndEnvironmentAreAlreadyUsed()
        {
            using (var unitOfWork = new UnitOfWork(this.dbContext))
            {
                // Arrange
                var stub = this.CreateStub();
                var productId = Guid.NewGuid();
                var referenceNumber = "XXXXXXX";
                var numbers = new List<string> { referenceNumber };
                stub.LoadForProduct(
                    this.tenantId,
                    productId,
                    DeploymentEnvironment.Development,
                    numbers);
                stub = this.CreateStub();
                stub.ConsumeForProduct(this.tenantId, productId, DeploymentEnvironment.Development);
                await unitOfWork.Commit();
                stub = this.CreateStub();

                // Act
                Action act = () =>
                    stub.ConsumeForProduct(this.tenantId, productId, DeploymentEnvironment.Development);

                // Assert
                act.Should().Throw<ReferenceNumberUnavailableException>();
            }
        }

        [Fact]
        public void LoadForProduct_Ignore_WhenDuplicateClaimNumberExistsForSpecificProductAndEnvironment()
        {
            // Prepare
            var stub = this.CreateStub();
            var productId = Guid.NewGuid();
            var referenceNumber = "ZZZZZZ";
            var numbers = new List<string> { referenceNumber };
            var duplicateClaimNumber = "AAAAAA";
            var newClaimNumber = "BBBBBB";
            var withDuplicatenumbers = new List<string> { duplicateClaimNumber, newClaimNumber };
            var newPolicyNumbers = new List<string> { referenceNumber, newClaimNumber };
            stub.LoadForProduct(
                this.tenantId,
                productId,
                DeploymentEnvironment.Development,
                numbers);

            // Act
            stub.LoadForProduct(
                this.tenantId,
                productId,
                DeploymentEnvironment.Development,
                newPolicyNumbers);

            // Assert
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            dbContext.ClaimNumbers
                .Where(c => c.TenantId == this.tenantId
                    && c.ProductId == productId
                    && c.Environment == DeploymentEnvironment.Development)
                .Select(claim => claim.Number).Should().HaveCount(newPolicyNumbers.Count);
        }

        [Fact]
        public void LoadForProduct_Success_WhenDuplicateClaimNumberExistsInDifferentEnvironment()
        {
            var stub = this.CreateStub();
            var productId = Guid.NewGuid();
            var referenceNumber = "UUUUUUU";
            var numbers = new List<string> { referenceNumber };
            stub.LoadForProduct(
                this.tenantId,
                productId,
                DeploymentEnvironment.Development,
                numbers);
            stub = this.CreateStub();

            stub.LoadForProduct(
                this.tenantId,
                productId,
                DeploymentEnvironment.Staging,
                numbers);
        }

        [Fact]
        public void LoadForProduct_RecordsHasNewIds_WhenCreatingNewInstances()
        {
            // Prepare
            var stub = this.CreateStub();
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
            var claims = dbContext.ClaimNumbers.Where(c =>
            c.TenantId == this.tenantId
            && c.ProductId == this.productId
            && c.Environment == DeploymentEnvironment.Development)
                .FirstOrDefault();
            claims.Should().NotBeNull();
            claims.ProductId.Should().Be(this.productId);
            claims.TenantId.Should().Be(this.tenantId);
        }

        private ClaimNumberRepository CreateStub()
        {
            IConnectionConfiguration connectionConfiguration = new ConnectionStrings();
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();
            connectionConfiguration.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);
            var stub = new ClaimNumberRepository(this.dbContext, connectionConfiguration, SystemClock.Instance);
            return stub;
        }
    }
}
