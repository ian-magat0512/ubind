// <copyright file="MockAutomationData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using RazorEngine.Templating;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Queries.ProductOrganisation;
    using UBind.Application.Services;
    using UBind.Application.Services.Imports;
    using UBind.Application.SystemEvents.Payload;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Events;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Product;
    using UBind.Domain.Queries.AdditionalPropertyDefinition;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.ResourceModels;
    using StatusCode = System.Net.HttpStatusCode;

    /// <summary>
    /// Represents a mock automaton data context for testing.
    /// </summary>
    [SystemEventTypeExtensionInitialize]
    public class MockAutomationData : AutomationData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockAutomationData"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the automation belongs to.</param>
        private MockAutomationData(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            TriggerData triggerData)
            : base(tenantId, organisationId, productId, null, environment, triggerData, GetDefaultServiceProvider())
        {
        }

        public static async Task<MockAutomationData> CreateDataWithHttpTriggerAndContent(
            Guid? tenantId = null,
            Guid organisationId = default,
            string contentList = null)
        {
            tenantId = tenantId ?? TenantFactory.DefaultId;
            return await CreateWithHttpTrigger(tenantId: tenantId, organisationId: organisationId, withTriggerContent: true, contentList: contentList);
        }

        public static async Task<MockAutomationData> CreateWithHttpTrigger(
            Guid? tenantId = null,
            Guid organisationId = default,
            Guid? productId = null,
            DeploymentEnvironment environment = DeploymentEnvironment.Development,
            bool withTriggerContent = false,
            string queryParams = null,
            string contentList = null)
        {
            if (withTriggerContent)
            {
                var content = @"{
                        ""greeting"": ""Hi World!"",
                        ""fooNumber"": 18,
                        ""isTrue"": true,
                        ""isFalse"": false,
                        ""address"": ""Unit B 209 Lawton Dr. Peaches Drive"",
                        ""path"": ""trigger.httpRequest.url"",
                        ""index"": 0,
                        ""items"": [""a1"",""a2"",""a3""],
                        ""persons"": [
                        {
                            ""name"": ""B1"",
                            ""age"": 10,
                            ""items"": [""p1"",""p2"",""p3""]
                        },
                        {
                            ""name"": ""B2"",
                            ""age"": 12,
                            ""items"": [""x1"",""x2"",""x3""]
                        }]}";

                if (!string.IsNullOrEmpty(contentList))
                {
                    content = contentList;
                }

                return await CreateWithHttpTrigger(
                    content, "application/json", tenantId, organisationId, productId, environment, queryParams);
            }
            else
            {
                return await CreateWithHttpTrigger(
                    null, null, tenantId, organisationId, productId, environment, queryParams);
            }
        }

        public static async Task<MockAutomationData> CreateWithHttpTrigger(
            string? content,
            string? contentType,
            Guid? tenantId = null,
            Guid organisationId = default,
            Guid? productId = null,
            DeploymentEnvironment environment = DeploymentEnvironment.Development,
            string queryParams = null)
        {
            tenantId = tenantId ?? TenantFactory.DefaultId;
            productId = productId ?? ProductFactory.DefaultId;
            var httpContextBuilder = new FakeHttpContextBuilder()
                .WithRequestPath(@"/api/v1/tenant/carl/product/dev/environment/development/automations/addressMatch")
                .WithMethod("POST")
                .WithQueryString(string.IsNullOrEmpty(queryParams) ? "?address=34 Malibu Point" : queryParams);
            if (content != null)
            {
                httpContextBuilder = httpContextBuilder.WithHttpContent(content, contentType);
            }

            var httpContext = httpContextBuilder.Build();
            var automationRequest = new AutomationRequest(httpContext);
            var triggerRequest = await automationRequest.ToTriggerRequest("secretCode");
            var triggerData = new HttpTriggerData(triggerRequest);
            return new MockAutomationData(tenantId.Value, organisationId, productId.Value, environment, triggerData);
        }

        public static MockAutomationData CreateWithEventTrigger(
            Guid? tenantId = null,
            DeploymentEnvironment environment = DeploymentEnvironment.Development,
            string json = null)
        {
            tenantId = tenantId ?? TenantFactory.DefaultId;
            if (json != null)
            {
                var triggerData = JsonConvert.DeserializeObject<EventTriggerData>(json);
                return new MockAutomationData(
                    tenantId.Value, default, ProductFactory.DefaultId, environment, triggerData);
            }

            var productContext = new ProductContext(
                tenantId.Value,
                ProductFactory.DefaultId,
                environment);
            var systemEvent = SystemEvent.CreateWithPayload(
                productContext.TenantId,
                default,
                productContext.ProductId,
                productContext.Environment,
                SystemEventType.Custom,
                new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid()),
                default);
            var defaultTriggerData = new EventTriggerData(systemEvent);
            return new MockAutomationData(
                tenantId.Value, default, ProductFactory.DefaultId, environment, defaultTriggerData);
        }

        public static MockAutomationData CreateWithCustomEventTrigger(SystemEvent @event)
        {
            var triggerData = new EventTriggerData(@event);
            return new MockAutomationData(
                TenantFactory.DefaultId,
                default,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Production,
                triggerData);
        }

        public static async Task<MockAutomationData> CreateWithHttpActionBinaryData(byte[] byteData, string alias = "myHttpRequest")
        {
            var automationData = CreateWithEventTrigger();
            var httpRequestAction = new HttpRequestAction("test", "test", "test", false, null, null, null, null, null, new TestClock());
            var actionData = new HttpRequestActionData(alias, alias, new TestClock());
            var httpResponseMessage = new HttpResponseMessage(StatusCode.OK);
            var byteContent = new ByteArrayContent(byteData);
            byteContent.Headers.Add("Content-Type", "application/octet-stream");
            httpResponseMessage.Content = byteContent;
            await httpRequestAction.ProcessResponse(httpResponseMessage, actionData);
            automationData.AddActionData(actionData);
            return automationData;
        }

        public static async Task<MockAutomationData> CreateWithHttpActionStringData(string stringData, string alias = "myHttpRequest")
        {
            var automationData = CreateWithEventTrigger();
            var httpRequestAction = new HttpRequestAction("test", "test", "test", false, null, null, null, null, null, new TestClock());
            var actionData = new HttpRequestActionData(alias, alias, new TestClock());
            var httpResponseMessage = new HttpResponseMessage(StatusCode.OK);
            var stringContent = new StringContent(stringData);
            httpResponseMessage.Content = stringContent;
            await httpRequestAction.ProcessResponse(httpResponseMessage, actionData);
            automationData.AddActionData(actionData);
            return automationData;
        }

        public static async Task<MockAutomationData> CreateWithHttpActionMultipartData(
            string stringData,
            byte[] byteData,
            string alias = "myHttpRequest")
        {
            var automationData = CreateWithEventTrigger();
            var httpRequestAction = new HttpRequestAction("test", "test", "test", false, null, null, null, null, null, new TestClock());
            var actionData = new HttpRequestActionData(alias, alias, new TestClock());
            var httpResponseMessage = new HttpResponseMessage(StatusCode.OK);
            var byteContent = new ByteArrayContent(byteData);
            var stringContent = new StringContent(stringData);
            var multipartContent = new MultipartContent();
            multipartContent.Add(stringContent);
            multipartContent.Add(byteContent);
            httpResponseMessage.Content = multipartContent;
            await httpRequestAction.ProcessResponse(httpResponseMessage, actionData);
            automationData.AddActionData(actionData);
            return automationData;
        }

        public static MockAutomationData CreateWithHttpRequestActionDataBinaryContent(
            byte[] byteData,
            string alias = "myHttpRequest")
        {
            var automationData = CreateWithEventTrigger();
            var actionData = new HttpRequestActionData(alias, alias, new TestClock());
            actionData.HttpRequest = new UBind.Application.Automation.Http.Request(
                "https://foo.bar/jane",
                HttpMethod.Get.ToString(),
                null,
                "application/octet-stream",
                null,
                byteData,
                null);
            automationData.AddActionData(actionData);
            return automationData;
        }

        public static MockAutomationData CreateWithPortalPageTrigger(Guid? tenantId = null, string json = null)
        {
            if (json != null)
            {
                var jsonTriggerData = JsonConvert.DeserializeObject<EventTriggerData>(json);
                return new MockAutomationData(
                    tenantId.Value, default, ProductFactory.DefaultId, DeploymentEnvironment.Development, jsonTriggerData);
            }

            var triggerData = new PortalPageTriggerData("testPortalPageTrigger", EntityType.Quote, PageType.List, "Details");
            return new MockAutomationData(
                tenantId ?? TenantFactory.DefaultId, default, ProductFactory.DefaultId, DeploymentEnvironment.Development, triggerData);
        }

        public static IServiceProvider GetDefaultServiceProvider()
        {
            return MockAutomationData.GetDefaultServiceCollection().BuildServiceProvider();
        }

        public static IServiceProvider GetServiceProviderForAdditionalProperties(Guid entityId, string entityType)
        {
            var serviceCollection = MockAutomationData.GetDefaultServiceCollection();
            var mockAddPropertyService = new Mock<IAdditionalPropertyValueService>();
            var propertyDefinition = new AdditionalPropertyDefinitionDto
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Alias = "test",
                IsRequired = true,
                IsUnique = false,
                PropertyType = Domain.Enums.AdditionalPropertyDefinitionType.Text,
            };
            Enum.TryParse(entityType, true, out AdditionalPropertyEntityType addPropEntityType);
            propertyDefinition.EntityType = addPropEntityType;
            var property = new AdditionalPropertyValueDto()
            {
                AdditionalPropertyDefinition = propertyDefinition,
                EntityId = entityId,
                Id = Guid.NewGuid(),
                Value = string.Empty,
            };
            var properties = new List<AdditionalPropertyValueDto>() { property };
            mockAddPropertyService.Setup(c => c.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                It.IsAny<Guid>(), It.IsAny<AdditionalPropertyEntityType>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(properties));
            var definition = new AdditionalPropertyDefinitionReadModel(
                TenantFactory.DefaultId,
                propertyDefinition.Id,
                default,
                propertyDefinition.Alias,
                propertyDefinition.Name,
                addPropEntityType,
                AdditionalPropertyDefinitionContextType.Tenant,
                TenantFactory.DefaultId,
                false,
                false,
                false,
                null,
                propertyDefinition.PropertyType,
                propertyDefinition.SchemaType);
            var mockAdditionalPropertyDefinitionRepository = new Mock<IAdditionalPropertyDefinitionRepository>();
            mockAdditionalPropertyDefinitionRepository
                .Setup(s => s.GetAdditionalPropertyDefinitionByEntityTypeAndPropertyAlias(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AdditionalPropertyEntityType>()))
                .Returns(definition);
            serviceCollection.AddScoped<IAdditionalPropertyValueService>(c => mockAddPropertyService.Object);
            serviceCollection.AddScoped<IAdditionalPropertyDefinitionRepository>(c => mockAdditionalPropertyDefinitionRepository.Object);
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(
                c => c.Send(It.IsAny<GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(definition));
            serviceCollection.AddScoped(c => mockMediator.Object);
            return MockAutomationData.GetServiceProviderForEntityProviders(entityId, false, serviceCollection);
        }

        public static IServiceProvider GetServiceProviderForEntityProviders(Guid entityId, bool withDocument, ServiceCollection serviceCollection = null)
        {
            if (serviceCollection == null)
            {
                serviceCollection = MockAutomationData.GetDefaultServiceCollection();
            }

            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");

            var mockMediator = new Mock<ICqrsMediator>();
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockcachingResolver = new Mock<ICachingResolver>();
            var tenantModel = new TenantWithRelatedEntities();
            var tenant = new Domain.Tenant(entityId, "arthur", "arthur", null, default, default, SystemClock.Instance.GetCurrentInstant());
            tenantModel.Tenant = tenant;
            mockTenantRepository.Setup(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(tenantModel);
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            serviceCollection.AddScoped<ITenantRepository>(c => mockTenantRepository.Object);

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            serviceCollection.AddScoped<ISerialisedEntityFactory>(c => new SerialisedEntityFactory(
                mockUrlConfiguration.Object,
                mockProductConfigProvider.Object,
                mockFormDataPrettifier.Object,
                mockcachingResolver.Object,
                mockMediator.Object,
                new DefaultPolicyTransactionTimeOfDayScheme()));

            var quoteDocument = new UBind.Domain.Aggregates.Quote.QuoteDocument("brown_fox.txt", "txt", 100, entityId, NodaTime.SystemClock.Instance.GetCurrentInstant());
            var quoteDocumentReadModel = QuoteDocumentReadModel.CreateQuoteDocumentReadModel(entityId, entityId, quoteDocument);
            var quoteVersionDocumentReadModel = QuoteDocumentReadModel.CreateQuoteVersionDocumentReadModel(entityId, entityId, quoteDocument);
            var policyTransactionDocumentReadModel = QuoteDocumentReadModel.CreatePolicyDocumentReadModel(entityId, entityId, quoteDocument);

            var claimFileAttachment = new ClaimFileAttachment("brown_fox.txt", "txt", 100, entityId, NodaTime.SystemClock.Instance.GetCurrentInstant());
            var claimDocument = ClaimAttachmentReadModel.CreateClaimAttachmentReadModel(entityId, claimFileAttachment);
            var claimVersionDocument = ClaimAttachmentReadModel.CreateClaimVersionAttachmentReadModel(entityId, entityId, claimFileAttachment);

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();
            var claimModel = new ClaimReadModelWithRelatedEntities();
            claimModel.Claim = new Mock<ClaimReadModel>(entityId).Object;
            if (withDocument)
            {
                claimModel.Documents = new List<ClaimAttachmentReadModel>() { claimDocument };
            }

            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(claimModel);
            serviceCollection.AddScoped<IClaimReadModelRepository>(c => mockClaimRepository.Object);

            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();
            var claimVersionModel = new ClaimVersionReadModelWithRelatedEntities();
            claimVersionModel.ClaimVersion = new ClaimVersionReadModel() { Id = entityId, ClaimId = Guid.NewGuid() };
            if (withDocument)
            {
                claimVersionModel.Documents = new List<ClaimAttachmentReadModel>() { claimVersionDocument };
            }

            mockClaimVersionRepository
                .Setup(c => c.GetClaimVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(claimVersionModel);
            serviceCollection.AddScoped(c => mockClaimVersionRepository.Object);

            var mockQuoteRepository = new Mock<IQuoteReadModelRepository>();
            var quoteModel = new QuoteReadModelWithRelatedEntities();
            quoteModel.Quote = new FakeNewQuoteReadModel(entityId);
            quoteModel.PolicyTransaction = new FakePolicyTransaction(tenant.Id, entityId);
            quoteModel.Policy = new FakePolicyReadModel(tenant.Id, entityId);
            if (withDocument)
            {
                quoteModel.Documents = new List<QuoteDocumentReadModel>() { quoteDocumentReadModel };
            }

            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(quoteModel);
            serviceCollection.AddScoped<IQuoteReadModelRepository>(c => mockQuoteRepository.Object);

            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();
            var quoteVersionModel = new QuoteVersionReadModelWithRelatedEntities();
            quoteVersionModel.QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = entityId };
            if (withDocument)
            {
                quoteVersionModel.Documents = new List<QuoteDocumentReadModel>() { quoteVersionDocumentReadModel };
            }

            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(quoteVersionModel);
            serviceCollection.AddScoped<IQuoteVersionReadModelRepository>(c => mockQuoteVersionRepository.Object);

            var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
            var userModel = new UserReadModelWithRelatedEntities();
            userModel.User = new FakeUserReadModel(entityId);
            mockUserReadModelRepository
                .Setup(c => c.GetUserWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(userModel);
            serviceCollection.AddScoped<IUserReadModelRepository>(c => mockUserReadModelRepository.Object);

            var mockProductRepository = new Mock<IProductRepository>();
            var product = new Domain.Product.Product(tenant.Id, entityId, "product_alias", entityId.ToString(), NodaTime.SystemClock.Instance.GetCurrentInstant());
            var productModel = new ProductWithRelatedEntities();
            productModel.Product = product;
            mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(productModel);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            serviceCollection.AddScoped<IProductRepository>(c => mockProductRepository.Object);
            serviceCollection.AddScoped<ICachingResolver>(x => mockcachingResolver.Object);

            var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();
            var personReadModel = new PersonReadModel(Guid.NewGuid());
            var deploymentEnvironment = DeploymentEnvironment.Staging;
            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var customer = new CustomerReadModel(entityId, personReadModel, deploymentEnvironment, null, currentInstant, false);
            customer.People = new Collection<PersonReadModel> { personReadModel };
            var customerModel = new CustomerReadModelWithRelatedEntities() { Customer = customer };
            mockCustomerReadModelRepository
                .Setup(c => c.GetCustomerWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(customerModel);
            serviceCollection.AddScoped<ICustomerReadModelRepository>(c => mockCustomerReadModelRepository.Object);

            var mockEmailRepository = new Mock<IEmailRepository>();
            var emailModel = new EmailReadModelWithRelatedEntities();
            var e = "xxx+1@email.com";
            var el = new List<string>() { e };
            var email = new UBind.Domain.ReadWriteModel.Email.Email(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                deploymentEnvironment,
                entityId,
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
            emailModel.Email = email;
            mockEmailRepository
                .Setup(c => c.GetEmailWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(emailModel);
            serviceCollection.AddScoped(c => mockEmailRepository.Object);

            var mockPolicyRepository = new Mock<IPolicyReadModelRepository>();
            var policyModel = new PolicyReadModelWithRelatedEntities();
            policyModel.Policy = new FakePolicyReadModel(tenant.Id, entityId);
            mockPolicyRepository
                .Setup(c => c.GetPolicyWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(policyModel);
            serviceCollection.AddScoped(c => mockPolicyRepository.Object);

            var mockPolicyTransactionRepository = new Mock<IPolicyTransactionReadModelRepository>();
            var polTransactionModel = new PolicyTransactionReadModelWithRelatedEntities();
            polTransactionModel.PolicyTransaction = new FakePolicyTransactionReadModel(tenant.Id, entityId);
            if (withDocument)
            {
                polTransactionModel.Documents = new List<QuoteDocumentReadModel>() { policyTransactionDocumentReadModel };
            }

            polTransactionModel.TimeZoneId = Timezones.AET.ToString();
            mockPolicyTransactionRepository
                .Setup(c => c.GetPolicyTransactionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(polTransactionModel);
            serviceCollection.AddScoped(c => mockPolicyTransactionRepository.Object);

            var mockOrganisationRepository = new Mock<IOrganisationReadModelRepository>();
            var orgModel = new OrganisationReadModelWithRelatedEntities();
            orgModel.Organisation = new OrganisationReadModel(TenantFactory.DefaultId, entityId, "orgAlias", "orgName", null, true, false, new TestClock().Timestamp);
            mockOrganisationRepository
                .Setup(c => c.GetOrganisationWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(orgModel);
            serviceCollection.AddScoped<IOrganisationReadModelRepository>(c => mockOrganisationRepository.Object);

            var fileContentRepository = new Mock<IFileContentRepository>();
            var content = Encoding.UTF8.GetBytes("This is a test file");
            var fileContent = FileContent.CreateFromBytes(tenant.Id, entityId, content);
            fileContentRepository.Setup(c => c.GetFileContentById(It.IsAny<Guid>())).Returns(fileContent);
            serviceCollection.AddScoped(c => fileContentRepository.Object);

            return serviceCollection.BuildServiceProvider();
        }

        public static IServiceProvider GetServiceProviderForCreateQuote(Guid tenantId, Guid productId, Guid organisationId, Domain.Aggregates.Quote.Quote? quote, TransactionType transactionType = TransactionType.NewBusiness)
        {
            var serviceCollection = GetDefaultServiceCollection();
            var mockApplicationQuoteService = new Mock<IApplicationQuoteService>();
            var mockProductFeatureSettingService = new Mock<IProductFeatureSettingService>();
            var mockQuoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            var mockPolicyReadModelRepository = new Mock<IPolicyReadModelRepository>();
            var mockCustomerReadModelRepository = new Mock<ICustomerReadModelRepository>();
            var mockHttpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
            var mockOrganisationReadModelRepository = new Mock<IOrganisationReadModelRepository>();
            var mockProductRepository = new Mock<IProductRepository>();

            var mockProductConfigurationProvider = new Mock<IProductConfigurationProvider>();
            var mockQuoteWorkflow = new Mock<IQuoteWorkflow>();
            mockQuoteWorkflow.Setup(x => x.IsResultingStateSupported(It.IsAny<string>())).Returns(true);
            var mockProductConfiguration = new Mock<IProductConfiguration>();
            mockProductConfiguration.Setup(x => x.QuoteWorkflow).Returns(mockQuoteWorkflow.Object);
            mockProductConfigurationProvider.Setup(x => x.GetProductConfiguration(It.IsAny<ReleaseContext>(), It.IsAny<WebFormAppType>()))
                .Returns(Task.FromResult(mockProductConfiguration.Object));
            mockHttpContextPropertiesResolver.Setup(c => c.PerformingUserId).Returns(Guid.NewGuid());

            var policyDetails = new Mock<IPolicyReadModelDetails>();
            if (quote?.Aggregate?.Policy != null)
            {
                policyDetails.Setup(c => c.PolicyId).Returns(quote.PolicyId);
                policyDetails.Setup(c => c.ProductId).Returns(quote.Aggregate.ProductId);
                policyDetails.Setup(c => c.OrganisationId).Returns(quote.Aggregate.OrganisationId);
                policyDetails.Setup(c => c.ExpiryTimestamp).Returns(quote.Aggregate.Policy.ExpiryTimestamp);
                policyDetails.Setup(c => c.Environment).Returns(quote.Aggregate.Environment);
                policyDetails.Setup(c => c.CustomerId).Returns(quote.Aggregate.CustomerId);
                policyDetails.Setup(c => c.CustomerPreferredName).Returns("Test Customer");
                policyDetails.Setup(c => c.PolicyNumber).Returns("P0001");
            }

            mockPolicyReadModelRepository
                .Setup(c => c.GetPolicyDetails(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(policyDetails.Object);

            mockPolicyReadModelRepository.Setup(x => x.GetPolicyDetails(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(policyDetails.Object);
            var policyWithRelatedEntities = new PolicyReadModelWithRelatedEntities();
            policyWithRelatedEntities.Policy = new FakePolicyReadModel(tenantId, quote?.Aggregate?.Policy?.PolicyId ?? default);
            mockPolicyReadModelRepository.Setup(x => x.GetPolicyWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(policyWithRelatedEntities);

            var customerDetail = new Mock<ICustomerReadModelSummary>();
            if (quote?.HasCustomer == true)
            {
                customerDetail.Setup(c => c.Id).Returns(quote.CustomerId.Value);

                var customer = new CustomerReadModelDetail();
                customer.Id = quote.CustomerId.Value;
                customer.OrganisationId = quote.Aggregate.OrganisationId;
                customer.TenantId = tenantId;
                customer.Environment = quote.Aggregate.Environment;
                customer.PreferredName = "Test Customer";
                mockCustomerReadModelRepository
                    .Setup(c => c.GetCustomerById(tenantId, quote.CustomerId.Value, false))
                    .Returns(customer);

                var differentCustomer = new CustomerReadModelDetail();
                differentCustomer.Id = Guid.NewGuid();
                differentCustomer.OrganisationId = quote.Aggregate.OrganisationId;
                differentCustomer.TenantId = tenantId;
                differentCustomer.Environment = quote.Aggregate.Environment;
                differentCustomer.PreferredName = "Test Customer";
                mockCustomerReadModelRepository
                    .Setup(c => c.GetCustomerById(tenantId, It.IsNotIn(quote.CustomerId.Value), It.IsAny<bool>()))
                    .Returns(differentCustomer);
                var customerReadModel = new FakeCustomerReadModel(Guid.NewGuid());
                var customerWithRelatedEntities = new CustomerReadModelWithRelatedEntities() { Customer = customerReadModel };
                mockCustomerReadModelRepository
                    .Setup(c => c.GetCustomerWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                    .Returns(customerWithRelatedEntities);
            }

            var mockAdditionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();
            var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
            var mockPolicyService = new Mock<IPolicyService>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var organisation = new OrganisationReadModel(
                tenantId, organisationId, "org-alias", "orgName", null, true, false, new TestClock().Timestamp);
            mockCachingResolver.Setup(x => x.GetOrganisationOrNull(tenantId, organisationId))
                .ReturnsAsync(organisation);
            var organisationWithRelatedEntities = new OrganisationReadModelWithRelatedEntities();
            organisationWithRelatedEntities.Organisation = organisation;
            mockOrganisationReadModelRepository.Setup(x => x.GetOrganisationWithRelatedEntities(tenantId, organisationId, It.IsAny<List<string>>()))
                .Returns(organisationWithRelatedEntities);

            var differentOrganisation = new OrganisationReadModel(tenantId, Guid.NewGuid(), "diff-org-alias", "diffOrgName", null, true, false, new TestClock().Timestamp);
            mockCachingResolver.Setup(x => x.GetOrganisationOrNull(tenantId, It.IsNotIn(organisationId)))
               .ReturnsAsync(differentOrganisation);
            organisationWithRelatedEntities = new OrganisationReadModelWithRelatedEntities();
            organisationWithRelatedEntities.Organisation = differentOrganisation;
            mockOrganisationReadModelRepository.Setup(x => x.GetOrganisationWithRelatedEntities(tenantId, It.IsNotIn(organisationId), It.IsAny<List<string>>()))
                .Returns(organisationWithRelatedEntities);

            mockCachingResolver.Setup(x => x.GetOrganisationOrThrow(tenantId, organisationId))
                .ReturnsAsync(organisation);
            mockCachingResolver.Setup(x => x.GetOrganisationOrThrow(tenantId, It.IsNotIn(organisationId)))
                .ReturnsAsync(differentOrganisation);

            var product = ProductFactory.Create(tenantId, productId);
            mockCachingResolver.Setup(x => x.GetProductOrNull(tenantId, productId))
               .Returns(Task.FromResult(product));
            mockCachingResolver.Setup(x => x.GetProductOrNull(tenantId, new GuidOrAlias(productId)))
               .Returns(Task.FromResult(product));
            mockCachingResolver.Setup(x => x.GetProductOrThrow(tenantId, productId))
               .Returns(Task.FromResult(product));
            var productWithRelatedEntities = new ProductWithRelatedEntities();
            productWithRelatedEntities.Product = product;
            mockProductRepository.Setup(x => x.GetProductWithRelatedEntities(tenantId, productId, It.IsAny<List<string>>()))
                .Returns(productWithRelatedEntities);

            var differentProduct = ProductFactory.Create(tenantId, Guid.NewGuid());
            mockCachingResolver.Setup(x => x.GetProductOrThrow(tenantId, It.IsNotIn(productId)))
              .Returns(Task.FromResult(differentProduct));
            mockCachingResolver.Setup(x => x.GetProductOrNull(tenantId, It.IsNotIn(new GuidOrAlias(productId))))
               .Returns(Task.FromResult(differentProduct));
            mockCachingResolver.Setup(x => x.GetProductOrNull(tenantId, It.IsNotIn(productId)))
             .Returns(Task.FromResult(differentProduct));
            productWithRelatedEntities = new ProductWithRelatedEntities();
            productWithRelatedEntities.Product = differentProduct;
            mockProductRepository.Setup(x => x.GetProductWithRelatedEntities(tenantId, It.IsNotIn(productId), It.IsAny<List<string>>()))
                .Returns(productWithRelatedEntities);

            var mockCqrsMediator = new Mock<ICqrsMediator>();
            var mockProductOrganisationSetting = new ProductOrganisationSetting(tenantId, organisationId, product.Id, true, new TestClock().Timestamp);
            mockCqrsMediator.Setup(x => x.Send(It.IsAny<GetProductOrganisationSettingQuery>(), CancellationToken.None))
                .Returns(Task.FromResult(mockProductOrganisationSetting));

            if (transactionType == TransactionType.NewBusiness && quote != null)
            {
                var quoteCreateEvent = quote.Aggregate.UnsavedEvents.OfType<QuoteAggregate.QuoteInitializedEvent>().FirstOrDefault();
                quote.SetReadModel(new NewQuoteReadModel(quoteCreateEvent));
                mockCqrsMediator.Setup(x => x.Send(It.IsAny<CreateNewBusinessQuoteCommand>(), CancellationToken.None))
                    .ReturnsAsync(quote?.ReadModel);
            }

            if (transactionType == TransactionType.Adjustment)
            {
                var adjustmentQuote = QuoteFactory.WithAdjustmentQuote(quote.Aggregate);
                var adjustmentQuoteCreatedEvent = adjustmentQuote?.Aggregate.UnsavedEvents.OfType<QuoteAggregate.AdjustmentQuoteCreatedEvent>().FirstOrDefault();
                adjustmentQuote.SetReadModel(new NewQuoteReadModel(quote.Aggregate, adjustmentQuoteCreatedEvent));
                mockCqrsMediator.Setup(x => x.Send(It.IsAny<CreateAdjustmentQuoteCommand>(), CancellationToken.None))
                    .ReturnsAsync(adjustmentQuote?.ReadModel);
            }

            if (transactionType == TransactionType.Renewal)
            {
                var renewalQuote = QuoteFactory.WithRenewalQuote(quote.Aggregate);
                var renewalQuoteCreatedEvent = renewalQuote.Aggregate.UnsavedEvents.OfType<QuoteAggregate.RenewalQuoteCreatedEvent>().FirstOrDefault();
                renewalQuote.SetReadModel(new NewQuoteReadModel(quote.Aggregate, renewalQuoteCreatedEvent));
                mockCqrsMediator.Setup(x => x.Send(It.IsAny<CreateRenewalQuoteCommand>(), CancellationToken.None))
                    .ReturnsAsync(renewalQuote?.ReadModel);
            }

            if (transactionType == TransactionType.Cancellation)
            {
                var cancellationQuote = QuoteFactory.WithCancellationQuote(quote.Aggregate);
                var cancellationQuoteCreatedEvent = cancellationQuote.Aggregate.UnsavedEvents.OfType<QuoteAggregate.CancellationQuoteCreatedEvent>().FirstOrDefault();
                cancellationQuote.SetReadModel(new NewQuoteReadModel(quote.Aggregate, cancellationQuoteCreatedEvent));
                mockCqrsMediator.Setup(x => x.Send(It.IsAny<CreateCancellationQuoteCommand>(), CancellationToken.None))
                    .ReturnsAsync(cancellationQuote?.ReadModel);
            }

            var mockClock = new Mock<IClock>();

            serviceCollection.AddScoped(c => mockApplicationQuoteService.Object);
            serviceCollection.AddScoped(c => mockProductFeatureSettingService.Object);
            serviceCollection.AddScoped(c => mockQuoteAggregateRepository.Object);
            serviceCollection.AddScoped(c => mockProductConfigurationProvider.Object);
            serviceCollection.AddScoped(c => mockAdditionalPropertyValueService.Object);
            serviceCollection.AddScoped(c => mockUserReadModelRepository.Object);
            serviceCollection.AddScoped(c => mockPolicyReadModelRepository.Object);
            serviceCollection.AddScoped(c => mockCustomerReadModelRepository.Object);
            serviceCollection.AddScoped(c => mockPolicyService.Object);
            serviceCollection.AddScoped(c => mockFormDataPrettifier.Object);
            serviceCollection.AddScoped(c => mockCachingResolver.Object);
            serviceCollection.AddScoped(c => mockCqrsMediator.Object);
            serviceCollection.AddScoped(c => mockClock.Object);
            serviceCollection.AddScoped(c => mockHttpContextPropertiesResolver.Object);
            serviceCollection.AddScoped(c => mockOrganisationReadModelRepository.Object);
            serviceCollection.AddScoped(c => mockProductRepository.Object);

            var customerTest = customerDetail.Object.Id;
            var test = mockCustomerReadModelRepository.Object.GetCustomerById(
                    tenantId, Guid.NewGuid(), false);

            return serviceCollection.BuildServiceProvider();
        }

        private static ServiceCollection GetDefaultServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            var internalUrlConfiguration = new InternalUrlConfiguration() { BaseApi = "https://localhost:44366" };
            serviceCollection.AddSingleton<IClock>(c => SystemClock.Instance);
            serviceCollection.AddSingleton<IInternalUrlConfiguration>(x => internalUrlConfiguration);
            var tenantAndProductResolver = new Mock<ICachingResolver>();
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns(internalUrlConfiguration.BaseApi);

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockMediator = new Mock<ICqrsMediator>();

            var tenant = new Domain.Tenant(TenantFactory.DefaultId, "arthur", "arthur", null, default, default, SystemClock.Instance.GetCurrentInstant());
            var product = new Domain.Product.Product(tenant.Id, ProductFactory.DefaultId, "bar", "arthur", SystemClock.Instance.GetCurrentInstant());
            var modelTenant = new TenantWithRelatedEntities() { Tenant = tenant };
            tenantAndProductResolver.Setup(t => t.GetTenantOrNull(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));
            tenantAndProductResolver.Setup(p => p.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(product));
            mockTenantRepository.Setup(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(modelTenant);
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            var actionRunner = new ActionRunner(new Mock<IJobClient>().Object, tenantAndProductResolver.Object, mockMediator.Object);
            serviceCollection.AddScoped<IActionRunner>(x => actionRunner);
            serviceCollection.AddScoped<ICachingResolver>(x => tenantAndProductResolver.Object);
            serviceCollection.AddScoped<ITenantRepository>(x => mockTenantRepository.Object);
            serviceCollection.AddScoped<IProductRepository>(x => mockProductRepository.Object);
            serviceCollection.AddScoped<ISerialisedEntityFactory>(c => new SerialisedEntityFactory(
                mockUrlConfiguration.Object,
                mockProductConfigProvider.Object,
                mockFormDataPrettifier.Object,
                tenantAndProductResolver.Object,
                mockMediator.Object,
                new DefaultPolicyTransactionTimeOfDayScheme()));
            serviceCollection.AddSingleton<IRazorEngineService>(RazorEngineService.Create());
            serviceCollection.AddLoggers();
            return serviceCollection;
        }
    }
}
