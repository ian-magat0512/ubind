// <copyright file="UniqueNumberSequenceGeneratorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using UBind.Domain;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class UniqueNumberSequenceGeneratorTests
    {
        [Fact]
        public void Next_GeneratesZero_ForUnknownProductEnvironmentMethodCombos()
        {
            // Arrange
            var sut = new UniqueNumberSequenceGenerator(DatabaseFixture.TestConnectionString);

            // Arrange
            var seed = sut.Next(TenantFactory.DefaultId, TenantFactory.DefaultId, DeploymentEnvironment.Staging, UniqueNumberUseCase.QuoteNumber);

            // Act
            Assert.Equal(0, seed);
        }

        [Fact]
        public void Next_GeneratesSequentialNumbers_ForMatchingProductEnvironmentMethodCombos()
        {
            // Arrange
            var sut = new UniqueNumberSequenceGenerator(DatabaseFixture.TestConnectionString);
            Guid tenantId = Guid.NewGuid();
            Guid productId = Guid.NewGuid();

            // Arrange
            var seed1 = sut.Next(tenantId, productId, DeploymentEnvironment.Staging, UniqueNumberUseCase.QuoteNumber);
            var seed2 = sut.Next(tenantId, productId, DeploymentEnvironment.Staging, UniqueNumberUseCase.QuoteNumber);
            var seed3 = sut.Next(tenantId, productId, DeploymentEnvironment.Staging, UniqueNumberUseCase.QuoteNumber);

            // Act
            Assert.Equal(0, seed1);
            Assert.Equal(1, seed2);
            Assert.Equal(2, seed3);
        }
    }
}
