// <copyright file="DynamicEntityProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.EntityProviders
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
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using Xunit;
    using SerialisedEntitySchemaObject = UBind.Domain.SerialisedEntitySchemaObject;

    public class DynamicEntityProviderTests
    {
        public DynamicEntityProviderTests()
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
        public async Task DynamicEntityProvider_Should_Return_QuoteEntity_When_EntityType_Is_Quote()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""quote"",
                               ""entityId"" : """ + quoteId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            var model = new QuoteReadModelWithRelatedEntities() { Quote = new FakeNewQuoteReadModel(quoteId) };
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository))).Returns(mockQuoteRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Quote));

            var quoteEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Quote;
            quoteEntity.Id.Should().Be(quoteId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_PolicyEntity_When_EntityType_Is_Policy()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""policy"",
                               ""entityId"" : """ + policyId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();

            var model = new PolicyReadModelWithRelatedEntities() { Policy = new FakePolicyReadModel(TenantFactory.DefaultId, policyId) };
            mockPolicyRepository
                .Setup(c => c.GetPolicyWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IPolicyReadModelRepository))).Returns(mockPolicyRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Policy));

            var policyEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Policy;
            policyEntity.Id.Should().Be(policyId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_PolicyTransactionEntity_When_EntityType_Is_PolicyTransaction()
        {
            // Arrange
            var policyTransactionId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""policyTransaction"",
                               ""entityId"" : """ + policyTransactionId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
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
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.PolicyTransaction));

            var polTransactionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.PolicyTransaction;
            polTransactionEntity.Id.Should().Be(policyTransactionId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_TenantEntity_When_EntityType_Is_Tenant()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""tenant"",
                               ""entityId"" : """ + tenantId + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();
            var tenant = TenantFactory.Create(tenantId);
            var model = new TenantWithRelatedEntities() { Tenant = tenant };
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockTenantRepository.Setup(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(model);
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(model.Tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(model.Tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(model.Tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(model.Tenant));
            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ITenantRepository))).Returns(mockTenantRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Tenant));

            var tenantEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Tenant;
            tenantEntity.Id.Should().Be(tenantId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_ProductEntity_When_EntityType_Is_Product()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""product"",
                               ""entityId"" : """ + productId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockProductRepository = new Mock<IProductRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();
            var tenant = TenantFactory.Create(TenantFactory.DefaultId);
            var product = new Product(TenantFactory.DefaultId, productId, "product name", productId.ToString(), NodaTime.SystemClock.Instance.GetCurrentInstant());
            var model = new ProductWithRelatedEntities() { Product = product };
            var mockTenantRepository = new Mock<ITenantRepository>();
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(model);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IProductRepository))).Returns(mockProductRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ITenantRepository))).Returns(mockTenantRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(tenant.Id, productId: product.Id);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Product));

            var productEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Product;
            productEntity.Id.Should().Be(product.Id.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_CustomerEntity_When_EntityType_Is_Customer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""customer"",
                               ""entityId"" : """ + customerId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();

            var personReadModel = new PersonReadModel(Guid.NewGuid());
            var deploymentEnvironment = DeploymentEnvironment.Staging;
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var customer = new CustomerReadModel(customerId, personReadModel, deploymentEnvironment, null, currentInstant, false);
            customer.People = new Collection<PersonReadModel> { personReadModel };
            var model = new CustomerReadModelWithRelatedEntities() { Customer = customer };
            mockCustomerReadModelRepository
                .Setup(c => c.GetCustomerWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(ICustomerReadModelRepository))).Returns(mockCustomerReadModelRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Customer));

            var customerEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Customer;
            customerEntity.Id.Should().Be(customerId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_UserEntity_When_EntityType_Is_User()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""user"",
                               ""entityId"" : """ + userId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();

            var model = new UserReadModelWithRelatedEntities() { User = new FakeUserReadModel(userId) };
            mockUserReadModelRepository
                .Setup(c => c.GetUserWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IUserReadModelRepository))).Returns(mockUserReadModelRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.User));

            var userEntity = entityObject.DataValue as SerialisedEntitySchemaObject.User;
            userEntity.Id.Should().Be(userId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_EmailEntity_When_EntityType_Is_Email()
        {
            // Arrange
            var emailId = Guid.NewGuid();
            var e = "xxx+1@email.com";
            var el = new List<string>() { e };
            var json = @"{ 
                               ""entityType"" : ""emailMessage"",
                               ""entityId"" : """ + emailId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
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
            mockServiceProvider.Setup(c => c.GetService(typeof(IEmailRepository))).Returns(mockEmailRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.EmailMessage));

            var emailEntity = entityObject.DataValue as SerialisedEntitySchemaObject.EmailMessage;
            emailEntity.Id.Should().Be(emailId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_DocumentEntity_When_EntityType_Is_Document()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""document"",
                               ""entityId"" : """ + documentId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();

            var model = new DocumentReadModelWithRelatedEntities() { Document = new FakeQuoteDocumentReadModel(documentId) };
            mockDocumentRepository
                .Setup(c => c.GetDocumentWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockConfiguration = new Mock<IEmailInvitationConfiguration>();
            mockConfiguration.Setup(c => c.InvitationLinkHost).Returns("https://localhost:4366");

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteDocumentReadModelRepository))).Returns(mockDocumentRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(IEmailInvitationConfiguration))).Returns(mockConfiguration.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Document));

            var documentEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Document;
            documentEntity.Id.Should().Be(documentId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_ClaimEntity_When_EntityType_Is_Claim()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""claim"",
                               ""entityId"" : """ + claimId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();

            var model = new ClaimReadModelWithRelatedEntities() { Claim = new Mock<ClaimReadModel>(claimId).Object };
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimReadModelRepository))).Returns(mockClaimRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Claim));

            var claimEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Claim;
            claimEntity.Id.Should().Be(claimId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_OrganisationEntity_When_EntityType_Is_Organisation()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""organisation"",
                               ""entityId"" : """ + organisationId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
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
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Organisation));

            var organisationEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Organisation;
            organisationEntity.Id.Should().Be(organisationId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_QuoteVersionEntity_When_EntityType_Is_QuoteVersion()
        {
            // Arrange
            var quoteVersionId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""quoteVersion"",
                               ""entityId"" : """ + quoteVersionId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();

            var model = new QuoteVersionReadModelWithRelatedEntities() { QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = quoteVersionId } };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteVersionReadModelRepository))).Returns(mockQuoteVersionRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.QuoteVersion));

            var quoteVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.QuoteVersion;
            quoteVersionEntity.Id.Should().Be(quoteVersionId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_ClaimVersionEntity_When_EntityType_Is_ClaimVersion()
        {
            // Arrange
            var claimVersionId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""claimVersion"",
                               ""entityId"" : """ + claimVersionId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();

            var model = new ClaimVersionReadModelWithRelatedEntities() { ClaimVersion = new ClaimVersionReadModel() { Id = claimVersionId } };
            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IClaimVersionReadModelRepository))).Returns(mockClaimVersionRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.ClaimVersion));

            var claimVersionEntity = entityObject.DataValue as SerialisedEntitySchemaObject.ClaimVersion;
            claimVersionEntity.Id.Should().Be(claimVersionId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Throw_EntityType_Is_Not_Supported()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""quoteXXX"",
                               ""entityId"" : """ + quoteId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();

            var model = new Mock<IQuoteReadModelWithRelatedEntities>();
            model.Setup(c => c.Quote).Returns(new FakeNewQuoteReadModel(quoteId));
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(model.Object);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository))).Returns(mockQuoteRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await dynamicEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Throw_When_Entity_Does_Not_Exists()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var json = @"{ 
                              ""entityType"" : ""quote"",
                              ""entityId"" : """ + quoteId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(default(IQuoteReadModelWithRelatedEntities));

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IQuoteReadModelRepository))).Returns(mockQuoteRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await dynamicEntityProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Cannot resolve entity reference");
            exception.Which.Error.AdditionalDetails.Should().Contain("Entity Type: Quote");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Quote ID: {quoteId}");
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_ReportEntity_When_EntityType_Is_Report()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var json = @"{ 
                               ""entityType"" : ""report"",
                               ""entityId"" : """ + reportId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockReportRepository = new Mock<IReportReadModelRepository>();

            var model = new FakeReportReadModel(reportId);
            mockReportRepository.Setup(c => c.SingleOrDefaultIncludeAllProperties(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(model);

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IReportReadModelRepository))).Returns(mockReportRepository.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Report));
            var reportEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Report;
            reportEntity.Id.Should().Be(reportId.ToString());
        }

        [Fact]
        public async Task DynamicEntityProvider_Should_Return_RoleEntity_When_EntityType_Is_Role()
        {
            // Arrange
            var fakeRole = RoleHelper.CreateTenantAdminRole(TenantFactory.DefaultId, Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());
            var roleId = fakeRole.Id;
            var json = @"{ 
                               ""entityType"" : ""role"",
                               ""entityId"" : """ + roleId.ToString() + @"""
                         }";

            var dynamicEntityProviderBuilder = JsonConvert.DeserializeObject<DynamicEntityProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockRoleRepository = new Mock<IRoleRepository>();

            mockRoleRepository.Setup(c => c.GetRoleById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(fakeRole);

            var tenant = TenantFactory.Create(TenantFactory.DefaultId);
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));

            var mockServiceProvider = await this.GetServiceProvider();
            mockServiceProvider.Setup(c => c.GetService(typeof(IRoleRepository))).Returns(mockRoleRepository.Object);
            mockServiceProvider.Setup(c => c.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            var dynamicEntityProvider = dynamicEntityProviderBuilder.Build(mockServiceProvider.Object);

            // Act
            var entityObject = (await dynamicEntityProvider.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            entityObject.DataValue.GetType().Should().Be(typeof(SerialisedEntitySchemaObject.Role));

            var roleEntity = entityObject.DataValue as SerialisedEntitySchemaObject.Role;
            roleEntity.Id.Should().Be(roleId.ToString());
        }

        private async Task<Mock<IServiceProvider>> GetServiceProvider()
        {
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var cachingResolver = new Mock<ICachingResolver>();
            cachingResolver.Setup(x => x.GetTenantOrThrow(automationData.ContextManager.Tenant.Id))
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
