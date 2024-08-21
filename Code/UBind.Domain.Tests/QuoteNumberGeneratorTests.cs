// <copyright file="QuoteNumberGeneratorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests
{
    using System;
    using FluentAssertions;
    using Moq;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class QuoteNumberGeneratorTests
    {
        private const DeploymentEnvironment Environment = DeploymentEnvironment.Staging;
        private Mock<IUniqueNumberSequenceGenerator> seedGenerator;
        private IQuoteReferenceNumberGenerator sut;

        public QuoteNumberGeneratorTests()
        {
            this.seedGenerator = new Mock<IUniqueNumberSequenceGenerator>();
            this.sut = new QuoteReferenceNumberGenerator(this.seedGenerator.Object);
        }

        /// <summary>
        /// Test for six letter code with known seeds.
        /// </summary>
        /// <param name="seed">Seed value.</param>
        /// <param name="expectedOutput">Expected quote number value.</param>
        [Theory]
        [InlineData(0, "NKVCWA")]
        [InlineData(1, "QUIYLR")]
        [InlineData(2, "UDWUBI")]
        [InlineData(10, "VCDKWO")]
        public void GenerateQuoteNumber_CorrectCodes_ForSixLetterCodeMethodWithKnownSeeds(int seed, string expectedOutput)
        {
            // Arrange
            this.seedGenerator
                .Setup(g => g.Next(TenantFactory.DefaultId, ProductFactory.DefaultId, Environment, UniqueNumberUseCase.QuoteNumber))
                .Returns(seed);

            // Act
            this.sut.SetProperties(TenantFactory.DefaultId, ProductFactory.DefaultId, Environment);
            var quoteNumber = this.sut.Generate();

            // Assert
            Assert.Equal(expectedOutput, quoteNumber);
        }

        /// <summary>
        /// Test for Tenant and Product id based Codes.
        /// </summary>
        [Fact]
        public void GenerateQuoteNumber_TenantAndProductBasedCodes_ForSixLetterCodeMethodWithKnownSeeds()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenantId2 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var tenantId3 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            this.seedGenerator
                .Setup(g => g.Next(tenantId, productId, Environment, UniqueNumberUseCase.QuoteNumber))
                .Returns(0);
            this.seedGenerator
                .Setup(g => g.Next(tenantId2, productId2, Environment, UniqueNumberUseCase.QuoteNumber))
                .Returns(0);

            // Act
            this.sut.SetProperties(tenantId, productId, Environment);
            var qn = this.sut.Generate();
            this.sut.SetProperties(tenantId, productId, Environment);
            var qn2 = this.sut.Generate();
            this.sut.SetProperties(tenantId2, productId2, Environment);
            var qn3 = this.sut.Generate();
            this.sut.SetProperties(Guid.NewGuid(), Guid.NewGuid(), Environment);
            var qn4 = this.sut.Generate();

            // Arrange 2
            this.seedGenerator
                .Setup(g => g.Next(tenantId, productId, Environment, UniqueNumberUseCase.QuoteNumber))
                .Returns(1);
            this.seedGenerator
                .Setup(g => g.Next(Guid.NewGuid(), Guid.NewGuid(), Environment, UniqueNumberUseCase.QuoteNumber))
                .Returns(1);

            // Act 2
            this.sut.SetProperties(tenantId, productId, Environment);
            var qn5 = this.sut.Generate();

            // Assert
            Assert.Equal(qn, qn2);
            Assert.NotEqual(qn2, qn3);
            Assert.NotEqual(qn, qn4);
            Assert.NotEqual(qn, qn5);
        }

        [Fact]
        public void GenerateQuoteNumber_ResultShouldBe_SixDigitAlphaNumeric()
        {
            // Arrange
            int numberOfDigit = 6;
            var numberGenerator = new QuoteReferenceNumberGenerator(new FakeUniqueNumberSequenceGenerator());

            // Act
            numberGenerator.SetProperties(Guid.NewGuid(), Guid.NewGuid(), DeploymentEnvironment.Development);

            for (int i = 0; i < 1000000; i++)
            {
                var result = numberGenerator.Generate();

                // Assert
                result.Length.Should().Be(numberOfDigit);
            }
        }

        class FakeUniqueNumberSequenceGenerator : IUniqueNumberSequenceGenerator
        {
            private int seed;

            public int Next(Guid tenantId, Guid productId, DeploymentEnvironment environment, UniqueNumberUseCase useCase)
            {
                return this.seed++;
            }

            public int Next(Guid tenantId, DeploymentEnvironment environment, UniqueNumberUseCase useCase)
            {
                return this.seed++;
            }
        }
    }
}
