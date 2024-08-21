// <copyright file="EmailAddressBlockingEventRepositoryIntegrationTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class EmailAddressBlockingEventRepositoryIntegrationTest
    {
        [Fact]
        public void GetLatestLockingEvent_ReturnsLockingEvent_WhenSingleUnlockingEventPersisted()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            string email = "eberit-tenant1-u1@test.com";
            var context1 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo = new EmailAddressBlockingEventRepository(context1);
            var lockAccount = EmailAddressBlockingEvent.EmailAddressBlocked(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                email,
                EmailAddressUnblockedReason.EmailAddressBlockedDueToMaximumUnsuccessfulAuthenticationAttempts,
                SystemClock.Instance.GetCurrentInstant());
            repo.Insert(lockAccount);
            repo.SaveChanges();
            var context2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new EmailAddressBlockingEventRepository(context2);

            // Act
            var accountStatus = sut.GetLatestBlockingEvent(tenant.Id, tenant.Details.DefaultOrganisationId, email);

            // Assert
            Assert.True(accountStatus.IsEmailAddressBlocked);
        }

        [Fact]
        public void GetLatestLockingEvent_ReturnsUnlockingEvent_WhenSingleUnlockingEventPersisted()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, Guid.NewGuid(),
                new TestClock().GetCurrentInstant());
            tenant.SetDefaultOrganisation(
                organisation.Id, new TestClock().GetCurrentInstant());

            string email = "eberit-tenant2-u2@test.com";
            var context1 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo = new EmailAddressBlockingEventRepository(context1);
            var lockAccount = EmailAddressBlockingEvent.EmailAddressUnblocked(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                email,
                EmailAddressUnblockedReason.EmailAddressUnblockedDueToSuccessfulAuthenticationAfterTimeout,
                SystemClock.Instance.GetCurrentInstant());
            repo.Insert(lockAccount);
            repo.SaveChanges();
            var context2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new EmailAddressBlockingEventRepository(context2);

            // Act
            var accountStatus = sut.GetLatestBlockingEvent(tenant.Id, tenant.Details.DefaultOrganisationId, email);

            // Assert
            Assert.False(accountStatus.IsEmailAddressBlocked);
        }

        [Fact]
        public void GetLatestLockingEvent_ReturnsLatestUnlockingEvent_WhenMultipleEventsPersisted()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            string email = "eberit-tenant3-u3@test.com";
            var clock = new TestClock();
            clock.StopTrackingSystemTime();
            var context1 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo = new EmailAddressBlockingEventRepository(context1);
            for (var i = 0; i < 10; ++i)
            {
                var lockingEvent = EmailAddressBlockingEvent.EmailAddressBlocked(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    email,
                    EmailAddressUnblockedReason.EmailAddressUnblockedDueToSuccessfulAuthenticationAfterTimeout,
                    clock.Now());
                repo.Insert(lockingEvent);
                clock.Increment(Duration.FromTicks(1));
            }

            var unlockingEvent = EmailAddressBlockingEvent.EmailAddressUnblocked(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                email,
                EmailAddressUnblockedReason.EmailAddressUnblockedByAdministrator,
                clock.Now());
            repo.Insert(unlockingEvent);
            repo.SaveChanges();
            var context2 = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new EmailAddressBlockingEventRepository(context2);

            // Act
            var latestLockingEvent = sut.GetLatestBlockingEvent(tenant.Id, tenant.Details.DefaultOrganisationId, email);

            // Assert
            Assert.False(latestLockingEvent.IsEmailAddressBlocked);
        }

        [Fact]
        public void GetLatestLockingEvent_ReturnHasNewIds_WhenCreatingEmailAddressBlockedEvent()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            string email = "eberit-tenant3-u3@test.com";
            var clock = new TestClock();
            clock.StopTrackingSystemTime();
            var context = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo = new EmailAddressBlockingEventRepository(context);
            var blockingEvent = EmailAddressBlockingEvent.EmailAddressBlocked(tenantId, organisationId, email, EmailAddressUnblockedReason.EmailAddressUnblockedDueToSuccessfulAuthenticationAfterTimeout, clock.Now());
            repo.Insert(blockingEvent);
            repo.SaveChanges();

            // Act
            var latestBlockingEvent = repo.GetLatestBlockingEvent(tenantId, organisationId, email);

            // Assert
            latestBlockingEvent.Should().NotBeNull();
            latestBlockingEvent.TenantId.Should().Be(tenantId);
            latestBlockingEvent.EmailAddress.Should().Be(email);
        }

        [Fact]
        public void GetLatestLockingEvent_ReturnHasNewIds_WhenCreatingEmailAddressUnBlockedEvent()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            string email = "eberit-tenant3-u3@test.com";
            var clock = new TestClock();
            clock.StopTrackingSystemTime();
            var context = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var repo = new EmailAddressBlockingEventRepository(context);
            var unBlockingEvent = EmailAddressBlockingEvent.EmailAddressUnblocked(tenantId, organisationId, email, EmailAddressUnblockedReason.EmailAddressUnblockedDueToSuccessfulAuthenticationAfterTimeout, clock.Now());
            repo.Insert(unBlockingEvent);
            repo.SaveChanges();

            // Act
            var latestBlockingEvent = repo.GetLatestBlockingEvent(tenantId, organisationId, email);

            // Assert
            latestBlockingEvent.Should().NotBeNull();
            latestBlockingEvent.TenantId.Should().Be(tenantId);
            latestBlockingEvent.EmailAddress.Should().Be(email);
        }
    }
}
