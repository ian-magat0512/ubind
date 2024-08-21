// <copyright file="AttachFilesToEntityActionIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.DocumentAttacher;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Unit tests for the different automation actions.
    /// </summary>
    [SystemEventTypeExtensionInitialize]
    public class AttachFilesToEntityActionIntegrationTests
    {
        [Theory]
        [InlineData("quoteVersion")]
        [InlineData("quote")]
        [InlineData("policyTransaction")]
        [InlineData("claimVersion")]
        [InlineData("claim")]
        public async Task AttachFilesToEntitiesAction_ShouldAttachFile_ToEntity(string entityType)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
                            ""name"": ""Attach File to Entity Test"",
                            ""alias"": ""attachFilesToEntityAction"",
                            ""description"": ""A test of attaching file to entity"",
                            ""asynchronous"": false,
                            ""runCondition"": {
                                ""textStartsWithCondition"": {
                                    ""text"": ""pikachu"",
                                    ""startsWith"": ""pik""
                                }
                            },
                            ""entities"": [
                            {
                                ""dynamicEntity"": 
                                {
                                    ""entityType"": """ + entityType + @""",
                                    ""entityId"": """ + entityId.ToString() + @"""

                                }
                            }
                            ],
                            ""attachments"": [
                                {
                                    ""sourceFile"": {
                                        ""textFile"": {
                                            ""outputFilename"": ""brown_fox.txt"",
                                            ""sourceData"": ""The quick brown fox jumps over the lazy dog.""
                                        }
                                    },
                                    ""outputFilename"": ""brown_fox.txt"",
                                    ""includeCondition"": {
                                        ""textIsEqualToCondition"": {
                                            ""text"": ""equal"",
                                            ""isEqualTo"": ""equal""
                                        }
                                    }
                                }
                            ]
                         }";

            var mockServiceProvider = this.GetServiceProvider(entityId, true);

            var attachFilesToEntityActionBuilder = JsonConvert.DeserializeObject<AttachFilesToEntitiesActionConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var attachFilesToEntityAction = attachFilesToEntityActionBuilder.Build(mockServiceProvider);
            var actionData = new AttachFilesToEntitiesActionData(attachFilesToEntityAction.Name, attachFilesToEntityAction.Alias, new TestClock());
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            await attachFilesToEntityAction.Execute(
                providerContext,
                actionData,
                false);

            // Assert
            actionData.EntityReferences.First().Key.Should().Be(entityId.ToString());
            actionData.EntityReferences.First().Value.ToLowerInvariant().Should().Be(entityType.ToLowerInvariant());
            actionData.Attachments.Should().HaveCount(1);
            actionData.Attachments[0].Should().Be("brown_fox.txt");
        }

        [Theory]
        [InlineData("user")]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("customer")]
        [InlineData("emailMessage")]
        public async Task AttachFilesToEntityAction_Should_Throw_When_Entity_Does_Not_Support_File_AttachmentAsync(string type)
        {
            // Arrange
            var entityId = TenantFactory.DefaultId;
            var json = @"{
                            ""name"": ""Attach File to Entity Test"",
                            ""alias"": ""attachFilesToEntityAction"",
                            ""description"": ""A test of attaching file to entity"",
                            ""asynchronous"": false,
                            ""runCondition"": {
                                ""textStartsWithCondition"": {
                                    ""text"": ""pikachu"",
                                    ""startsWith"": ""pik""
                                }
                            },
                            ""entities"": [
                                 {
                                    ""dynamicEntity"": 
                                    {
                                        ""entityType"": """ + type + @""",
                                        ""entityId"": """ + entityId.ToString() + @"""

                                    }
                                }
                            ],
                            ""attachments"": [
                                {
                                    ""sourceFile"": {
                                        ""textFile"": {
                                            ""outputFilename"": ""brown_fox.txt"",
                                            ""sourceData"": ""The quick brown fox jumps over the lazy dog.""
                                        }
                                    },
                                    ""outputFilename"": ""brown_fox.txt"",
                                    ""includeCondition"": {
                                        ""textIsEqualToCondition"": {
                                            ""text"": ""equal"",
                                            ""isEqualTo"": ""equal""
                                        }
                                    }
                                }
                            ]
                         }";

            var mockServiceProvider = this.GetServiceProvider(entityId, false);

            var attachFilesToEntityActionBuilder = JsonConvert.DeserializeObject<AttachFilesToEntitiesActionConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var attachFilesToEntityAction = attachFilesToEntityActionBuilder.Build(mockServiceProvider);
            var actionData = new AttachFilesToEntitiesActionData(attachFilesToEntityAction.Name, attachFilesToEntityAction.Alias, new TestClock());

            var @eventPolicyRenewed = new QuoteAggregate.PolicyRenewedEvent(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                "NAAAAA",
                default,
                default,
                default,
                default,
                null,
                default(Guid),
                new TestClock().Now(),
                Guid.NewGuid());
            var systemEventTypePolicyRenewed = SystemEventTypeMap.Map(@eventPolicyRenewed);
            var productContext = new ProductContext(TenantFactory.DefaultId, ProductFactory.DefaultId, DeploymentEnvironment.Development);
            var policyRenewedSystemEvent = SystemEvent.CreateWithoutPayload(
                productContext.TenantId,
                Guid.NewGuid(),
                productContext.ProductId,
                productContext.Environment,
                systemEventTypePolicyRenewed.First(),
                new TestClock().Timestamp);
            var automationData = MockAutomationData.CreateWithCustomEventTrigger(policyRenewedSystemEvent);
            var providerContext = new ProviderContext(automationData);

            // Act
            Func<Task> act = async () => await attachFilesToEntityAction.Execute(providerContext, actionData);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be($"{type.ToUpperFirstChar()} entity does not support file attachments");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity ID: {entityId}");
            exception.Which.Error.AdditionalDetails.Should().Contain("Attachments: brown_fox.txt");
        }

        private IServiceProvider GetServiceProvider(Guid entityId, bool isSupportedEntity)
        {
            var serviceCollection = new ServiceCollection();

            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            var mockMediator = new Mock<ICqrsMediator>();
            var tenant = new Domain.Tenant(
                TenantFactory.DefaultId, "arthur", "arthur", null, default, default, SystemClock.Instance.GetCurrentInstant());
            var tenantModel = new TenantWithRelatedEntities
            {
                Tenant = tenant,
            };
            mockTenantRepository.Setup(c => c.GetTenantWithRelatedEntitiesById(It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(tenantModel);
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockCachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockCachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            serviceCollection.AddScoped(c => mockTenantRepository.Object);
            serviceCollection.AddScoped(x => mockCachingResolver.Object);

            // Document Attacher
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDocumentAttacher = new Mock<IDocumentAttacher>();
            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();

            mockDocumentAttacher.Setup(c => c.CanAttach(It.IsAny<IEntity>())).Returns(isSupportedEntity);
            serviceCollection.AddScoped(c => mockDocumentAttacher.Object);

            // Url configuration
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            serviceCollection.AddScoped<ISerialisedEntityFactory>(c => new SerialisedEntityFactory(
                mockUrlConfiguration.Object,
                mockProductConfigProvider.Object,
                mockFormDataPrettifier.Object,
                mockCachingResolver.Object,
                mockMediator.Object,
                new DefaultPolicyTransactionTimeOfDayScheme()));

            // Repositories
            var mockClaimRepository = new Mock<IClaimReadModelRepository>();
            var claimModel = new ClaimReadModelWithRelatedEntities();
            claimModel.Claim = new Mock<ClaimReadModel>(entityId).Object;
            mockClaimRepository
                .Setup(c => c.GetClaimWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(claimModel);
            serviceCollection.AddScoped(c => mockClaimRepository.Object);

            var mockClaimVersionRepository = new Mock<IClaimVersionReadModelRepository>();
            var claimVersionModel = new ClaimVersionReadModelWithRelatedEntities();
            claimVersionModel.ClaimVersion = new ClaimVersionReadModel() { Id = entityId, ClaimId = Guid.NewGuid() };
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
            mockQuoteRepository
                .Setup(c => c.GetQuoteWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(quoteModel);
            serviceCollection.AddScoped(c => mockQuoteRepository.Object);

            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();
            var quoteVersionModel = new QuoteVersionReadModelWithRelatedEntities();
            quoteVersionModel.QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = entityId };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(quoteVersionModel);
            serviceCollection.AddScoped(c => mockQuoteVersionRepository.Object);

            var mockUserReadModelRepository = new Mock<IUserReadModelRepository>();
            var userModel = new UserReadModelWithRelatedEntities();
            userModel.User = new FakeUserReadModel(entityId);
            mockUserReadModelRepository
                .Setup(c => c.GetUserWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(userModel);
            serviceCollection.AddScoped(c => mockUserReadModelRepository.Object);

            var mockProductRepository = new Mock<IProductRepository>();
            var product = new Domain.Product.Product(entityId, entityId, "product_alias", entityId.ToString(), NodaTime.SystemClock.Instance.GetCurrentInstant());
            var productModel = new ProductWithRelatedEntities
            {
                Product = product,
            };
            mockProductRepository.Setup(c => c.GetProductWithRelatedEntities(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<List<string>>())).Returns(productModel);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            serviceCollection.AddScoped(c => mockProductRepository.Object);

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
            serviceCollection.AddScoped(c => mockCustomerReadModelRepository.Object);

            var mockDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();
            var documentModel = new DocumentReadModelWithRelatedEntities();
            documentModel.Document = new FakeQuoteDocumentReadModel(entityId);
            mockDocumentRepository
                .Setup(c => c.GetDocumentWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(documentModel);
            var mockConfiguration = new Mock<IEmailInvitationConfiguration>();
            mockConfiguration.Setup(c => c.InvitationLinkHost).Returns("https://localhost:4366");
            serviceCollection.AddScoped(c => mockDocumentRepository.Object);
            serviceCollection.AddScoped(c => mockConfiguration.Object);

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
            serviceCollection.AddScoped(c => mockOrganisationRepository.Object);
            serviceCollection.AddScoped(c => new Mock<ICqrsMediator>().Object);
            serviceCollection.AddScoped<IClock>(c => new TestClock());
            serviceCollection.AddLoggers();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
