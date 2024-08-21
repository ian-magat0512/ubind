// <copyright file="CustomerServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Queries.Customer;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class CustomerServiceIntegrationTests
    {
        private readonly ApplicationStack stack;
        private readonly Guid? performingUserId = Guid.NewGuid();

        public CustomerServiceIntegrationTests()
        {
            this.stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
        }

        [Fact]
        public async Task UpdateCustomer_RetainCreatedDateAsIs_ShouldNotUpdateTheCreatedDateWhenUpdatingCustomerDetails()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var currentInstance = SystemClock.Instance.GetCurrentInstant();

            var tenant = TenantFactory.Create(tenantId);
            var product = ProductFactory.Create(productId, tenant.Id);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, this.performingUserId, currentInstance);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);
                await stack.CreateOrganisation(organisation);

                var personCommonProperties = new PersonCommonProperties
                {
                    FirstName = "Ken",
                    LastName = "Gurow",
                    FullName = "Ken Gurow",
                };

                // Act
                var customerPerson = PersonAggregate.CreatePerson(
                    tenant.Id, organisation.Id, this.performingUserId, currentInstance);
                await stack.PersonAggregateRepository.Save(customerPerson);

                var customer = await stack.CreateCustomerForExistingPerson(customerPerson, environment, null, null);
                var originalCustomerDetail = stack.CustomerService.GetCustomerById(customer.TenantId, customer.Id);

                // update person detail
                var personAggregate = stack.PersonAggregateRepository.GetById(tenant.Id, customer.PrimaryPersonId);
                personAggregate.UpdateFullName(personCommonProperties.FullName, this.performingUserId, SystemClock.Instance.GetCurrentInstant());

                await stack.PersonAggregateRepository.Save(personAggregate);
                var updatedCustomer = stack.CustomerService.GetCustomerById(customer.TenantId, customer.Id);

                // Assert
                originalCustomerDetail.CreatedTicksSinceEpoch.Should().Be(updatedCustomer.CreatedTicksSinceEpoch);
                originalCustomerDetail.LastModifiedTicksSinceEpoch.Should().Be(updatedCustomer.CreatedTicksSinceEpoch);
            }
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task CustomerService_CapsListsAt1000()
        {
            // Arrange
            Mock<System.Data.Entity.DbSet<CustomerReadModel>> mockCustomerReadModel = new Mock<System.Data.Entity.DbSet<CustomerReadModel>>();
            List<CustomerReadModel> customerReadModels = new List<CustomerReadModel>();
            var tenant = TenantFactory.Create(Guid.NewGuid());
            this.stack.CreateTenant(tenant);

            var role = new Role(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                Domain.Permissions.DefaultRole.TenantAdmin,
                this.stack.Clock.Now());
            this.stack.RoleRepository.Insert(role);
            this.stack.RoleRepository.SaveChanges();
            var email = "tester" + Guid.NewGuid() + "@ubind.io";
            var userSignupModel = new Application.User.UserSignupModel()
            {
                AlternativeEmail = email,
                WorkPhoneNumber = "123",
                Email = email,
                Environment = DeploymentEnvironment.Staging,
                FullName = "123",
                HomePhoneNumber = "123",
                MobilePhoneNumber = "123",
                PreferredName = "123",
                UserType = UserType.Client,
                TenantId = tenant.Id,
            };

            var user = await this.stack.UserService.CreateUser(userSignupModel);
            for (int i = 0; i < 1001; ++i)
            {
                var customerPerson = PersonAggregate.CreatePerson(
                     tenant.Id,
                     tenant.Details.DefaultOrganisationId,
                     this.performingUserId,
                     SystemClock.Instance.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = email,
                };

                customerPerson.Update(
                    new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, SystemClock.Instance.Now());
                await this.stack.PersonAggregateRepository.Save(customerPerson);

                var customer = await this.stack.CreateCustomerForExistingPerson(customerPerson, DeploymentEnvironment.Staging, user.Id, null);
            }

            EntityListFilters filters = new EntityListFilters
            {
                TenantId = tenant.Id,
                OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId },
                Environment = DeploymentEnvironment.Staging,
            };

            // Act
            var handler = new GetCustomersMatchingFiltersQueryHandler(this.stack.CustomerReadModelRepository);
            var query = new GetCustomersMatchingFiltersQuery(tenant.Id, filters);
            var customers = await handler.Handle(query, CancellationToken.None);

            // Assert
            customers.Should().HaveCount(3);
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetCustomersForUser_ShouldIncludeCustomersFromSubOrganisations_WhenUserIsFromDefaultOrganisation()
        {
            // Arrange
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());
            var currentInstance = SystemClock.Instance.GetCurrentInstant();
            var environment = QuoteFactory.DefaultEnvironment;

            var tenantAlias = tenant.Details.Alias;
            var tenantName = tenant.Details.Name;
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenantAlias, tenantName, null, this.performingUserId, currentInstance);
            var anotherOrganisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenantAlias, tenantName, null, this.performingUserId, currentInstance);

            this.stack.CreateTenant(tenant);
            this.stack.CreateProduct(product);

            await this.stack.OrganisationAggregateRepository.Save(organisation);
            tenant.SetDefaultOrganisation(organisation.Id, currentInstance.Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            await this.stack.OrganisationAggregateRepository.Save(anotherOrganisation);

            this.CreateClientAdminAndCustomerRolesForTenant(tenant);

            var email = "tester" + Guid.NewGuid() + "@ubind.io";
            var userSignupModel = new Application.User.UserSignupModel()
            {
                AlternativeEmail = email,
                WorkPhoneNumber = "123",
                Email = email,
                Environment = environment,
                FullName = "123",
                HomePhoneNumber = "123",
                MobilePhoneNumber = "123",
                PreferredName = "123",
                UserType = UserType.Client,
                TenantId = tenant.Id,
                OrganisationId = organisation.Id,
            };

            var user = await this.stack.UserService.CreateUser(userSignupModel);
            var userAuthData = new UserAuthenticationData(
                tenant.Id, organisation.Id, UserType.Client, user.Id, default);

            var personCommonProperties = new PersonCommonProperties
            {
                Email = email,
            };

            // Customer 1 from default organisation
            var customerPerson1 = PersonAggregate.CreatePerson(
                tenant.Id, organisation.Id, this.performingUserId, currentInstance);
            customerPerson1.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, currentInstance);
            await this.stack.PersonAggregateRepository.Save(customerPerson1);
            await this.stack.CreateCustomerForExistingPerson(customerPerson1, environment, user.Id, null);

            // Customer 2 from another organisation
            var customerPerson2 = PersonAggregate.CreatePerson(
                tenant.Id, anotherOrganisation.Id, this.performingUserId, currentInstance);
            customerPerson2.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, currentInstance);
            await this.stack.PersonAggregateRepository.Save(customerPerson2);
            await this.stack.CreateCustomerForExistingPerson(customerPerson2, environment, user.Id, null);

            EntityListFilters filters = new EntityListFilters
            {
                TenantId = tenant.Id,
                OrganisationIds = new List<Guid> { organisation.Id },
                Environment = environment,
            };

            // Act
            var handler = new GetCustomersMatchingFiltersQueryHandler(this.stack.CustomerReadModelRepository);
            var query = new GetCustomersMatchingFiltersQuery(tenant.Id, filters);
            var customers = await handler.Handle(query, CancellationToken.None);

            // Assert
            customers.Should().HaveCount(2)
                .And.Contain(customer => customer.OrganisationId == organisation.Id)
                .And.Contain(customer => customer.OrganisationId == anotherOrganisation.Id);
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task GetCustomersForUser_ShouldIncludeCustomersForSpecificOrganisation_WhenUserIsNotFromDefaultOrganisation()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var currentInstance = SystemClock.Instance.GetCurrentInstant();

            var tenantAlias = tenant.Details.Alias;
            var tenantName = tenant.Details.Name;
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenantAlias, tenantName, null, this.performingUserId, currentInstance);
            var anotherOrganisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenantAlias, tenantName, null, this.performingUserId, currentInstance);

            this.stack.TenantRepository.Insert(tenant);
            this.stack.TenantRepository.SaveChanges();
            this.stack.ProductRepository.Insert(product);
            this.stack.ProductRepository.SaveChanges();

            await this.stack.OrganisationAggregateRepository.Save(organisation);
            tenant.SetDefaultOrganisation(organisation.Id, currentInstance.Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            await this.stack.OrganisationAggregateRepository.Save(anotherOrganisation);

            this.CreateClientAdminAndCustomerRolesForTenant(tenant);

            var email = "tester" + Guid.NewGuid() + "@ubind.io";
            var userSignupModel = new Application.User.UserSignupModel()
            {
                AlternativeEmail = email,
                WorkPhoneNumber = "123",
                Email = email,
                Environment = environment,
                FullName = "123",
                HomePhoneNumber = "123",
                MobilePhoneNumber = "123",
                PreferredName = "123",
                UserType = UserType.Client,
                TenantId = tenant.Id,
                OrganisationId = organisation.Id,
            };

            var user = await this.stack.UserService.CreateUser(userSignupModel);
            var userAuthData = new UserAuthenticationData(
                tenant.Id, anotherOrganisation.Id, UserType.Client, user.Id, default);

            var personCommonProperties = new PersonCommonProperties
            {
                Email = email,
            };

            // Customer 1 from default organisation
            var customerPerson1 = PersonAggregate.CreatePerson(
                tenant.Id, organisation.Id, this.performingUserId, currentInstance);
            customerPerson1.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, currentInstance);
            await this.stack.PersonAggregateRepository.Save(customerPerson1);
            await this.stack.CreateCustomerForExistingPerson(customerPerson1, environment, user.Id, null);

            // Customer 2 from another organisation
            var customerPerson2 = PersonAggregate.CreatePerson(
                tenant.Id, anotherOrganisation.Id, this.performingUserId, currentInstance);
            customerPerson2.Update(
                new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, currentInstance);
            await this.stack.PersonAggregateRepository.Save(customerPerson2);
            await this.stack.CreateCustomerForExistingPerson(customerPerson2, environment, user.Id, null);

            EntityListFilters filters = new EntityListFilters
            {
                TenantId = tenant.Id,
                OrganisationIds = new List<Guid> { anotherOrganisation.Id },
                Environment = environment,
            };

            // Act
            var handler = new GetCustomersMatchingFiltersQueryHandler(this.stack.CustomerReadModelRepository);
            var query = new GetCustomersMatchingFiltersQuery(tenant.Id, filters);
            var customers = await handler.Handle(query, CancellationToken.None);

            // Assert
            customers.Should().HaveCount(1).And.Contain(customer => customer.OrganisationId == anotherOrganisation.Id);
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public async Task CreateCustomerForNewPerson_ShouldMatchTheGivenOrganisationId_ForNewPerson()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenant = TenantFactory.Create(Guid.NewGuid());
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            var currentInstance = SystemClock.Instance.GetCurrentInstant();

            var tenantAlias = tenant.Details.Alias;
            var tenantName = tenant.Details.Name;
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenantAlias, tenantName, null, this.performingUserId, currentInstance);

            this.stack.TenantRepository.Insert(tenant);
            this.stack.TenantRepository.SaveChanges();
            this.stack.ProductRepository.Insert(product);
            this.stack.ProductRepository.SaveChanges();

            await this.stack.OrganisationAggregateRepository.Save(organisation);
            tenant.SetDefaultOrganisation(organisation.Id, currentInstance.Plus(Duration.FromMinutes(1)));
            this.stack.TenantRepository.SaveChanges();

            this.CreateClientAdminAndCustomerRolesForTenant(tenant);

            var email = "tester" + Guid.NewGuid() + "@ubind.io";
            var userSignupModel = new Application.User.UserSignupModel()
            {
                AlternativeEmail = email,
                WorkPhoneNumber = "123",
                Email = email,
                Environment = environment,
                FullName = "123",
                HomePhoneNumber = "123",
                MobilePhoneNumber = "123",
                PreferredName = "123",
                UserType = UserType.Client,
                TenantId = tenant.Id,
                OrganisationId = organisation.Id,
            };
            var user = await this.stack.UserService.CreateUser(userSignupModel);

            var personCommonProperties = new PersonCommonProperties
            {
                AlternativeEmail = email,
                WorkPhoneNumber = "123",
                Email = email,
                FullName = "123",
                HomePhoneNumber = "123",
                MobilePhoneNumber = "123",
                PreferredName = "123",
                TenantId = tenant.Id,
                OrganisationId = organisation.Id,
            };

            // Customer 1 from default organisation
            var personAggregate = PersonAggregate.CreatePerson(
                tenant.Id, organisation.Id, this.performingUserId, currentInstance);
            var customerDetails = new PersonalDetails(tenant.Id, personCommonProperties);

            await this.stack.PersonAggregateRepository.Save(personAggregate);

            // await this.stack.CustomerService
            //    .CreateCustomerForNewPerson(tenant.Id, environment, customerDetails, user.Id, null);
            EntityListFilters filters = new EntityListFilters
            {
                TenantId = tenant.Id,
                OrganisationIds = new List<Guid> { organisation.Id },
                Environment = environment,
            };

            // Act
            var handler = new GetCustomersMatchingFiltersQueryHandler(this.stack.CustomerReadModelRepository);
            var query = new GetCustomersMatchingFiltersQuery(tenant.Id, filters);
            var customers = await handler.Handle(query, CancellationToken.None);

            // Assert
            customers.Should().HaveCount(1).And.Contain(customer => customer.OrganisationId == organisation.Id);
        }

        [Fact]
        public async Task UpdateCustomer_ShouldNotUpdateTheCustomerWhenInvalidPhoneNumberFails()
        {
            // Arrange
            var environment = QuoteFactory.DefaultEnvironment;
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var currentInstance = SystemClock.Instance.GetCurrentInstant();

            var tenant = TenantFactory.Create(tenantId);
            var product = ProductFactory.Create(productId, tenant.Id);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, this.performingUserId, currentInstance);

            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                stack.CreateTenant(tenant);
                stack.CreateProduct(product);
                await stack.CreateOrganisation(organisation);

                var phoneNumbers = new List<PhoneNumberField>
                {
                    new PhoneNumberField { PhoneNumber = "02 8503 8000", Label = "AU" }, // AU
                    new PhoneNumberField { PhoneNumber = "+1 541-754-3010", Label = "US" }, // US
                    new PhoneNumberField { PhoneNumber = "+44 20 1234 5678", Label = "UK" }, // UK
                    new PhoneNumberField { PhoneNumber = "+1 416-555-1234", Label = "CA" }, // CA
                    new PhoneNumberField { PhoneNumber = "+61 2 1234 5678", Label = "AU" }, // AU
                    new PhoneNumberField { PhoneNumber = "+49 30 12345678", Label = "DE" }, // DE
                    new PhoneNumberField { PhoneNumber = "+33 1 23 45 67 89", Label = "FR" } // FR
                };

                var email = "tester" + Guid.NewGuid() + "@ubind.io";

                var userSignupModel = new UserSignupModel
                {
                    AlternativeEmail = email,
                    WorkPhoneNumber = "123",
                    Email = email,
                    Environment = environment,
                    FullName = "john doe",
                    HomePhoneNumber = "123",
                    MobilePhoneNumber = "123",
                    PhoneNumbers = phoneNumbers,
                    PreferredName = "john",
                    UserType = UserType.Client,
                    TenantId = tenant.Id,
                    OrganisationId = organisation.Id,
                };

                var personalDetails = new PersonalDetails(userSignupModel);
                personalDetails.Validate();

                // Act
                var customerPerson = PersonAggregate.CreatePerson(
                    tenant.Id, organisation.Id, this.performingUserId, currentInstance);

                customerPerson.PhoneNumbers = phoneNumbers;

                await stack.PersonAggregateRepository.Save(customerPerson);

                var customer = await stack.CreateCustomerForExistingPerson(customerPerson, environment, null, null);
                var originalCustomerDetail = stack.CustomerService.GetCustomerById(customer.TenantId, customer.Id);

                // update person detail
                var personAggregate = stack.PersonAggregateRepository.GetById(tenant.Id, customer.PrimaryPersonId);
                personAggregate.UpdateFullName(userSignupModel.FullName, this.performingUserId, SystemClock.Instance.GetCurrentInstant());

                await stack.PersonAggregateRepository.Save(personAggregate);
                var updatedCustomer = stack.CustomerService.GetCustomerById(customer.TenantId, customer.Id);

                // Assert
                originalCustomerDetail.CreatedTicksSinceEpoch.Should().Be(updatedCustomer.CreatedTicksSinceEpoch);
                originalCustomerDetail.LastModifiedTicksSinceEpoch.Should().Be(updatedCustomer.CreatedTicksSinceEpoch);
            }
        }

        private void CreateClientAdminAndCustomerRolesForTenant(Tenant tenant)
        {
            this.stack.RoleRepository.Insert(RoleHelper.CreateTenantAdminRole(
                tenant.Id, tenant.Details.DefaultOrganisationId, SystemClock.Instance.Now()));
            this.stack.RoleRepository.Insert(RoleHelper.CreateCustomerRole(
                tenant.Id, tenant.Details.DefaultOrganisationId, SystemClock.Instance.Now()));
            this.stack.RoleRepository.SaveChanges();
        }
    }
}
