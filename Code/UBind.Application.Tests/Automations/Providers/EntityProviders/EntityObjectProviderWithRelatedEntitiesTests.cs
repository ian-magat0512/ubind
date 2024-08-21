// <copyright file="EntityObjectProviderWithRelatedEntitiesTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.EntityProviders
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
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
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Person.Fields;
    using UBind.Domain.Configuration;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class EntityObjectProviderWithRelatedEntitiesTests
    {
        private Mock<IOrganisationReadModelRepository> mockOrganisationRepository;
        private Mock<IPolicyTransactionReadModelRepository> mockPolicyTransactionRepository;
        private Mock<IPolicyReadModelRepository> mockPolicyRepository;
        private Mock<IEmailRepository> mockEmailRepository = new Mock<IEmailRepository>();
        private Mock<IQuoteDocumentReadModelRepository> mockDocumentRepository;
        private Mock<ICustomerReadModelRepository> mockCustomerReadModelRepository;
        private Mock<IPersonReadModelRepository> mockPersonReadModelRepository;
        private Mock<IProductRepository> mockProductRepository;
        private Mock<ITenantRepository> mockTenantRepository;
        private Mock<IUserReadModelRepository> mockUserReadModelRepository;
        private Mock<IQuoteVersionReadModelRepository> mockQuoteVersionRepository;
        private Mock<IQuoteReadModelRepository> mockQuoteRepository;
        private Mock<IClaimVersionReadModelRepository> mockClaimVersionRepository;
        private Mock<IClaimReadModelRepository> mockClaimRepository;
        private Mock<IPortalReadModelRepository> mockPortalRepository;
        private Mock<ICachingResolver> mockcachingResolver;

        public EntityObjectProviderWithRelatedEntitiesTests()
        {
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Domain.Entities.Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Domain.Entities.Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("policy")]
        [InlineData("policyTransaction")]
        [InlineData("messages")]
        [InlineData("documents")]
        [InlineData("quoteVersions")]
        public async Task EntityObjectProvider_Should_Accept_QuoteEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""quote"": """ + quoteId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(quoteId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var properties = typeof(SerialisedEntitySchemaObject.Quote).GetProperties()
                .Where(p => p.GetCustomAttribute<JsonPropertyAttribute>() != null);
            var property = properties.FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>()
                .PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(quoteId);

            this.mockQuoteRepository
                .Verify(c => c.GetQuoteWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), quoteId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("quote")]
        [InlineData("messages")]
        [InlineData("documents")]
        public async Task EntityObjectProvider_Should_Accept_QuoteVersionEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var quoteVersionId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""quoteVersion"":  """ + quoteVersionId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(quoteVersionId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var properties = typeof(SerialisedEntitySchemaObject.QuoteVersion).GetProperties()
                .Where(p => p.GetCustomAttribute<JsonPropertyAttribute>() != null);
            var property = properties.FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>()
                .PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(quoteVersionId);

            this.mockQuoteVersionRepository
                .Verify(c => c.GetQuoteVersionWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), quoteVersionId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("policy")]
        [InlineData("messages")]
        [InlineData("documents")]
        [InlineData("claimVersions")]
        public async Task EntityObjectProvider_Should_Accept_ClaimEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""claim"": """ + claimId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(claimId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var properties = typeof(SerialisedEntitySchemaObject.Claim).GetProperties(
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<JsonPropertyAttribute>() != null);
            var property = properties.FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>()
                .PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(claimId);

            this.mockClaimRepository.Verify(
                c => c.GetClaimWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), claimId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("claim")]
        [InlineData("policy")]
        [InlineData("messages")]
        [InlineData("documents")]
        public async Task EntityObjectProvider_Should_Accept_ClaimVersionEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var claimVersionId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""claimVersion"":  """ + claimVersionId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(claimVersionId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var properties = typeof(SerialisedEntitySchemaObject.ClaimVersion).GetProperties()
                .Where(p => p.GetCustomAttribute<JsonPropertyAttribute>() != null);
            var property = properties.FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject, "id").Should().Be(claimVersionId);

            this.mockClaimVersionRepository.Verify(
                c => c.GetClaimVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), claimVersionId, new List<string>() { property.Name }),
                Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("policyTransactions")]
        [InlineData("claims")]
        [InlineData("messages")]
        [InlineData("documents")]
        [InlineData("quotes")]
        public async Task EntityObjectProvider_Should_Accept_PolicyEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""policy"": """ + policyId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(policyId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);
            var properties = typeof(SerialisedEntitySchemaObject.Policy).GetProperties()
                .Where(p => p.GetCustomAttribute<JsonPropertyAttribute>() != null);
            var property = properties.FirstOrDefault(p
                => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(policyId);

            this.mockPolicyRepository
                .Verify(c => c.GetPolicyWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), policyId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("policy")]
        [InlineData("quote")]
        [InlineData("messages")]
        [InlineData("documents")]
        public async Task EntityObjectProvider_Should_Accept_PolicyTransactionEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var policyTransactionId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""policyTransaction"": """ + policyTransactionId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(policyTransactionId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.PolicyTransaction).GetProperties().FirstOrDefault(p =>
                p.GetCustomAttribute<JsonIgnoreAttribute>() == null &&
                p.GetCustomAttribute<JsonPropertyAttribute>() != null &&
                p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(policyTransactionId);

            this.mockPolicyTransactionRepository
                .Verify(c => c.GetPolicyTransactionWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), policyTransactionId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        public async Task EntityObjectProvider_Should_Accept_ProductEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var productId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""product"": """ + productId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(productId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.Product).GetProperties().FirstOrDefault(p =>
                p.GetCustomAttribute<JsonIgnoreAttribute>() == null &&
                p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(productId);

            this.mockProductRepository.Verify(c => c.GetProductWithRelatedEntities(It.IsAny<Guid>(), productId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        public async Task EntityObjectProvider_Should_Accept_PortalEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var portalId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""portal"": """ + portalId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(portalId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.Portal).GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(
                new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "name").Should().Be("My Portal");

            this.mockPortalRepository
                .Verify(c => c.GetPortalWithRelatedEntities(It.IsAny<Guid>(), portalId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("defaultOrganisation")]
        public async Task EntityObjectProvider_Should_Accept_TenantEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""tenant"": """ + tenantId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(tenantId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.Tenant).GetProperties().FirstOrDefault(p =>
                p.GetCustomAttribute<JsonIgnoreAttribute>() == null &&
                p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(tenantId);

            this.mockTenantRepository.Verify(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        public async Task EntityObjectProvider_Should_Accept_OrganisationEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""organisation"": """ + organisationId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(organisationId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.Organisation).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(organisationId);

            this.mockOrganisationRepository
                .Verify(c => c.GetOrganisationWithRelatedEntities(It.IsAny<Guid>(), organisationId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("person")]
        [InlineData("roles")]
        public async Task EntityObjectProvider_Should_Accept_UserEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""user"": """ + userId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(userId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.User).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(userId);

            this.mockUserReadModelRepository
                .Verify(c => c.GetUserWithRelatedEntities(TenantFactory.DefaultId, userId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("owner")]
        [InlineData("people")]
        [InlineData("quotes")]
        [InlineData("policies")]
        [InlineData("claims")]
        [InlineData("messages")]
        [InlineData("documents")]
        public async Task EntityObjectProvider_Should_Accept_CustomerEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""customer"": """ + entityId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(entityId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(Customer).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(entityId);

            this.mockCustomerReadModelRepository
                .Verify(c => c.GetCustomerWithRelatedEntities(TenantFactory.DefaultId, It.IsAny<DeploymentEnvironment?>(), entityId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("tags")]
        [InlineData("relationships")]
        public async Task EntityObjectProvider_Should_Accept_EmailEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var emailId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""emailMessage"": """ + emailId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(emailId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.EmailMessage).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(emailId);

            this.mockEmailRepository
                .Verify(c => c.GetEmailWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), emailId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        public async Task EntityObjectProvider_Should_Accept_DocumentEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""document"": """ + documentId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(documentId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.Document).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(documentId);

            this.mockDocumentRepository
                .Verify(c => c.GetDocumentWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), documentId, new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("customer")]
        [InlineData("user")]
        public async Task EntityObjectProvider_Should_Accept_PersonEntityProvider_With_Related_Entity(string relatedEntity)
        {
            // Arrange
            var personId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""person"": """ + personId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(personId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var property = typeof(SerialisedEntitySchemaObject.Person).GetProperties().FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyAttribute>().PropertyName.EqualsIgnoreCase(relatedEntity));
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").ToString().Should().Be(personId.ToString());

            // repeating fields
            List<Dictionary<string, object>> emailAddresses = (List<Dictionary<string, object>>)DataObjectHelper
                .GetPropertyValue(entityObject.DataValue, "emailAddresses");
            emailAddresses[0]["emailAddress"].Should().Be("person_email@test.com");
            emailAddresses[0]["label"].Should().Be("Work");

            List<Dictionary<string, object>> phoneNumbers = (List<Dictionary<string, object>>)DataObjectHelper
                .GetPropertyValue(entityObject.DataValue, "phoneNumbers");
            phoneNumbers[0]["phoneNumber"].Should().Be("0412341234");
            phoneNumbers[0]["label"].Should().Be("Work");

            List<Dictionary<string, object>> streetAddresses = (List<Dictionary<string, object>>)DataObjectHelper
                .GetPropertyValue(entityObject.DataValue, "streetAddresses");
            streetAddresses[0]["address"].Should().Be("123");
            streetAddresses[0]["suburb"].Should().Be("Sub");
            streetAddresses[0]["postcode"].Should().Be("4000");
            streetAddresses[0]["state"].Should().Be("QLD");
            streetAddresses[0]["label"].Should().Be("Home");

            List<Dictionary<string, object>> websiteAddresses = (List<Dictionary<string, object>>)DataObjectHelper
                .GetPropertyValue(entityObject.DataValue, "websiteAddresses");
            websiteAddresses[0]["websiteAddress"].Should().Be("www.test.com");
            websiteAddresses[0]["label"].Should().Be("Business");

            List<Dictionary<string, object>> messengerIds = (List<Dictionary<string, object>>)DataObjectHelper
                .GetPropertyValue(entityObject.DataValue, "messengerIds");
            messengerIds[0]["messengerId"].Should().Be("skype001");
            messengerIds[0]["label"].Should().Be("Skype");

            List<Dictionary<string, object>> socialMediaIds = (List<Dictionary<string, object>>)DataObjectHelper
                .GetPropertyValue(entityObject.DataValue, "socialMediaIds");
            socialMediaIds[0]["socialMediaId"].Should().Be("peacebook");
            socialMediaIds[0]["label"].Should().Be("Facebook");

            this.mockPersonReadModelRepository.Verify(
                c => c.GetPersonWithRelatedEntities(
                    TenantFactory.DefaultId,
                    personId,
                    new List<string>() { property.Name }), Times.Once);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant", "defaultOrganisation")]
        [InlineData("organisation", "tenant")]
        [InlineData("product", "tenant")]
        [InlineData("customer", "organisation")]
        [InlineData("user", "organisation")]
        [InlineData("policy", "organisation")]
        [InlineData("policyTransaction", "organisation")]
        [InlineData("claimVersion", "organisation")]
        [InlineData("claim", "organisation")]
        [InlineData("quoteVersion", "organisation")]
        [InlineData("quote", "organisation")]
        [InlineData("emailMessage", "organisation")]
        [InlineData("document", "organisation")]
        [InlineData("customer", "policies")]
        [InlineData("user", "roles")]
        [InlineData("policy", "quotes")]
        [InlineData("policyTransaction", "documents")]
        [InlineData("claimVersion", "messages")]
        [InlineData("claim", "claimVersions")]
        [InlineData("quoteVersion", "documents")]
        [InlineData("quote", "quoteVersions")]
        [InlineData("emailMessage", "tags")]
        public async Task EntityObjectProvider_Should_Accept_DynamicEntityProvider_With_Related_Entity(string entity, string relatedEntity)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""dynamicEntity"": { 
                                     ""entityType"" : """ + entity + @""",
                                     ""entityId"" : """ + entityId.ToString() + @"""
                                }
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                        }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = this.GetServiceProvider(entityId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "id").Should().Be(entityId);
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, relatedEntity).Should().NotBeNull();
        }

        [Theory]
        [InlineData("organisation", "/tenant/defaultOrganisation")]
        [InlineData("product", "/tenant/defaultOrganisation")]
        [InlineData("customer", "/organisation/tenant")]
        [InlineData("user", "/organisation/tenant")]
        [InlineData("policy", "/organisation/tenant")]
        [InlineData("policyTransaction", "/organisation/tenant")]
        [InlineData("claimVersion", "/organisation/tenant")]
        [InlineData("claim", "/organisation/tenant")]
        [InlineData("quoteVersion", "/organisation/tenant")]
        [InlineData("quote", "/organisation/tenant")]
        [InlineData("emailMessage", "/organisation/tenant")]
        [InlineData("document", "/organisation/tenant")]
        [InlineData("policyTransaction", "/policy/quotes")]
        [InlineData("claimVersion", "/claim/claimVersions")]
        [InlineData("quoteVersion", "/customer/organisation")]
        [InlineData("customer", "/policies/tenant")]
        [InlineData("policy", "/claims/tenant")]
        [InlineData("claimVersion", "/messages")]
        [InlineData("claim", "/messages")]
        [InlineData("claim", "/claimVersions/claim")]
        [InlineData("quoteVersion", "/documents/tenant")]
        [InlineData("quote", "/documents/tenant")]
        [InlineData("policy", "/quotes/product")]
        [InlineData("policy", "/quotes/quoteVersions")]
        [InlineData("policyTransaction", "/documents/tenant")]
        [InlineData("claim", "/claimVersions/policy")]
        [InlineData("quoteVersion", "/messages")]
        [InlineData("quote", "/messages")]
        [InlineData("quote", "/quoteVersions/customer/owner/tenant")]
        public async Task EntityObjectProvider_ShouldAcceptEntity_WithNestedSingleRelatedEntity(
            string entity, string relatedEntityPath)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
                            ""entity"" :
                            {
                                ""dynamicEntity"": { 
                                     ""entityType"" : """ + entity + @""",
                                     ""entityId"" : """ + entityId.ToString() + @"""
                                }
                            },
                            ""includeOptionalProperties"" : [""" + relatedEntityPath + @"""]
                        }";

            var entityObjectProviderBuilder = JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(entityId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            var id = DataObjectHelper.GetPropertyValueOrThrow(entityObject.DataValue, "id");
            id.ToString().Should().Be(entityId.ToString());

            var jPointer = new PocoJsonPointer(relatedEntityPath, string.Empty);
            object parentObject = entityObject.DataValue;
            for (int i = 0; i < jPointer.ReferenceTokens.Length; i++)
            {
                var token = jPointer.ReferenceTokens[i];
                var relatedEntityResult = DataObjectHelper.GetPropertyValue(parentObject, token);
                relatedEntityResult.Should().NotBeNull();

                if (i < jPointer.ReferenceTokens.Length - 1)
                {
                    if (DataObjectHelper.IsArray(relatedEntityResult))
                    {
                        parentObject = (relatedEntityResult as IList)[0];
                    }
                    else
                    {
                        parentObject = relatedEntityResult;
                    }
                }
            }
        }

        /// <summary>
        /// Unit tests for https://jira.aptiture.com/browse/UB-9911.
        /// Entity object should ONLY be included the formData as defined in optionalProperties.
        /// It should NOT be included in the related entities.
        /// </summary>
        /// <param name="entity">The name of the entity.</param>
        /// <param name="optionalProperties">The optional properties to include in the result.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("policy", "/policyTransactions", "/quotes", "/claims")]
        [InlineData("policyTransaction", "/policy", "/quote")]
        [InlineData("claimVersion", "/claim", "/policy")]
        [InlineData("quote", "/quoteVersions", "/policyTransaction", "/policy")]
        [InlineData("quoteVersion", "/quote")]
        public async Task EntityObjectProvider_ShouldNotHaveFormData_OnRelatedEntities(
            string entity, params string[] optionalProperties)
        {
            var entityId = Guid.NewGuid();

            // formData should only appear on the entity
            var includeOptionalProperties = string.Join(",", optionalProperties.Append("/formData")
                .Select(x => string.Format("\"{0}\"", x)).ToList());
            var json = @"{
                            ""entity"" :
                            {
                                """ + entity + @""": """ + entityId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [ " + includeOptionalProperties + @" ]
                         }";

            var entityObjectProviderBuilder =
                JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(entityId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            var id = DataObjectHelper.GetPropertyValueOrThrow(entityObject.DataValue, "id");
            id.ToString().Should().Be(entityId.ToString());

            // Entity should have formData
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "formData").Should().NotBeNull();

            foreach (var relatedEntityPath in optionalProperties)
            {
                var jPointer = new PocoJsonPointer(relatedEntityPath, string.Empty);
                object parentObject = entityObject.DataValue;
                for (int i = 0; i < jPointer.ReferenceTokens.Length; i++)
                {
                    var token = jPointer.ReferenceTokens[i];
                    var relatedEntityResult = DataObjectHelper.GetPropertyValue(parentObject, token);
                    relatedEntityResult.Should().NotBeNull();

                    // Related entity should NOT have formData
                    var relatedEntityFormData = DataObjectHelper.GetPropertyValue(relatedEntityResult, "formData");
                    relatedEntityFormData.Should().BeNull();

                    if (i < jPointer.ReferenceTokens.Length - 1)
                    {
                        if (DataObjectHelper.IsArray(relatedEntityResult))
                        {
                            parentObject = (relatedEntityResult as IList)[0];
                        }
                        else
                        {
                            parentObject = relatedEntityResult;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unit tests for https://jira.aptiture.com/browse/UB-9911.
        /// Entity object should ONLY be included the formData as defined in optionalProperties.
        /// It should NOT be included in the related entities.
        /// </summary>
        /// <param name="entity">The name of the entity.</param>
        /// <param name="optionalProperties">The optional properties to include in the result.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("policy", "/policyTransactions", "/quotes", "/claims")]
        [InlineData("policyTransaction", "/policy", "/quote")]
        [InlineData("claimVersion", "/claim", "/policy")]
        [InlineData("quote", "/quoteVersions", "/policyTransaction", "/policy")]
        [InlineData("quoteVersion", "/quote")]
        public async Task EntityObjectProvider_ShouldNotHaveFormDataFormatted_OnEntity(
            string entity, params string[] optionalProperties)
        {
            var entityId = Guid.NewGuid();

            // formData should only appear on the entity
            var includeOptionalProperties = string.Join(",", optionalProperties.Append("/formData")
                .Select(x => string.Format("\"{0}\"", x)).ToList());
            var json = @"{
                            ""entity"" :
                            {
                                """ + entity + @""": """ + entityId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [ " + includeOptionalProperties + @" ]
                         }";

            var entityObjectProviderBuilder =
                JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(entityId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            var id = DataObjectHelper.GetPropertyValueOrThrow(entityObject.DataValue, "id");
            id.ToString().Should().Be(entityId.ToString());

            // Entity should have formData
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "formData").Should().NotBeNull();

            // Entity should not have formDataFormatted
            DataObjectHelper.GetPropertyValue(entityObject.DataValue, "formDataFormatted").Should().BeNull();
        }

        /// <summary>
        /// Unit tests for https://jira.aptiture.com/browse/UB-9911.
        /// </summary>
        /// <param name="entity">The name of the entity.</param>
        /// <param name="optionalProperties">The optional properties to include in the result.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData("policy", "/formData", "/policyTransactions", "/quotes", "/claims")]
        [InlineData("policy", "/formData", "/policyTransactions/formData", "/quotes/formData", "/claims/formData")]
        [InlineData("policyTransaction", "/formData", "/policy", "/quote")]
        [InlineData("policyTransaction", "/formData", "/policy/formData", "/quote/formData")]
        [InlineData("claimVersion", "/formData", "/claim", "/policy")]
        [InlineData("claimVersion", "/formData", "/claim/formData", "/policy/formData")]
        [InlineData("customer", "/policies", "/claims", "/quotes")]
        [InlineData("customer", "/policies/formData", "/claims/formData", "/quotes/formData")]
        [InlineData("quote", "/formData", "/quoteVersions", "/policyTransaction", "/policy")]
        [InlineData("quote", "/formData", "/quoteVersions/formData", "/policyTransaction/formData", "/policy/formData")]
        [InlineData("quoteVersion", "/formData", "/quote")]
        [InlineData("quoteVersion", "/formData", "/quote/formData")]
        [InlineData("customer", "/quotes/policy/tenant/defaultOrganisation")]
        [InlineData("quote", "/formDataFormatted")]
        public async Task EntityObjectProvider_ShouldLoad_EntityProviderIncludingAllOptionalProperties(
            string entity, params string[] optionalProperties)
        {
            var entityId = Guid.NewGuid();
            var includeOptionalProperties =
                string.Join(",", optionalProperties.Select(x => string.Format("\"{0}\"", x)).ToList());
            var json = @"{
                            ""entity"" :
                            {
                                """ + entity + @""": """ + entityId.ToString() + @"""
                            },
                            ""includeOptionalProperties"" : [ " + includeOptionalProperties + @" ]
                         }";

            var entityObjectProviderBuilder =
                JsonConvert.DeserializeObject<EntityObjectProviderConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);

            var mockServiceProvider = this.GetServiceProvider(entityId);
            var entityObjectProvider = entityObjectProviderBuilder.Build(mockServiceProvider);

            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObject = (await entityObjectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.Should().NotBeNull();
            var id = DataObjectHelper.GetPropertyValueOrThrow(entityObject.DataValue, "id");
            id.ToString().Should().Be(entityId.ToString());

            foreach (var relatedEntityPath in optionalProperties)
            {
                var jPointer = new PocoJsonPointer(relatedEntityPath, string.Empty);
                object parentObject = entityObject.DataValue;
                for (int i = 0; i < jPointer.ReferenceTokens.Length; i++)
                {
                    var token = jPointer.ReferenceTokens[i];
                    var relatedEntityResult = DataObjectHelper.GetPropertyValue(parentObject, token);
                    relatedEntityResult.Should().NotBeNull();

                    if (i < jPointer.ReferenceTokens.Length - 1)
                    {
                        if (DataObjectHelper.IsArray(relatedEntityResult))
                        {
                            parentObject = (relatedEntityResult as IList)[0];
                        }
                        else
                        {
                            parentObject = relatedEntityResult;
                        }
                    }
                }
            }
        }

        private IServiceProvider GetServiceProvider(Guid entityId)
        {
            var clock = new TestClock();
            var tenant = new Domain.Tenant(entityId, "name", entityId.ToString(), null, default, default, clock.Timestamp);
            var tenantDetail = new Domain.TenantDetails("arthur", "arthur", default, false, false, default, default, clock.Timestamp);
            var product = new Domain.Product.Product(TenantFactory.DefaultId, entityId, "product_alias", entityId.ToString(), clock.Timestamp);
            var productDetail = new Domain.Product.ProductDetails("arthur", product.Id.ToString(), false, false, clock.Timestamp);
            var organisation = new OrganisationReadModel(TenantFactory.DefaultId, entityId, "orgAlias", "orgName", null, true, false, clock.Timestamp);

            var claim = new Mock<ClaimReadModel>(entityId).Object;
            var claimVersion = new ClaimVersionReadModel() { Id = entityId, ClaimId = Guid.NewGuid() };

            var policy = new FakePolicyReadModel(tenant.Id, entityId);
            var policyTransaction = new FakePolicyTransactionReadModel(tenant.Id, entityId);

            var quote = new FakeNewQuoteReadModel(entityId);
            var quoteVersion = new QuoteVersionReadModel() { QuoteVersionId = entityId };

            var personData = new PersonData();
            personData.FirstName = "personFirstName";
            personData.Email = "person_primary@test.com";
            personData.AlternativeEmail = "person_alternative@test.com";
            personData.EmailAddresses = new List<EmailAddressField>
                {
                    new EmailAddressField("Work", string.Empty, new Domain.ValueTypes.EmailAddress("person_email@test.com")),
                };
            personData.PhoneNumbers = new List<PhoneNumberField>
                {
                    new PhoneNumberField("Work", string.Empty, new Domain.ValueTypes.PhoneNumber("0412341234")),
                };
            personData.StreetAddresses = new List<StreetAddressField>
                {
                    new StreetAddressField("Home", string.Empty, new Domain.ValueTypes.Address
                    { Line1 = "123", Postcode = "4000", State = Domain.ValueTypes.State.QLD, Suburb = "Sub" }),
                };
            personData.WebsiteAddresses = new List<WebsiteAddressField>
                {
                    new WebsiteAddressField("Business", string.Empty, new Domain.ValueTypes.WebAddress("www.test.com")),
                };
            personData.MessengerIds = new List<MessengerIdField>
                {
                    new MessengerIdField("Skype", string.Empty, "skype001"),
                };
            personData.SocialMediaIds = new List<SocialMediaIdField>
                {
                    new SocialMediaIdField("Facebook", string.Empty, "peacebook"),
                };
            var person = PersonReadModel.CreateFromPersonData(tenant.Id, entityId, default, default, personData, clock.Timestamp);

            var user = new FakeUserReadModel(entityId);
            var customer = new FakeCustomerReadModel(entityId);
            var quoteDocument = new FakeQuoteDocumentReadModel(entityId);

            var claimFileAttachment = new ClaimFileAttachment("name", "type", 1234, Guid.NewGuid(), clock.Timestamp);
            var claimDocument = ClaimAttachmentReadModel.CreateClaimAttachmentReadModel(entityId, claimFileAttachment);
            var claimVersionDocument = ClaimAttachmentReadModel.CreateClaimVersionAttachmentReadModel(entityId, entityId, claimFileAttachment);

            var relationShip = new UBind.Domain.ReadWriteModel.Relationship(tenant.Id, EntityType.Message, entityId, RelationshipType.QuoteMessage, EntityType.Quote, entityId, clock.Timestamp);
            var tag = new UBind.Domain.ReadWriteModel.Tag(EntityType.Message, Guid.NewGuid(), TagType.EmailType, "value", clock.Timestamp);
            var emailAddress = "xxx+1@email.com";
            var emailList = new List<string>() { emailAddress };
            var email = new UBind.Domain.ReadWriteModel.Email.Email(
                Guid.NewGuid(),
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                entityId,
                emailList,
                emailAddress,
                emailList,
                emailList,
                emailList,
                "test",
                "test",
                "test",
                null,
                new TestClock().Timestamp);

            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();

            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            var mockMediator = new Mock<ICqrsMediator>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ISerialisedEntityFactory>(c => new SerialisedEntityFactory(
                mockUrlConfiguration.Object,
                mockProductConfigProvider.Object,
                mockFormDataPrettifier.Object,
                mockcachingResolver.Object,
                mockMediator.Object,
                new DefaultPolicyTransactionTimeOfDayScheme()));

            serviceCollection.AddScoped(c => mockcachingResolver.Object);
            this.mockClaimRepository = new Mock<IClaimReadModelRepository>();
            var claimModel = new ClaimReadModelWithRelatedEntities();
            claimModel.Claim = new Mock<ClaimReadModel>(entityId).Object;
            claimModel.Product = product;
            claimModel.ProductDetails = new List<ProductDetails>() { productDetail };
            claimModel.Tenant = tenant;
            claimModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            claimModel.Organisation = organisation;
            claimModel.Policy = policy;
            claimModel.Customer = customer;
            claimModel.Owner = user;
            claimModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            claimModel.ClaimVersions = new List<ClaimVersionReadModel>() { claimVersion };
            claimModel.Documents = new List<ClaimAttachmentReadModel>() { claimDocument };
            this.mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(claimModel);
            serviceCollection.AddScoped(c => this.mockClaimRepository.Object);

            this.mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();
            var claimVersionModel = new ClaimVersionReadModelWithRelatedEntities();
            claimVersionModel.Claim = new Mock<ClaimReadModel>(entityId).Object;
            claimVersionModel.Product = product;
            claimVersionModel.ProductDetails = new List<ProductDetails>() { productDetail };
            claimVersionModel.Tenant = tenant;
            claimVersionModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            claimVersionModel.Organisation = organisation;
            claimVersionModel.Policy = policy;
            claimVersionModel.Customer = customer;
            claimVersionModel.Owner = user;
            claimVersionModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            claimVersionModel.ClaimVersion = claimVersion;
            claimVersionModel.Documents = new List<ClaimAttachmentReadModel>() { claimVersionDocument };
            this.mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(claimVersionModel);
            serviceCollection.AddScoped(c => this.mockClaimVersionRepository.Object);

            this.mockQuoteRepository = new Mock<IQuoteReadModelRepository>();
            var quoteModel = new QuoteReadModelWithRelatedEntities();
            quoteModel.Quote = quote;
            quoteModel.Product = product;
            quoteModel.ProductDetails = new List<ProductDetails>() { productDetail };
            quoteModel.Tenant = tenant;
            quoteModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            quoteModel.Organisation = organisation;
            quoteModel.Policy = policy;
            quoteModel.PolicyTransaction = policyTransaction;
            quoteModel.Customer = customer;
            quoteModel.Owner = user;
            quoteModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            quoteModel.QuoteVersions = new List<QuoteVersionReadModel>() { quoteVersion };
            quoteModel.Documents = new List<QuoteDocumentReadModel>() { quoteDocument };
            this.mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(quoteModel);
            serviceCollection.AddScoped(c => this.mockQuoteRepository.Object);

            this.mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();
            var quoteVersionModel = new QuoteVersionReadModelWithRelatedEntities();
            quoteVersionModel.Quote = quote;
            quoteVersionModel.Product = product;
            quoteVersionModel.ProductDetails = new List<ProductDetails>() { productDetail };
            quoteVersionModel.Tenant = tenant;
            quoteVersionModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            quoteVersionModel.Organisation = organisation;
            quoteVersionModel.Customer = customer;
            quoteVersionModel.Owner = user;
            quoteVersionModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            quoteVersionModel.QuoteVersion = quoteVersion;
            quoteVersionModel.Documents = new List<QuoteDocumentReadModel>() { quoteDocument };
            this.mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(quoteVersionModel);
            serviceCollection.AddScoped(c => this.mockQuoteVersionRepository.Object);

            this.mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
            var userModel = new UserReadModelWithRelatedEntities();
            userModel.User = user;
            userModel.Person = person;
            userModel.Roles = new List<Domain.Entities.Role>() { new Domain.Entities.Role(TenantFactory.DefaultId, Guid.NewGuid(), Domain.Permissions.DefaultRole.Customer, NodaTime.SystemClock.Instance.GetCurrentInstant()) };
            userModel.Tenant = tenant;
            userModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            userModel.Organisation = organisation;
            this.mockUserReadModelRepository
                .Setup(c => c.GetUserWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(userModel);
            serviceCollection.AddScoped(c => this.mockUserReadModelRepository.Object);

            this.mockTenantRepository = new Mock<ITenantRepository>();
            var tenantModel = new TenantWithRelatedEntities();
            tenantModel.Products = new List<Domain.Product.Product>() { product };
            tenantModel.Tenant = tenant;
            tenantModel.DefaultOrganisation = organisation;
            this.mockTenantRepository.Setup(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(tenantModel);
            this.mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            serviceCollection.AddScoped(c => this.mockTenantRepository.Object);

            this.mockProductRepository = new Mock<IProductRepository>();
            var productModel = new ProductWithRelatedEntities();
            productModel.Product = product;
            productModel.Tenant = tenant;
            quoteVersionModel.Documents = new List<QuoteDocumentReadModel>() { quoteDocument };
            this.mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(productModel);
            this.mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            serviceCollection.AddScoped(c => this.mockProductRepository.Object);

            this.mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();
            var customerModel = new CustomerReadModelWithRelatedEntities();
            customerModel.Customer = customer;
            customerModel.Quotes = new List<NewQuoteReadModel>() { quote };
            customerModel.Claims = new List<ClaimReadModel>() { claim };
            customerModel.Tenant = tenant;
            customerModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            customerModel.Organisation = organisation;
            customerModel.Policies = new List<PolicyReadModel>() { policy };
            customerModel.PolicyTransactions = new List<Domain.ReadModel.Policy.PolicyTransaction>() { policyTransaction };
            customerModel.Owner = user;
            customerModel.People = new List<PersonReadModel>() { person };
            customerModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            customerModel.QuoteDocuments = new List<QuoteDocumentReadModel>() { quoteDocument };
            customerModel.ClaimDocuments = new List<ClaimAttachmentReadModel>() { claimDocument };
            this.mockCustomerReadModelRepository
                .Setup(c => c.GetCustomerWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(customerModel);
            serviceCollection.AddScoped(c => this.mockCustomerReadModelRepository.Object);

            this.mockDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();
            var documentModel = new DocumentReadModelWithRelatedEntities();
            documentModel.Document = quoteDocument;
            documentModel.Tenant = tenant;
            documentModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            documentModel.Organisation = organisation;
            this.mockDocumentRepository
                .Setup(c => c.GetDocumentWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(documentModel);
            var mockConfiguration = new Mock<IEmailInvitationConfiguration>();
            mockConfiguration.Setup(c => c.InvitationLinkHost).Returns("https://localhost:4366");
            serviceCollection.AddScoped(c => this.mockDocumentRepository.Object);
            serviceCollection.AddScoped(c => mockConfiguration.Object);

            this.mockEmailRepository = new Mock<IEmailRepository>();
            var emailModel = new EmailReadModelWithRelatedEntities();
            emailModel.Email = email;
            emailModel.FromRelationships = new List<UBind.Domain.ReadWriteModel.Relationship>() { relationShip };
            emailModel.ToRelationships = new List<UBind.Domain.ReadWriteModel.Relationship>() { relationShip };
            emailModel.Tags = new List<UBind.Domain.ReadWriteModel.Tag>() { tag };
            emailModel.Tenant = tenant;
            emailModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            emailModel.Organisation = organisation;
            this.mockEmailRepository
                .Setup(c => c.GetEmailWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(emailModel);
            serviceCollection.AddScoped(c => this.mockEmailRepository.Object);

            this.mockPolicyRepository = new Mock<IPolicyReadModelRepository>();
            var policyModel = new PolicyReadModelWithRelatedEntities();
            policyModel.Quotes = new List<NewQuoteReadModel>() { quote };
            policyModel.Product = product;
            policyModel.ProductDetails = new List<ProductDetails>() { productDetail };
            policyModel.Tenant = tenant;
            policyModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            policyModel.Organisation = organisation;
            policyModel.Policy = policy;
            policyModel.PolicyTransactions = new List<Domain.ReadModel.Policy.PolicyTransaction>() { policyTransaction };
            policyModel.Customer = customer;
            policyModel.Owner = user;
            policyModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            policyModel.QuoteDocuments = new List<QuoteDocumentReadModel>() { quoteDocument };
            policyModel.ClaimDocuments = new List<ClaimAttachmentReadModel>() { claimDocument };
            policyModel.Claims = new List<ClaimReadModel>() { claim };
            this.mockPolicyRepository
                .Setup(c => c.GetPolicyWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(policyModel);
            serviceCollection.AddScoped(c => this.mockPolicyRepository.Object);

            this.mockPolicyTransactionRepository = new Mock<IPolicyTransactionReadModelRepository>();
            var polTransactionModel = new PolicyTransactionReadModelWithRelatedEntities();
            polTransactionModel.Quote = quote;
            polTransactionModel.Product = product;
            polTransactionModel.ProductDetails = new List<ProductDetails>() { productDetail };
            polTransactionModel.Tenant = tenant;
            polTransactionModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            polTransactionModel.Organisation = organisation;
            polTransactionModel.Policy = policy;
            polTransactionModel.PolicyTransaction = policyTransaction;
            polTransactionModel.Customer = customer;
            polTransactionModel.Owner = user;
            polTransactionModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            polTransactionModel.Documents = new List<QuoteDocumentReadModel>() { quoteDocument };
            polTransactionModel.TimeZoneId = Timezones.AET.ToString();
            this.mockPolicyTransactionRepository
                .Setup(c => c.GetPolicyTransactionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(polTransactionModel);
            serviceCollection.AddScoped(c => this.mockPolicyTransactionRepository.Object);

            this.mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();
            var orgModel = new OrganisationReadModelWithRelatedEntities();
            orgModel.Organisation = organisation;
            orgModel.Tenant = tenant;
            this.mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(orgModel);
            serviceCollection.AddScoped(c => this.mockOrganisationRepository.Object);

            this.mockPortalRepository = new Mock<IPortalReadModelRepository>();
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
                OrganisationId = organisation.Id,
            };
            var portalModel = new PortalWithRelatedEntities() { Portal = portal, Tenant = tenant, Organisation = organisation };
            this.mockPortalRepository
                .Setup(c => c.GetPortalWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(portalModel);
            serviceCollection.AddScoped(c => this.mockPortalRepository.Object);

            this.mockPersonReadModelRepository = new Mock<IPersonReadModelRepository>();
            var personModel = new PersonReadModelWithRelatedEntities();
            personModel.Person = person;
            personModel.Tenant = tenant;
            personModel.Organisation = organisation;
            personModel.User = user;
            personModel.Customer = customer;
            this.mockPersonReadModelRepository.Setup(c => c.GetPersonWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(personModel);
            serviceCollection.AddScoped(c => this.mockPersonReadModelRepository.Object);

            this.mockcachingResolver = new Mock<ICachingResolver>();
            this.mockcachingResolver.Setup(x =>
            x.GetTenantOrThrow(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));
            this.mockcachingResolver.Setup(x =>
            x.GetTenantOrNull(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));

            this.mockcachingResolver.Setup(x =>
            x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(product));
            this.mockcachingResolver.Setup(x =>
            x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(product));

            this.mockcachingResolver.Setup(x =>
            x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
             .Returns(Task.FromResult(product));
            this.mockcachingResolver.Setup(x =>
            x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>()))
             .Returns(Task.FromResult(product));

            serviceCollection.AddScoped(c => this.mockcachingResolver.Object);

            serviceCollection.AddScoped<IClock>(c => new TestClock());

            return serviceCollection.BuildServiceProvider();
        }
    }
}
