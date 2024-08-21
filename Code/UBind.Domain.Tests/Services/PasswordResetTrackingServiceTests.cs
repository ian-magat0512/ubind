// <copyright file="PasswordResetTrackingServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class PasswordResetTrackingServiceTests
    {
        private Tenant tenant = TenantFactory.Create();
        private string email = "u1@test.com";

        [Fact]
        public void ShouldBlockRequest_ReturnsFalse_WhenNoRecords()
        {
            // Arrange
            var attemptRepository = new FakePasswordResetRepository();
            var trackingService = new PasswordResetTrackingService(attemptRepository, SystemClock.Instance);

            // Act
            var block = trackingService.ShouldBlockRequest(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                this.email,
                3,
                1);

            // Assert
            Assert.False(block);
        }

        [Fact]
        public void ShouldBlockRequest_ReturnsFalse_WhenOldAttemptsArePartOfTheRecordAttempt()
        {
            // Arrange
            var clock = new TestClock();
            var attemptRepository = new FakePasswordResetRepository();
            var trackingService = new PasswordResetTrackingService(attemptRepository, clock);
            var organisationId = this.tenant.Details.DefaultOrganisationId;

            // old attempt
            clock.Increment(Duration.FromDays(-300));

            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            clock.StartTrackingSystemTime();
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);

            // Act
            var block = trackingService.ShouldBlockRequest(
                this.tenant.Id,
                organisationId,
                this.email,
                3,
                1);

            // Assert
            Assert.False(block);
        }

        [Fact]
        public void ShouldBlockRequest_ReturnFalse_WhenConsecutivePasswordResetWithGoodTimingInbetween()
        {
            // Arrange
            var clock = new TestClock();
            var attemptRepository = new FakePasswordResetRepository();
            var trackingService = new PasswordResetTrackingService(attemptRepository, clock);
            var organisationId = this.tenant.Details.DefaultOrganisationId;

            clock.StopTrackingSystemTime();
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            clock.Increment(Duration.FromSeconds(30));
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            clock.Increment(Duration.FromSeconds(30.001));
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            trackingService = new PasswordResetTrackingService(attemptRepository, SystemClock.Instance);

            // Act
            var block = trackingService.ShouldBlockRequest(
                this.tenant.Id,
                organisationId,
                this.email,
                3,
                1);

            // Assert
            Assert.False(block);
        }

        [Fact]
        public void ShouldBlockRequest_ReturnsFalse_WhenWaitOutTheResetPassword()
        {
            // Arrange
            var clock = new TestClock();
            var attemptRepository = new FakePasswordResetRepository();
            var trackingService = new PasswordResetTrackingService(attemptRepository, clock);
            var organisationId = this.tenant.Details.DefaultOrganisationId;

            clock.StopTrackingSystemTime();

            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);

            clock.Increment(Duration.FromSeconds(60));

            // Act
            var block = trackingService.ShouldBlockRequest(
                this.tenant.Id,
                organisationId,
                this.email,
                3,
                1);

            // Assert
            Assert.False(block);
        }

        [Fact]
        public void ShouldBlockRequest_ReturnTrue_WhenWaitOutTheResetPasswordBlockButASecondTooShort()
        {
            // Arrange
            var clock = new TestClock();
            var attemptRepository = new FakePasswordResetRepository();
            var trackingService = new PasswordResetTrackingService(attemptRepository, clock);
            var organisationId = this.tenant.Details.DefaultOrganisationId;

            clock.StopTrackingSystemTime();

            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);

            clock.Increment(Duration.FromSeconds(59));

            // Act
            var block = trackingService.ShouldBlockRequest(
                this.tenant.Id,
                organisationId,
                this.email,
                3,
                1);

            // Assert
            Assert.True(block);
        }

        [Fact]
        public void ShouldBlockRequest_ReturnTrue_WhenThresholdTriggeredEvenIfOnlyOneRequestWasInMostRecentPeriod()
        {
            // Arrange
            var clock = new TestClock();
            var attemptRepository = new FakePasswordResetRepository();
            var trackingService = new PasswordResetTrackingService(attemptRepository, clock);
            var organisationId = this.tenant.Details.DefaultOrganisationId;

            clock.StopTrackingSystemTime();

            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);

            clock.Increment(Duration.FromMinutes(29));
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);

            clock.Increment(Duration.FromMinutes(29));

            // Act
            var block = trackingService.ShouldBlockRequest(
                this.tenant.Id,
                organisationId,
                this.email,
                3,
                30);

            // Assert
            Assert.True(block);
        }

        [Fact]
        public void ShouldBlockRequest_ReturnFalse_WhenThresholdTriggeredEvenIfOnlyOneRequestWasInMostRecentPeriod()
        {
            // Arrange
            var clock = new TestClock();
            var attemptRepository = new FakePasswordResetRepository();
            var trackingService = new PasswordResetTrackingService(attemptRepository, clock);
            var organisationId = this.tenant.Details.DefaultOrganisationId;

            clock.StopTrackingSystemTime();
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);

            clock.Increment(Duration.FromMinutes(29));
            trackingService.Record(this.tenant.Id, organisationId, this.email, null);

            clock.Increment(Duration.FromMinutes(29));

            // Act
            var block = trackingService.ShouldBlockRequest(
                this.tenant.Id,
                organisationId,
                this.email,
                3,
                30);

            // Assert
            Assert.True(block);
        }

        private class FakePasswordResetRepository : IEmailRequestRecordRepository<PasswordResetRecord>
        {
            private readonly List<PasswordResetRecord> passwordResets = new List<PasswordResetRecord>();

            public IEnumerable<PasswordResetRecord> GetLatestRecords(Guid tenantId, Guid organisationId, string emailAddress, int max = 5)
            {
                return this.passwordResets
                    .Where(r => r.TenantId == tenantId)
                   .Where(r => r.OrganisationId == organisationId)
                   .Where(r => r.EmailAddress == emailAddress)
                   .OrderByDescending(r => r.CreatedTicksSinceEpoch)
                   .Take(max)
                   .ToList();
            }

            public void Insert(PasswordResetRecord loginAttempt)
            {
                this.passwordResets.Add(loginAttempt);
            }

            public void SaveChanges()
            {
                // No op
            }
        }
    }
}
