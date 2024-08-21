// <copyright file="UserHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Transactions;
using UBind.Application.Commands.User;
using UBind.Application.Services;
using UBind.Application.Services.Email;
using UBind.Application.Tests.Fakes;
using UBind.Application.User;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Authentication;
using UBind.Domain.Entities;
using UBind.Domain.Enums;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Organisation;
using UBind.Domain.ReadModel.User;
using UBind.Domain.Repositories;
using UBind.Domain.Services;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Domain.Tests.Fakes;
using UBind.Persistence;

public class UserHelper
{
    public static ServiceCollection GetServiceCollectionWithUserService(Guid tenantId, Guid userId, Guid? organisationId = null)
    {
        var services = new ServiceCollection();
        var cachingResolverMock = new Mock<ICachingResolver>();
        var mockTenantRepository = new Mock<ITenantRepository>();
        var mockPersonRepository = new Mock<IPersonAggregateRepository>();
        mockPersonRepository.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(PersonAggregate.CreatePerson(tenantId, Guid.NewGuid(), Guid.NewGuid(), Instant.MaxValue));
        var personRepository = mockPersonRepository.Object;
        cachingResolverMock.Setup(x => x.GetTenantOrThrow(tenantId)).ReturnsAsync(new Tenant(tenantId));
        var tenantRepository = mockTenantRepository.Object;
        var mockUbindDbContext = new Mock<IUBindDbContext>();
        mockUbindDbContext.SetupProperty(p => p.TransactionStack, new Stack<TransactionScope>());
        mockUbindDbContext.Setup(s => s.GetDbSet<EventRecordWithGuidId>())
                       .Returns(new Mock<IDbSet<EventRecordWithGuidId>>().Object);
        mockUbindDbContext.Setup(s => s.GetContextAggregates<UserAggregate>())
                       .Returns(new HashSet<UserAggregate>());
        var mockOrganisationReadModelRepository = new Mock<IOrganisationReadModelRepository>();
        if (organisationId != null)
        {
            var organisation = new OrganisationReadModel(
                    tenantId,
                    (Guid)organisationId,
                    "sampleAlias",
                    "sampleName",
                    null,
                    true,
                    false,
                    new TestClock(true).Timestamp);
            mockOrganisationReadModelRepository.Setup(x => x.Get(tenantId, (Guid)organisationId))
                .Returns(organisation);
        }
        var clock = new TestClock(true);
        var userReadModelUpdateRepositoryFake = new FakeWritableReadModelRepository<UserReadModel>();
        var userLoginEmailUpdateRepositoryFake = new FakeWritableReadModelRepository<UserLoginEmail>();
        var propertyTypeEvaluatorService = new PropertyTypeEvaluatorService(
            Mock.Of<IReadOnlyDictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>>());
        var userReadModelWriter = new UserReadModelWriter(
            userReadModelUpdateRepositoryFake,
            userLoginEmailUpdateRepositoryFake,
            Mock.Of<IUserLoginEmailRepository>(),
            Mock.Of<IRoleRepository>(),
            propertyTypeEvaluatorService);
        var userAggregateRepository = new UserAggregateRepository(
            mockUbindDbContext.Object,
            new Mock<IEventRecordRepository>().Object,
            userReadModelWriter,
            new Mock<IAggregateSnapshotService<UserAggregate>>().Object,
            clock,
            Mock.Of<ILogger<UserAggregateRepository>>(),
            new Mock<IServiceProvider>().AddLoggers().Object);
        var userService = new User.UserService(
            userAggregateRepository,
            new Mock<ICustomerAggregateRepository>().Object,
            personRepository,
            new Mock<IUserReadModelRepository>().Object,
            new Mock<IRoleRepository>().Object,
            new Mock<IUserProfilePictureRepository>().Object,
            mockOrganisationReadModelRepository.Object,
            new Mock<IUserLoginEmailRepository>().Object,
            new Mock<IPasswordHashingService>().Object,
            new Mock<ICustomerService>().Object,
            new Mock<IHttpContextPropertiesResolver>().Object,
            new Mock<IQuoteAggregateResolverService>().Object,
            new Mock<IUserActivationInvitationService>().Object,
            new Mock<ICqrsMediator>().Object,
            new Mock<IAdditionalPropertyValueService>().Object,
            new Mock<IPasswordComplexityValidator>().Object,
            new Mock<IClock>().Object,
            new Mock<IUBindDbContext>().Object,
            cachingResolverMock.Object,
            new Mock<IAuthenticationMethodReadModelRepository>().Object,
            new Mock<IUserSessionDeletionService>().Object,
            Mock.Of<IUserSystemEventEmitter>());
        services.AddTransient<User.IUserService>(c => userService);
        services.AddTransient<ITenantRepository>(c => tenantRepository);
        services.AddTransient<IErrorNotificationService>(c => new Mock<IErrorNotificationService>().Object);
        services.AddSingleton<ICqrsMediator, CqrsMediator>();
        services.AddSingleton<ICqrsRequestContext>(_ => new CqrsRequestContext());
        services.AddTransient<IUBindDbContext>(_ => mockUbindDbContext.Object);
        services.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));
        return services;
    }
}
