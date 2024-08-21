// <copyright file="EntityObjectListProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
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
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence;
    using Xunit;

    public class EntityObjectListProviderTests
    {
        private Mock<ISystemEventRepository> mockEventRepository;
        private Mock<IOrganisationReadModelRepository> mockOrganisationRepository;
        private Mock<IPolicyTransactionReadModelRepository> mockPolicyTransactionRepository;
        private Mock<IPolicyReadModelRepository> mockPolicyRepository;
        private Mock<IEmailRepository> mockEmailRepository = new Mock<IEmailRepository>();
        private Mock<ISmsRepository> mockSmsRepository = new Mock<ISmsRepository>();
        private Mock<IQuoteDocumentReadModelRepository> mockDocumentRepository;
        private Mock<ICustomerReadModelRepository> mockCustomerReadModelRepository;
        private Mock<IProductRepository> mockProductRepository;
        private Mock<ITenantRepository> mockTenantRepository;
        private Mock<IUserReadModelRepository> mockUserReadModelRepository;
        private Mock<IQuoteVersionReadModelRepository> mockQuoteVersionRepository;
        private Mock<IQuoteReadModelRepository> mockQuoteRepository;
        private Mock<IClaimVersionReadModelRepository> mockClaimVersionRepository;
        private Mock<ICachingResolver> mockCachingResolver = new Mock<ICachingResolver>();
        private Mock<IClaimReadModelRepository> mockClaimRepository;
        private Mock<IPortalReadModelRepository> mockPortalRepository;

        public EntityObjectListProviderTests()
        {
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);
        }

        [Theory]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("user")]
        [InlineData("policy")]
        [InlineData("policyTransaction")]
        [InlineData("emailMessage")]
        [InlineData("document")]
        [InlineData("quote")]
        [InlineData("quoteVersion")]
        [InlineData("claim")]
        [InlineData("claimVersion")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList(string entityType)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": {
                                        ""objectPathLookupText"": ""/trigger/httpRequest/getParameters/entityType""
                                    }
                                }
                            }
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var mockServiceProvider = this.GetServiceProvider(entityId);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(queryParams: $"?entityType={entityType}");
            var providerContext = new ProviderContext(automationData);

            // Act
            var resolveEntityObjectList = await entityObjectListProvider.Resolve(providerContext);
            var entityObjectList = resolveEntityObjectList.GetValueOrThrowIfFailed().ToList();

            // Assert
            entityObjectList.Should().NotBeNull();
            entityObjectList.Should().HaveCount(1);

            DataObjectHelper.GetPropertyValueOrThrow(entityObjectList[0], "id").ToString()
                .Should().Be(entityId.ToString());
        }

        [Fact]
        public async Task EntityObjectListProvider_Should_Not_Load_Quote_Versions_Unless_Specified()
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""quote""
                                },
                            },
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.QuotesIncludedProperties.Should().NotContain("versions");
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("policy")]
        [InlineData("policyTransaction")]
        [InlineData("emails")]
        [InlineData("documents")]
        [InlineData("versions")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_Quote_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""quote""
                                }
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.QuotesIncludedProperties.Should().Contain(relatedEntity);
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("quote")]
        [InlineData("emails")]
        [InlineData("documents")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_QuoteVersion_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""quoteVersion""
                                },
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.QuoteVersionsIncludedProperties.Should().Contain(relatedEntity);
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("policy")]
        [InlineData("emails")]
        [InlineData("documents")]
        [InlineData("versions")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_Claim_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""claim""
                                }
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder
                = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.ClaimsIncludedProperties.Should().Contain(relatedEntity);
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("claim")]
        [InlineData("policy")]
        [InlineData("emails")]
        [InlineData("documents")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_ClaimVersion_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""claimVersion""
                                },
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.ClaimVersionsIncludedProperties.Should().Contain(relatedEntity);
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("policyTransactions")]
        [InlineData("claims")]
        [InlineData("emails")]
        [InlineData("documents")]
        [InlineData("quotes")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_Policy_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""policy""
                                },
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.PoliciesIncludedProperties.Should().Contain(relatedEntity);
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("product")]
        [InlineData("customer")]
        [InlineData("owner")]
        [InlineData("policy")]
        [InlineData("quote")]
        [InlineData("emails")]
        [InlineData("documents")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_PolicyTransaction_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""policyTransaction""
                                },
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.PolicyTransactionsIncludedProperties.Should().Contain(relatedEntity);
        }

        [Theory]
        [InlineData("product", "tenant")]
        [InlineData("tenant", "defaultOrganisation")]
        [InlineData("organisation", "tenant")]
        [InlineData("document", "tenant")]
        [InlineData("document", "organisation")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_OtherEntities_And_IncludeRelatedEntities(string entityType, string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": {
                                        ""objectPathLookupText"": ""/trigger/httpRequest/getParameters/entityType""
                                    }
                                },
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();
            var tenant = new Tenant(
                Tenant.MasterTenantId,
                entityId.ToString(),
                entityId.ToString(),
                null,
                default,
                default,
                SystemClock.Instance.GetCurrentInstant());
            var mockServiceProvider = this.GetServiceProvider(entityId);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));

            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(queryParams: $"?entityType={entityType}");
            var providerContext = new ProviderContext(automationData);

            // Act
            var resolveEntityObjectList = await entityObjectListProvider.Resolve(providerContext);
            var entityObjectList = resolveEntityObjectList.GetValueOrThrowIfFailed().ToList();

            // Assert
            entityObjectList.Should().NotBeNull();
            entityObjectList.Should().HaveCount(1);

            DataObjectHelper.GetPropertyValueOrThrow(entityObjectList[0], "id").ToString()
                .Should().Be(entityId.ToString());

            DataObjectHelper.GetPropertyValueOrThrow(entityObjectList[0], relatedEntity)
                .Should().NotBeNull();
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("tags")]
        [InlineData("relationships")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_Email_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""emailMessage""
                                },
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.EmailsIncludedProperties.Should().Contain(relatedEntity);
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("person")]
        [InlineData("roles")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_User_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""user""
                                },
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.UsersIncludedProperties.Should().Contain(relatedEntity);
        }

        [Theory]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("owner")]
        [InlineData("people")]
        [InlineData("quotes")]
        [InlineData("policies")]
        [InlineData("policyTransactions")]
        [InlineData("claims")]
        [InlineData("emails")]
        [InlineData("documents")]
        public async Task EntityObjectListProvider_Should_Accept_EntityList_With_Customer_And_IncludeRelatedEntities(string relatedEntity)
        {
            // Arrange
            var json = @"{
                            ""entityList"": {
                                ""entityQueryList"": {
                                    ""entityType"": ""customer""
                                },
                            },
                            ""includeOptionalProperties"" : [""/" + relatedEntity + @"""]
                         }";

            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var entityObjectListProviderBuilder = JsonConvert.DeserializeObject<EntityObjectListProviderConfigModel>(json, converters);
            var entityId = Guid.NewGuid();

            var fakeEntityQueryService = new FakeEntityQueryService();
            var mockServiceProvider = this.GetServiceProvider(entityId, fakeEntityQueryService);
            var entityObjectListProvider = entityObjectListProviderBuilder.Build(mockServiceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            var entityObjectList = await entityObjectListProvider.Resolve(providerContext);

            // Assert
            fakeEntityQueryService.CustomersIncludedProperties.Should().Contain(relatedEntity);
        }

        private IServiceProvider GetServiceProvider(Guid entityId, IEntityQueryService entityQueryService = null)
        {
            var clock = SystemClock.Instance;
            var tenant = new Tenant(entityId, entityId.ToString(), entityId.ToString(), null, default, default, clock.Now());
            var tenantDetail = new TenantDetails("arthur", "arthur", null, false, false, default, default, clock.Now());
            var product = new Product(tenant.Id, entityId, "arthur", "product_alias", clock.Now());
            var productDetail = new ProductDetails("arthur", "arthur", false, false, clock.Now());
            var organisation = new OrganisationReadModel(tenant.Id, entityId, "orgAlias", "orgName", null, true, false, clock.Now());

            var claim = new Mock<ClaimReadModel>(entityId).Object;
            var claimVersion = new ClaimVersionReadModel() { Id = entityId, ClaimId = Guid.NewGuid(), CreatedTimestamp = clock.Now() };

            var policy = new FakePolicyReadModel(tenant.Id, entityId);
            var policyTransaction = new FakePolicyTransactionReadModel(TenantFactory.DefaultId, entityId);

            var quote = new FakeNewQuoteReadModel(entityId);
            var quoteVersion = new QuoteVersionReadModel() { QuoteVersionId = entityId, CreatedTimestamp = clock.Now() };

            var person = new FakePersonalDetails() { MobilePhoneNumber = "04 12345678", WorkPhoneNumber = "04 12345678", HomePhoneNumber = "04 12345678", AlternativeEmail = "x@e.com", Email = "a@b.com", PreferredName = "ABC", FullName = "ABC" };
            var user = new FakeUserReadModel(entityId);
            var customer = new FakeCustomerReadModel(entityId);
            var quoteDocument = new FakeQuoteDocumentReadModel(entityId);

            var claimFileAttachment = new ClaimFileAttachment("name", "type", 1234, Guid.NewGuid(), clock.Now());
            var claimDocument = ClaimAttachmentReadModel.CreateClaimAttachmentReadModel(entityId, claimFileAttachment);
            var claimVersionDocument = ClaimAttachmentReadModel.CreateClaimVersionAttachmentReadModel(entityId, entityId, claimFileAttachment);

            var relationShip = new UBind.Domain.ReadWriteModel.Relationship(tenant.Id, EntityType.Message, entityId, RelationshipType.QuoteMessage, EntityType.Quote, entityId, clock.Now());
            var tag = new UBind.Domain.ReadWriteModel.Tag(EntityType.Message, Guid.NewGuid(), TagType.EmailType, "value", clock.Now());
            var emailAddress = "xxx+1@email.com";
            var emailAddressList = new List<string>() { emailAddress };
            var email = new UBind.Domain.ReadWriteModel.Email.Email(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                entityId,
                emailAddressList,
                emailAddress,
                emailAddressList,
                emailAddressList,
                emailAddressList,
                "test",
                "test",
                "test",
                null,
                clock.Now());

            var sms = new Domain.ReadWriteModel.Sms();

            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            this.mockCachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            this.mockCachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            this.mockCachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            this.mockCachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            var mockMediator = new Mock<ICqrsMediator>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ISerialisedEntityFactory>(c => new SerialisedEntityFactory(
                mockUrlConfiguration.Object,
                mockProductConfigProvider.Object,
                mockFormDataPrettifier.Object,
                this.mockCachingResolver.Object,
                mockMediator.Object,
                new DefaultPolicyTransactionTimeOfDayScheme()));
            serviceCollection.AddScoped(c => this.mockCachingResolver.Object);
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
            var claimList = new List<ClaimReadModelWithRelatedEntities>() { claimModel };
            this.mockClaimRepository
                .Setup(c => c.CreateQueryForClaimDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(claimList.AsQueryable());
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
            var claimVersionList = new List<ClaimVersionReadModelWithRelatedEntities>() { claimVersionModel };
            this.mockClaimVersionRepository
                .Setup(c => c.CreateQueryForClaimVersionDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>())).Returns(claimVersionList.AsQueryable);
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
            var quoteList = new List<QuoteReadModelWithRelatedEntities>() { quoteModel };
            this.mockQuoteRepository
                .Setup(c => c.CreateQueryForQuoteDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(quoteList.AsQueryable());
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
            var quoteVersionList = new List<QuoteVersionReadModelWithRelatedEntities>() { quoteVersionModel };
            this.mockQuoteVersionRepository
                .Setup(c => c.CreateQueryForQuoteDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(quoteVersionList.AsQueryable());
            serviceCollection.AddScoped(c => this.mockQuoteVersionRepository.Object);

            this.mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
            var userModel = new UserReadModelWithRelatedEntities();
            userModel.User = user;
            var personId = Guid.NewGuid();
            var personData = new Domain.Aggregates.Person.PersonData(person, personId);
            userModel.Person = PersonReadModel.CreateFromPersonData(personData.TenantId, personId, customer.Id, user.Id, personData, clock.GetCurrentInstant());
            userModel.Roles = new List<Role>() { new Role(TenantFactory.DefaultId, Guid.NewGuid(), Domain.Permissions.DefaultRole.Customer, NodaTime.SystemClock.Instance.GetCurrentInstant()) };
            userModel.Tenant = tenant;
            userModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            userModel.Organisation = organisation;
            var userList = new List<UserReadModelWithRelatedEntities>() { userModel };
            this.mockUserReadModelRepository
                .Setup(c => c.CreateQueryForUserDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(userList.AsQueryable());
            serviceCollection.AddScoped(c => this.mockUserReadModelRepository.Object);

            this.mockTenantRepository = new Mock<ITenantRepository>();
            var tenantModel = new TenantWithRelatedEntities();
            tenantModel.Products = new List<Product>() { product };
            tenantModel.Tenant = tenant;
            tenantModel.DefaultOrganisation = organisation;
            var tenantList = new List<TenantWithRelatedEntities>() { tenantModel };
            this.mockTenantRepository
                .Setup(c => c.CreateQueryForTenantWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(tenantList.AsQueryable());
            serviceCollection.AddScoped(c => this.mockTenantRepository.Object);

            this.mockProductRepository = new Mock<IProductRepository>();
            var productModel = new ProductWithRelatedEntities();
            productModel.Product = product;
            productModel.Tenant = tenant;
            var productList = new List<ProductWithRelatedEntities>() { productModel };
            this.mockProductRepository
                .Setup(c => c.CreateQueryForProductDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(productList.AsQueryable());
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
            customerModel.PolicyTransactions = new List<PolicyTransaction>() { policyTransaction };
            customerModel.Owner = user;
            customerModel.People = new List<PersonReadModel>() { userModel.Person };
            customerModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            customerModel.QuoteDocuments = new List<QuoteDocumentReadModel>() { quoteDocument };
            customerModel.ClaimDocuments = new List<ClaimAttachmentReadModel>() { claimDocument };
            var customerList = new List<CustomerReadModelWithRelatedEntities>() { customerModel };
            this.mockCustomerReadModelRepository
                .Setup(c => c.CreateQueryForCustomerDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(customerList.AsQueryable());
            serviceCollection.AddScoped(c => this.mockCustomerReadModelRepository.Object);

            this.mockDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();
            var documentModel = new DocumentReadModelWithRelatedEntities();
            documentModel.Document = quoteDocument;
            documentModel.Tenant = tenant;
            documentModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            documentModel.Organisation = organisation;
            var documentList = new List<DocumentReadModelWithRelatedEntities>() { documentModel };
            this.mockDocumentRepository
                .Setup(c => c.CreateQueryForDocumentDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(documentList.AsQueryable());
            var mockConfiguration = new Mock<IEmailInvitationConfiguration>();
            mockConfiguration.Setup(c => c.InvitationLinkHost).Returns("https://localhost:4366");
            serviceCollection.AddScoped(c => this.mockDocumentRepository.Object);
            serviceCollection.AddScoped(c => mockConfiguration.Object);

            this.mockEmailRepository = new Mock<IEmailRepository>();
            var emailModel = new EmailReadModelWithRelatedEntities();
            emailModel.Email = email;
            emailModel.ToRelationships = new List<UBind.Domain.ReadWriteModel.Relationship>() { relationShip };
            emailModel.FromRelationships = new List<UBind.Domain.ReadWriteModel.Relationship>() { relationShip };
            emailModel.Tags = new List<UBind.Domain.ReadWriteModel.Tag>() { tag };
            emailModel.Tenant = tenant;
            emailModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            emailModel.Organisation = organisation;
            var emailList = new List<EmailReadModelWithRelatedEntities>() { emailModel };
            this.mockEmailRepository
                .Setup(c => c.CreateQueryForEmailDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(emailList.AsQueryable());
            serviceCollection.AddScoped(c => this.mockEmailRepository.Object);

            this.mockSmsRepository = new Mock<ISmsRepository>();
            var smsModel = new SmsReadModelWithRelatedEntities();
            smsModel.Sms = sms;
            smsModel.ToRelationships = new List<UBind.Domain.ReadWriteModel.Relationship>() { relationShip };
            smsModel.FromRelationships = new List<UBind.Domain.ReadWriteModel.Relationship>() { relationShip };
            smsModel.Tags = new List<UBind.Domain.ReadWriteModel.Tag>() { tag };
            smsModel.Tenant = tenant;
            smsModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            smsModel.Organisation = organisation;
            var smsList = new List<SmsReadModelWithRelatedEntities>() { smsModel };
            this.mockSmsRepository
                .Setup(c => c.CreateQueryForSmsDetailsWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(smsList.AsQueryable());
            serviceCollection.AddScoped<ISmsRepository>(c => this.mockSmsRepository.Object);

            this.mockPolicyRepository = new Mock<IPolicyReadModelRepository>();
            var policyModel = new PolicyReadModelWithRelatedEntities();
            policyModel.Quotes = new List<NewQuoteReadModel>() { quote };
            policyModel.Product = product;
            policyModel.ProductDetails = new List<ProductDetails>() { productDetail };
            policyModel.Tenant = tenant;
            policyModel.TenantDetails = new List<TenantDetails>() { tenantDetail };
            policyModel.Organisation = organisation;
            policyModel.Policy = policy;
            policyModel.PolicyTransactions = new List<PolicyTransaction>() { policyTransaction };
            policyModel.Customer = customer;
            policyModel.Owner = user;
            policyModel.Emails = new List<UBind.Domain.ReadWriteModel.Email.Email>() { email };
            policyModel.QuoteDocuments = new List<QuoteDocumentReadModel>() { quoteDocument };
            policyModel.ClaimDocuments = new List<ClaimAttachmentReadModel>() { claimDocument };
            policyModel.Claims = new List<ClaimReadModel>() { claim };
            var policyList = new List<PolicyReadModelWithRelatedEntities>() { policyModel };
            this.mockPolicyRepository
                .Setup(c => c.CreateQueryForPolicyDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(policyList.AsQueryable());
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
            var polTransactionList = new List<PolicyTransactionReadModelWithRelatedEntities>() { polTransactionModel };
            this.mockPolicyTransactionRepository
                .Setup(c => c.CreateQueryForPolicyTransactionDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<List<string>>()))
                .Returns(polTransactionList.AsQueryable());
            serviceCollection.AddScoped(c => this.mockPolicyTransactionRepository.Object);

            this.mockEventRepository = new Mock<ISystemEventRepository>();
            serviceCollection.AddScoped(c => this.mockEventRepository.Object);

            this.mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();
            var orgModel = new OrganisationReadModelWithRelatedEntities();
            orgModel.Organisation = organisation;
            orgModel.Tenant = tenant;
            var orgList = new List<OrganisationReadModelWithRelatedEntities>() { orgModel };
            this.mockOrganisationRepository
                .Setup(c => c.CreateQueryForOrganisationDetailsWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(orgList.AsQueryable());
            serviceCollection.AddScoped(c => this.mockOrganisationRepository.Object);

            this.mockPortalRepository = new Mock<IPortalReadModelRepository>();
            var portalModel = new PortalWithRelatedEntities();
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
                OrganisationId = tenant.Details.DefaultOrganisationId,
            };

            portalModel.Tenant = tenant;
            var portalList = new List<PortalWithRelatedEntities>() { portalModel };
            this.mockPortalRepository
                .Setup(c => c.CreateQueryForPortalWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(portalList.AsQueryable());
            serviceCollection.AddScoped(c => this.mockPortalRepository.Object);

            var mockDbContext = new Mock<UBindDbContext>();
            serviceCollection.AddScoped<IClock>(c => new TestClock());
            if (entityQueryService != null)
            {
                serviceCollection.AddScoped(s => entityQueryService);
            }
            else
            {
                serviceCollection.AddScoped<IEntityQueryService, AutomationEntityQueryService>();
            }

            serviceCollection.AddScoped(c => mockDbContext.Object);
            return serviceCollection.BuildServiceProvider();
        }
    }
}
