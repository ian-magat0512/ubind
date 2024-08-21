// <copyright file="LoginAttemptResultEmailRequestRecordRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class LoginAttemptResultEmailRequestRecordRepositoryIntegrationTests
    {
        private Guid tenantId = Guid.NewGuid();
        private Guid organisationId = Guid.NewGuid();
        private string email = "u1@test.com";

        [Fact]
        public void GetLatestLoginAttempts_ReturnsSingleFailure_WhenOnlySingleFailureHasBeenPersisted()
        {
            // Arrange
            var clock = SystemClock.Instance;
            string failureReason = "fail";
            var failedLoginAttempt = LoginAttemptResult.CreateFailure(
                 this.tenantId, this.organisationId, this.email, null, clock.Now(), failureReason);
            var dbContext1 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo1 = new EmailRequestRecordRepository<LoginAttemptResult>(dbContext1);
            repo1.Insert(failedLoginAttempt);
            repo1.SaveChanges();
            var dbContext2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new EmailRequestRecordRepository<LoginAttemptResult>(dbContext2);

            // Act
            var latestLoginAttempts = sut.GetLatestRecords(this.tenantId, this.organisationId, this.email);

            // Assert
            Assert.False(latestLoginAttempts.First().Succeeded);
            Assert.Equal(failureReason, latestLoginAttempts.First().Error);
        }

        [Fact]
        public void GetLatestLoginAttempts_ReturnsSingleSuccess_WhenOnlySingleSuccessPersisted()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var failedLoginAttempt = LoginAttemptResult.CreateSuccess(
                this.tenantId, this.organisationId, this.email, null, clock.Now());
            var dbContext1 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo1 = new EmailRequestRecordRepository<LoginAttemptResult>(dbContext1);
            repo1.Insert(failedLoginAttempt);
            repo1.SaveChanges();
            var dbContext2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new EmailRequestRecordRepository<LoginAttemptResult>(dbContext2);

            // Act
            var latestLoginAttempts = sut.GetLatestRecords(this.tenantId, this.organisationId, this.email);

            // Assert
            Assert.True(latestLoginAttempts.First().Succeeded);
        }

        [Fact]
        public void GetLatestLoginAttempts_ReturnsOnlyFiveResults_WhenMoreThanFivePersistedPersisted()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var dbContext1 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo1 = new EmailRequestRecordRepository<LoginAttemptResult>(dbContext1);
            for (var attempt = 1; attempt <= 10; ++attempt)
            {
                var failedLoginAttempt = LoginAttemptResult.CreateSuccess(
                    this.tenantId, this.organisationId, this.email, null, clock.Now());
                repo1.Insert(failedLoginAttempt);
            }

            repo1.SaveChanges();
            var dbContext2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new EmailRequestRecordRepository<LoginAttemptResult>(dbContext2);

            // Act
            var latestLoginAttempts = sut.GetLatestRecords(this.tenantId, this.organisationId, this.email);

            // Assert
            Assert.Equal(5, latestLoginAttempts.Count());
        }

        [Fact]
        public void GetLatestLoginAttempts_ReturnsMostRecentFiveResults_WhenMoreThanFivePersistedPersisted()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var dbContext1 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo1 = new EmailRequestRecordRepository<LoginAttemptResult>(dbContext1);
            for (var attempt = 1; attempt <= 5; ++attempt)
            {
                var failedLoginAttempt = LoginAttemptResult.CreateSuccess(
                    this.tenantId, this.organisationId, this.email, null, clock.Now());
                repo1.Insert(failedLoginAttempt);
            }

            for (var attempt = 1; attempt <= 5; ++attempt)
            {
                var failedLoginAttempt = LoginAttemptResult.CreateFailure(
                    this.tenantId, this.organisationId, this.email, null, clock.Now().PlusNanoseconds(1000), "fail");
                repo1.Insert(failedLoginAttempt);
            }

            repo1.SaveChanges();
            var dbContext2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new EmailRequestRecordRepository<LoginAttemptResult>(dbContext2);

            // Act
            var latestLoginAttempts = sut.GetLatestRecords(this.tenantId, this.organisationId, this.email);

            // Assert
            Assert.True(latestLoginAttempts.All(r => r.Succeeded == false));
        }

        [Fact]
        public void GetLatestRecords_RecordsHasNewIds_FromSuccessAndFailedLoginAttempts()
        {
            var organisationId = Guid.NewGuid();
            using (ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                for (var attempt = 1; attempt <= 5; ++attempt)
                {
                    var successLoginAttempt = LoginAttemptResult.CreateSuccess(this.tenantId, organisationId, this.email, null, stack.Clock.Now());
                    stack.LoginAttemptResultRepository.Insert(successLoginAttempt);
                }

                for (var attempt = 1; attempt <= 5; ++attempt)
                {
                    var failedLoginAttempt = LoginAttemptResult.CreateFailure(this.tenantId, organisationId, this.email, null, stack.Clock.Now().PlusNanoseconds(1000), "fail");
                    stack.LoginAttemptResultRepository.Insert(failedLoginAttempt);
                }

                stack.LoginAttemptResultRepository.SaveChanges();

                // Act
                var latestLoginAttempts = stack.LoginAttemptResultRepository.GetLatestRecords(this.tenantId, organisationId, this.email);

                // Assert
                foreach (var attempt in latestLoginAttempts)
                {
                    attempt.TenantId.Should().Be(this.tenantId);
                }
            }
        }
    }
}
