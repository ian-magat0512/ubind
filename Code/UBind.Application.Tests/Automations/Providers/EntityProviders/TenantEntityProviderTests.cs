// <copyright file="TenantEntityProviderTests.cs" company="uBind">
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
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class TenantEntityProviderTests
    {
        [Fact]
        public async Task TenantEntityProvider_Should_Return_TenantEntity_When_Pass_With_TenantId()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var json = @"{ 
                              ""tenantId"" : """ + tenantId + @"""
                         }";

            var tenantEntityProviderBuilder = JsonConvert.DeserializeObject<TenantEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();

            var tenant = new Tenant(tenantId, "test", "test", null, default, default, default);
            var model = new TenantWithRelatedEntities() { Tenant = tenant };
            mockTenantRepository.Setup(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(model);
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ITenantRepository))).Returns(mockTenantRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var tenantEntityProvider = tenantEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await tenantEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Tenant));

            var tenantEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Tenant;
            tenantEntity.Id.Should().Be(tenantId.ToString());
        }

        [Fact]
        public async Task TenantEntityProvider_Should_Throw_When_TenantId_DoesNot_Exists()
        {
            // Arrange
            var tenantId = "invalid_tenant";
            var json = @"{ 
                              ""tenantId"" : """ + tenantId + @"""
                         }";

            var tenantEntityProviderBuilder = JsonConvert.DeserializeObject<TenantEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();

            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(default(Tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(default(Tenant)));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ITenantRepository))).Returns(mockTenantRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var tenantEntityProvider = tenantEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await tenantEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Tenant");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity Tenant ID: {tenantId}");
        }

        [Fact]
        public async Task TenantEntityProvider_Should_Return_TenantEntity_When_Pass_With_TenantAlias()
        {
            // Arrange
            var tenantAlias = "valid_tenantAlias";
            var json = @"{ 
                              ""tenantAlias"" : """ + tenantAlias + @"""
                         }";

            var tenantEntityProviderBuilder = JsonConvert.DeserializeObject<TenantEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();

            var tenant = new Tenant(Guid.NewGuid(), tenantAlias, tenantAlias, null, default, default, SystemClock.Instance.GetCurrentInstant());
            var model = new TenantWithRelatedEntities() { Tenant = tenant };
            mockTenantRepository.Setup(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(model);
            mockTenantRepository.Setup(c => c.GetTenantByAlias(It.IsAny<string>(), It.IsAny<bool>())).Returns(tenant);
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantByAliasOrThrow(It.IsAny<string>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantByAliasOrNull(It.IsAny<string>())).Returns(Task.FromResult(tenant));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ITenantRepository))).Returns(mockTenantRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var tenantEntityProvider = tenantEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await tenantEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Tenant));

            var tenantEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Tenant;
            tenantEntity.Alias.Should().Be(tenantAlias);
        }

        [Fact]
        public async Task TenantEntityProvider_Should_Throw_When_TenantAlias_DoesNot_Exists()
        {
            // Arrange
            var tenantAlias = "invalid_tenant_alias";
            var json = @"{ 
                              ""tenantId"" : """ + tenantAlias + @"""
                         }";

            var tenantEntityProviderBuilder = JsonConvert.DeserializeObject<TenantEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();

            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(default(Tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(default(Tenant)));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ITenantRepository))).Returns(mockTenantRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var tenantEntityProvider = tenantEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await tenantEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Tenant");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity Tenant ID: {tenantAlias}");
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
