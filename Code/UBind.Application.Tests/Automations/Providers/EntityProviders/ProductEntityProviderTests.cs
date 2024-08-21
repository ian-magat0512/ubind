// <copyright file="ProductEntityProviderTests.cs" company="uBind">
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
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class ProductEntityProviderTests
    {
        [Fact]
        public async Task ProductEntityProvider_Should_Return_ProductEntity_When_Pass_With_ProductId()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var json = @"{ 
                              ""productId"" : """ + productId + @"""
                         }";

            var productEntityProviderBuilder = JsonConvert.DeserializeObject<ProductEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockProductRepository = new Mock<IProductRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();

            var product = new Product(TenantFactory.DefaultId, productId, "product_alias", "product_alias", NodaTime.SystemClock.Instance.GetCurrentInstant());
            var model = new ProductWithRelatedEntities() { Product = product };
            mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(product.TenantId, product.Id, It.IsAny<List<string>>())).Returns(model);
            mockProductRepository.Setup(c => c.GetProductById(product.TenantId, product.Id, false)).Returns(product);
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(IProductRepository))).Returns(mockProductRepository.Object);
            var productEntityProvider = productEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await productEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Product));

            var productEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Product;
            productEntity.Id.Should().Be(productId.ToString());
        }

        [Fact]
        public async Task ProductEntityProvider_Should_Throw_When_ProductId_DoesNot_Exists()
        {
            // Arrange
            var productId = "invalid_product_id";
            var json = @"{ 
                              ""productId"" : """ + productId + @"""
                         }";

            var productEntityProviderBuilder = JsonConvert.DeserializeObject<ProductEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockProductRepository = new Mock<IProductRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(default(IProductWithRelatedEntities));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(default(Product)));
            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IProductRepository))).Returns(mockProductRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var productEntityProvider = productEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await productEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Product");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity Product ID: {productId}");
        }

        [Fact]
        public async Task ProductEntityProvider_Should_Return_ProductEntity_When_Pass_With_ProductAlias()
        {
            // Arrange
            var productAlias = "valid_product_Alias";
            var json = @"{ 
                              ""productAlias"" : """ + productAlias + @"""
                         }";

            var productEntityProviderBuilder = JsonConvert.DeserializeObject<ProductEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockProductRepository = new Mock<IProductRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();

            var productId = Guid.NewGuid();
            var tenant = TenantFactory.Create(TenantFactory.DefaultId);
            var product = new Product(TenantFactory.DefaultId, productId, productAlias, productAlias, NodaTime.SystemClock.Instance.GetCurrentInstant());
            var model = new ProductWithRelatedEntities() { Product = product };
            mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(product.TenantId, product.Id, It.IsAny<List<string>>())).Returns(model);
            mockProductRepository.Setup(c => c.GetProductByAlias(product.TenantId, productAlias)).Returns(product);
            mockProductRepository.Setup(c => c.GetProductById(product.TenantId, product.Id, false)).Returns(product);
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrNull(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));

            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IProductRepository))).Returns(mockProductRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var productEntityProvider = productEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await productEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Product));

            var productEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Product;
            productEntity.Id.Should().Be(productId.ToString());
        }

        [Fact]
        public async Task ProductEntityProvider_Should_Throw_When_ProductAlias_DoesNot_Exists()
        {
            // Arrange
            var productAlias = "invalid_product_alias";
            var json = @"{ 
                              ""productAlias"" : """ + productAlias + @"""
                         }";

            var productEntityProviderBuilder = JsonConvert.DeserializeObject<ProductEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockProductRepository = new Mock<IProductRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();
            var tenant = TenantFactory.Create(TenantFactory.DefaultId);
            mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(default(IProductWithRelatedEntities));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(default(Product)));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(default(Product)));
            var mockServiceProvider = this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IProductRepository))).Returns(mockProductRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var productEntityProvider = productEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await productEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Product");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity Product Alias: {productAlias}");
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
