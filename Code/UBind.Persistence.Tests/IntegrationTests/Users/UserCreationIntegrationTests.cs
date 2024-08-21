// <copyright file="UserCreationIntegrationTests.cs" company="uBind">
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
    using NodaTime;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Person;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class UserCreationIntegrationTests
    {
        private readonly Guid performingUserId = Guid.NewGuid();
        private readonly Func<ApplicationStack> applicationStack = ()
            => new ApplicationStack(DatabaseFixture.TestConnectionStringName, ApplicationStackConfiguration.Default);

        [Fact]
        public async Task CreateClientUser_WithExistingClientUserEmail_ShouldThrowException_OnDifferentOrganisations()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var email = Guid.NewGuid().ToString();

                var tenant = TenantFactory.Create(Guid.NewGuid());
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();

                var organisation = await this.CreateOrganisation(tenant);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                await this.CreateUser(tenant, email);
                var filters = new UserReadModelFilters();
                filters.OrganisationIds = new List<Guid> { organisation.Id };

                // Act
                var users = stack.UserReadModelRepository.GetUsers(tenant.Id, filters);

                // Assert
                users.Should().NotBeNullOrEmpty();

                // Arrange
                var anotherOrganisation = await this.CreateOrganisation(tenant);
                var signupModel = this.CreateUserSignupModel(tenant.Id, anotherOrganisation.Id, email);

                // Act
                Func<Task<UserAggregate>> action = async () => await stack.UserService.CreateUser(signupModel);

                // Assert
                await action.Should().ThrowAsync<ErrorException>();
            }
        }

        [Fact]
        public async Task CreateClientUser_WithExistingClientUserEmail_ShouldThrowException_OnTheSameOrganisations()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var email = Guid.NewGuid().ToString();
                var tenant = TenantFactory.Create(Guid.NewGuid());

                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();

                var organisation = await this.CreateOrganisation(tenant);

                await this.CreateUser(tenant, email);
                var filters = new UserReadModelFilters();
                filters.OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId };

                // Act
                var users = stack.UserReadModelRepository.GetUsers(
                    tenant.Id,
                    filters);

                // Assert
                users.Should().NotBeNullOrEmpty();

                // Arrange
                var signupModel = this.CreateUserSignupModel(tenant.Id, tenant.Details.DefaultOrganisationId, email);

                // Act
                Func<Task<UserAggregate>> action = async () => await stack.UserService.CreateUser(signupModel);

                // Assert
                await action.Should().ThrowAsync<ErrorException>();
            }
        }

        [Fact]
        public async Task CreateClientUser_WithExistingCustomerUserEmail_ShouldThrowException_OnDifferentOrganisations()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var email = Guid.NewGuid().ToString();
                var tenant = TenantFactory.Create(Guid.NewGuid());
                var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());

                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.Insert(product);

                var organisation = await this.CreateOrganisation(tenant);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var customerRole
                    = RoleHelper.CreateCustomerRole(tenant.Id, organisation.Id, stack.Clock.Now());
                stack.RoleRepository.Insert(customerRole);

                var customerUser = await this.CreateCustomerUser(tenant, organisation.Id, product.Id, email);
                var filters = new UserReadModelFilters();
                filters.OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId };

                // Act
                var users = stack.UserReadModelRepository.GetUsers(tenant.Id, filters);

                // Assert
                users.Should().NotBeNullOrEmpty();

                // Arrange
                var anotherOrganisation = await this.CreateOrganisation(tenant);
                var signupModel = this.CreateUserSignupModel(tenant.Id, anotherOrganisation.Id, email);

                // Act
                Func<Task<UserAggregate>> action = async () => await stack.UserService.CreateUser(signupModel);

                // Assert
                await action.Should().ThrowAsync<ErrorException>();
            }
        }

        [Fact]
        public async Task CreateClientUser_WithExistingCustomerUserEmail_ShouldThrowException_OnTheSameOrganisations()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var email = Guid.NewGuid().ToString();
                var tenant = TenantFactory.Create(Guid.NewGuid());
                var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());

                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.Insert(product);

                var organisation = await this.CreateOrganisation(tenant);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var customerRole
                    = RoleHelper.CreateCustomerRole(tenant.Id, organisation.Id, stack.Clock.Now());
                stack.RoleRepository.Insert(customerRole);

                var customerUser = await this.CreateCustomerUser(tenant, organisation.Id, product.Id, email);
                var filters = new UserReadModelFilters();
                filters.OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId };

                // Act
                var users = stack.UserReadModelRepository.GetUsers(tenant.Id, filters);

                // Assert
                users.Should().NotBeNullOrEmpty();

                // Arrange
                var signupModel = this.CreateUserSignupModel(tenant.Id, tenant.Details.DefaultOrganisationId, email);

                // Act
                Func<Task<UserAggregate>> action = async () => await stack.UserService.CreateUser(signupModel);

                // Assert
                await action.Should().ThrowAsync<ErrorException>();
            }
        }

        [Fact]
        public async Task CreateClientUser_WithNonExistingEmail_ShouldSucceed()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var email = Guid.NewGuid().ToString();

                var tenant = TenantFactory.Create(Guid.NewGuid());
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();

                var organisation = await this.CreateOrganisation(tenant);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                await this.CreateUser(tenant, email);
                var filters = new UserReadModelFilters();
                filters.OrganisationIds = new List<Guid> { organisation.Id };

                // Act
                var users = stack.UserReadModelRepository.GetUsers(tenant.Id, filters);

                // Assert
                users.Should().NotBeNullOrEmpty();

                var newEmail = Guid.NewGuid().ToString();
                await this.CreateUser(tenant, newEmail);
            }
        }

        [Fact]
        public async Task CreateCustomerUser_WithNonExistingEmail_ShouldNotRemovePhoneNumber()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var environment = DeploymentEnvironment.Staging;
                var email = Guid.NewGuid().ToString();
                var homePoneNumber = "0422 222 222";
                var workPoneNumber = "0422 222 111";

                var tenant = TenantFactory.Create(Guid.NewGuid());
                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();

                var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
                stack.ProductRepository.Insert(product);
                stack.ProductRepository.SaveChanges();

                var organisation = await this.CreateOrganisation(tenant);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var customerRole
                    = RoleHelper.CreateCustomerRole(tenant.Id, organisation.Id, stack.Clock.Now());
                stack.RoleRepository.Insert(customerRole);
                stack.RoleRepository.SaveChanges();

                var person = PersonAggregate.CreatePerson(
                     tenant.Id,
                     tenant.Details.DefaultOrganisationId,
                     this.performingUserId,
                     stack.Clock.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = email,
                    HomePhoneNumber = homePoneNumber,
                    WorkPhoneNumber = workPoneNumber,
                };
                person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(person);

                await stack.CreateCustomerForExistingPerson(
                        person, DeploymentEnvironment.Staging, null, null);

                var userCustomer = await stack.UserService.CreateCustomerUserForPersonAndSendActivationInvitation(
                    tenant.Id,
                    person.Id,
                    environment);

                // Act
                var handler = new CustomerHasUserAccountQueryHandler(
                    stack.PersonReadModelRepository, stack.UserReadModelRepository);
                var query = new CustomerHasUserAccountQuery(userCustomer.TenantId, (Guid)userCustomer.CustomerId);
                var hasUserAccount = await handler.Handle(query, CancellationToken.None);

                // Assert
                hasUserAccount.Should().BeTrue();
            }
        }

        [Fact]
        public async Task CreateCustomerUser_WithExistingCustomerUserEmail_ShouldThrowException_OnSameOrganisations()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var email = Guid.NewGuid().ToString();
                var tenant = TenantFactory.Create(Guid.NewGuid());
                var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());

                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.Insert(product);

                var organisation = await this.CreateOrganisation(tenant);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var customerRole
                    = RoleHelper.CreateCustomerRole(tenant.Id, organisation.Id, stack.Clock.Now());
                stack.RoleRepository.Insert(customerRole);

                var customerUser = await this.CreateCustomerUser(tenant, organisation.Id, product.Id, email);
                var filters = new UserReadModelFilters();
                filters.OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId };

                // Act
                var users = stack.UserReadModelRepository.GetUsers(tenant.Id, filters);

                // Assert
                users.Should().NotBeNullOrEmpty();

                Func<Task<UserAggregate>> action = async ()
                    => await this.CreateCustomerUser(tenant, organisation.Id, product.Id, email);
                await action.Should().ThrowAsync<ErrorException>();
            }
        }

        [Fact]
        public async Task CreateCustomerUser_WithExistingCustomerUserEmail_ShouldSucceed_OnDifferentOrganisations()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var email = Guid.NewGuid().ToString();
                var tenant = TenantFactory.Create(Guid.NewGuid());
                var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());

                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.Insert(product);

                var organisation = await this.CreateOrganisation(tenant);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var customerRole
                    = RoleHelper.CreateCustomerRole(tenant.Id, organisation.Id, stack.Clock.Now());
                stack.RoleRepository.Insert(customerRole);

                var customerUser = await this.CreateCustomerUser(tenant, organisation.Id, product.Id, email);
                var filters = new UserReadModelFilters();
                filters.OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId };

                // Act
                var users = stack.UserReadModelRepository.GetUsers(tenant.Id, filters);

                // Assert
                users.Should().NotBeNullOrEmpty();

                var anotherOrganisation = await this.CreateOrganisation(tenant);
                await this.CreateCustomerUser(tenant, anotherOrganisation.Id, product.Id, email);
            }
        }

        [Fact]
        public async Task CreateNonPrimaryCustomerUser_WithEmail_ShouldSucceed()
        {
            using (var stack = this.applicationStack())
            {
                // Arrange
                var primaryPersonEmail = Guid.NewGuid().ToString();
                var nonPrimaryPersonEmail = Guid.NewGuid().ToString();
                var tenant = TenantFactory.Create(Guid.NewGuid());
                var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());

                stack.TenantRepository.Insert(tenant);
                stack.TenantRepository.SaveChanges();
                stack.ProductRepository.Insert(product);

                var organisation = await this.CreateOrganisation(tenant);
                tenant.SetDefaultOrganisation(organisation.Id, stack.Clock.Now().Plus(Duration.FromMinutes(1)));
                stack.TenantRepository.SaveChanges();

                var customerRole
                    = RoleHelper.CreateCustomerRole(tenant.Id, organisation.Id, stack.Clock.Now());
                stack.RoleRepository.Insert(customerRole);

                var primaryCustomer = await this.CreateCustomer(tenant, organisation.Id, product.Id, primaryPersonEmail);
                var nonPrimaryPerson = await this.CreatePersonAndAssociateWithCustomer(
                    tenant,
                    organisation.Id,
                    product.Id,
                    nonPrimaryPersonEmail,
                    primaryCustomer.Id);

                var organisationReadModel = new OrganisationReadModel(
                    tenant.Id, organisation.Id, tenant.Details.Alias, tenant.Details.Name, null, true, false, stack.Clock.Now());
                MemoryCachingHelper.Upsert($"tenantId:{tenant.Id}", tenant, DateTimeOffset.Now.AddMinutes(10));
                MemoryCachingHelper.Upsert(
                    $"tenantId:{tenant.Id}|organisationId:{organisation.Id}",
                    organisationReadModel,
                    DateTimeOffset.Now.AddMinutes(10));

                // Act
                var personId = await stack.Mediator.Send(new CreateCustomerPersonUserAccountCommand(
                    tenant.Id,
                    null,
                    DeploymentEnvironment.Staging,
                    nonPrimaryPerson,
                    nonPrimaryPerson.OrganisationId,
                    nonPrimaryPerson.Id));

                var personWithUserAccount = await stack.Mediator.Send(new GetPersonSummaryByIdQuery(
                    tenant.Id, nonPrimaryPerson.Id));

                // Assert
                personId.Should().NotBeEmpty();
                personWithUserAccount.UserId.Should().HaveValue();
                personWithUserAccount.UserHasBeenInvitedToActivate.Should().BeTrue();
                personWithUserAccount.Id.Equals(nonPrimaryPerson.Id);
                personWithUserAccount.Email.Equals(nonPrimaryPersonEmail);
            }
        }

        private async Task<UBind.Domain.Aggregates.Organisation.Organisation> CreateOrganisation(Tenant tenant)
        {
            using (var stack = this.applicationStack())
            {
                var organisation = UBind.Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, this.performingUserId,
                    stack.Clock.Now());
                await stack.OrganisationAggregateRepository.Save(organisation);
                return organisation;
            }
        }

        private async Task<UserAggregate> CreateCustomerUser(
            Tenant tenant, Guid organisationId, Guid productId, string email)
        {
            using (var stack = this.applicationStack())
            {
                var environment = DeploymentEnvironment.Staging;
                var person = PersonAggregate.CreatePerson(
                     tenant.Id,
                     organisationId,
                     this.performingUserId,
                     SystemClock.Instance.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = email,
                };
                person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(person);

                await stack.CreateCustomerForExistingPerson(
                    person, DeploymentEnvironment.Staging, null, null);

                var userCustomer = await stack.UserService.CreateCustomerUserForPersonAndSendActivationInvitation(
                    tenant.Id, person.Id, environment);

                return userCustomer;
            }
        }

        private async Task CreateUser(Tenant tenant, string email)
        {
            using (var stack = this.applicationStack())
            {
                var person = PersonAggregate.CreatePerson(
                     tenant.Id,
                     tenant.Details.DefaultOrganisationId,
                     this.performingUserId,
                     stack.Clock.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = email,
                };
                person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(person);

                var userId = Guid.NewGuid();
                var persistedUserAggregate
                    = UserAggregate.CreateUser(person.TenantId, userId, UserType.Master, person, this.performingUserId, null, stack.Clock.Now());
                persistedUserAggregate.CreateActivationInvitation(this.performingUserId, stack.Clock.Now());
                await stack.UserAggregateRepository.Save(persistedUserAggregate);
            }
        }

        private UserSignupModel CreateUserSignupModel(Guid tenantId, Guid organisationId, string email)
        {
            return new UserSignupModel()
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
                TenantId = tenantId,
                OrganisationId = organisationId,
            };
        }

        private async Task<CustomerAggregate> CreateCustomer(
            Tenant tenant, Guid organisationId, Guid productId, string email)
        {
            using (var stack = this.applicationStack())
            {
                var person = PersonAggregate.CreatePerson(
                     tenant.Id,
                     organisationId,
                     this.performingUserId,
                     SystemClock.Instance.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = email,
                };
                person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(person);

                var customerAggregate = await stack.CreateCustomerForExistingPerson(
                        person, DeploymentEnvironment.Staging, null, null);

                return customerAggregate;
            }
        }

        private async Task<PersonAggregate> CreatePersonAndAssociateWithCustomer(
            Tenant tenant, Guid organisationId, Guid productId, string email, Guid customerId)
        {
            using (var stack = this.applicationStack())
            {
                var person = PersonAggregate.CreatePerson(
                     tenant.Id,
                     organisationId,
                     this.performingUserId,
                     SystemClock.Instance.Now());
                var personCommonProperties = new PersonCommonProperties
                {
                    Email = email,
                };
                person.Update(new PersonalDetails(tenant.Id, personCommonProperties), this.performingUserId, stack.Clock.Now());
                person.AssociateWithCustomer(customerId, this.performingUserId, stack.Clock.Now());
                await stack.PersonAggregateRepository.Save(person);

                return person;
            }
        }
    }
}
