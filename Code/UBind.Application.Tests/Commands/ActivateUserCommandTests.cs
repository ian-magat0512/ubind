// <copyright file="ActivateUserCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.User;
    using UBind.Application.Tests.Helpers;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ActivateUserCommandTests
    {
        private readonly ServiceCollection serviceCollection;
        private readonly Guid performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;
        private readonly Mock<IUserAggregateRepository> mockUserAggregateRepository = new Mock<IUserAggregateRepository>();
        private readonly Mock<IPasswordHashingService> passwordHashingService = new Mock<IPasswordHashingService>();
        private Guid userId;
        private Guid tenantId;

        public ActivateUserCommandTests()
        {
            this.tenantId = Guid.NewGuid();
            this.userId = Guid.NewGuid();
            var passwordComplexityValidator = PasswordComplexityValidator.Default;
            var services = UserHelper.GetServiceCollectionWithUserService(this.tenantId, this.userId);
            services.AddSingleton<IClock>(SystemClock.Instance);
            services
                .AddSingleton<ICommandHandler<ActivateUserCommand, UserReadModel>,
                    ActivateUserCommandHandler>();

            services.AddSingleton(this.mockUserAggregateRepository.Object);
            services.AddSingleton(passwordComplexityValidator);
            services.AddSingleton(this.passwordHashingService.Object);
            this.serviceCollection = services;
        }

        [Fact]
        public async Task ActivateUserCommand_ActivateUser_WhenPasswordIsStrong()
        {
            // Arrange
            var email = "customer_one@email";
            var strongPassword = "ubindTest123*";

            var masterTenantUser = this.CreateUserAggregate(this.tenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(this.tenantId, masterTenantUser.Id)).Returns(masterTenantUser);

            var userReadModel = this.CreateUserReadModels(this.tenantId);
            var user = this.CreateUserAggregate(this.tenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(this.tenantId, userReadModel.Id)).Returns(user);

            var person1 = new FakePersonalDetails
            {
                TenantId = this.tenantId,
                FullName = "Customer test One",
                FirstName = "Customer",
                MiddleNames = "test",
                LastName = "One",
                NamePrefix = "Dr",
                NameSuffix = "Jr",
                PreferredName = "customerOne",
                Email = email,
            };

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<ActivateUserCommand, UserReadModel>>();

            // Act
            var result = await sut.Handle(
                new ActivateUserCommand(this.tenantId, user.Id, this.performingUserId, strongPassword),
                CancellationToken.None);

            // Assert
            result.HasBeenActivated.Should().BeTrue();
        }

        [Fact]
        public async Task ActivateUserCommand_ThrowException_WhenPasswordIsWeakAsync()
        {
            // Arrange
            var email = "customer_one@email";
            var weakPassword = "weakPassword";

            var masterTenantUser = this.CreateUserAggregate(Tenant.MasterTenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(this.tenantId, masterTenantUser.Id)).Returns(masterTenantUser);
            this.mockUserAggregateRepository.Setup(e => e.GetById(this.tenantId, masterTenantUser.Id)).Returns(masterTenantUser);

            var userReadModel = this.CreateUserReadModels(this.tenantId);
            var user = this.CreateUserAggregate(this.tenantId, this.performingUserId);
            this.mockUserAggregateRepository.Setup(e => e.GetById(this.tenantId, userReadModel.Id)).Returns(user);
            var person1 = new FakePersonalDetails
            {
                TenantId = this.tenantId,
                FullName = "Customer test One",
                FirstName = "Customer",
                MiddleNames = "test",
                LastName = "One",
                NamePrefix = "Dr",
                NameSuffix = "Jr",
                PreferredName = "customerOne",
                Email = email,
            };

            var tenant = TenantFactory.Create(this.tenantId);

            var personOne = PersonAggregate.CreatePersonFromPersonalDetails(
                this.tenantId, tenant.Details.DefaultOrganisationId, person1, this.performingUserId, this.clock.Now());

            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<ICommandHandler<ActivateUserCommand, UserReadModel>>();

            // Act
            Func<Task> act = async () => await sut.Handle(
                new ActivateUserCommand(tenant.Id, user.Id, this.performingUserId, weakPassword),
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ErrorException>();
        }

        private UserAggregate CreateUserAggregate(Guid performingTenantId, Guid performingUserId)
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
