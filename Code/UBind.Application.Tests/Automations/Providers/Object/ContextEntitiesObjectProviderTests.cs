// <copyright file="ContextEntitiesObjectProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Services.Imports;
    using UBind.Application.SystemEvents.Payload;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Events;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class ContextEntitiesObjectProviderTests
    {
        private readonly IServiceProvider dependencyProvider = new Mock<IServiceProvider>().Object;

        [Theory]
        [InlineData(@"[""/quote"", ""/product"", ""/claimVersion""]")]
        [InlineData(@"{ ""contextEntities"": [ ""/quote"", ""/quoteVersion"", ""/customer"" ] }")]
        public void ContextEntitiesObjectProvider_ShouldBeCreated_WithListOfEntityTypes(string jsonString)
        {
            // Arrange
            // Act
            var model = JsonConvert.DeserializeObject<ContextEntitiesObjectProviderConfigModel>(
                jsonString,
                AutomationDeserializationConfiguration.ModelSettings);
            var provider = model.Build(this.dependencyProvider);

            // Assert
            model.ContextEntities.Should().HaveCount(3);
            provider.Should().NotBeNull();
        }

        [Fact]
        public async Task ContextEntitiesObjectProvider_ShouldCreate_AnObjectOutOfTheConfiguredEntityTypes()
        {
            // Arrange
            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            Mock<ITenantRepository> mockTenantRepository = new Mock<ITenantRepository>();
            Mock<IProductRepository> mockProductRepository = new Mock<IProductRepository>();
            Mock<IOrganisationReadModelRepository> mockOrganisationReadModelRepository = new Mock<IOrganisationReadModelRepository>();
            var tenant = new Tenant(Guid.NewGuid(), "foofoo", "alias", null, default, default, default);
            var organisationReadModel
                = new OrganisationReadModel(tenant.Id, Guid.NewGuid(), "org", "org", null, true, false, default);
            mockOrganisationReadModelRepository.Setup(s => s.GetOrganisationWithRelatedEntities(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new OrganisationReadModelWithRelatedEntities
                {
                    Organisation = organisationReadModel,
                });
            TenantWithRelatedEntities tenantWithRelatedEntities = new TenantWithRelatedEntities()
            {
                Tenant = tenant,
                Products = new List<Product>(),
                Details = default,
            };
            var product = new Product(tenant.Id, Guid.NewGuid(), "foofoofoo", "alias", default);
            ProductWithRelatedEntities productWithRelatedEntities = new ProductWithRelatedEntities()
            {
                Product = product,
                Details = product.Details,
                Tenant = tenant,
                TenantDetails = tenant.Details,
            };

            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            var mockMediator = new Mock<ICqrsMediator>();
            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var internalUrlConfiguration = new InternalUrlConfiguration() { BaseApi = "https://localhost:44366" };
            mockServiceProvider.Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    mockcachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));
            mockTenantRepository.Setup(x =>
                x.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>()))
                .Returns(tenantWithRelatedEntities);
            mockTenantRepository.Setup(x =>
                x.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            mockProductRepository.Setup(x =>
                x.GetProductWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<string>>()))
                .Returns(productWithRelatedEntities);
            mockProductRepository.Setup(x =>
                x.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false))
                .Returns(product);
            mockServiceProvider.Setup(x => x.GetService(typeof(ITenantRepository)))
                .Returns(mockTenantRepository.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IProductRepository)))
                .Returns(mockProductRepository.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(ICachingResolver)))
              .Returns(mockcachingResolver.Object);
            mockServiceProvider.Setup(x => x.GetService(typeof(IInternalUrlConfiguration)))
                .Returns(internalUrlConfiguration);
            mockServiceProvider.Setup(x => x.GetService(typeof(IClock)))
                .Returns(SystemClock.Instance);
            mockServiceProvider.Setup(x => x.GetService(typeof(IOrganisationReadModelRepository)))
                .Returns(mockOrganisationReadModelRepository.Object);
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var systemEvent = SystemEvent.CreateWithPayload(
                tenant.Id,
                organisationReadModel.Id,
                product.Id,
                DeploymentEnvironment.Development,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                default);
            var automationData =
                AutomationData.CreateFromSystemEvent(systemEvent, null, mockServiceProvider.Object);

            string jsonString = @"[""/tenant"", ""/organisation"", ""/product/tenant"", ""/quote""]";
            var model = JsonConvert.DeserializeObject<ContextEntitiesObjectProviderConfigModel>(
                jsonString, AutomationDeserializationConfiguration.ModelSettings);
            var provider = model.Build(this.dependencyProvider);

            // Act
            var data = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            DataObjectHelper.ContainsProperty(data, "tenant").Should().BeTrue();
            DataObjectHelper.ContainsProperty(data, "quote").Should().BeFalse();
            DataObjectHelper.ContainsProperty(data, "product").Should().BeTrue();
            DataObjectHelper.GetPropertyValue(data, "tenant").Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(data, "organisation").Should().NotBeNull();

            var tenantData = DataObjectHelper.GetPropertyValue(data.DataValue, "tenant");
            DataObjectHelper.GetPropertyValue(tenantData, "id").Should().Be(systemEvent.TenantId);

            var productData = DataObjectHelper.GetPropertyValue(data.DataValue, "product");
            var referenceTenantData = DataObjectHelper.GetPropertyValue(productData, "tenant");
            referenceTenantData.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(referenceTenantData, "id").Should().Be(systemEvent.TenantId);
        }
    }
}
