// <copyright file="EntityObjectProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class EntityObjectProviderTests
    {
        private IClock clock = new SequentialClock();

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_QuoteEntityProvider()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var json = @"{
                            ""quote"": """ + quoteId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(quoteId) };
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IQuoteReadModelRepository)))
                .Returns(mockQuoteRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").ToString().Should().Be(quoteId.ToString());
        }

        [Theory]
        [InlineData("calculation")]
        [InlineData("calculationFormatted")]
        [InlineData("formData")]
        [InlineData("formDataFormatted")]
        public async Task EntityObjectProvider_Should_ReturnQuoteFromProvider_WithIncludedOptionalProperties(string optionalProperty)
        {
            var quoteId = Guid.NewGuid();
            var json = @"{
                               ""entity"": { ""quote"": """ + quoteId.ToString() + @""" },
                               ""includeOptionalProperties"": [ ""/" + optionalProperty + @"""]
                         }";
            var providerBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();
            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(quoteId) };
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IQuoteReadModelRepository)))
                .Returns(mockQuoteRepository.Object);
            var provider = providerBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await provider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(quoteId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, optionalProperty);
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_QuoteVersionEntityProvider()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var quoteVersion = 1;
            var expectedQuoteVersionId = Guid.NewGuid();
            var json = @"{
                            ""quoteVersion"": {
                                    ""quoteId"" : """ + quoteId.ToString() + @""",
                                    ""versionNumber"" : " + quoteVersion + @"
                              }
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            var model = new QuoteVersionReadModelWithRelatedEntities() { QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = expectedQuoteVersionId } };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository)))
                .Returns(mockQuoteVersionRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(expectedQuoteVersionId);
        }

        [Theory]
        [InlineData("calculation")]
        [InlineData("calculationFormatted")]
        [InlineData("formData")]
        [InlineData("formDataFormatted")]
        public async Task EntityObjectProvider_Should_ReturnQuoteVersionFromProvider_WithIncludedOptionalProperties(string optionalProperty)
        {
            var quoteId = Guid.NewGuid();
            var quoteVersion = 1;
            var expectedQuoteVersionId = Guid.NewGuid();
            var json = @"{
                               ""entity"": { ""quoteVersion"": { ""quoteId"":  """ + quoteId.ToString() + @""", ""versionNumber"": """ + quoteVersion + @""" } },
                               ""includeOptionalProperties"": [ ""/" + optionalProperty + @"""]
                         }";
            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            var model = new QuoteVersionReadModelWithRelatedEntities() { QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = expectedQuoteVersionId } };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository)))
                .Returns(mockQuoteVersionRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(expectedQuoteVersionId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, optionalProperty).Should().BeTrue();
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_ClaimEntityProvider()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var json = @"{
                            ""claim"": """ + claimId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            var model = new ClaimReadModelWithRelatedEntities() { Claim = new Mock<ClaimReadModel>(claimId).Object };
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IClaimReadModelRepository)))
                .Returns(mockClaimRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(claimId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "formData").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "formDataFormatted").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "calculation").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "calculationFormatted").Should().BeFalse();
        }

        [Theory]
        [InlineData("calculation")]
        [InlineData("calculationFormatted")]
        [InlineData("formData")]
        [InlineData("formDataFormatted")]
        public async Task EntityObjectProvider_Should_ReturnClaimFromProvider_WithIncludedOptionalProperties(string optionalProperty)
        {
            var claimId = Guid.NewGuid();
            var json = @"{
                               ""entity"": { ""claim"": """ + claimId.ToString() + @""" },
                               ""includeOptionalProperties"": [ ""/" + optionalProperty + @"""]
                         }";
            var providerBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();
            var model = new ClaimReadModelWithRelatedEntities() { Claim = new Mock<ClaimReadModel>(claimId).Object };
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IClaimReadModelRepository)))
                .Returns(mockClaimRepository.Object);
            var provider = providerBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await provider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(claimId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, optionalProperty).Should().BeTrue();
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_ClaimVersionEntityProvider()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var claimVersionNo = 1;
            var expectedclaimVersionNoId = Guid.NewGuid();
            var json = @"{
                            ""claimVersion"": {
                                  ""claimId"" : """ + claimId.ToString() + @""",
                                  ""versionNumber"" : " + claimVersionNo + @"
                              }
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            var model = new ClaimVersionReadModelWithRelatedEntities() { ClaimVersion = new ClaimVersionReadModel() { Id = expectedclaimVersionNoId } };
            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository)))
                .Returns(mockClaimVersionRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(expectedclaimVersionNoId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "formData").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "formDataFormatted").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "calculation").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "calculationFormatted").Should().BeFalse();
        }

        [Theory]
        [InlineData("calculation")]
        [InlineData("calculationFormatted")]
        [InlineData("formData")]
        [InlineData("formDataFormatted")]
        public async Task EntityObjectProvider_Should_ReturnClaimVersionFromProvider_WithIncludedOptionalProperties(string optionalProperty)
        {
            var claimId = Guid.NewGuid();
            var claimVersionNo = 1;
            var expectedclaimVersionNoId = Guid.NewGuid();
            var json = @"{
                               ""entity"": { ""claimVersion"": { ""claimId"":  """ + claimId.ToString() + @""", ""versionNumber"": """ + claimVersionNo + @""" } },
                               ""includeOptionalProperties"": [ ""/" + optionalProperty + @"""]
                         }";
            var providerBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();
            var model = new ClaimVersionReadModelWithRelatedEntities() { ClaimVersion = new ClaimVersionReadModel() { Id = expectedclaimVersionNoId } };
            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository)))
                .Returns(mockClaimVersionRepository.Object);
            var provider = providerBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await provider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(expectedclaimVersionNoId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, optionalProperty).Should().BeTrue();
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_PolicyEntityProvider()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var json = @"{
                            ""policy"": """ + policyId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();

            var model = new PolicyReadModelWithRelatedEntities() { Policy = new FakePolicyReadModel(TenantFactory.DefaultId, policyId) };
            mockPolicyRepository
                .Setup(c => c.GetPolicyWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IPolicyReadModelRepository)))
                .Returns(mockPolicyRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(policyId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "formData").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "formDataFormatted").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "premium").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "premiumFormatted").Should().BeFalse();
        }

        [Theory]
        [InlineData("premium")]
        [InlineData("premiumFormatted")]
        [InlineData("formData")]
        [InlineData("formDataFormatted")]
        public async Task EntityObjectProvider_Should_ReturnPolicyFromProvider_WithIncludedOptionalProperties(string optionalProperty)
        {
            var policyId = Guid.NewGuid();
            var json = @"{
                               ""entity"": { ""policy"": """ + policyId.ToString() + @""" },
                               ""includeOptionalProperties"": [ ""/" + optionalProperty + @"""]
                         }";
            var providerBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();
            var model = new PolicyReadModelWithRelatedEntities() { Policy = new FakePolicyReadModel(TenantFactory.DefaultId, policyId) };
            mockPolicyRepository
                .Setup(c => c.GetPolicyWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IPolicyReadModelRepository)))
                .Returns(mockPolicyRepository.Object);
            var provider = providerBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await provider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(policyId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, optionalProperty).Should().BeTrue();
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_PolicyTransactionEntityProvider()
        {
            // Arrange
            var policyTransactionId = Guid.NewGuid();
            var json = @"{
                            ""policyTransaction"": """ + policyTransactionId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyTransactionRepository = new Mock<IPolicyTransactionReadModelRepository>();

            var model = new PolicyTransactionReadModelWithRelatedEntities()
            {
                PolicyTransaction = new FakePolicyTransactionReadModel(TenantFactory.DefaultId, policyTransactionId),
                TimeZoneId = Timezones.AET.ToString(),
            };
            mockPolicyTransactionRepository
                .Setup(c => c.GetPolicyTransactionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IPolicyTransactionReadModelRepository)))
                .Returns(mockPolicyTransactionRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(policyTransactionId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "formData").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "formDataFormatted").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "calculation").Should().BeFalse();
            DataObjectHelper.ContainsProperty(entityObject.DataValue, "calculationFormatted").Should().BeFalse();
        }

        [Theory]
        [InlineData("calculation")]
        [InlineData("calculationFormatted")]
        [InlineData("formData")]
        [InlineData("formDataFormatted")]
        public async Task EntityObjectProvider_Should_ReturnPolicyTransactionFromProvider_WithIncludedOptionalProperties(string optionalProperty)
        {
            var policyTransactionId = Guid.NewGuid();
            var json = @"{
                               ""entity"": { ""policyTransaction"": """ + policyTransactionId.ToString() + @""" },
                               ""includeOptionalProperties"": [ ""/" + optionalProperty + @"""]
                         }";
            var providerBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyTransactionRepository = new Mock<IPolicyTransactionReadModelRepository>();

            var model = new PolicyTransactionReadModelWithRelatedEntities()
            {
                PolicyTransaction = new FakePolicyTransactionReadModel(TenantFactory.DefaultId, policyTransactionId),
                TimeZoneId = Timezones.AET.ToString(),
            };
            mockPolicyTransactionRepository
                .Setup(c => c.GetPolicyTransactionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IPolicyTransactionReadModelRepository)))
                .Returns(mockPolicyTransactionRepository.Object);
            var provider = providerBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await provider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(policyTransactionId);
            DataObjectHelper.ContainsProperty(entityObject.DataValue, optionalProperty).Should().BeTrue();
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_ProductEntityProvider()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var json = @"{
                            ""product"": """ + productId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockProductRepository = new Mock<IProductRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();
            var tenant = TenantFactory.Create(TenantFactory.DefaultId);
            var product = new Product(tenant.Id, productId, "product_alias", productId.ToString(), NodaTime.SystemClock.Instance.GetCurrentInstant());
            var model = new ProductWithRelatedEntities() { Product = product };
            mockProductRepository
                .Setup(c => c.GetProductWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);
            mockProductRepository
                .Setup(c => c.GetProductById(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), false))
                .Returns(product);
            mockcachingResolver
                .Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));
            mockcachingResolver
                .Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(product));
            mockcachingResolver
                .Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(product));
            mockcachingResolver
                .Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(product));
            mockcachingResolver
                .Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult(product));
            mockcachingResolver
                .Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));
            mockcachingResolver
                .Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(product));
            mockcachingResolver
                .Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(product));
            mockcachingResolver
                .Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(product));
            mockcachingResolver
                .Setup(x => x.GetProductByAliasOrNull(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult(product));

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IProductRepository)))
                .Returns(mockProductRepository.Object);
            mockServiceProvider
                .Setup(c => c.GetService(typeof(ICachingResolver)))
                .Returns(mockcachingResolver.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(productId);
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_TenantEntityProvider()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var json = @"{
                            ""tenant"": """ + tenantId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();

            var tenant = new Domain.Tenant(
                tenantId,
                "Test Tenant",
                "test-tenant",
                null,
                default,
                default,
                this.clock.Now());
            var model = new TenantWithRelatedEntities() { Tenant = tenant };
            mockTenantRepository
                .Setup(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);
            mockTenantRepository
                .Setup(c => c.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            mockcachingResolver
                .Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));
            mockcachingResolver
                .Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));
            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(ITenantRepository)))
                .Returns(mockTenantRepository.Object);
            mockServiceProvider
                .Setup(c => c.GetService(typeof(ICachingResolver)))
                .Returns(mockcachingResolver.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(tenantId);
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_OrganisationEntityProvider()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var json = @"{
                            ""organisation"": """ + organisationId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();

            var organisation = new OrganisationReadModel(TenantFactory.DefaultId, organisationId, "orgAlias", "orgName", null, true, false, new TestClock().Timestamp);
            var model = new OrganisationReadModelWithRelatedEntities() { Organisation = organisation };
            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IOrganisationReadModelRepository)))
                .Returns(mockOrganisationRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(organisationId);
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_UserEntityProvider()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = @"{
                            ""user"": """ + userId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockUserRepository = new Mock<IUserReadModelRepository>();

            var model = new UserReadModelWithRelatedEntities() { User = new FakeUserReadModel(userId) };
            mockUserRepository
                .Setup(c => c.GetUserWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IUserReadModelRepository)))
                .Returns(mockUserRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(userId);
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_CustomerEntityProvider()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var json = @"{
                            ""customer"": """ + customerId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockCustomerRepository = new Mock<ICustomerReadModelRepository>();

            var personReadModel = new PersonReadModel(Guid.NewGuid());
            var deploymentEnvironment = DeploymentEnvironment.Staging;
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var customer = new CustomerReadModel(customerId, personReadModel, deploymentEnvironment, null, currentInstant, false);
            customer.People = new Collection<PersonReadModel> { personReadModel };
            var model = new CustomerReadModelWithRelatedEntities() { Customer = customer };
            mockCustomerRepository
                .Setup(c => c.GetCustomerWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(ICustomerReadModelRepository)))
                .Returns(mockCustomerRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(customerId);
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_EmailEntityProvider()
        {
            // Arrange
            var emailId = Guid.NewGuid();
            var e = "xxx+1@email.com";
            var el = new List<string>() { e };
            var json = @"{
                            ""emailMessage"": """ + emailId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockEmailRepository = new Mock<IEmailRepository>();

            var email = new UBind.Domain.ReadWriteModel.Email.Email(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                emailId,
                el,
                e,
                el,
                el,
                el,
                "test",
                "test",
                "test",
                null,
                new TestClock().Timestamp);
            var model = new EmailReadModelWithRelatedEntities() { Email = email };
            mockEmailRepository
                .Setup(c => c.GetEmailWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IEmailRepository)))
                .Returns(mockEmailRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(emailId);
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_DocumentEntityProvider()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var json = @"{
                            ""document"": """ + documentId.ToString() + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();

            var model = new DocumentReadModelWithRelatedEntities() { Document = new FakeQuoteDocumentReadModel(documentId) };
            mockDocumentRepository
                .Setup(c => c.GetDocumentWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IQuoteDocumentReadModelRepository)))
                .Returns(mockDocumentRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(documentId);
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_PortalEntityProvider()
        {
            // Arrange
            var portalId = Guid.NewGuid();
            var json = @"{ 
                              ""portal"" : """ + portalId + @"""
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPortalRepository = new Mock<IPortalReadModelRepository>();
            var tenant = new Domain.Tenant(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                null,
                default,
                default,
                this.clock.Now());
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

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IPortalReadModelRepository)))
                .Returns(mockPortalRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "alias").Should().Be("myPortal");
        }

        [Fact]
        public async Task EntityObjectProvider_Should_Accept_DynamicEntityProvider()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var json = @"{
                            ""dynamicEntity"": { 
                                 ""entityType"" : ""quote"",
                                 ""entityId"" : """ + quoteId.ToString() + @"""
                             }
                        }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(quoteId) };
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(IQuoteReadModelRepository)))
                .Returns(mockQuoteRepository.Object);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(quoteId);
        }

        private async Task<Mock<IServiceProvider>> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockMediator = new Mock<ICqrsMediator>();
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var cachingResolver = new Mock<ICachingResolver>();
            cachingResolver
                .Setup(x => x.GetTenantOrThrow(automationData.ContextManager.Tenant.Id))
                .Returns(Task.FromResult(TenantFactory.Create(automationData.ContextManager.Tenant.Id)));

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(c => c.GetService(typeof(ISerialisedEntityFactory)))
                .Returns(new SerialisedEntityFactory(
                    mockUrlConfiguration.Object,
                    mockProductConfigProvider.Object,
                    mockFormDataPrettifier.Object,
                    cachingResolver.Object,
                    mockMediator.Object,
                    new DefaultPolicyTransactionTimeOfDayScheme()));
            mockServiceProvider
                .Setup(c => c.GetService(typeof(ICachingResolver)))
                .Returns(cachingResolver.Object);

            return mockServiceProvider;
        }
    }
}
