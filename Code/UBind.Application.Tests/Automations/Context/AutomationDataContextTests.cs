// <copyright file="AutomationDataContextTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation.Data;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.SystemEvents.Payload;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using ProviderEntity = UBind.Domain.SerialisedEntitySchemaObject;

    public class AutomationDataContextTests
    {
        private ProductContext productContext = new ProductContext(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DeploymentEnvironment.Development);

        [Theory]
        [InlineData("/context/tenant/id")]
        [InlineData("context.tenant.id")]
        [SystemEventTypeExtensionInitialize]
        public async Task LazyLoad_DoNotLoadEntity_IfTryingToAccessIdOnlyAsync(string path)
        {
            // Arrange
            var serviceProvider = MockAutomationData.GetServiceProviderForEntityProviders(TenantFactory.DefaultId, false);
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                this.productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                default);
            var automationData = AutomationData.CreateFromSystemEvent(systemEvent, null, serviceProvider);

            // Act
            await automationData.GetValue(path);

            // Assert
            var tenantEntity = (ProviderEntity.Tenant)automationData.Context[EntityType.Tenant.ToCamelCaseString()];
            tenantEntity.IsLoaded.Should().BeFalse();
        }

        [Theory]
        [InlineData("/context/tenant/name")]
        [InlineData("context.tenant.name")]
        [SystemEventTypeExtensionInitialize]
        public async Task LazyLoad_SuccessfulLoadedEntity_IfTryingToAccessUnloadedDataAsync(string path)
        {
            // Arrange
            var serviceProvider = MockAutomationData.GetServiceProviderForEntityProviders(TenantFactory.DefaultId, false);
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                this.productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                default);
            var automationData = AutomationData.CreateFromSystemEvent(systemEvent, null, serviceProvider);

            // Act
            await automationData.GetValue(path);

            // Assert
            var tenantEntity = (ProviderEntity.Tenant)automationData.Context[EntityType.Tenant.ToCamelCaseString()];
            tenantEntity.IsLoaded.Should().BeTrue();
        }
    }
}
