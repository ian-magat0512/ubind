// <copyright file="UserActivationInvitationServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Users
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class UserActivationInvitationServiceIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        // The transaction in DatabaseIntegrationTests is used to rollback change from each database integration test before executing the next one.
        // However, it will prevent database changes from being readable in other concurrent threads, and so will test like this onen fail.
        // It can be temporarily turned off for manually running tests like this one.
        [Fact(Skip = "This test needs to be run outside the transaction setup in DatabaseIntegrationTests in order to work.")]
        public async Task QueueActivationEmail_RetriesAndSucceeds_WhenInitialAttemptsTriggerConcurrencyException()
        {
            // Arrange
            Guid userAggregateId;
            Guid personAggregateId;
            DeploymentEnvironment environment = DeploymentEnvironment.Staging;
            var tenant = TenantFactory.Create();
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.TenantRepository.Insert(tenant);
                var personAggregate = PersonAggregate.CreatePerson(
                    tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, stack.Clock.Now());
                personAggregate.Update(new FakePersonalDetails(), this.performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(personAggregate);
                personAggregateId = personAggregate.Id;
                var userAggregate = UserAggregate.CreateUser(
                    personAggregate.TenantId, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, stack.Clock.Now());
                await stack.UserAggregateRepository.Save(userAggregate);
                userAggregateId = userAggregate.Id;
            }

            // Reload quote.
            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);

            // Make the first three attempts to persist docs and aggregate fail due to concurrency exception.
            var numberOfFails = 3;
            var callCount = 0;
            void MakeConcurrentUpdate()
            {
                ++callCount;
                if (numberOfFails > 0)
                {
                    var thread = new Thread(async () =>
                    {
                        var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
                        var parallelUser = stack3.UserAggregateRepository.GetById(tenant.Id, userAggregateId);
                        parallelUser.CreateActivationInvitation(this.performingUserId, stack2.Clock.Now());
                        await stack3.UserAggregateRepository.Save(parallelUser);
                    });
                    thread.Start();
                    thread.Join();
                }

                --numberOfFails;
            }

            stack2.DbContext.BeforeSaveChanges += dbContext => MakeConcurrentUpdate();

            var reloadedPersonAggregate = stack2.PersonAggregateRepository.GetById(tenant.Id, personAggregateId);
            var reloadedUserAggregate = stack2.UserAggregateRepository.GetById(tenant.Id, userAggregateId);

            // Act
            await stack2.UserActivationInvitationService.CreateActivationInvitationAndSendEmail(
                tenant,
                default,
                userAggregateId,
                environment);

            // Assert
            callCount.Should().Be(4, "The test did not make 4 save attempts (3 fails + success) as expected.");
            using (var stack4 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // For manual inspection of invitations.
                var persistedAggregate = stack4.UserAggregateRepository.GetById(tenant.Id, userAggregateId);
            }
        }
    }
}
