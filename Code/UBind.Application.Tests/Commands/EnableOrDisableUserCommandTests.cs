// <copyright file="EnableOrDisableUserCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.User;
    using UBind.Application.Services;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories.Redis;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class EnableOrDisableUserCommandTests
    {
        private readonly ServiceCollection serviceCollection;
        private readonly Guid performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;
        private readonly Mock<IUserAggregateRepository> mockUserAggregateRepository = new Mock<IUserAggregateRepository>();
        private readonly Mock<IUserReadModelRepository> mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
        private readonly Mock<IPersonAggregateRepository> mockPersonAggregateRepository = new Mock<IPersonAggregateRepository>();
        private readonly Mock<IUserSessionRepository> userSessionRepository = new Mock<IUserSessionRepository>();
        private readonly Mock<IHttpContextPropertiesResolver> mockHttpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
        private readonly Mock<IUserSessionDeletionService> mockUserSessionDeletionService = new Mock<IUserSessionDeletionService>();

        public EnableOrDisableUserCommandTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IClock>(SystemClock.Instance);
            services
                .AddSingleton<ICommandHandler<EnableOrDisableUsersCommand, List<UserModel>>,
                    EnableOrDisableUsersCommandHandler>();

            services.AddSingleton(this.mockUserAggregateRepository.Object);
            services.AddSingleton(this.mockUserReadModelRepository.Object);
            services.AddSingleton(this.mockPersonAggregateRepository.Object);
            services.AddSingleton(this.userSessionRepository.Object);
            services.AddSingleton(this.mockHttpContextPropertiesResolver.Object);
            services.AddSingleton(this.mockUserSessionDeletionService.Object);

            this.serviceCollection = services;
        }

        [Fact]
        public async Task ToggleUserStatusCommand_ShouldEnableUser_WhenBlockedIsFalse()
        {
            ///// Arrange

            var email = "customer_one@email";
            var tenantId = Tenant.MasterTenantId;

            var masterTenantUser = this.CreateUserAggregate(Tenant.MasterTenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(masterTenantUser.TenantId, masterTenantUser.Id)).Returns(masterTenantUser);
            this.mockUserAggregateRepository.Setup(e => e.GetById(masterTenantUser.TenantId, masterTenantUser.Id)).Returns(masterTenantUser);
            var userReadModel = this.CreateUserReadModels(tenantId);
            var user = this.CreateUserAggregate(tenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(userReadModel.TenantId, userReadModel.Id)).Returns(user);
            this.userSessionRepository.Setup(t => t.DeleteAllSessionsForUser(user.TenantId, user.Id));

            var person1 = new FakePersonalDetails
            {
                TenantId = tenantId,
                FullName = "Customer test One",
                FirstName = "Customer",
                MiddleNames = "test",
                LastName = "One",
                NamePrefix = "Dr",
                NameSuffix = "Jr",
                PreferredName = "customerOne",
                Email = email,
            };

            var tenant = TenantFactory.Create(tenantId);

            var personOne = PersonAggregate.CreatePersonFromPersonalDetails(
                tenantId, tenant.Details.DefaultOrganisationId, person1, this.performingUserId, this.clock.Now());

            this.mockPersonAggregateRepository.Setup(e => e.GetById(tenant.Id, user.PersonId)).Returns(personOne);

            List<UserReadModel> userReadModels = new List<UserReadModel>() { userReadModel };
            this.mockUserReadModelRepository.Setup(e => e.GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(email)).Returns(userReadModels);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<EnableOrDisableUsersCommand, List<UserModel>>>();

            //// Act
            var result = await sut.Handle(
                new EnableOrDisableUsersCommand(userReadModels.Select(c => new UserModel(c)), true), CancellationToken.None);

            //// Assert
            user.Blocked.Should().BeTrue();
        }

        [Fact]
        public async Task ToggleUserStatusCommand_ShouldDisableUser_WhenBlockedIsTrue()
        {
            ///// Arrange

            var email = "customer_one@email";
            var tenantId = Tenant.MasterTenantId;

            var masterTenantUser = this.CreateUserAggregate(Tenant.MasterTenantId, this.performingUserId);
            this.mockUserAggregateRepository
                .Setup(e => e.GetById(masterTenantUser.TenantId, masterTenantUser.Id))
                .Returns(masterTenantUser);
            this.mockUserAggregateRepository
                .Setup(e => e.GetById(masterTenantUser.TenantId, masterTenantUser.Id))
                .Returns(masterTenantUser);
            var userReadModel = this.CreateUserReadModels(tenantId);
            var user = this.CreateUserAggregate(tenantId, this.performingUserId);
            this.mockUserAggregateRepository
                .Setup(e => e.GetById(userReadModel.TenantId, userReadModel.Id))
                .Returns(user);
            this.userSessionRepository.Setup(t => t.DeleteAllSessionsForUser(user.TenantId, user.Id));

            var person1 = new FakePersonalDetails
            {
                TenantId = tenantId,
                FullName = "Customer test One",
                FirstName = "Customer",
                MiddleNames = "test",
                LastName = "One",
                NamePrefix = "Dr",
                NameSuffix = "Jr",
                PreferredName = "customerOne",
                Email = email,
            };

            var tenant = TenantFactory.Create(tenantId);

            var personOne = PersonAggregate.CreatePersonFromPersonalDetails(
                tenantId, tenant.Details.DefaultOrganisationId, person1, this.performingUserId, this.clock.Now());

            this.mockPersonAggregateRepository.Setup(e => e.GetById(tenant.Id, user.PersonId)).Returns(personOne);

            IEnumerable<UserReadModel> userReadModels = new List<UserReadModel>() { userReadModel };
            this.mockUserReadModelRepository.Setup(e => e.GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(email)).Returns(userReadModels);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<EnableOrDisableUsersCommand, List<UserModel>>>();

            //// Act
            var result = await sut.Handle(
                new EnableOrDisableUsersCommand(userReadModels.Select(c => new UserModel(c)), false), CancellationToken.None);

            //// Assert
            user.Blocked.Should().BeFalse();
        }

        private UserAggregate CreateUserAggregate(Guid performingTenantId, Guid performingUserId)
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var tenant = new Tenant(
                performingTenantId, performingTenantId.ToString(), performingTenantId.ToString(), null, default, default, timestamp);
            var person = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, performingUserId, timestamp);
            var user = UserAggregate.CreateUser(tenant.Id, performingUserId, UserType.Client, person, performingUserId, null, timestamp);
            return user;
        }

        private UserReadModel CreateUserReadModels(Guid tenantId)
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var tenant = new Tenant(tenantId, default, default, null, default, default, timestamp);
            var personAggregate = PersonAggregate.CreatePerson(
            tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, this.clock.Now());
            var personData = new PersonData(personAggregate);
            var userAggregate = UserAggregate.CreateUser(
                tenant.Id,
                Guid.NewGuid(),
                UserType.Client,
                personAggregate,
                this.performingUserId,
                null,
                SystemClock.Instance.GetCurrentInstant());

            var userReadModel = new UserReadModel(
            Guid.NewGuid(),
            personData,
            Guid.NewGuid(),
            null,
            SystemClock.Instance.GetCurrentInstant(),
            UserType.Client,
            userAggregate.Environment);
            return userReadModel;
        }
    }
}
