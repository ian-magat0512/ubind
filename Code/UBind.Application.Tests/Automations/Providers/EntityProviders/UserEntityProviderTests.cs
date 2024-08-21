// <copyright file="UserEntityProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.EntityProviders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class UserEntityProviderTests
    {
        [Fact]
        public async Task UserEntityProvider_Should_Return_UserEntity_When_Pass_With_UserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = @"{ 
                              ""userId"" : """ + userId.ToString() + @"""
                         }";

            var userEntityProviderBuilder = JsonConvert.DeserializeObject<UserEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();

            var model = new UserReadModelWithRelatedEntities() { User = new FakeUserReadModel(userId) };
            mockUserReadModelRepository
                .Setup(c => c.GetUserWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IUserReadModelRepository))).Returns(mockUserReadModelRepository.Object);
            var userEntityProvider = userEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await userEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.User));

            var userEntity = entityObject.DataValue as SerialisedEntitySchemaObject.User;
            userEntity.Id.Should().Be(userId.ToString());
        }

        [Fact]
        public async Task UserEntityProvider_Should_Throw_When_UserId_DoesNot_Exists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = @"{ 
                              ""userId"" : """ + userId.ToString() + @"""
                         }";

            var userEntityProviderBuilder = JsonConvert.DeserializeObject<UserEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();

            mockUserReadModelRepository
                .Setup(c => c.GetUserWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(IUserReadModelWithRelatedEntities));

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IUserReadModelRepository))).Returns(mockUserReadModelRepository.Object);
            var userEntityProvider = userEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await userEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: User");
            exception.Which.Error.AdditionalDetails.Should().Contain($"User ID: {userId}");
        }

        [Fact]
        public async Task UserEntityProvider_Should_Return_UserEntity_When_Pass_With_EmailAddress()
        {
            // Arrange
            var email = "xxxx+12@email.com";
            var expectedUserId = Guid.NewGuid();

            var json = @"{ 
                              ""userAccountEmail"" : """ + email + @"""
                         }";

            var userEntityProviderBuilder = JsonConvert.DeserializeObject<UserEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();

            var model = new UserReadModelWithRelatedEntities() { User = new FakeUserReadModel(expectedUserId) };
            mockUserReadModelRepository
                .Setup(c => c.GetUserWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IUserReadModelRepository))).Returns(mockUserReadModelRepository.Object);
            var userEntityProvider = userEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await userEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.User));

            var userEntity = entityObject.DataValue as SerialisedEntitySchemaObject.User;
            userEntity.Id.Should().Be(expectedUserId.ToString());
        }

        [Fact]
        public async Task UserEntityProvider_Should_Throw_When_EmailAddress_DoesNot_Exists()
        {
            // Arrange
            var email = "xxxx+12@email.com";
            var json = @"{ 
                              ""userAccountEmail"" : """ + email + @"""
                         }";

            var userEntityProviderBuilder = JsonConvert.DeserializeObject<UserEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();

            mockUserReadModelRepository
                .Setup(c => c.GetUserWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(default(IUserReadModelWithRelatedEntities));

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IUserReadModelRepository))).Returns(mockUserReadModelRepository.Object);
            var userEntityProvider = userEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await userEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: User");
            exception.Which.Error.AdditionalDetails.Should().Contain($"User Account Email: {email}");
        }

        private async Task<Mock<IServiceProvider>> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var cachingResolver = new Mock<ICachingResolver>();
            cachingResolver.Setup(x =>
            x.GetTenantOrThrow(
            automationData.ContextManager.Tenant.Id))
                .Returns(Task.FromResult(TenantFactory.Create(automationData.ContextManager.Tenant.Id)));

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMediator = new Mock<ICqrsMediator>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    cachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));

            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver)))
                .Returns(cachingResolver.Object);

            return mockServiceProvider;
        }
    }
}
