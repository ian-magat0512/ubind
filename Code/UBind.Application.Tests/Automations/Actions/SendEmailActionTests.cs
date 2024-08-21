// <copyright file="SendEmailActionTests.cs" company="uBind">
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
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using MimeKit;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Services;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using Action = UBind.Application.Automation.Actions.Action;

    /// <summary>
    /// Unit tests for the different automation actions.
    /// </summary>
    public class SendEmailActionTests
    {
        private IServiceProvider dependencyProvider;

        public SendEmailActionTests()
        {
            var entityId = new Guid("dcbbf092-9ef5-4303-a565-c3576a379e6e");
            var environment = DeploymentEnvironment.Development;
            var messagingService = new Mock<IMessagingService>();
            var emailService = new Mock<IEmailService>();
            messagingService.Setup(s => s.SendAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<MimeMessage>())).Verifiable();

            var organisationService = new Mock<IOrganisationService>();
            organisationService.Setup(o => o.GetDefaultOrganisationForTenant(It.IsAny<Guid>()))
                .Returns(new OrganisationReadModelSummary()
                {
                    Id = Guid.NewGuid(),
                });

            var serviceCollection = new ServiceCollection();

            var mockMediator = new Mock<ICqrsMediator>();
            var mockCachingResolver = new Mock<ICachingResolver>();
            mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(TenantFactory.Create(TenantFactory.DefaultId)));
            serviceCollection.AddScoped<ICachingResolver>(c => mockCachingResolver.Object);

            serviceCollection.AddScoped<IMessagingService>(c => messagingService.Object);
            serviceCollection.AddScoped<IEmailService>(c => emailService.Object);
            serviceCollection.AddScoped<IOrganisationService>(c => organisationService.Object);
            serviceCollection.AddSingleton<IClock>(c => SystemClock.Instance);

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            serviceCollection.AddScoped<ISerialisedEntityFactory>(c => new SerialisedEntityFactory(
                mockUrlConfiguration.Object,
                mockProductConfigProvider.Object,
                mockFormDataPrettifier.Object,
                mockCachingResolver.Object,
                mockMediator.Object,
                new DefaultPolicyTransactionTimeOfDayScheme()));

            var mockQuoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();
            var quoteVersionModel = new QuoteVersionReadModelWithRelatedEntities();
            quoteVersionModel.QuoteVersion = new QuoteVersionReadModel() { QuoteVersionId = entityId };
            mockQuoteVersionRepository
                .Setup(c => c.GetQuoteVersionWithRelatedEntities(
                    It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment?>(), It.IsAny<Guid>(), It.IsAny<List<string>>()))
                .Returns(quoteVersionModel);
            serviceCollection.AddScoped(c => mockQuoteVersionRepository.Object);

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

            serviceCollection.AddLoggers();
            this.dependencyProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task HandleExecution_shouldParseAllEmailProperties()
        {
            // Arrange
            var actionConfig = EmailAutomationTestJson.GetBasic();
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var action = this.CreateSendEmailActionUsingJson(actionConfig);
            var actionData = action.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await action.Execute(new ProviderContext(automationData), actionData);

            // Assert
            var emailAction = actionData as SendEmailActionData;
            emailAction.Email.From.ToString().Should().Be("sender@company.com");
            emailAction.Email.To.ToList()[0].ToString().Should().Be("person@gmail.com");
            emailAction.Email.Cc.ToList()[0].ToString().Should().Be("copyme123@gmail.com");
            emailAction.Email.Bcc.ToList()[0].ToString().Should().Be("secret-copy@company.com");
            emailAction.Email.Subject.Should().Be("This is an email subject");
        }

        [Fact]
        public async Task SendEmailAction_shouldParseAll_Email_And_Attachments()
        {
            // Arrange
            var actionConfig = EmailAutomationTestJson.GetEmailWithAttachment();
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var action = this.CreateSendEmailActionUsingJson(actionConfig);
            var actionData = action.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await action.Execute(new ProviderContext(automationData), actionData);

            // Assert
            var emailAction = actionData as SendEmailActionData;

            // Email in Automation Data should have 2 attachments only because the include condition of "lorem ipsum.txt" is false.
            emailAction.Email.Attachments.Should().HaveCount(2);

            var fileAttachmentInfo = emailAction.Email.Attachments.ToList()[0];
            fileAttachmentInfo.FileName.ToString().Should().Be("testing.txt");
            fileAttachmentInfo.IsIncluded.Should().Be(true);
            fileAttachmentInfo.MimeType.Should().Be("text/plain");

            // If OutputFileName is null, use the filename of the text file provider
            var fileAttachmentInfo2 = emailAction.Email.Attachments.ToList()[1];
            fileAttachmentInfo2.FileName.ToString().Should().Be("brownfox.txt");
            fileAttachmentInfo2.IsIncluded.Should().Be(true);
            fileAttachmentInfo2.MimeType.Should().Be("text/plain");
        }

        [Fact]
        public async Task SendEmailAction_ShouldParseAll_Email_And_Tags_And_Relationships()
        {
            // Arrange
            var actionConfig = EmailAutomationTestJson.GetEmailWithTagsAndRelationships();
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var action = this.CreateSendEmailActionUsingJson(actionConfig);
            var actionData = action.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await action.Execute(new ProviderContext(automationData), actionData);

            // Assert
            var emailAction = actionData as SendEmailActionData;

            // Automation Data should have 2 tags.
            emailAction.Tags.Should().HaveCount(2);
            emailAction.Tags.Should().Contain("automation");
            emailAction.Tags.Should().Contain("quoteVersion");

            emailAction.Relationships.Should().HaveCount(2);

            var info1 = Domain.RelationshipType.MessageRecipient.GetAttributeOfType<RelationshipTypeInformationAttribute>();
            var alias1 = info1.Alias.ToCamelCase();

            var info2 = Domain.RelationshipType.MessageRecipient.GetAttributeOfType<RelationshipTypeInformationAttribute>();
            var name2 = info2.Name.Humanize(LetterCasing.Title);

            emailAction.Relationships.Should().Contain(c => c.Alias == alias1);
            emailAction.Relationships.Should().Contain(c => c.Name == name2);
        }

        private SendEmailAction CreateSendEmailActionUsingJson(string actionConfig)
        {
            var actionModel = JsonConvert.DeserializeObject<IBuilder<Action>>(
                actionConfig, AutomationDeserializationConfiguration.ModelSettings);
            return actionModel.Build(this.dependencyProvider) as SendEmailAction;
        }

        private static class EmailAutomationTestJson
        {
            private static string mainJsonAction = @"{
                        ""sendEmailAction"": {
                            ""name"": ""Send email Action Test"",
                            ""alias"": ""sendEmailActionTest"",
                            ""description"": ""A test of the send email action"",
                            ""asynchronous"": false,
                           ";

            private static string emailJson = @"""email"": {                                 
                                    ""from"": ""sender@company.com"",
                                    ""replyTo"": [""reply-to@company.com""],
                                    ""to"": [""person@gmail.com""],
                                    ""cc"": [""copyme123@gmail.com""],
                                    ""bcc"": [""secret-copy@company.com""],
                                    ""subject"": ""This is an email subject"",
                                    ""textBody"": ""Dear person,\n\nplease find attached a bunch of stuff.\n\nRegards,\nMe."",
                                    ""htmlBody"": ""<html><body><p>Dear person,</p>please find attached a bunch of stuff.</p>Regards,<br/>Me.</body></html>"",
                                    ""comments"": ""Some email comment."",
                                    ""keywords"": [
                                        ""keyword1"",
                                        ""keyword2""
                                    ],
                                    ""headers"": [
                                   ]
                                }";

            private static string emailJsonWithAttachment = @"""email"": {
                                    ""from"": ""sender@company.com"",
                                    ""replyTo"": [""reply-to@company.com""],
                                    ""to"": [""person@gmail.com""],
                                    ""cc"": [""copyme123@gmail.com""],
                                    ""bcc"": [""secret-copy@company.com""],
                                    ""subject"": ""This is an email subject"",
                                    ""textBody"": ""Dear person,\n\nplease find attached a bunch of stuff.\n\nRegards,\nMe."",
                                    ""htmlBody"": ""<html><body><p>Dear person,</p>please find attached a bunch of stuff.</p>Regards,<br/>Me.</body></html>"",
                                    ""comments"": ""Some email comment."",
                                    ""keywords"": [
                                        ""keyword1"",
                                        ""keyword2""
                                    ],
                                    ""headers"": [
                                   ],
                                    ""attachments"": [
                                        {
                                            ""sourceFile"": {
                                                ""textFile"": {
                                                    ""sourceData"": ""Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."",
                                                    ""outputFileName"": ""lorem_ipsum.txt""
                                                }
                                            },
                                            ""outputFileName"": ""testing.txt"",
                                            ""includeCondition"": {
                                                ""textEndsWithCondition"": {
                                                    ""text"": ""pikachu"",
                                                    ""endsWith"": ""chu""
                                                }
                                            }
                                        },
                                        {
                                            ""sourceFile"": {
                                                ""textFile"": {
                                                    ""sourceData"": ""Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."",
                                                    ""outputFileName"": ""lorem ipsum.txt""
                                                }
                                            },
                                            ""outputFileName"": ""testing2.txt"",
                                            ""includeCondition"": {
                                                ""textStartsWithCondition"": {
                                                    ""text"": ""pikachu"",
                                                    ""startsWith"": ""chu""
                                                }
                                            }
                                        },
                                        {
                                            ""sourceFile"": {
                                                ""textFile"": {
                                                    ""sourceData"": ""The quick brown fox jumps over the lazy dog."",
                                                    ""outputFileName"": ""brownfox.txt""
                                                }
                                            },
                                            ""includeCondition"": {
                                                ""textEndsWithCondition"": {
                                                    ""text"": ""pikachu"",
                                                    ""endsWith"": ""chu""
                                                }
                                            }
                                        },
                                    ]
                                }";

            private static string tagsAndRelationships = @"""tags"" : [""automation"",""quoteVersion""],
                                                            ""relationships"" : [
                                                                {
                                                                    ""relationshipType"": ""messageRecipient"",
                                                                    ""targetEntity"": {
                                                                        ""dynamicEntity"": 
                                                                        {
                                                                            ""entityType"": ""customer"",
                                                                            ""entityId"": ""dcbbf092-9ef5-4303-a565-c3576a379e6e""
                                                                    }
                                                                }
                                                                                            },
                                                                                            {
                                                                    ""relationshipType"": ""quoteVersionMessage"",
                                                                                                ""sourceEntity"": {
                                                                        ""dynamicEntity"": 
                                                                                                    {
                                                                            ""entityType"": ""quoteVersion"",
                                                                            ""entityId"": ""dcbbf092-9ef5-4303-a565-c3576a379e6e""
                                                                        }
                                                                    }
                                                                }
                                                            ]";

            public static string GetBasic()
            {
                return mainJsonAction + emailJson + "}}";
            }

            public static string GetEmailWithAttachment()
            {
                return mainJsonAction + emailJsonWithAttachment + "}}";
            }

            public static string GetEmailWithTagsAndRelationships()
            {
                return mainJsonAction + emailJson + "," + tagsAndRelationships + "}}";
            }
        }
    }
}
