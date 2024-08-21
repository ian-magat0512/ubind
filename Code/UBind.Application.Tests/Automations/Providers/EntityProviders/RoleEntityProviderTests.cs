// <copyright file="RoleEntityProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.EntityProviders
{
    using System;
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
    using UBind.Domain.Entities;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class RoleEntityProviderTests
    {
        public RoleEntityProviderTests()
        {
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);
        }

        [Fact]
        public async Task RoleEntityProvider_Should_Return_RoleEntity_When_Pass_With_RoleId()
        {
            // Arrange
            var role = RoleHelper.CreateTenantAdminRole(TenantFactory.DefaultId, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());
            var roleId = role.Id;
            var json = @"{ 
                              ""roleId"" : """ + roleId.ToString() + @"""
                         }";

            var roleEntityProviderBuilder = JsonConvert.DeserializeObject<RoleEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockroleRepository = new Mock<IRoleRepository>();

            mockroleRepository.Setup(c => c.GetRoleById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(role);

            var tenant = TenantFactory.Create(TenantFactory.DefaultId);
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IRoleRepository))).Returns(mockroleRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var roleEntityProvider = roleEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var entityObject = (await roleEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Role));

            var roleEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Role;
            roleEntity.Id.Should().Be(roleId.ToString());
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMediator = new Mock<ICqrsMediator>();
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
