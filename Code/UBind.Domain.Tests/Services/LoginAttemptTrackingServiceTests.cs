// <copyright file="LoginAttemptTrackingServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using NodaTime;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class LoginAttemptTrackingServiceTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Guid organisationId = Guid.NewGuid();
        private readonly string email = "u1@test.com";
        private readonly TestClock clock = new TestClock();
        private Guid tenantId;

        public LoginAttemptTrackingServiceTests()
        {
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public void IsEmailBlocked_Is_False_No_EmailBlockingEvent_Occur()
        {
            // Arrange
            var mockLoginAttemptResultRepository = new Mock<IEmailRequestRecordRepository<LoginAttemptResult>>();
            var mockEmailBlockingEventRepository = new Mock<IEmailAddressBlockingEventRepository>();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));
            var mockOrganisationReadModelRepository = this.SetupGetOrganisationAggregateRepository(organisation);
            var mockUserSystemEventEmitter = this.SetupGetUserSystemEventEmitter();
            var mockUserLoginEmailRepository = this.SetupGetUserLoginEmailRepository();

            // Act
            var lockingTrackingservice = new LoginAttemptTrackingService(
                mockLoginAttemptResultRepository.Object,
                mockEmailBlockingEventRepository.Object,
                mockOrganisationReadModelRepository.Object,
                mockUserSystemEventEmitter.Object,
                mockUserLoginEmailRepository.Object,
                this.clock);
            var isEmailBlocked = lockingTrackingservice.IsLoginAttemptEmailBlocked(
                tenant.Id, this.organisationId, this.email);

            // Assert
            Assert.False(isEmailBlocked);
        }

        [Fact]
        public void IsEmailBlocked_Is_False_After_AccountUnlockedEvent_Occur()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));
            var mockOrganisationReadModelRepository = this.SetupGetOrganisationAggregateRepository(organisation);
            var mockUserSystemEventEmitter = this.SetupGetUserSystemEventEmitter();
            var mockUserLoginEmailRepository = this.SetupGetUserLoginEmailRepository();

            var accountUnlockedEvent = EmailAddressBlockingEvent.EmailAddressUnblocked(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                this.email,
                EmailAddressUnblockedReason.EmailAddressUnblockedByAdministrator,
                this.clock.GetCurrentInstant());
            fakeEmailBlockingEventRepository.Insert(accountUnlockedEvent);

            // Act
            var lockingTrackingservice = new LoginAttemptTrackingService(
                fakeLoginAttemptResultRepository,
                fakeEmailBlockingEventRepository,
                mockOrganisationReadModelRepository.Object,
                mockUserSystemEventEmitter.Object,
                mockUserLoginEmailRepository.Object,
                this.clock);
            var isEmailBlocked = lockingTrackingservice.IsLoginAttemptEmailBlocked(
                tenant.Id, this.organisationId, this.email);

            // Assert
            Assert.False(isEmailBlocked);
        }

        [Fact]
        public void IsEmailBlocked_Is_True_After_AccountLockedEvent_Occur()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));
            var mockOrganisationReadModelRepository = this.SetupGetOrganisationAggregateRepository(organisation);
            var mockUserSystemEventEmitter = this.SetupGetUserSystemEventEmitter();
            var mockUserLoginEmailRepository = this.SetupGetUserLoginEmailRepository();

            var accountLockedEvent = EmailAddressBlockingEvent.EmailAddressBlocked(
                tenant.Id,
                this.organisationId,
                this.email,
                EmailAddressUnblockedReason.EmailAddressBlockedDueToMaximumUnsuccessfulAuthenticationAttempts,
                SystemClock.Instance.GetCurrentInstant());
            fakeEmailBlockingEventRepository.Insert(accountLockedEvent);

            // Act
            var lockingTrackingservice = new LoginAttemptTrackingService(
                fakeLoginAttemptResultRepository,
                fakeEmailBlockingEventRepository,
                mockOrganisationReadModelRepository.Object,
                mockUserSystemEventEmitter.Object,
                mockUserLoginEmailRepository.Object,
                this.clock);
            var isEmailBlocked = lockingTrackingservice.IsLoginAttemptEmailBlocked(
                tenant.Id, this.organisationId, this.email);
            var latestBlockingEvent = fakeEmailBlockingEventRepository.GetLatestBlockingEvent(
                tenant.Id, this.organisationId, this.email);

            // Assert
            Assert.True(isEmailBlocked);
            Assert.True(latestBlockingEvent.EmailAddressUnblockedReason == 2);
        }

        [Fact]
        public async Task IsEmailBlocked_Is_True_After_Six_RecordLoginFailureAndBlockEmailIfNecessary_Occurrence()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));
            var mockOrganisationReadModelRepository = this.SetupGetOrganisationAggregateRepository(organisation);
            var mockUserSystemEventEmitter = this.SetupGetUserSystemEventEmitter();
            var mockUserLoginEmailRepository = this.SetupGetUserLoginEmailRepository();

            var emailAddressUnblockedEvent = EmailAddressBlockingEvent.EmailAddressBlocked(
                tenant.Id,
                this.organisationId,
                this.email,
                EmailAddressUnblockedReason.EmailAddressBlockedDueToMaximumUnsuccessfulAuthenticationAttempts,
                SystemClock.Instance.GetCurrentInstant());
            fakeEmailBlockingEventRepository.Insert(emailAddressUnblockedEvent);

            // Act
            var lockingTrackingservice = new LoginAttemptTrackingService(
                fakeLoginAttemptResultRepository,
                fakeEmailBlockingEventRepository,
                mockOrganisationReadModelRepository.Object,
                mockUserSystemEventEmitter.Object,
                mockUserLoginEmailRepository.Object,
                this.clock);

            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            var isEmailBlocked = lockingTrackingservice.IsLoginAttemptEmailBlocked(
                tenant.Id, this.organisationId, this.email);

            // Assert
            Assert.True(isEmailBlocked);
        }

        [Fact]
        public async Task IsEmailBlocked_Is_False_After_Less_Than_Six_RecordLoginFailureAndBlockEmailIfNecessary_Occurrence()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));
            var mockOrganisationReadModelRepository = this.SetupGetOrganisationAggregateRepository(organisation);
            var mockUserSystemEventEmitter = this.SetupGetUserSystemEventEmitter();
            var mockUserLoginEmailResporitory = this.SetupGetUserLoginEmailRepository();
            var emailAddressUnblockedEvent = EmailAddressBlockingEvent.EmailAddressBlocked(
                tenant.Id,
                this.organisationId,
                this.email,
                EmailAddressUnblockedReason.EmailAddressBlockedDueToMaximumUnsuccessfulAuthenticationAttempts,
                SystemClock.Instance.GetCurrentInstant());
            fakeEmailBlockingEventRepository.Insert(emailAddressUnblockedEvent);

            // Act
            var lockingTrackingservice = new LoginAttemptTrackingService(
                fakeLoginAttemptResultRepository,
                fakeEmailBlockingEventRepository,
                mockOrganisationReadModelRepository.Object,
                mockUserSystemEventEmitter.Object,
                mockUserLoginEmailResporitory.Object,
                this.clock);
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            await lockingTrackingservice.RecordLoginFailureAndBlockEmailIfNecessary(
                this.tenantId, this.organisationId, this.email, null, "fail", this.clock.GetCurrentInstant());
            var isEmailBlocked = lockingTrackingservice.IsLoginAttemptEmailBlocked(
                tenant.Id, this.organisationId, this.email);

            // Assert
            Assert.True(isEmailBlocked);
        }

        [Fact]
        public void LockingStatus_ShouldAccountBeLockedOnFailedLogin_IsFalse_When_Account_Is_Already_Locked()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            // first login attempt
            fakeEmailBlockingEventRepository.Insert(
                EmailAddressBlockingEvent.EmailAddressBlocked(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    this.email,
                    EmailAddressUnblockedReason.EmailAddressBlockedDueToMaximumUnsuccessfulAuthenticationAttempts,
                    this.clock.GetCurrentInstant()));

            var latestLoginAttempts = fakeLoginAttemptResultRepository.GetLatestRecords(
                tenant.Id, this.organisationId, this.email);
            var latestBlockingEvent = fakeEmailBlockingEventRepository.GetLatestBlockingEvent(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.email);

            // Act
            var blockingStatus = new EmailAddressBlockingStatus(
                latestLoginAttempts, latestBlockingEvent, SystemClock.Instance);

            // Assert
            Assert.False(blockingStatus.ShouldEmailAddressBeBlockedOnNextFailedLogin);
        }

        [Fact]
        public void LockingStatus_ShouldAccountBeLockedOnFailedLogin_IsFalse_When_Login_Attempt_Less_Than_Five()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            // first login attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateFailure(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // second login attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateFailure(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // third login attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateFailure(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // fourth login attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateSuccess(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant()));

            var latestLoginAttempts = fakeLoginAttemptResultRepository.GetLatestRecords(
                this.tenantId, this.organisationId, this.email);
            var latestLockingEvent = fakeEmailBlockingEventRepository.GetLatestBlockingEvent(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.email);

            // Act
            var blockingStatus = new EmailAddressBlockingStatus(latestLoginAttempts, latestLockingEvent, SystemClock.Instance);

            // Assert
            Assert.False(blockingStatus.ShouldEmailAddressBeBlockedOnNextFailedLogin);
        }

        [Fact]
        public void LockingStatus_ShouldAccountBeLockedOnFailedLogin_IsFalse_When_Login_Attempt_Greater_Than_Five_With_One_SuccessAttempt()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            // first login attempt
            fakeLoginAttemptResultRepository.Insert(
                 LoginAttemptResult.CreateFailure(
                     this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // second login attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateFailure(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // third login attempt
            fakeLoginAttemptResultRepository.Insert(
                 LoginAttemptResult.CreateFailure(
                     this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // fourth login attempt: Success Attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateSuccess(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant()));

            // fifth login attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateFailure(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // sixth login attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateFailure(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            var latestLoginAttempts = fakeLoginAttemptResultRepository.GetLatestRecords(
                this.tenantId, this.organisationId, this.email);
            var latestLockingEvent = fakeEmailBlockingEventRepository.GetLatestBlockingEvent(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.email);

            // Act
            var blockingStatus = new EmailAddressBlockingStatus(latestLoginAttempts, latestLockingEvent, SystemClock.Instance);

            // Assert
            Assert.False(blockingStatus.ShouldEmailAddressBeBlockedOnNextFailedLogin);
        }

        [Fact]
        public void LockingStatus_ShouldAccountBeLockedOnFailedLogin_IsTrue_When_Login_Attempt_Greater_Than_Five_With_No_SuccessAttempt()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();

            var tenant = TenantFactory.Create(this.tenantId);
            var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                this.clock.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, this.clock.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            // first login attempt
            fakeLoginAttemptResultRepository.Insert(
                LoginAttemptResult.CreateFailure(
                    this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // second login attempt
            fakeLoginAttemptResultRepository.Insert(
              LoginAttemptResult.CreateFailure(
                  this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // third login attempt
            fakeLoginAttemptResultRepository.Insert(
              LoginAttemptResult.CreateFailure(
                  this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // fourth login attempt
            fakeLoginAttemptResultRepository.Insert(
               LoginAttemptResult.CreateFailure(
                  this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // fifth login attempt
            fakeLoginAttemptResultRepository.Insert(
              LoginAttemptResult.CreateFailure(
                  this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            // sixth login attempt
            fakeLoginAttemptResultRepository.Insert(
              LoginAttemptResult.CreateFailure(
                  this.tenantId, this.organisationId, this.email, null, this.clock.GetCurrentInstant(), "Fail"));

            var latestLoginAttempts = fakeLoginAttemptResultRepository.GetLatestRecords(
                this.tenantId, this.organisationId, this.email);
            var latestLockingEvent = fakeEmailBlockingEventRepository.GetLatestBlockingEvent(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.email);

            // Act
            var blockingStatus = new EmailAddressBlockingStatus(latestLoginAttempts, latestLockingEvent, SystemClock.Instance);

            // Assert
            Assert.True(blockingStatus.ShouldEmailAddressBeBlockedOnNextFailedLogin);
        }

        private Mock<IOrganisationAggregateRepository> SetupGetOrganisationAggregateRepository(Organisation organisation)
        {
            var mockOrganisationAggregateRepository = new Mock<IOrganisationAggregateRepository>();
            mockOrganisationAggregateRepository.Setup(o => o.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(organisation);
            return mockOrganisationAggregateRepository;
        }

        private Mock<IUserSystemEventEmitter> SetupGetUserSystemEventEmitter()
        {
            var mockUserSystemEventEmitter = new Mock<IUserSystemEventEmitter>();
            mockUserSystemEventEmitter
                .Setup(o => o.CreateAndEmitSystemEvents(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<SystemEventType>>(), It.IsAny<Guid>(), It.IsAny<Instant>())).Returns(Task.FromResult(0));
            mockUserSystemEventEmitter
               .Setup(o => o.CreateAndEmitLoginSystemEvents(
                   It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<SystemEventType>>(), It.IsAny<Instant>())).Returns(Task.FromResult(0));
            mockUserSystemEventEmitter
               .Setup(o => o.CreateAndEmitLoginSystemEvents(
                   It.IsAny<UserReadModel>(), It.IsAny<List<SystemEventType>>(), It.IsAny<Instant>())).Returns(Task.FromResult(0));
            return mockUserSystemEventEmitter;
        }

        private Mock<IUserLoginEmailRepository> SetupGetUserLoginEmailRepository()
        {
            var mock = new Mock<IUserLoginEmailRepository>();
            mock.Setup(o => o.GetUserLoginByEmail(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(It.IsAny<UserLoginEmail>());
            return mock;
        }

        private class FakeLoginAttemptResultRepository : IEmailRequestRecordRepository<LoginAttemptResult>
        {
            private readonly List<LoginAttemptResult> loginAttemptResults = new List<LoginAttemptResult>();

            public IEnumerable<LoginAttemptResult> GetLatestRecords(
                Guid tenantId, Guid organisationId, string emailAddress, int max = 5)
            {
                return this.loginAttemptResults
                   .Where(r => r.TenantId == tenantId)
                   .Where(r => r.OrganisationId == organisationId)
                   .Where(r => r.EmailAddress == emailAddress)
                   .OrderByDescending(r => r.CreatedTicksSinceEpoch)
                   .Take(max)
                   .ToList();
            }

            public void Insert(LoginAttemptResult loginAttempt)
            {
                this.loginAttemptResults.Add(loginAttempt);
            }

            public void SaveChanges()
            {
                // No op
            }
        }

        private class FakePasswordResetRepository : IEmailRequestRecordRepository<PasswordResetRecord>
        {
            private readonly List<PasswordResetRecord> passwordResets = new List<PasswordResetRecord>();

            public IEnumerable<PasswordResetRecord> GetLatestRecords(
                Guid tenantId, Guid organisationId, string emailAddress, int max = 5)
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

        private class FakeEmailBlockingEventRepository : IEmailAddressBlockingEventRepository
        {
            private readonly List<EmailAddressBlockingEvent> emailBlockingEvents = new List<EmailAddressBlockingEvent>();

            public EmailAddressBlockingEvent GetLatestBlockingEvent(Guid tenantId, Guid organisationId, string emailAddress)
            {
                return this.emailBlockingEvents
                   .Where(e => e.TenantId == tenantId)
                   .Where(e => e.OrganisationId == organisationId)
                   .Where(e => e.EmailAddress == emailAddress)
                   .OrderByDescending(e => e.CreatedTicksSinceEpoch)
                   .FirstOrDefault();
            }

            public void Insert(EmailAddressBlockingEvent lockingEvent)
            {
                this.emailBlockingEvents.Add(lockingEvent);
            }

            public void SaveChanges()
            {
                // No op
            }
        }
    }
}
