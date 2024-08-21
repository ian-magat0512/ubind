// <copyright file="UserReadModelRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.ReadModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class UserReadModelRepositoryIntegrationTests
    {
        [Fact]
        public async Task ListUsers_OnlyShowsUsers_FromCorrectTenant()
        {
            // Arrange
            var clock = new TestClock();
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);

            var tenant = await this.CreateTenant("urmrit-tenant", stack1);
            var fooTenant = await this.CreateTenant("urmrit-foo-tenant", stack1);
            var barTenant = await this.CreateTenant("urmrit-bar-tenant", stack1);
            var performingUserId = Guid.NewGuid();
            var product = ProductFactory.Create(tenant.Id, Guid.NewGuid());
            stack1.ProductRepository.Insert(product);
            stack1.DbContext.SaveChanges();

            var fooOrganisation = Organisation.CreateNewOrganisation(
                fooTenant.Id,
                fooTenant.Details.Alias,
                fooTenant.Details.Name,
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(fooOrganisation);
            fooTenant.SetDefaultOrganisation(
                fooOrganisation.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));

            var barOrganisation = Organisation.CreateNewOrganisation(
                barTenant.Id,
                barTenant.Details.Alias,
                barTenant.Details.Name,
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(barOrganisation);
            barTenant.SetDefaultOrganisation(
                barOrganisation.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));

            stack1.DbContext.SaveChanges();

            // Arrange
            await this.CreateUser(fooTenant.Id, fooTenant.Details.DefaultOrganisationId, stack1, "user1@example.com");
            await this.CreateUser(barTenant.Id, barTenant.Details.DefaultOrganisationId, stack1, "user1@example.com");

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var filters = new UserReadModelFilters();
            filters.OrganisationIds = new List<Guid> { fooTenant.Details.DefaultOrganisationId };

            // Act
            var users = stack2.UserReadModelRepository.GetUsers(
                fooTenant.Id, filters);

            // Assert
            users.Should().ContainSingle();
        }

        [Fact]
        public async Task ListUsers_IncludesUsersWithNoOrganisationId_WhenListingForDefaultOrganisation()
        {
            // Arrange
            var clock = new TestClock();
            var performingUserId = Guid.NewGuid();
            var tenant = TenantFactory.Create(Guid.NewGuid());

            using (var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var guid = Guid.NewGuid();
                var tenantAlias = "test-tenant-" + guid;
                var tenantName = "Test Tenant " + guid;
                var tenantId = await stack1.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
                tenant = await stack1.Mediator.Send(new GetTenantByIdQuery(tenantId));

                var organisation = Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, performingUserId,
                    clock.GetCurrentInstant());
                await stack1.OrganisationAggregateRepository.Save(organisation);

                tenant.SetDefaultOrganisation(
                    organisation.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
                stack1.TenantRepository.SaveChanges();

                await this.CreateUser(tenant.Id, tenant.Details.DefaultOrganisationId, stack1, "lu-user1@example.com");

                var anotherOrganisation = Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null,
                    performingUserId,
                    clock.GetCurrentInstant());
                await stack1.OrganisationAggregateRepository.Save(anotherOrganisation);
                await this.CreateUser(tenant.Id, anotherOrganisation.Id, stack1, "lu-user2@example.com");
            }

            using (var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var filters = new UserReadModelFilters();
                filters.OrganisationIds = new List<Guid> { tenant.Details.DefaultOrganisationId };

                // Act
                var users = stack2.UserReadModelRepository.GetUsers(
                    tenant.Id, filters);

                // Assert
                users.Should().HaveCount(1);
                users.Should().Contain(u => u.Email == "lu-user1@example.com");
                users.Should().NotContain(u => u.Email == "lu-user2@example.com");
            }
        }

        [Fact]
        public async Task ListUsers_DoesNotIncludeUsersWithNoOrganisationId_WhenListingForNonDefaultOrganisation()
        {
            // Arrange
            var clock = new TestClock();
            var performingUserId = Guid.NewGuid();
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var guid = Guid.NewGuid();
            var tenantAlias = "test-tenant-" + guid;
            var tenantName = "Test Tenant " + guid;
            var tenantId = await stack1.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
            var tenant = await stack1.Mediator.Send(new GetTenantByIdQuery(tenantId));

            var organisation1 = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(organisation1);

            tenant.SetDefaultOrganisation(organisation1.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack1.TenantRepository.SaveChanges();

            var organisation2 = Organisation.CreateNewOrganisation(
                tenant.Id,
                "different-organisation-alias",
                "Different organisation name",
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(organisation2);

            await this.CreateUser(tenant.Id, tenant.Details.DefaultOrganisationId, stack1, "lu-user11@example.com");
            await this.CreateUser(tenant.Id, organisation2.Id, stack1, "lu-user22@example.com");

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var filters = new UserReadModelFilters();
            filters.OrganisationIds = new List<Guid> { organisation2.Id };

            // Act
            var users = stack2.UserReadModelRepository.GetUsers(tenant.Id, filters);

            // Assert
            users.Should().HaveCount(1);
            users.Should().NotContain(u => u.Email == "lu-user11@example.com");
            users.Should().Contain(u => u.Email == "lu-user22@example.com");
        }

        [Theory]
        [InlineData("username@domain.com", "username@domain.com")] // match exact email
        [InlineData("username+test1@domain.com", "username@domain.com")] // email with same username and domain name with plus sign
        public async Task GetUserByEmail_ShouldReturn_EmailMatch(string userEmail, string findEmail)
        {
            // Arrange
            var clock = new TestClock();
            var performingUserId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);

            Tenant tenant = stack1.TenantRepository.GetTenantById(tenantId);
            if (tenant == null)
            {
                var guid = Guid.NewGuid();
                var tenantAlias = "test-tenant-" + guid;
                var tenantName = "Test Tenant " + guid;
                tenantId = await stack1.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
                tenant = await stack1.Mediator.Send(new GetTenantByIdQuery(tenantId));
            }

            var organisation1 = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(organisation1);

            tenant.SetDefaultOrganisation(organisation1.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack1.TenantRepository.SaveChanges();

            await this.CreateUser(tenant.Id, organisation1.Id, stack1, userEmail);

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);

            // Act
            var user = stack2.UserReadModelRepository.GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(findEmail).FirstOrDefault();

            // Assert
            user.Should().NotBeNull();
        }

        [Theory]
        [InlineData("username@domainname.com", "username@different-domain-name.com")] // email with same username but different domain.
        [InlineData("username1@domainname.com", "username2@domainname.com")] // email with same domain but different username
        [InlineData("usernameabcd@domainname.com", "usernameabc@domainname.com")] // email with same starting point in the username and same domain.
        [InlineData("username1@domain1.com", "username2@domain2.com")] // email with different username and different domain name.
        [InlineData("username+test1@domainname.com", "username@different-domain-name.com")] // email with same username but different domain and with plus sign.
        [InlineData("username1+test2@domainname.com", "username2@domainname.com")] // email with same domain but different username and with plus sign
        [InlineData("username1+test3@domain1.com", "username2@domain2.com")] // email with different username and different domain name and with plus sign.
        public async Task GetUserByEmail_ShouldNotReturn_EmailThatDoesNotMatch(string userEmail, string findEmail)
        {
            // Arrange
            var clock = new TestClock();
            var performingUserId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            Tenant tenant = stack1.TenantRepository.GetTenantById(tenantId);
            if (tenant == null)
            {
                var guid = Guid.NewGuid();
                var tenantAlias = "test-tenant-" + guid;
                var tenantName = "Test Tenant " + guid;
                tenantId = await stack1.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
                tenant = await stack1.Mediator.Send(new GetTenantByIdQuery(tenantId));
            }

            var organisation1 = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(organisation1);

            tenant.SetDefaultOrganisation(organisation1.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack1.TenantRepository.SaveChanges();

            await this.CreateUser(tenant.Id, organisation1.Id, stack1, userEmail);

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);

            // Act
            var users = stack2.UserReadModelRepository.GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(findEmail);

            // Assert
            users.Count().Should().Be(0);
        }

        [Fact]
        public async Task ListUsers_IncludesUsersWithDefaultOrganisationId_WhenListingForDefaultOrganisation()
        {
            // Arrange
            var clock = new TestClock();
            var performingUserId = Guid.NewGuid();

            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var guid = Guid.NewGuid();
            var tenantAlias = "test-tenant-" + guid;
            var tenantName = "Test Tenant " + guid;
            var tenantId = await stack1.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
            var tenant1 = await stack1.Mediator.Send(new GetTenantByIdQuery(tenantId));

            var organisation1 = Organisation.CreateNewOrganisation(
                tenant1.Id,
                tenant1.Details.Alias,
                tenant1.Details.Name,
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(organisation1);

            tenant1.SetDefaultOrganisation(organisation1.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack1.TenantRepository.SaveChanges();

            var organisation2 = Organisation.CreateNewOrganisation(
                tenant1.Id,
                "different-organisation-alias",
                "Different organisation name",
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(organisation2);

            await this.CreateUser(tenant1.Id, tenant1.Details.DefaultOrganisationId, stack1, "lu-user111@example.com");
            await this.CreateUser(tenant1.Id, organisation1.Id, stack1, "lu-user222@example.com");
            await this.CreateUser(tenant1.Id, organisation2.Id, stack1, "lu-user333@example.com");

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var filters = new UserReadModelFilters();
            filters.OrganisationIds = new List<Guid> { tenant1.Details.DefaultOrganisationId };

            // Act
            var users = stack2.UserReadModelRepository.GetUsers(tenant1.Id, filters);

            // Assert
            users.Should().HaveCount(2);
            users.Should().Contain(u => u.Email == "lu-user111@example.com");
            users.Should().Contain(u => u.Email == "lu-user222@example.com");
            users.Should().NotContain(u => u.Email == "lu-user333@example.com");
        }

        [Fact]
        public async Task ListUsers_IncludesUsersWithNonDefaultOrganisationId_WhenListingForNonDefaultOrganisation()
        {
            // Arrange
            var clock = new TestClock();
            var performingUserId = Guid.NewGuid();

            var stack1 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var guid = Guid.NewGuid();
            var tenantAlias = "test-tenant-" + guid;
            var tenantName = "Test Tenant " + guid;
            var tenantId = await stack1.Mediator.Send(new CreateTenantCommand(tenantName, tenantAlias, null));
            var tenant1 = await stack1.Mediator.Send(new GetTenantByIdQuery(tenantId));

            var organisation1 = Organisation.CreateNewOrganisation(
                tenant1.Id,
                tenant1.Details.Alias,
                tenant1.Details.Name,
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(organisation1);

            tenant1.SetDefaultOrganisation(organisation1.Id, clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack1.TenantRepository.SaveChanges();

            var organisation2 = Organisation.CreateNewOrganisation(
                tenant1.Id,
                "different-organisation-alias",
                "Different organisation name",
                null, performingUserId,
                clock.GetCurrentInstant());
            await stack1.OrganisationAggregateRepository.Save(organisation2);

            await this.CreateUser(tenant1.Id, tenant1.Details.DefaultOrganisationId, stack1, "lu-user1111@example.com");
            await this.CreateUser(tenant1.Id, organisation1.Id, stack1, "lu-user2222@example.com");
            await this.CreateUser(tenant1.Id, organisation2.Id, stack1, "lu-user3333@example.com");

            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var filters = new UserReadModelFilters();
            filters.OrganisationIds = new List<Guid> { organisation2.Id };

            // Act
            var users = stack2.UserReadModelRepository.GetUsers(tenant1.Id, filters);

            // Assert
            users.Should().HaveCount(1);
            users.Should().NotContain(u => u.Email == "lu-user1111@example.com");
            users.Should().NotContain(u => u.Email == "lu-user2222@example.com");
            users.Should().Contain(u => u.Email == "lu-user3333@example.com");
        }

        private async Task CreateUser(Guid tenantId, Guid organisationId, ApplicationStack stack, string email)
        {
            var userSignupModel = new UserSignupModel
            {
                TenantId = tenantId,
                OrganisationId = organisationId,
                Email = email,
                UserType = UserType.Master,
                Environment = DeploymentEnvironment.Development,
            };
            await stack.UserService.CreateUser(userSignupModel);
        }

        private async Task<Tenant> CreateTenant(string alias, ApplicationStack stack)
        {
            var tenantId = await stack.Mediator.Send(new CreateTenantCommand(alias, alias, null));
            return await stack.Mediator.Send(new GetTenantByIdQuery(tenantId));
        }
    }
}
