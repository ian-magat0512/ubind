// <copyright file="PersonAggregateRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Person
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Commands.Person;
    using UBind.Application.Person;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection("Database collection")]
    public class PersonAggregateRepositoryIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public async Task Save_PersonDetails_ForCustomer_Succeed()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var aggregateIds = await this.CreateTestPersonsForCustomer(tenantId, organisationId);
            Guid personOneAggregateId = aggregateIds.person1.Id;
            Guid personTwoAggregateId = aggregateIds.person2.Id;
            Guid customerOneAggregateId = aggregateIds.customerAggregate.Id;

            // Assert
            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var savedPersonOneAggregate = stack2.PersonAggregateRepository.GetById(tenantId, personOneAggregateId);
                var savedPersonTwoAggregate = stack2.PersonAggregateRepository.GetById(tenantId, personTwoAggregateId);
                var savedPersonReadModel
                    = stack2.PersonReadModelRepository.GetPersonsByCustomerId(tenantId, customerOneAggregateId).ToList();
                var expectedPerson1 = savedPersonReadModel.FirstOrDefault(p => p.Id == personOneAggregateId);
                var expectedPerson2 = savedPersonReadModel.FirstOrDefault(p => p.Id == personTwoAggregateId);

                Assert.True(savedPersonReadModel.Count == 2);

                // note : person 1 is being saved via customer initialize which formats the fullname
                expectedPerson1.FullName.Should().Be($"{savedPersonOneAggregate.FirstName} {savedPersonOneAggregate.LastName}");
                expectedPerson1.FirstName.Should().Be(savedPersonOneAggregate.FirstName);
                expectedPerson1.LastName.Should().Be(savedPersonOneAggregate.LastName);
                expectedPerson1.MiddleNames.Should().Be(savedPersonOneAggregate.MiddleNames);
                expectedPerson1.NamePrefix.Should().Be(savedPersonOneAggregate.NamePrefix);
                expectedPerson1.NameSuffix.Should().Be(savedPersonOneAggregate.NameSuffix);
                expectedPerson1.Email.Should().Be(savedPersonOneAggregate.Email);

                expectedPerson2.FullName.Should().Be($"{savedPersonTwoAggregate.FirstName} {savedPersonTwoAggregate.LastName}");
                expectedPerson2.FirstName.Should().Be(savedPersonTwoAggregate.FirstName);
                expectedPerson2.LastName.Should().Be(savedPersonTwoAggregate.LastName);
                expectedPerson2.MiddleNames.Should().Be(savedPersonTwoAggregate.MiddleNames);
                expectedPerson2.NamePrefix.Should().Be(savedPersonTwoAggregate.NamePrefix);
                expectedPerson2.NameSuffix.Should().Be(savedPersonTwoAggregate.NameSuffix);
                expectedPerson2.Email.Should().Be(savedPersonTwoAggregate.Email);
            }
        }

        [Fact]
        public async Task Save_PersonDetails_ForUser_Succeed()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var userId = Guid.NewGuid();
            Guid personOneAggregateId = default;
            Guid personTwoAggregateId = default;

            // Act
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var personOne = this.CreatePerson(tenantId, 1);
                var personAggregateOne = PersonAggregate.CreatePersonFromPersonalDetails(
                    tenant.Id, tenant.Details.DefaultOrganisationId, personOne, this.performingUserId, clock.Now());
                personOneAggregateId = personAggregateOne.Id;
                await stack1.PersonAggregateRepository.Save(personAggregateOne);
                var personTwo = this.CreatePerson(tenantId, 2);
                var personAggregateTwo = PersonAggregate.CreatePersonFromPersonalDetails(
                    tenant.Id, tenant.Details.DefaultOrganisationId, personTwo, this.performingUserId, clock.Now());
                personTwoAggregateId = personAggregateTwo.Id;
                await stack1.PersonAggregateRepository.Save(personAggregateTwo);
                personAggregateOne = stack1.PersonAggregateRepository.GetById(tenant.Id, personOneAggregateId);
                personAggregateOne.RecordUserAccountCreatedForPerson(userId, this.performingUserId, clock.Now());
                await stack1.PersonAggregateRepository.Save(personAggregateOne);
                personAggregateTwo = stack1.PersonAggregateRepository.GetById(tenant.Id, personTwoAggregateId);
                personAggregateTwo.RecordUserAccountCreatedForPerson(userId, this.performingUserId, clock.Now());
                await stack1.PersonAggregateRepository.Save(personAggregateTwo);
            }

            // Assert
            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var savedPersonOneAggregate = stack2.PersonAggregateRepository.GetById(tenant.Id, personOneAggregateId);
                var savedPersonTwoAggregate = stack2.PersonAggregateRepository.GetById(tenant.Id, personTwoAggregateId);
                var savedPersonReadModel = stack2.PersonReadModelRepository.GetPersonsByUserId(tenantId, userId).ToList();
                var expectePerson1 = savedPersonReadModel.Where(p => p.Id == personOneAggregateId).FirstOrDefault();
                var expectePerson2 = savedPersonReadModel.Where(p => p.Id == personTwoAggregateId).FirstOrDefault();

                Assert.True(savedPersonReadModel.Count == 2);
                Assert.Equal(
                    expectePerson1.FullName,
                    $"{savedPersonOneAggregate.FirstName} {savedPersonOneAggregate.LastName}");
                Assert.Equal(expectePerson1.FirstName, savedPersonOneAggregate.FirstName);
                Assert.Equal(expectePerson1.LastName, savedPersonOneAggregate.LastName);
                Assert.Equal(expectePerson1.MiddleNames, savedPersonOneAggregate.MiddleNames);
                Assert.Equal(expectePerson1.NamePrefix, savedPersonOneAggregate.NamePrefix);
                Assert.Equal(expectePerson1.NameSuffix, savedPersonOneAggregate.NameSuffix);
                Assert.Equal(expectePerson1.Email, savedPersonOneAggregate.Email);
                Assert.Equal(
                    expectePerson2.FullName,
                    $"{savedPersonTwoAggregate.FirstName} {savedPersonTwoAggregate.LastName}");
                Assert.Equal(expectePerson2.FirstName, savedPersonTwoAggregate.FirstName);
                Assert.Equal(expectePerson2.LastName, savedPersonTwoAggregate.LastName);
                Assert.Equal(expectePerson2.MiddleNames, savedPersonTwoAggregate.MiddleNames);
                Assert.Equal(expectePerson2.NamePrefix, savedPersonTwoAggregate.NamePrefix);
                Assert.Equal(expectePerson2.NameSuffix, savedPersonTwoAggregate.NameSuffix);
                Assert.Equal(expectePerson2.Email, savedPersonTwoAggregate.Email);
            }
        }

        [Fact]
        public async Task Set_PersonAsPrimary_ForCustomer_Succeed()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var aggregateIds = await this.CreateTestPersonsForCustomer(tenantId, organisationId);
            var personOneAggregateId = aggregateIds.person1.Id;
            var personTwoAggregateId = aggregateIds.person2.Id;
            var customerOneAggregateId = aggregateIds.customerAggregate.Id;

            this.CreateCustomerRole(tenantId, organisationId);

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var savedPersonReadModels
                    = stack2.PersonReadModelRepository.GetPersonsByCustomerId(tenantId, customerOneAggregateId).ToList();

                var customerAggregate = stack2.CustomerAggregateRepository.GetById(tenantId, customerOneAggregateId);
                var primaryPerson
                    = savedPersonReadModels.FirstOrDefault(p => p.Id == customerAggregate.PrimaryPersonId);
                var nonPrimaryPerson = savedPersonReadModels.FirstOrDefault(p => p.Id == personTwoAggregateId);
                savedPersonReadModels.Count.Should().Be(2);
                customerAggregate.PrimaryPersonId.Should().Be(personOneAggregateId);
                customerAggregate.PrimaryPersonId.Should().NotBe(personTwoAggregateId);

                // Act: Set person two as the primary person
                customerAggregate = stack2.CustomerAggregateRepository.GetById(tenantId, primaryPerson.CustomerId.Value);
                customerAggregate.SetPrimaryPerson(nonPrimaryPerson.Id, this.performingUserId, clock.Now());
                await stack2.CustomerAggregateRepository.Save(customerAggregate);

                // Assert
                customerAggregate.PrimaryPersonId.Should().NotBe(personOneAggregateId);
                customerAggregate.PrimaryPersonId.Should().Be(personTwoAggregateId);
            }
        }

        [Fact]
        public async Task Delete_PersonAndRelatedUser_FromCustomerPeople_Succeed()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var aggregateIds = await this.CreateTestPersonsForCustomer(tenantId, organisationId);
            var personOneAggregateId = aggregateIds.person1.Id;
            var personTwoAggregateId = aggregateIds.person2.Id;
            var customerOneAggregateId = aggregateIds.customerAggregate.Id;

            this.CreateCustomerRole(tenantId, organisationId);

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var savedPersonReadModels
                    = stack2.PersonReadModelRepository.GetPersonsByCustomerId(tenantId, customerOneAggregateId).ToList();
                var customerAggregate = stack2.CustomerAggregateRepository.GetById(tenantId, customerOneAggregateId);
                var primaryPerson
                    = savedPersonReadModels.FirstOrDefault(p => p.Id == customerAggregate.PrimaryPersonId);
                var nonPrimaryPerson = savedPersonReadModels.FirstOrDefault(p => p.Id == personTwoAggregateId);
                savedPersonReadModels.Count.Should().Be(2);

                // Act: Creating user for a primary person
                var primaryCustomerUser = await stack2.UserService.CreateUserForPerson(aggregateIds.person1);
                var primaryCustomerUserLogin = stack2.UserLoginEmailRepository
                    .GetUserLoginByEmail(tenantId, organisationId, primaryCustomerUser.LoginEmail);

                // Assert: Making sure that there is a user login created
                primaryCustomerUserLogin.Should().NotBeNull();

                // Act: Creating user for a non-primary person
                var nonPrimaryPersonCustomerUser = await stack2.UserService.CreateUserForPerson(aggregateIds.person2);
                var nonPrimaryCustomerUserLogin = stack2.UserLoginEmailRepository
                    .GetUserLoginByEmail(tenantId, organisationId, nonPrimaryPersonCustomerUser.LoginEmail);

                // Assert: Making sure that there is a user login created
                nonPrimaryCustomerUserLogin.Should().NotBeNull();

                // Act: Delete the person 2
                var command
                    = new DeletePersonAndRelatedUserAccountCommand(((IEntityReadModel<Guid>)nonPrimaryPerson).TenantId, nonPrimaryPerson.Id);
                var commandHandler = new DeletePersonAndRelatedUserAccountCommandHandler(
                    stack2.PersonAggregateRepository,
                    stack2.UserAggregateRepository,
                    stack2.UserLoginEmailRepository,
                    stack2.CustomerAggregateRepository,
                    stack2.UserService,
                    stack2.HttpContextPropertiesResolver,
                    stack2.Clock);
                var result = await commandHandler.Handle(command, CancellationToken.None);
                stack2.DbContext.SaveChanges();

                // Assert: Check the person read model record after deletion
                savedPersonReadModels = stack2.PersonReadModelRepository
                    .GetPersonsByCustomerId(tenantId, customerOneAggregateId).ToList();
                savedPersonReadModels.Count.Should().Be(1);
                savedPersonReadModels.FirstOrDefault().Id.Should().Be(primaryPerson.Id);

                // Assert: Check person aggregate record after deletion
                var updatedPersonAggregate = stack2.PersonAggregateRepository.GetById(tenantId, primaryPerson.Id);
                updatedPersonAggregate.IsDeleted.Should().BeFalse();
                var updatedPersonAggregate2 = stack2.PersonAggregateRepository.GetById(tenantId, nonPrimaryPerson.Id);
                updatedPersonAggregate2.IsDeleted.Should().BeTrue();

                // Assert: Check user login emails after deletion
                var updatedNonPrimaryPersonCustomerUserLogin = stack2.UserLoginEmailRepository
                    .GetUserLoginByEmail(tenantId, organisationId, nonPrimaryPersonCustomerUser.LoginEmail);
                updatedNonPrimaryPersonCustomerUserLogin.Should().BeNull();
                var updatedPrimaryPersonCustomerUserLogin = stack2.UserLoginEmailRepository
                    .GetUserLoginByEmail(tenantId, organisationId, primaryCustomerUser.LoginEmail);
                updatedPrimaryPersonCustomerUserLogin.Should().NotBeNull();

                // Assert: Check user read model record after deletion
                var updatedPrimaryPersonUser = stack2.UserReadModelRepository
                    .GetUser(primaryCustomerUser.TenantId, primaryCustomerUser.Id);
                updatedPrimaryPersonUser.Should().NotBeNull();
                var updatedNonPrimaryPersonUser = stack2.UserReadModelRepository
                    .GetUser(nonPrimaryPersonCustomerUser.TenantId, nonPrimaryPersonCustomerUser.Id);
                updatedNonPrimaryPersonUser.Should().BeNull();

                // Assert: Check user aggregate record after deletion
                var updatedPrimaryPersonUserAggregate = stack2.UserAggregateRepository.GetById(tenantId, primaryCustomerUser.Id);
                updatedPrimaryPersonUserAggregate.Blocked.Should().BeFalse();
                var updatedNonPrimaryPersonUserAggregate
                    = stack2.UserAggregateRepository.GetById(tenantId, nonPrimaryPersonCustomerUser.Id);
                updatedNonPrimaryPersonUserAggregate.Blocked.Should().BeTrue();
            }
        }

        [Fact]
        public async Task Delete_PrimaryPersonAndRelatedUser_FromCustomerPeople_ReturnAnError()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var tenantId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var aggregateIds = await this.CreateTestPersonsForCustomer(tenantId, organisationId);
            var personOneAggregateId = aggregateIds.person1.Id;
            var personTwoAggregateId = aggregateIds.person2.Id;
            var customerOneAggregateId = aggregateIds.customerAggregate.Id;

            this.CreateCustomerRole(tenantId, organisationId);

            // Assert
            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var savedPersonReadModels
                    = stack2.PersonReadModelRepository.GetPersonsByCustomerId(tenantId, customerOneAggregateId).ToList();
                var customerAggregate = stack2.CustomerAggregateRepository.GetById(tenantId, customerOneAggregateId);
                var primaryPerson
                    = savedPersonReadModels.FirstOrDefault(p => p.Id == customerAggregate.PrimaryPersonId);
                var nonPrimaryPerson = savedPersonReadModels.FirstOrDefault(p => p.Id == personTwoAggregateId);
                savedPersonReadModels.Count.Should().Be(2);

                // Act: Creating user for a primary person
                var person1 = PersonAggregate.CreatePerson(
                    tenantId, organisationId, this.performingUserId, SystemClock.Instance.Now());
                person1.UpdateEmail($"{person1.Id}@ubind.io", this.performingUserId, clock.GetCurrentInstant());
                var primaryCustomerUser = await stack2.UserService.CreateUserForPerson(person1);
                var primaryCustomerUserLogin = stack2.UserLoginEmailRepository
                    .GetUserLoginByEmail(tenantId, organisationId, primaryCustomerUser.LoginEmail);

                // Assert: Making sure that there is a user login created
                primaryCustomerUserLogin.Should().NotBeNull();

                // Act: Creating user for a non-primary person
                var person2 = PersonAggregate.CreatePerson(
                    tenantId, organisationId, this.performingUserId, SystemClock.Instance.Now());
                person2.UpdateEmail($"{person2.Id}@ubind.io", this.performingUserId, clock.GetCurrentInstant());
                var nonPrimaryPersonCustomerUser = await stack2.UserService.CreateUserForPerson(person2);
                var nonPrimaryCustomerUserLogin = stack2.UserLoginEmailRepository
                    .GetUserLoginByEmail(tenantId, organisationId, nonPrimaryPersonCustomerUser.LoginEmail);

                // Assert: Making sure that there is a user login created
                nonPrimaryCustomerUserLogin.Should().NotBeNull();

                // Act: Delete the primary person
                var command = new DeletePersonAndRelatedUserAccountCommand(Guid.NewGuid(), primaryPerson.Id);
                var commandHandler = new DeletePersonAndRelatedUserAccountCommandHandler(
                    stack2.PersonAggregateRepository,
                    stack2.UserAggregateRepository,
                    stack2.UserLoginEmailRepository,
                    stack2.CustomerAggregateRepository,
                    stack2.UserService,
                    stack2.HttpContextPropertiesResolver,
                    stack2.Clock);
                Func<Task> func = async () => await commandHandler.Handle(command, CancellationToken.None);

                // Assert: Error exception should be thrown
                (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("person.not.found");

                // Assert: Check the person read model record after deletion
                savedPersonReadModels = stack2.PersonReadModelRepository
                    .GetPersonsByCustomerId(tenantId, customerOneAggregateId).ToList();
                savedPersonReadModels.Count.Should().Be(2);

                // Assert: Check person aggregate record after deletion
                var updatedPersonAggregate = stack2.PersonAggregateRepository.GetById(tenantId, primaryPerson.Id);
                updatedPersonAggregate.IsDeleted.Should().BeFalse();
                var updatedPersonAggregate2 = stack2.PersonAggregateRepository.GetById(tenantId, nonPrimaryPerson.Id);
                updatedPersonAggregate2.IsDeleted.Should().BeFalse();

                // Assert: Check user login emails after deletion
                var updatedNonPrimaryPersonCustomerUserLogin = stack2.UserLoginEmailRepository
                    .GetUserLoginByEmail(tenantId, organisationId, nonPrimaryPersonCustomerUser.LoginEmail);
                updatedNonPrimaryPersonCustomerUserLogin.Should().NotBeNull();
                var updatedPrimaryPersonCustomerUserLogin = stack2.UserLoginEmailRepository
                    .GetUserLoginByEmail(tenantId, organisationId, primaryCustomerUser.LoginEmail);
                updatedPrimaryPersonCustomerUserLogin.Should().NotBeNull();

                // Assert: Check user read model record after deletion
                var updatedPrimaryPersonUser = stack2.UserReadModelRepository
                    .GetUser(primaryCustomerUser.TenantId, primaryCustomerUser.Id);
                updatedPrimaryPersonUser.Should().NotBeNull();
                var updatedNonPrimaryPersonUser = stack2.UserReadModelRepository
                    .GetUser(nonPrimaryPersonCustomerUser.TenantId, nonPrimaryPersonCustomerUser.Id);
                updatedNonPrimaryPersonUser.Should().NotBeNull();

                // Assert: Check user aggregate record after deletion
                var updatedPrimaryPersonUserAggregate = stack2.UserAggregateRepository.GetById(tenantId, primaryCustomerUser.Id);
                updatedPrimaryPersonUserAggregate.Blocked.Should().BeFalse();
                var updatedNonPrimaryPersonUserAggregate
                    = stack2.UserAggregateRepository.GetById(tenantId, nonPrimaryPersonCustomerUser.Id);
                updatedNonPrimaryPersonUserAggregate.Blocked.Should().BeFalse();
            }
        }

        [Fact]
        public async Task Save_PersistedCorrectly_WhenCreatingAggregateWithEvents()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var organisationId = Guid.NewGuid();
            tenant.Details.UpdateDefaultOrganisation(organisationId);
            PersonAggregate person = null;
            CustomerAggregate customer = null;

            // Act
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var personCommonProperties = new PersonCommonProperties
                {
                    FullName = "test",
                    FirstName = "test2",
                    LastName = "lastName",
                    Email = "email@gmail.com",
                    OrganisationId = organisationId,
                };

                var performingUserId = stack.HttpContextPropertiesResolver.PerformingUserId;
                var personDetails = new PersonalDetails(tenant.Id, personCommonProperties);
                person = PersonAggregate.CreatePersonFromPersonalDetails(
                    tenant.Id, organisationId, personDetails, performingUserId, stack.Clock.Now());
                person.Update(personDetails, performingUserId, stack.Clock.Now());
                person.RecordUserAccountCreatedForPerson(userId, performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(person);

                customer = CustomerAggregate.CreateNewCustomer(
                    tenant.Id,
                    person,
                    Domain.DeploymentEnvironment.Development,
                    this.performingUserId,
                    null,
                    stack.Clock.Now());
                await stack.CustomerAggregateRepository.Save(customer);

                person = stack.PersonAggregateRepository.GetById(tenant.Id, person.Id);
                person.AssociateWithCustomer(customer.Id, performingUserId, stack.Clock.Now());
                person.AssignMissingTenantId(tenant.Id, performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(person);
            }

            // Assert
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var aggregateResult = stack.PersonAggregateRepository.GetById(tenant.Id, person.Id);
                var readModelResult = stack.PersonReadModelRepository.GetPersonSummaryById(person.TenantId, person.Id);

                aggregateResult.TenantId.Should().Be(tenant.Id);
                aggregateResult.OrganisationId.Should().Be(organisationId);
                aggregateResult.CustomerId.Should().Be(customer.Id);
                aggregateResult.UserId.Should().Be(userId);
                ((IEntityReadModel<Guid>)readModelResult).TenantId.Should().Be(tenant.Id);
                readModelResult.OrganisationId.Should().Be(organisationId);
                readModelResult.CustomerId.Should().Be(customer.Id);
                readModelResult.UserId.Should().Be(userId);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task ReplayAllEventsByAggregateId_RecreateExistingPeopleToPersonReadModelTable_Succeed()
        {
            // Arrange
            var clock = SystemClock.Instance;
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            Guid personOneAggregateId = default;
            Guid personTwoAggregateId = default;

            // Act
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var personOne = this.CreatePerson(tenantId, 1);
                var personAggregateOne = PersonAggregate.CreatePersonFromPersonalDetails(
                    tenant.Id, tenant.Details.DefaultOrganisationId, personOne, this.performingUserId, clock.Now());
                personOneAggregateId = personAggregateOne.Id;

                var personTwo = this.CreatePerson(tenantId, 2);
                var personAggregateTwo = PersonAggregate.CreatePersonFromPersonalDetails(
                    tenant.Id, tenant.Details.DefaultOrganisationId, personTwo, this.performingUserId, clock.Now());
                personTwoAggregateId = personAggregateTwo.Id;

                await stack1.PersonAggregateRepository.Save(personAggregateOne);
                await stack1.PersonAggregateRepository.Save(personAggregateTwo);

                var customer1 = CustomerAggregate.CreateNewCustomer(
                    tenant.Id,
                    personAggregateOne,
                    Domain.DeploymentEnvironment.Development,
                    this.performingUserId,
                    null,
                    clock.Now());
                var user1 = UserAggregate.CreateUser(
                    tenant.Id, Guid.NewGuid(), UserType.Customer, personAggregateTwo, this.performingUserId, null, clock.Now());
                await stack1.CustomerAggregateRepository.Save(customer1);
                await stack1.UserAggregateRepository.Save(user1);

                using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
                {
                    IPersonService personService = new PersonService(
                        stack2.CustomerAggregateRepository,
                        stack2.CustomerReadModelRepository,
                        stack2.PersonAggregateRepository,
                        stack2.PersonReadModelRepository,
                        stack2.HttpContextPropertiesResolver,
                        stack2.UserAggregateRepository,
                        stack2.UserReadModelRepository,
                        stack2.TenantRepository,
                        stack2.Clock);

                    // delete the created persons from personReadModel as being created on initialize event
                    // and test whether it is recreating the record or not
                    // Note : we need to implement the person inialize event in order to recreate the person when replaying events
                    stack2.DbContext.Database.ExecuteSqlCommand("delete from dbo.EmailAddressReadModels where PersonReadModel_Id in('" + personOneAggregateId + "','" + personTwoAggregateId + "')");
                    stack2.DbContext.Database.ExecuteSqlCommand("delete from dbo.PhoneNumberReadModels where PersonReadModel_Id in('" + personOneAggregateId + "','" + personTwoAggregateId + "')");
                    stack2.DbContext.Database.ExecuteSqlCommand("delete from dbo.PersonReadModels where id in('" + personOneAggregateId + "','" + personTwoAggregateId + "')");

                    // call the replay event function via personService's RecreateExistingPeopleToPersonReadModelTable
                    personService.RecreateExistingPeopleToPersonReadModelTable();

                    // Assert
                    var savedPersonOneAggregate = stack2.PersonAggregateRepository.GetById(tenant.Id, personOneAggregateId);
                    var savedPersonTwoAggregate = stack2.PersonAggregateRepository.GetById(tenant.Id, personTwoAggregateId);
                    var copiedCustomerPerson = stack2.PersonReadModelRepository.GetPersonSummaryById(tenant.Id, personOneAggregateId);
                    var copiedUserPerson = stack2.PersonReadModelRepository.GetPersonSummaryById(tenant.Id, personTwoAggregateId);

                    // customer
                    Assert.Equal(copiedCustomerPerson.FullName, savedPersonOneAggregate.FirstName + " " + savedPersonOneAggregate.LastName);
                    Assert.Equal(copiedCustomerPerson.FirstName, savedPersonOneAggregate.FirstName);
                    Assert.Equal(copiedCustomerPerson.LastName, savedPersonOneAggregate.LastName);
                    Assert.Equal(copiedCustomerPerson.MiddleNames, savedPersonOneAggregate.MiddleNames);
                    Assert.Equal(copiedCustomerPerson.NamePrefix, savedPersonOneAggregate.NamePrefix);
                    Assert.Equal(copiedCustomerPerson.NameSuffix, savedPersonOneAggregate.NameSuffix);
                    Assert.Equal(copiedCustomerPerson.Email, savedPersonOneAggregate.Email);

                    // user
                    Assert.Equal(copiedUserPerson.FullName, savedPersonTwoAggregate.FirstName + " " + savedPersonTwoAggregate.LastName);
                    Assert.Equal(copiedUserPerson.FirstName, savedPersonTwoAggregate.FirstName);
                    Assert.Equal(copiedUserPerson.LastName, savedPersonTwoAggregate.LastName);
                    Assert.Equal(copiedUserPerson.MiddleNames, savedPersonTwoAggregate.MiddleNames);
                    Assert.Equal(copiedUserPerson.NamePrefix, savedPersonTwoAggregate.NamePrefix);
                    Assert.Equal(copiedUserPerson.NameSuffix, savedPersonTwoAggregate.NameSuffix);
                    Assert.Equal(copiedUserPerson.Email, savedPersonTwoAggregate.Email);
                }
            }
        }

        private record CustomerPerson(CustomerAggregate customerAggregate, PersonAggregate person1, PersonAggregate person2);

        private async Task<CustomerPerson> CreateTestPersonsForCustomer(Guid tenantId, Guid organisationId)
        {
            var clock = SystemClock.Instance;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var personOne = this.CreatePerson(tenantId, 1);
                var personAggregateOne = PersonAggregate.CreatePersonFromPersonalDetails(
                    tenantId, organisationId, personOne, this.performingUserId, clock.Now());
                var personOneAggregateId = personAggregateOne.Id;
                await stack1.PersonAggregateRepository.Save(personAggregateOne);

                var personTwo = this.CreatePerson(tenantId, 2);
                var personAggregateTwo = PersonAggregate.CreatePersonFromPersonalDetails(
                    tenantId, organisationId, personTwo, this.performingUserId, clock.Now());
                var personTwoAggregateId = personAggregateTwo.Id;

                var customerOne = await stack1.CreateCustomerForExistingPerson(personAggregateOne, DeploymentEnvironment.Staging, null, null);

                var newCustomerAggregate = stack1.CustomerAggregateRepository.GetById(tenantId, customerOne.Id);
                newCustomerAggregate.SetPrimaryPerson(personAggregateOne.Id, this.performingUserId, clock.Now());
                await stack1.CustomerAggregateRepository.Save(newCustomerAggregate);

                personAggregateTwo.AssociateWithCustomer(customerOne.Id, this.performingUserId, clock.Now());
                await stack1.PersonAggregateRepository.Save(personAggregateTwo);

                return new CustomerPerson(customerOne, personAggregateOne, personAggregateTwo);
            }
        }

        private void CreateCustomerRole(Guid tenantId, Guid organisationId)
        {
            var clock = SystemClock.Instance;
            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var customerRole = RoleHelper.CreateCustomerRole(tenantId, organisationId, clock.Now());
                stack1.RoleRepository.Insert(customerRole);
                stack1.RoleRepository.SaveChanges();
            }
        }

        private FakePersonalDetails CreatePerson(Guid tenantId, int personNo)
        {
            var person = new FakePersonalDetails();
            if (personNo == 1)
            {
                person = new FakePersonalDetails
                {
                    TenantId = tenantId,
                    FullName = "Customer test One",
                    FirstName = "Customer",
                    MiddleNames = "test",
                    LastName = "One",
                    NamePrefix = "Dr",
                    NameSuffix = "Jr",
                    PreferredName = "customerOne",
                    Email = "customer_one@email",
                };
            }

            if (personNo == 2)
            {
                person = new FakePersonalDetails
                {
                    TenantId = tenantId,
                    FullName = "Customer test Two",
                    FirstName = "Customer",
                    MiddleNames = "test",
                    LastName = "two",
                    NamePrefix = "Dr",
                    NameSuffix = "Jr",
                    PreferredName = "customerTwo",
                    Email = "customer_two@email",
                };
            }

            return person;
        }
    }
}
