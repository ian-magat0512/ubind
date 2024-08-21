// <copyright file="GetUsersByEmailQueryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NodaTime;
    using UBind.Application.Queries.User;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using Xunit;

    public class GetUsersByEmailQueryTests
    {
        private readonly ServiceCollection serviceCollection;
        private readonly Guid performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;
        private readonly Mock<IUserAggregateRepository> mockUserAggregateRepository = new Mock<IUserAggregateRepository>();
        private readonly Mock<IUserReadModelRepository> mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
        private readonly Mock<IHttpContextPropertiesResolver> mockHttpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();

        public GetUsersByEmailQueryTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IClock>(SystemClock.Instance);
            services
                .AddSingleton<IQueryHandler<GetUsersMatchingEmailAddressIncludingPlusAddressingQuery, IEnumerable<UserModel>>,
                    GetUsersMatchingEmailAddressIncludingPlusAddressingQueryHandler>();

            services.AddSingleton(this.mockUserAggregateRepository.Object);
            services.AddSingleton(this.mockUserReadModelRepository.Object);
            services.AddSingleton(this.mockHttpContextPropertiesResolver.Object);
            this.serviceCollection = services;
        }

        [Fact]
        public async Task GetUserByEmailCommand_ShouldReturnUserAccrossTenancy_WhenTenantIdIsNull()
        {
            // Arrange
            var email = "user@domain.com";
            var tenantId = Guid.NewGuid();
            var otherTenantId = Guid.NewGuid();

            var masterTenantUser = this.CreateUser(Tenant.MasterTenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(masterTenantUser.TenantId, masterTenantUser.Id)).Returns(masterTenantUser);
            this.mockUserAggregateRepository.Setup(e => e.GetById(masterTenantUser.TenantId, masterTenantUser.Id)).Returns(masterTenantUser);

            var userReadModel = this.CreateUserReadModels(tenantId);
            var user = this.CreateUser(tenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(userReadModel.TenantId, userReadModel.Id)).Returns(user);

            var otherUserReadModel = this.CreateUserReadModels(otherTenantId);
            var otherUser = this.CreateUser(otherTenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(otherUserReadModel.TenantId, otherUserReadModel.Id)).Returns(otherUser);

            IEnumerable<UserReadModel> userReadModels = new List<UserReadModel>() { userReadModel, otherUserReadModel };
            this.mockUserReadModelRepository.Setup(e => e.GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(email)).Returns(userReadModels);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetUsersMatchingEmailAddressIncludingPlusAddressingQuery, IEnumerable<UserModel>>>();

            // Act
            var result = await sut.Handle(
                new GetUsersMatchingEmailAddressIncludingPlusAddressingQuery(
                    null,
                    null,
                    email,
                    false,
                    this.performingUserId), CancellationToken.None);

            // Assert
            this.mockUserReadModelRepository.Verify(e => e.GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(email));
            result.Count().Should().Be(userReadModels.Count());
        }

        [Fact]
        public async Task GetUserByEmailCommand_ShouldReturnUserBelongsToTenant_WhenTenantIdIsSpecified()
        {
            //// Arrange

            var email = "user@domain.com";
            var tenantId = Guid.NewGuid();
            var otherTenantId = Guid.NewGuid();

            var masterTenantUser = this.CreateUser(Tenant.MasterTenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(masterTenantUser.TenantId, masterTenantUser.Id)).Returns(masterTenantUser);
            this.mockUserAggregateRepository.Setup(e => e.GetById(masterTenantUser.TenantId, masterTenantUser.Id)).Returns(masterTenantUser);

            var userReadModel = this.CreateUserReadModels(tenantId);
            var user = this.CreateUser(tenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(userReadModel.TenantId, this.performingUserId)).Returns(user);
            this.mockUserAggregateRepository.Setup(e => e.GetById(userReadModel.TenantId, userReadModel.Id)).Returns(user);

            var otherUserReadModel = this.CreateUserReadModels(otherTenantId);
            var otherUser = this.CreateUser(otherTenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(otherUserReadModel.TenantId, this.performingUserId)).Returns(otherUser);
            this.mockUserAggregateRepository.Setup(e => e.GetById(otherUserReadModel.TenantId, otherUserReadModel.Id)).Returns(otherUser);
            IEnumerable<UserReadModel> userReadModels = new List<UserReadModel>() { userReadModel };

            this.mockUserReadModelRepository.Setup(e => e.GetUsersMatchingEmailAddressIncludingPlusAddressing(tenantId, email)).Returns(userReadModels);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetUsersMatchingEmailAddressIncludingPlusAddressingQuery, IEnumerable<UserModel>>>();

            //// Act
            var result = await sut.Handle(
                                       new GetUsersMatchingEmailAddressIncludingPlusAddressingQuery(
                                           tenantId,
                                           null,
                                           email,
                                           false,
                                           this.performingUserId), CancellationToken.None);

            //// Assert
            this.mockUserReadModelRepository.Verify(e => e.GetUsersMatchingEmailAddressIncludingPlusAddressing(tenantId, email));
            result.Count().Should().Be(userReadModels.Count());
        }

        [Fact]
        public async Task GetUserByEmailCommand_ShouldReturnUserBelongsToTenant_WhenUserIsNotMasterTenant()
        {
            //// Arrange

            var email = "user@domain.com";
            var tenantId = Guid.NewGuid();
            var performingUserTenant = Guid.NewGuid();

            var performingUser = this.CreateUser(performingUserTenant, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(performingUser.TenantId, performingUser.Id)).Returns(performingUser);
            this.mockUserAggregateRepository.Setup(e => e.GetById(performingUser.TenantId, performingUser.Id)).Returns(performingUser);

            var userReadModel = this.CreateUserReadModels(tenantId);
            var user = this.CreateUser(tenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(userReadModel.TenantId, userReadModel.Id)).Returns(user);
            IEnumerable<UserReadModel> userReadModels = new List<UserReadModel>() { userReadModel };
            this.mockUserReadModelRepository.Setup(e => e.GetUsersMatchingEmailAddressIncludingPlusAddressing(performingUserTenant, email)).Returns(userReadModels);

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryHandler<GetUsersMatchingEmailAddressIncludingPlusAddressingQuery, IEnumerable<UserModel>>>();

            //// Act
            var result = await sut.Handle(
                                       new GetUsersMatchingEmailAddressIncludingPlusAddressingQuery(
                                           performingUserTenant,
                                           null,
                                           email,
                                           false,
                                           this.performingUserId), CancellationToken.None);

            //// Assert
            this.mockUserReadModelRepository.Verify(e => e.GetUsersMatchingEmailAddressIncludingPlusAddressing(performingUserTenant, email));
            result.Count().Should().Be(userReadModels.Count());
        }

        private UserAggregate CreateUser(Guid performingUserId, Guid performingTenantId)
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var tenant = new Tenant(performingTenantId, performingTenantId.ToString(), performingTenantId.ToString(), null, default, default, timestamp);
            var person = PersonAggregate.CreatePerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, performingUserId, timestamp);
            var user = UserAggregate.CreateUser(tenant.Id, performingUserId, UserType.Client, person, performingUserId, null, timestamp);
            return user;
        }

        private UserReadModel CreateUserReadModels(Guid tenantId)
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant();
            var tenant = new Tenant(tenantId, tenantId.ToString(), tenantId.ToString(), null, default, default, timestamp);
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
