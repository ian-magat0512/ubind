// <copyright file="PortalEntityProviderTests.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class PortalEntityProviderTests
    {
        [Fact]
        public async Task PortalEntityProvider_Should_Return_PortalEntity_When_Pass_With_PortalId()
        {
            // Arrange
            var portalId = Guid.NewGuid();
            var json = @"{ 
                              ""portalId"" : """ + portalId + @"""
                         }";

            var portalEntityProviderBuilder = JsonConvert.DeserializeObject<PortalEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPortalRepository = new Mock<IPortalReadModelRepository>();

            var tenant = TenantFactory.Create();
            var portal = new PortalReadModel
            {
                TenantId = tenant.Id,
                Id = Guid.NewGuid(),
                Name = "My Portal",
                Alias = "myPortal",
                Title = "My Portal",
                StyleSheetUrl = null,
                Disabled = false,
                Deleted = false,
            };
            var portalModel = new PortalWithRelatedEntities() { Portal = portal, Tenant = tenant };
            mockPortalRepository
                .Setup(c => c.GetPortalWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(portalModel);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IPortalReadModelRepository))).Returns(mockPortalRepository.Object);
            var portalEntityProvider = portalEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await portalEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Portal));

            var portalEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Portal;
            portalEntity.Name.Should().Be("My Portal");
        }

        [Fact]
        public async Task PortalEntityProvider_Should_Throw_When_PortalId_DoesNot_Exists()
        {
            // Arrange
            var portalId = Guid.NewGuid();
            var json = @"{ 
                              ""portalId"" : """ + portalId + @"""
                         }";

            var portalEntityProviderBuilder = JsonConvert.DeserializeObject<PortalEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPortalRepository = new Mock<IPortalReadModelRepository>();

            mockPortalRepository
                .Setup(c => c.GetPortalWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(PortalWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IPortalReadModelRepository))).Returns(mockPortalRepository.Object);
            var portalEntityProvider = portalEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await portalEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Portal");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Portal ID: {portalId}");
        }

        [Fact]
        public async Task PortalEntityProvider_Should_Return_ProductEntity_When_Pass_With_PortalAlias()
        {
            // Arrange
            var portalAlias = "valid_portal_Alias";
            var json = @"{ 
                              ""portalAlias"" : """ + portalAlias + @"""
                         }";

            var portalEntityProviderBuilder = JsonConvert.DeserializeObject<PortalEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPortalRepository = new Mock<IPortalReadModelRepository>();
            var tenant = TenantFactory.Create();
            var portal = new PortalReadModel
            {
                TenantId = tenant.Id,
                Id = Guid.NewGuid(),
                Name = "My Portal",
                Alias = portalAlias,
                Title = "My Portal",
                StyleSheetUrl = null,
                Disabled = false,
                Deleted = false,
            };
            var portalModel = new PortalWithRelatedEntities() { Portal = portal, Tenant = tenant };
            mockPortalRepository
                .Setup(c => c.GetPortalWithRelatedEntitiesByAlias(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(portalModel);

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IPortalReadModelRepository)))
                .Returns(mockPortalRepository.Object);
            var portalEntityProvider = portalEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var entityObject = (await portalEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Portal));

            var portalEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Portal;
            portalEntity.Alias.Should().Be(portalAlias);
        }

        [Fact]
        public async Task PortalEntityProvider_Should_Throw_When_PortalAlias_DoesNot_Exists()
        {
            // Arrange
            var portalAlias = "invalid_portal_alias";
            var json = @"{ 
                              ""portalAlias"" : """ + portalAlias + @"""
                         }";

            var portalEntityProviderBuilder = JsonConvert.DeserializeObject<PortalEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPortalRepository = new Mock<IPortalReadModelRepository>();

            mockPortalRepository
                .Setup(c => c.GetPortalWithRelatedEntitiesByAlias(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(default(PortalWithRelatedEntities));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IPortalReadModelRepository)))
                .Returns(mockPortalRepository.Object);
            var portalEntityProvider = portalEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await portalEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Portal");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Portal Alias: {portalAlias}");
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockMediator = new Mock<ICqrsMediator>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockCachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));

            return mockServiceProvider;
        }
    }
}
