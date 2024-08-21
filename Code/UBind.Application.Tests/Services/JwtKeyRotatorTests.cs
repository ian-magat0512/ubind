// <copyright file="JwtKeyRotatorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;
    using FluentAssertions;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class JwtKeyRotatorTests
    {
        private readonly Mock<IJwtKeyRepository> jwtKeyRepositoryMock = new Mock<IJwtKeyRepository>();
        private readonly Mock<IClock> clockMock = new Mock<IClock>();
        private readonly Mock<IUBindDbContext> dbContextMock = new Mock<IUBindDbContext>();
        private readonly Mock<IRecurringJobManager> recurringJobManagerMock = new Mock<IRecurringJobManager>();
        private readonly Mock<ILogger<JwtKeyRotator>> loggerMock = new Mock<ILogger<JwtKeyRotator>>();
        private readonly IClock clock;

        public JwtKeyRotatorTests()
        {
            this.clock = new TestClock();
            var transactionStack = new Stack<TransactionScope>();
            this.dbContextMock.SetupGet(s => s.TransactionStack).Returns(transactionStack);
        }

        [Fact]
        public void RotateKeys_ShouldRotate_WhenKeyAgeExceedsRotationDuration()
        {
            // Arrange
            var now = this.clock.Now();
            this.clockMock.Setup(x => x.GetCurrentInstant()).Returns(now);
            var oldJwtKey = new JwtKey(Guid.NewGuid(), "oldKey", now - Duration.FromDays(91));
            this.jwtKeyRepositoryMock.Setup(x => x.GetActiveKeys()).Returns(new List<JwtKey> { oldJwtKey });
            var jwtKeyRotator = new JwtKeyRotator(
                this.jwtKeyRepositoryMock.Object,
                this.clockMock.Object,
                this.dbContextMock.Object,
                this.recurringJobManagerMock.Object,
                this.loggerMock.Object);

            // Act
            jwtKeyRotator.RotateKeys();

            // Assert
            oldJwtKey.IsRotated.Should().BeTrue();
            this.dbContextMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Fact]
        public void RotateKeys_ShouldExpire_WhenKeyAgeExceedsExpirationDuration()
        {
            // Arrange
            var now = this.clock.Now();
            this.clockMock.Setup(x => x.GetCurrentInstant()).Returns(now);
            var keyToRotate = new JwtKey(Guid.NewGuid(), "key2", now - Duration.FromDays(91));
            var keyToExpire = new JwtKey(Guid.NewGuid(), "key1", now - Duration.FromDays(181));
            this.jwtKeyRepositoryMock.Setup(x => x.GetActiveKeys()).Returns(new List<JwtKey> { keyToRotate, keyToExpire });
            var jwtKeyRotator = new JwtKeyRotator(
                this.jwtKeyRepositoryMock.Object,
                this.clockMock.Object,
                this.dbContextMock.Object,
                this.recurringJobManagerMock.Object,
                this.loggerMock.Object);

            // Act
            jwtKeyRotator.RotateKeys();

            // Assert
            keyToExpire.IsExpired.Should().BeTrue();
            this.dbContextMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Fact]
        public void RotateKeys_ShouldGenerateNewKey_WhenNoActiveKeysExists()
        {
            // Arrange
            var now = this.clock.Now();
            this.clockMock.Setup(x => x.GetCurrentInstant()).Returns(now);
            this.jwtKeyRepositoryMock.Setup(x => x.GetActiveKeys()).Returns(new List<JwtKey>());
            var jwtKeyRotator = new JwtKeyRotator(
                this.jwtKeyRepositoryMock.Object,
                this.clockMock.Object,
                this.dbContextMock.Object,
                this.recurringJobManagerMock.Object,
                this.loggerMock.Object);

            // Act
            jwtKeyRotator.RotateKeys();

            // Assert
            this.jwtKeyRepositoryMock.Verify(x => x.AddKey(It.IsAny<JwtKey>()), Times.Once());
            this.dbContextMock.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Fact]
        public void RotateKeys_ShouldGenerateNewKey_WhenOnlyOneActiveRotatedKeyExists()
        {
            // Arrange
            var now = this.clock.Now();
            this.clockMock.Setup(x => x.GetCurrentInstant()).Returns(now);
            var rotatedJwtKey = new JwtKey(Guid.NewGuid(), "rotatedKey", now - Duration.FromDays(91)) { IsRotated = true };
            this.jwtKeyRepositoryMock.Setup(x => x.GetActiveKeys()).Returns(new List<JwtKey> { rotatedJwtKey });
            var jwtKeyRotator = new JwtKeyRotator(
                this.jwtKeyRepositoryMock.Object,
                this.clockMock.Object,
                this.dbContextMock.Object,
                this.recurringJobManagerMock.Object,
                this.loggerMock.Object);

            // Act
            jwtKeyRotator.RotateKeys();

            // Assert
            this.jwtKeyRepositoryMock.Verify(x => x.AddKey(It.IsAny<JwtKey>()), Times.Once());
            this.dbContextMock.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}
