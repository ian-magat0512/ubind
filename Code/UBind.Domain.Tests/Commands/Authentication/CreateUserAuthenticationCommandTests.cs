// <copyright file="CreateUserAuthenticationCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Command.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Authentication;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class CreateUserAuthenticationCommandTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Tenant tenant = TenantFactory.Create();
        private readonly Guid organisationId = Guid.NewGuid();
        private readonly string password = "strongPassword123*";
        private readonly string invalidPassword = "strongPassword123#";
        private readonly string email = "u1@test.com";

        [Fact(Skip = "Ignored since tests broken by switch to using exceptions for login failures. Fix by reverting behaviour to use results.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task Account_IsLocked_After_SixConsecutiveFailedAttempt()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();
            var fakeOrganisationAggregateRepository = new FakeOrganisationAggregateRepository();
            var fakeUserUserSystemEventEmitter = new FakeUserSystemEventEmitter();
            var fakeUserLoginEmailRepository = new FakeUserLoginEmailRepository();

            // Act
            // First invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Second invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Third invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fourth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fifth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Sixth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            var latestBlockingEvent = fakeEmailBlockingEventRepository.GetLatestBlockingEvent(
                this.tenant.Id, this.tenant.Details.DefaultOrganisationId, this.email);

            // Assert
            Assert.True(latestBlockingEvent.IsEmailAddressBlocked);
        }

        [Fact(Skip = "Ignored since tests broken by switch to using exceptions for login failures. Fix by reverting behaviour to use results.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task Account_IsUnlocked_After_SixConsecutiveFailedAttempt_When_UnlockEvent_Occur()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();
            var fakeOrganisationAggregateRepository = new FakeOrganisationAggregateRepository();
            var fakeUserUserSystemEventEmitter = new FakeUserSystemEventEmitter();
            var fakeUserLoginEmailRepository = new FakeUserLoginEmailRepository();

            // Act
            // First invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Second invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Third invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fourth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fifth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Unblock event
            var unblockEvent = EmailAddressBlockingEvent.EmailAddressUnblocked(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                this.email,
                EmailAddressUnblockedReason.EmailAddressUnblockedDueToSuccessfulAuthenticationAfterTimeout,
                SystemClock.Instance.GetCurrentInstant());
            fakeEmailBlockingEventRepository.Insert(unblockEvent);

            // Sixth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Seventh invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            var latestBlockingEvent = fakeEmailBlockingEventRepository.GetLatestBlockingEvent(
                this.tenant.Id, this.tenant.Details.DefaultOrganisationId, this.email);

            // Assert
            Assert.False(latestBlockingEvent.IsEmailAddressBlocked);
        }

        [Fact(Skip = "Ignored since tests broken by switch to using exceptions for login failures. Fix by reverting behaviour to use results.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task Account_IsLocked_After_SixConsecutiveFailedAttempt_After_UnlockEvent_Occur()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();
            var fakeOrganisationAggregateRepository = new FakeOrganisationAggregateRepository();
            var fakeUserUserSystemEventEmitter = new FakeUserSystemEventEmitter();
            var fakeUserLoginEmailRepository = new FakeUserLoginEmailRepository();

            // Act
            // First invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Second invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Third invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fourth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fifth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Unlock event
            var unblockEvent = EmailAddressBlockingEvent.EmailAddressUnblocked(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                this.email,
                EmailAddressUnblockedReason.EmailAddressUnblockedDueToSuccessfulAuthenticationAfterTimeout,
                SystemClock.Instance.GetCurrentInstant());
            fakeEmailBlockingEventRepository.Insert(unblockEvent);

            // First invalid attempt after unlock event
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Second invalid attempt after unlock event
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Third invalid attempt after unlock event
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fourth invalid attempt after unlock event
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fifth invalid attempt after unlock event
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Sixth invalid attempt after unlock event
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            var latestBlockingEvent = fakeEmailBlockingEventRepository.GetLatestBlockingEvent(
                this.tenant.Id, this.tenant.Details.DefaultOrganisationId, this.email);

            // Assert
            Assert.True(latestBlockingEvent.IsEmailAddressBlocked);
        }

        [Fact(Skip = "Ignored since tests broken by switch to using exceptions for login failures. Fix by reverting behaviour to use results.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task LoginAttempt_For_ValidAttempt_Succeeded_When_LessThan_SixConsecutiveFailedAttempt()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();
            var fakeOrganisationAggregateRepository = new FakeOrganisationAggregateRepository();
            var fakeUserUserSystemEventEmitter = new FakeUserSystemEventEmitter();
            var fakeUserLoginEmailRepository = new FakeUserLoginEmailRepository();

            // Act
            // First invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Second invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Third invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fourth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Fifth invalid attempt
            await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // First valid attempt
            Func<Task<UserReadModel>> act = async () => await this.ValidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact(Skip = "Ignored since tests broken by switch to using exceptions for login failures. Fix by reverting behaviour to use results.")]
        public async Task LoginAttempt_Fail_For_ValidAtempt_When_Account_Is_Already_Locked()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();
            var fakeOrganisationAggregateRepository = new FakeOrganisationAggregateRepository();
            var fakeUserUserSystemEventEmitter = new FakeUserSystemEventEmitter();
            var fakeUserLoginEmailRepository = new FakeUserLoginEmailRepository();

            // Act
            // Locking the account
            var blockingEvent = EmailAddressBlockingEvent.EmailAddressBlocked(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                this.email,
                EmailAddressUnblockedReason.EmailAddressBlockedDueToMaximumUnsuccessfulAuthenticationAttempts,
                SystemClock.Instance.GetCurrentInstant());
            fakeEmailBlockingEventRepository.Insert(blockingEvent);

            // First valid attempt after locking event
            Func<Task<UserReadModel>> act = async () => await this.ValidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Assert
            await act.Should().ThrowAsync<ErrorException>();
        }

        [Fact(Skip = "Ignored since tests broken by switch to using exceptions for login failures. Fix by reverting behaviour to use results.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task LoginAttempt_Succeeded_For_ValidAttempt_When_Account_Is_Already_UnLocked()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();
            var fakeOrganisationAggregateRepository = new FakeOrganisationAggregateRepository();
            var fakeUserUserSystemEventEmitter = new FakeUserSystemEventEmitter();
            var fakeUserLoginEmailRepository = new FakeUserLoginEmailRepository();

            // Act
            // Unlocking event
            var unblockingEvent = EmailAddressBlockingEvent.EmailAddressUnblocked(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                this.email,
                EmailAddressUnblockedReason.EmailAddressUnblockedDueToSuccessfulAuthenticationAfterTimeout,
                SystemClock.Instance.GetCurrentInstant());
            fakeEmailBlockingEventRepository.Insert(unblockingEvent);

            // First valid attempt after unlocking event
            Func<Task<UserReadModel>> act = async () => await this.ValidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact(Skip = "Ignored since tests broken by switch to using exceptions for login failures. Fix by reverting behaviour to use results.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task LoginAttempt_Succeeded_For_ValidAttempt_When_No_AccountLocking_Event_Occur()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();
            var fakeOrganisationAggregateRepository = new FakeOrganisationAggregateRepository();
            var fakeUserUserSystemEventEmitter = new FakeUserSystemEventEmitter();
            var fakeUserLoginEmailRepository = new FakeUserLoginEmailRepository();

            // Act
            // Valid attempt
            Func<Task<UserReadModel>> act = async () => await this.ValidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact(Skip = "Ignored since tests broken by switch to using exceptions for login failures. Fix by reverting behaviour to use results.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task LoginAttempt_Fail_For_InvalidAttempt_When_No_AccountLocking_Event_Occur()
        {
            // Arrange
            var fakeLoginAttemptResultRepository = new FakeLoginAttemptResultRepository();
            var fakeEmailBlockingEventRepository = new FakeEmailBlockingEventRepository();
            var fakeOrganisationAggregateRepository = new FakeOrganisationAggregateRepository();
            var fakeUserUserSystemEventEmitter = new FakeUserSystemEventEmitter();
            var fakeUserLoginEmailRepository = new FakeUserLoginEmailRepository();

            // Act
            Func<Task<UserReadModel>> act = async () => await this.InvalidUserOrPasswordAttempt(
                fakeLoginAttemptResultRepository, fakeEmailBlockingEventRepository, fakeOrganisationAggregateRepository, fakeUserUserSystemEventEmitter, fakeUserLoginEmailRepository);

            // Assert
            await act.Should().ThrowAsync<ErrorException>();
        }

        private async Task<UserReadModel> InvalidUserOrPasswordAttempt(
            IEmailRequestRecordRepository<LoginAttemptResult> fakeLoginAttemptResultRepository,
            IEmailAddressBlockingEventRepository fakeEmailBlockingEventRepository,
            IOrganisationAggregateRepository fakeOrganisationAggregateRepository,
            IUserSystemEventEmitter fakeUserSystemEventEmitter,
            IUserLoginEmailRepository fakeUserLoginEmailRepository)
        {
            var loginTrackingService = new LoginAttemptTrackingService(
                fakeLoginAttemptResultRepository,
                fakeEmailBlockingEventRepository,
                fakeOrganisationAggregateRepository,
                fakeUserSystemEventEmitter,
                fakeUserLoginEmailRepository,
                SystemClock.Instance);

            var command = new AuthenticateUserCommand(this.tenant.Id, this.organisationId, this.email, this.invalidPassword);
            var handler = this.CreateUserAuthenticationCommand(loginTrackingService);
            var user = await handler.Handle(command, CancellationToken.None);
            return user;
        }

        private async Task<UserReadModel> ValidUserOrPasswordAttempt(
            IEmailRequestRecordRepository<LoginAttemptResult> fakeLoginAttemptResultRepository,
            IEmailAddressBlockingEventRepository fakeEmailBlockingEventRepository,
            IOrganisationAggregateRepository fakeOrganisationAggregateRepository,
            IUserSystemEventEmitter fakeUserSystemEventEmitter,
            IUserLoginEmailRepository fakeUserLoginEmailRepository)
        {
            var loginTrackingService = new LoginAttemptTrackingService(
                fakeLoginAttemptResultRepository,
                fakeEmailBlockingEventRepository,
                fakeOrganisationAggregateRepository,
                fakeUserSystemEventEmitter,
                fakeUserLoginEmailRepository,
                SystemClock.Instance);

            var command = new AuthenticateUserCommand(this.tenant.Id, this.organisationId, this.email, this.password);
            var handler = this.CreateUserAuthenticationCommand(loginTrackingService);
            var user = await handler.Handle(command, CancellationToken.None);
            return user;
        }

        private AuthenticateUserCommandHandler CreateUserAuthenticationCommand(ILoginAttemptTrackingService loginTrackingService)
        {
            var passwordHashingService = new PasswordHashingService();
            var userAggregateRepository = new Mock<IUserAggregateRepository>();
            var userReadModelRepository = new Mock<IUserReadModelRepository>();
            var userLoginEmailRepository = new Mock<IUserLoginEmailRepository>();
            var organisationService = new Mock<IOrganisationService>();
            var userSystemEventEmitter = new Mock<IUserSystemEventEmitter>();
            var cachingResolver = new Mock<ICachingResolver>();
            var httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();

            var personAggregate = PersonAggregate.CreatePerson(
                this.tenant.Id,
                this.tenant.Details.DefaultOrganisationId,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            var userAggregate = UserAggregate.CreateUser(
                this.tenant.Id, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, SystemClock.Instance.GetCurrentInstant());

            var invitationID = userAggregate.CreateActivationInvitation(
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());

            var saltAndHasPassword = passwordHashingService.SaltAndHash(this.password);
            userAggregate.Activate(
                invitationID, saltAndHasPassword, this.performingUserId, SystemClock.Instance.GetCurrentInstant());

            userAggregateRepository.Setup(uar => uar.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(userAggregate);

            var personData = new PersonData(personAggregate);
            var userReadModel = new UserReadModel(
                Guid.NewGuid(),
                personData,
                Guid.NewGuid(),
                null,
                SystemClock.Instance.GetCurrentInstant(),
                UserType.Client,
                userAggregate.Environment);

            userReadModelRepository.Setup(urs => urs.GetUser(userAggregate.TenantId, userAggregate.Id)).Returns(userReadModel);

            var command = new AuthenticateUserCommandHandler(
                passwordHashingService,
                userLoginEmailRepository.Object,
                userAggregateRepository.Object,
                userReadModelRepository.Object,
                loginTrackingService,
                organisationService.Object,
                userSystemEventEmitter.Object,
                cachingResolver.Object,
                SystemClock.Instance,
                httpContextPropertiesResolver.Object);
            return command;
        }

        private class FakeUserSystemEventEmitter : IUserSystemEventEmitter
        {
            public Task CreateAndEmitLoginSystemEvents(Guid tenantId, Guid userId, List<SystemEventType> eventTypes, Instant? timestamp = null)
            {
                return Task.FromResult(0);
            }

            public Task CreateAndEmitLoginSystemEvents(UserReadModel user, List<SystemEventType> eventTypes, Instant? timestamp = null)
            {
                return Task.FromResult(0);
            }

            public Task CreateAndEmitSystemEvents(UserReadModel user, List<SystemEventType> eventTypes, Guid? performingUserId = null, Instant? timestamp = null)
            {
                return Task.FromResult(0);
            }

            public Task CreateAndEmitSystemEvents(Guid tenantId, Guid userId, List<SystemEventType> eventTypes, Guid? performingUserId = null, Instant? timestamp = null)
            {
                return Task.FromResult(0);
            }

            public void Dispatch(UserAggregate aggregate, IEvent<UserAggregate, Guid> @event, int sequenceNumber, IEnumerable<Type> observerTypes = null)
            {
            }
        }

        private class FakeOrganisationAggregateRepository : IOrganisationAggregateRepository
        {
            private readonly List<Organisation> organisations = new List<Organisation>();

            public Task ApplyChangesToDbContext(Organisation aggregate)
            {
                throw new NotImplementedException();
            }

            public Organisation GetById(Guid tenantId, Guid id)
            {
                return this.organisations.FirstOrDefault(o => o.Id == id);
            }

            public Task<Organisation> GetByIdAtSequenceNumber(Guid tenantId, Guid id, int sequenceNumber)
            {
                return Task.FromResult(this.organisations.FirstOrDefault(o => o.Id == id));
            }

            Task IAggregateRepository<Organisation, Guid>.DeleteById(Guid tenantId, Guid id)
            {
                // No op;
                return Task.FromResult(0);
            }

            public Task ReplayEventByAggregateId(Guid tenantId, Guid id, int sequenceNumber, IEnumerable<Type> observerTypes = null)
            {
                // No op;
                return Task.FromResult(0);
            }

            public Task ReplayAllEventsByAggregateId(
                Guid tenantId, Guid id, IEnumerable<Type> observerTypes = null, Guid? overrideTenantId = null)
            {
                // No op;
                return Task.FromResult(0);
            }

            public Task ReplayEventsOfTypeByAggregateId(Guid tenantId, Guid id, Type[] types)
            {
                // No op;
                return Task.FromResult(0);
            }

            public Task Save(Organisation aggregate)
            {
                // No op;
                return Task.FromResult(0);
            }

            public Task<Organisation?> GetByIdAsync(Guid tenantId, Guid id)
            {
                // No op;
                return Task.FromResult<Organisation?>(null);
            }

            public Task<Organisation?> GetByIdWithoutUsingSnapshot(Guid tenantId, Guid id)
            {
                // No op;
                return Task.FromResult<Organisation?>(null);
            }

            public int GetSnapshotSaveInterval()
            {
                throw new NotImplementedException();
            }
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
                // No op;
            }
        }

        private class FakeEmailBlockingEventRepository : IEmailAddressBlockingEventRepository
        {
            private readonly List<EmailAddressBlockingEvent> emailBlockingEvents = new List<EmailAddressBlockingEvent>();

            public EmailAddressBlockingEvent GetLatestBlockingEvent(
                Guid tenantId, Guid organisationId, string emailAddress)
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
                // No op;
            }
        }

        private class FakeUserLoginEmailRepository : IUserLoginEmailRepository
        {
            private readonly List<UserLoginEmail> userLoginEmails = new List<UserLoginEmail>();

            public void Add(UserLoginEmail userLoginEmail)
            {
                throw new NotImplementedException();
            }

            public void Delete(Guid tenantId, Guid organisationId, Guid userId)
            {
                throw new NotImplementedException();
            }

            public UserLoginEmail? GetLatestUserLoginEmail(Guid tenantId, Guid userId)
            {
                return this.userLoginEmails
                    .Where(e => e.TenantId == tenantId)
                    .Where(e => e.Id == userId)
                    .OrderByDescending(e => e.CreatedTicksSinceEpoch)
                    .FirstOrDefault();
            }

            public UserLoginEmail? GetUserLoginByEmail(Guid tenantId, Guid organisationId, string email)
            {
                return this.userLoginEmails
                    .Where(e => e.TenantId == tenantId)
                    .Where(e => e.Id.Equals(email))
                    .OrderByDescending(e => e.CreatedTicksSinceEpoch)
                    .FirstOrDefault();
            }

            public UserLoginEmail GetUserLoginEmailByEmail(Guid tenantId, Guid organisationId, PortalUserType portalUserType, string loginEmail)
            {
                throw new NotImplementedException();
            }

            public UserLoginEmail GetUserLoginEmailById(Guid tenantId, Guid organisationId, PortalUserType portalUserType, Guid id)
            {
                throw new NotImplementedException();
            }

            public List<UserLoginEmail> GetUserLoginsByEmail(Guid tenantId, string email)
            {
                return this.userLoginEmails
                     .Where(e => e.TenantId == tenantId && e.LoginEmail == email)
                     .OrderByDescending(e => e.CreatedTicksSinceEpoch)
                     .ToList();
            }

            public void Insert(UserLoginEmail userLoginEmail)
            {
                this.userLoginEmails.Add(userLoginEmail);
            }

            public void SaveChanges()
            {
                // No op;
            }
        }

        private class FakeUserAggregateRepository : IUserAggregateRepository
        {
            private readonly List<UserAggregate> userAggregates = new List<UserAggregate>();

            public Task ApplyChangesToDbContext(UserAggregate aggregate)
            {
                throw new NotImplementedException();
            }

            public UserAggregate GetById(Guid tenantId, Guid id)
            {
                return this.userAggregates.FirstOrDefault(p => p.TenantId == tenantId && p.Id == id);
            }

            public Task<UserAggregate?> GetByIdAsync(Guid tenantId, Guid id)
            {
                throw new NotImplementedException();
            }

            public Task<UserAggregate?> GetByIdWithoutUsingSnapshot(Guid tenantId, Guid id)
            {
                throw new NotImplementedException();
            }

            public Task<UserAggregate> GetByIdAtSequenceNumber(Guid tenantId, Guid id, int sequenceNumber)
            {
                throw new NotImplementedException();
            }

            Task IAggregateRepository<UserAggregate, Guid>.DeleteById(Guid tenantId, Guid id)
            {
                throw new NotImplementedException();
            }

            public Task ReplayAllEventsByAggregateId(Guid tenantId, Guid id, IEnumerable<Type> observerTypes = null, Guid? overrideTenantId = null)
            {
                throw new NotImplementedException();
            }

            public Task ReplayEventByAggregateId(Guid tenantId, Guid id, int sequenceNumber, IEnumerable<Type> observerTypes = null)
            {
                throw new NotImplementedException();
            }

            public Task ReplayEventsOfTypeByAggregateId(Guid tenantId, Guid id, Type[] eventTypes)
            {
                throw new NotImplementedException();
            }

            public Task Save(UserAggregate aggregate)
            {
                throw new NotImplementedException();
            }

            public int GetSnapshotSaveInterval()
            {
                throw new NotImplementedException();
            }
        }
    }
}