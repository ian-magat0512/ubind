// <copyright file="UserAggregateRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.User
{
    using System;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class UserAggregateRepositoryIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public async Task Save_ThrowsConcurrencyException_WhenAggregateHasBeenUpdatedSinceLoading()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var person = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
            await stack1.PersonAggregateRepository.Save(person);
            var userId = Guid.NewGuid();
            var persistedUserAggregate = UserAggregate.CreateUser(person.TenantId, userId, UserType.Client, person, userId, null, clock.Now());
            await stack1.UserAggregateRepository.Save(persistedUserAggregate);

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var updatedUserAggreate = stack2.UserAggregateRepository.GetById(tenant.Id, userId);
            updatedUserAggreate.Block(this.performingUserId, clock.Now());

            var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var staleUserAggregate = stack3.UserAggregateRepository.GetById(tenant.Id, userId);

            await stack2.UserAggregateRepository.Save(updatedUserAggreate);

            staleUserAggregate.Block(this.performingUserId, clock.Now());

            // Act + Assert
            await Assert.ThrowsAsync<ConcurrencyException>(
                () => stack3.UserAggregateRepository.Save(staleUserAggregate));
        }

        [Fact]
        public async Task Save_ThrowsDuplicateLoginEmailException_WhenUserAggregateHasDuplicationLoginEmail()
        {
            // Arrange
            var email = "duplicate@example.com";
            var clock = SystemClock.Instance;
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var person1 = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
            await stack1.PersonAggregateRepository.Save(person1);
            var user1Id = Guid.NewGuid();
            var userAggregate1 = UserAggregate.CreateUser(person1.TenantId, user1Id, UserType.Client, person1, this.performingUserId, null, clock.Now());

            userAggregate1.SetLoginEmail(email, this.performingUserId, clock.Now());
            await stack1.UserAggregateRepository.Save(userAggregate1);

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var person2 = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
            await stack2.PersonAggregateRepository.Save(person2);
            var user2Id = Guid.NewGuid();
            var userAggregate2 = UserAggregate.CreateUser(person2.TenantId, user2Id, UserType.Client, person2, this.performingUserId, null, clock.Now());
            userAggregate2.SetLoginEmail(email, this.performingUserId, clock.Now());

            // Act + Assert
            await Assert.ThrowsAsync<UBind.Domain.Exceptions.DuplicateUserEmailException>(
                () => stack2.UserAggregateRepository.Save(userAggregate2));
        }

        [Fact]
        public async Task Save_AddsAssignedRolesToReadModel()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var userId = Guid.NewGuid();
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, tenant.Details.DefaultOrganisationId, clock.Now());
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var person = PersonAggregate.CreatePerson(
                    tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
                await stack1.PersonAggregateRepository.Save(person);
                var userAggregate = UserAggregate.CreateUser(person.TenantId, userId, UserType.Client, person, userId, null, clock.Now());
                stack1.RoleRepository.Insert(role);
                userAggregate.AssignRole(role, this.performingUserId, clock.Now());

                // Act
                await stack1.UserAggregateRepository.Save(userAggregate);
            }

            // Assert
            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var loadedUser = stack2.UserAggregateRepository.GetById(tenant.Id, userId);
                Assert.Contains(role.Id, loadedUser.RoleIds);
            }
        }

        [Fact]
        public async Task Save_RemovesRetractedRolesFromReadModel()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var userId = Guid.NewGuid();
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, tenant.Details.DefaultOrganisationId, clock.Now());
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var person = PersonAggregate.CreatePerson(
                    tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
                await stack1.PersonAggregateRepository.Save(person);
                var userAggregate = UserAggregate.CreateUser(person.TenantId, userId, UserType.Client, person, userId, null, clock.Now());
                stack1.RoleRepository.Insert(role);

                userAggregate.AssignRole(role, this.performingUserId, clock.Now());

                // Act -stack1
                await stack1.UserAggregateRepository.Save(userAggregate);
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var user = stack2.UserAggregateRepository.GetById(tenant.Id, userId);
                user.RetractRole(role, this.performingUserId, clock.Now());

                // Act -stack2
                await stack2.UserAggregateRepository.Save(user);
            }

            // Assert
            using (var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var loadedUser = stack3.UserAggregateRepository.GetById(tenant.Id, userId);
                Assert.DoesNotContain(role.Id, loadedUser.RoleIds);
            }
        }

        [Fact]
        public async Task Save_UserPersonDetails_ShouldRetrieveNameComponents()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var userId = Guid.NewGuid();
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, tenant.Details.DefaultOrganisationId, clock.Now());
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
                var person = PersonAggregate.CreatePersonFromPersonalDetails(
                    tenant.Id, defaultOrganisationId, new FakePersonalDetails(), this.performingUserId, clock.Now());

                await stack1.PersonAggregateRepository.Save(person);
                var userAggregate = UserAggregate.CreateUser(person.TenantId, userId, UserType.Client, person, this.performingUserId, null, clock.Now());
                stack1.RoleRepository.Insert(role);
                userAggregate.AssignRole(role, this.performingUserId, clock.Now());

                // Act
                await stack1.UserAggregateRepository.Save(userAggregate);
            }

            // Assert
            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var user = stack2.UserAggregateRepository.GetById(tenant.Id, userId);
                var person = stack2.PersonAggregateRepository.GetById(tenant.Id, user.PersonId);
                var fullName = PersonPropertyHelper.GetFullNameFromParts(
                      person.PreferredName,
                      person.NamePrefix,
                      person.FirstName,
                      person.LastName,
                      person.NameSuffix,
                      person.MiddleNames);

                Assert.Equal("Dr John (Jonno) Doe Smith Jr", fullName);
                Assert.Equal("Dr", person.NamePrefix);
                Assert.Equal("John", person.FirstName);
                Assert.Equal("Doe", person.MiddleNames);
                Assert.Equal("Smith", person.LastName);
                Assert.Equal("Jr", person.NameSuffix);
                Assert.Equal("Jonno", person.PreferredName);
                Assert.Equal("uBind", person.Company);
                Assert.Equal("Developer", person.Title);
            }
        }
    }
}
