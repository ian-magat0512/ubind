// <copyright file="FilterListItemsListProviderConfigModelIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Application.Automations.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Queries.Services;
    using UBind.Application.Tests;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.Claim;
    using UBind.Persistence.ReadModels.Portal;
    using UBind.Persistence.ReadModels.Quote;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection("Database collection")]
    [SystemEventTypeExtensionInitialize]
    public class FilterListItemsListProviderConfigModelIntegrationTests
    {
        private Guid tenantId;

        public FilterListItemsListProviderConfigModelIntegrationTests()
        {
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public async Task Build_ReturnsAProviderThatCanSuccessfullyExecuteSingleDatabaseQuery_ForEntityListFilteredOnSubListCondition()
        {
            // Arrange
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var systemEventRepository = new SystemEventRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                var tagRepository = new TagRepository(dbContext);
                var fooTags = new List<string> { "foo" };
                var barTags = new List<string> { "bar" };
                for (int i = 0; i < 10; ++i)
                {
                    var systemEvent = SystemEvent.CreateCustom(
                        this.tenantId,
                        default,
                        ProductFactory.DefaultId,
                        DeploymentEnvironment.None,
                        "fooAlias",
                        "payload",
                        SystemClock.Instance.Now());
                    systemEvent.SetTags(i % 2 == 0 ? fooTags : barTags);
                    systemEventRepository.Insert(systemEvent);
                    tagRepository.AddRange(systemEvent.Tags);
                }

                dbContext.SaveChanges();
            }

            var json = @"{
    ""filterListItemsList"": {
        ""list"": {
            ""entityQueryList"": {
                ""entityType"": ""event""
            },
        },
        ""itemAlias"": ""event"",
        ""condition"": {
            ""orCondition"": [
                {
                    ""listCondition"": {
                        ""list"": {
                            ""objectPathLookupList"": ""#/event/tags""
                        },
                        ""condition"": {
                            ""textIsEqualToCondition"": {
                                ""text"": {
                                    ""objectPathLookupText"": ""#/item""
                                },
                                ""isEqualTo"": ""foo""
                            }
                        },
                        ""matchType"": ""any""
                    }
                }
            ]
        }
    }
}";

            var sut = JsonConvert.DeserializeObject<IBuilder<IDataListProvider<object>>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var queryService = this.CreateAutomationEntityQueryService(dbContext);
                var tenantRepository = new TenantRepository(dbContext);
                var dependencyProvider = new Mock<IServiceProvider>();
                dependencyProvider
                    .Setup(dp => dp.GetService(typeof(IEntityQueryService)))
                    .Returns(queryService);
                var automationData = MockAutomationData.CreateWithEventTrigger(tenantId: this.tenantId);

                // Act
                var provider = sut.Build(dependencyProvider.Object);

                // Assert
                var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
                result.ToList().Should().HaveCount(5);
            }
        }

        [Fact]
        public async Task Build_ReturnsAProviderThatCanSuccessfullyExecuteSingleDatabaseQuery_ForEntityListFilteredOnSubListConditionIncludingLiteralList()
        {
            // Arrange
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var systemEventRepository = new SystemEventRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                var tagRepository = new TagRepository(dbContext);
                var fooTags = new List<string> { "foo" };
                var barTags = new List<string> { "bar" };
                var bazTags = new List<string> { "baz" };
                for (int i = 0; i < 9; ++i)
                {
                    var tags =
                        i % 3 == 0 ? fooTags :
                        i % 3 == 1 ? barTags :
                        bazTags;
                    var systemEvent = SystemEvent.CreateCustom(
                        this.tenantId,
                        default,
                        ProductFactory.DefaultId,
                        DeploymentEnvironment.None,
                        "fooAlias",
                        "payload",
                        SystemClock.Instance.Now());
                    systemEvent.SetTags(tags);
                    systemEventRepository.Insert(systemEvent);
                    tagRepository.AddRange(systemEvent.Tags);
                }

                dbContext.SaveChanges();
            }

            var json = @"{
    ""filterListItemsList"": {
        ""list"": {
            ""entityQueryList"": {
                ""entityType"": ""event""
            },
        },
        ""itemAlias"": ""event"",
        ""condition"": {
            ""andCondition"": [
                {
                    ""listCondition"": {
                        ""list"": {
                            ""objectPathLookupList"": ""#/event/tags""
                        },
                        ""itemAlias"": ""tag"",
                        ""condition"": {
                            ""listCondition"": {
                                ""list"": [ ""foo"", ""bar"" ],
                                ""itemAlias"": ""acceptableTag"",
                                ""condition"": {
                                    ""textIsEqualToCondition"": {
                                        ""text"": {
                                            ""objectPathLookupText"": ""#/tag""
                                        },
                                        ""isEqualTo"": {
                                            ""objectPathLookupText"": ""#/acceptableTag""
                                        }
                                    }
                                },
                                ""matchType"": ""any""
                            }
                        },
                        ""matchType"": ""any""
                    }
                }
            ]
        }
    }
}";

            var sut = JsonConvert.DeserializeObject<IBuilder<IDataListProvider<object>>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var logMessages = new List<string>();
            using (var dbContext = new TestUBindDbContext(
                DatabaseFixture.TestConnectionString,
                message => logMessages.Add(message)))
            {
                var queryService = this.CreateAutomationEntityQueryService(dbContext);
                var dependencyProvider = new Mock<IServiceProvider>();
                dependencyProvider
                    .Setup(dp => dp.GetService(typeof(IEntityQueryService)))
                    .Returns(queryService);
                var automationData = MockAutomationData.CreateWithEventTrigger(tenantId: this.tenantId);

                // Act
                var provider = sut.Build(dependencyProvider.Object);

                // Assert
                var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
                result.ToList().Should().HaveCount(6);
                logMessages.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Build_ReturnsAProviderThatCanSuccessfullyExecuteSingleDatabaseQuery_WhenConfigurationIncludesUntranslatableProviders()
        {
            // Arrange
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var systemEventRepository = new SystemEventRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                var tagRepository = new TagRepository(dbContext);
                var fooTags = new List<string> { "foo" };
                var barTags = new List<string> { "bar" };
                var bazTags = new List<string> { "baz" };
                for (int i = 0; i < 9; ++i)
                {
                    var tags =
                        i % 3 == 0 ? fooTags :
                        i % 3 == 1 ? barTags :
                        bazTags;
                    var systemEvent = SystemEvent.CreateCustom(
                        this.tenantId,
                        default,
                        ProductFactory.DefaultId,
                        DeploymentEnvironment.None,
                        "fooAlias",
                        "payload",
                        SystemClock.Instance.Now());
                    systemEvent.SetTags(tags);
                    systemEventRepository.Insert(systemEvent);
                    tagRepository.AddRange(systemEvent.Tags);
                }

                dbContext.SaveChanges();
            }

            var json = @"{
    ""filterListItemsList"": {
        ""list"": {
            ""entityQueryList"": {
                ""entityType"": ""event""
            },
        },
        ""itemAlias"": ""event"",
        ""condition"": {
            ""andCondition"": [
                {
                    ""listCondition"": {
                        ""list"": {
                            ""objectPathLookupList"": ""#/event/tags""
                        },
                        ""itemAlias"": ""tag"",
                        ""condition"": {
                            ""textIsEqualToCondition"": {
                                ""text"": {
                                    ""objectPathLookupText"": ""#/tag""
                                },
                                ""isEqualTo"": {
                                    ""liquidText"": {
                                        ""liquidTemplate"": ""{{key1}}"",
                                        ""dataObject"": [
                                            {
                                                ""propertyName"": ""key1"",
                                                ""value"": ""foo""
                                            }
                                        ]
                                    }
                                }
                            }
                        },
                        ""matchType"": ""any""
                    }
                }
            ]
        }
    }
}";

            var sut = JsonConvert.DeserializeObject<IBuilder<IDataListProvider<object>>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var logMessages = new List<string>();
            using (var dbContext = new TestUBindDbContext(
                DatabaseFixture.TestConnectionString,
                message => logMessages.Add(message)))
            {
                var queryService = this.CreateAutomationEntityQueryService(dbContext);
                var dependencyProvider = new Mock<IServiceProvider>();
                dependencyProvider
                    .Setup(dp => dp.GetService(typeof(IEntityQueryService)))
                    .Returns(queryService);
                dependencyProvider.AddLoggers();

                var automationData = MockAutomationData.CreateWithEventTrigger(tenantId: this.tenantId);

                // Act
                var provider = sut.Build(dependencyProvider.Object);

                // Assert
                var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
                result.ToList().Should().HaveCount(3);
                logMessages.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Build_ReturnsAProviderThatCanSuccessfullyExecuteSingleDatabaseQuery_WhenConfigurationIncludesFilterWithFunctionBasedExpression()
        {
            // Arrange
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var systemEventRepository = new SystemEventRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                var tagRepository = new TagRepository(dbContext);
                var fooTags = new List<string> { "foo" };
                var barTags = new List<string> { "bar" };
                var bazTags = new List<string> { "baz" };
                for (int i = 0; i < 9; ++i)
                {
                    var tags =
                        i % 3 == 0 ? fooTags :
                        i % 3 == 1 ? barTags :
                        bazTags;
                    var systemEvent = SystemEvent.CreateCustom(
                        this.tenantId,
                        default,
                        ProductFactory.DefaultId,
                        DeploymentEnvironment.None,
                        "fooAlias",
                        "payload",
                        SystemClock.Instance.Now());
                    systemEvent.SetTags(tags);
                    systemEventRepository.Insert(systemEvent);
                    tagRepository.AddRange(systemEvent.Tags);
                }

                dbContext.SaveChanges();
            }

            var json = @"{
                ""filterListItemsList"": {
                    ""list"": {
                        ""entityQueryList"": {
                            ""entityType"": ""event""
                        },
                    },
                    ""itemAlias"": ""event"",
                    ""condition"": {
                        ""andCondition"": [
                            {
                                ""listCondition"": {
                                    ""list"": {
                                        ""objectPathLookupList"": ""#/event/tags""
                                    },
                                    ""itemAlias"": ""tag"",
                                    ""condition"": {
                                        ""textStartsWithCondition"": {
                                            ""text"": {
                                                ""objectPathLookupText"": ""#/tag""
                                            },
                                            ""startsWith"": ""b""
                                        }
                                    },
                                    ""matchType"": ""any""
                                }
                            }
                        ]
                    }
                }
            }";

            var sut = JsonConvert.DeserializeObject<IBuilder<IDataListProvider<object>>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var logMessages = new List<string>();
            using (var dbContext = new TestUBindDbContext(
                DatabaseFixture.TestConnectionString,
                message => logMessages.Add(message)))
            {
                var queryService = this.CreateAutomationEntityQueryService(dbContext);
                var dependencyProvider = new Mock<IServiceProvider>();
                dependencyProvider
                    .Setup(dp => dp.GetService(typeof(IEntityQueryService)))
                    .Returns(queryService);
                var automationData = MockAutomationData.CreateWithEventTrigger(tenantId: this.tenantId);

                // Act
                var provider = sut.Build(dependencyProvider.Object);

                // Assert
                var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
                result.ToList().Should().HaveCount(6);
                logMessages.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Build_ReturnsAProviderThatCanSuccessfullyQueryForQuotes()
        {
            var productId = Guid.NewGuid();
            var environment = DeploymentEnvironment.Development;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(this.tenantId);
                stack.CreateTenant(tenant);

                for (int i = 0; i < 10; ++i)
                {
                    var productNumber = (i % 2) + 1;
                    var aggregate = QuoteFactory.CreateNewPolicy(
                        this.tenantId,
                        i == 0 ? productId : Guid.NewGuid(),
                        environment,
                        policyNumber: $"Pa-{productNumber}");
                    await stack.QuoteAggregateRepository.Save(aggregate);
                }
            }

            var json = @"{
    ""filterListItemsList"": {
        ""list"": {
            ""entityQueryList"": {
                ""entityType"": ""quote""
            },
        },
        ""itemAlias"": ""quote"",
        ""condition"": {
            ""textIsEqualToCondition"": {
                ""text"": {
                    ""objectPathLookupText"": ""#/quote/policyNumber""
                },
                ""isEqualTo"": """ + "Pa-1" + @"""
            }
        }
    }
}";

            var sut = JsonConvert.DeserializeObject<IBuilder<IDataListProvider<object>>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var logMessages = new List<string>();
            using (var dbContext = new TestUBindDbContext(
                DatabaseFixture.TestConnectionString,
                message => logMessages.Add(message)))
            {
                var queryService = this.CreateAutomationEntityQueryService(dbContext);
                var dependencyProvider = new Mock<IServiceProvider>();
                dependencyProvider
                    .Setup(dp => dp.GetService(typeof(IEntityQueryService)))
                    .Returns(queryService);
                var automationData = MockAutomationData.CreateWithEventTrigger(this.tenantId, environment);

                // Act
                var provider = sut.Build(dependencyProvider.Object);

                // Assert
                var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
                result.ToList().Should().HaveCount(5);
                logMessages.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Build_ReturnsAProviderThatUsesDefaultItemAlias_WhenItemAliasNotProvided()
        {
            // Arrange
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(this.tenantId);
                stack.CreateTenant(tenant);

                for (int i = 0; i < 10; ++i)
                {
                    var productNumber = (i % 2) + 1;
                    var aggregate = QuoteFactory.CreateNewPolicy(
                        this.tenantId, environment: DeploymentEnvironment.Development, policyNumber: $"Pb-{productNumber}");
                    await stack.QuoteAggregateRepository.Save(aggregate);
                }
            }

            var json = @"{
    ""filterListItemsList"": {
        ""list"": {
            ""entityQueryList"": {
                ""entityType"": ""quote""
            },
        },
        ""condition"": {
            ""textIsEqualToCondition"": {
                ""text"": {
                    ""objectPathLookupText"": ""#/quote/policyNumber""
                },
                ""isEqualTo"": ""Pb-1""
            }
        }
    }
}";

            var sut = JsonConvert.DeserializeObject<IBuilder<IDataListProvider<object>>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var logMessages = new List<string>();
            using (var dbContext = new TestUBindDbContext(
                DatabaseFixture.TestConnectionString,
                message => logMessages.Add(message)))
            {
                var queryService = this.CreateAutomationEntityQueryService(dbContext);
                var dependencyProvider = new Mock<IServiceProvider>();
                dependencyProvider
                    .Setup(dp => dp.GetService(typeof(IEntityQueryService)))
                    .Returns(queryService);
                var automationData =
                    MockAutomationData.CreateWithEventTrigger(this.tenantId, DeploymentEnvironment.Development);

                // Act
                var provider = sut.Build(dependencyProvider.Object);

                // Assert
                var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
                result.ToList().Should().HaveCount(5);
                logMessages.Should().NotBeEmpty();
            }
        }

        private AutomationEntityQueryService CreateAutomationEntityQueryService(IUBindDbContext dbContext)
        {
            var tenant = TenantFactory.Create(this.tenantId);
            var tenantRepository = new TenantRepository(dbContext);
            var mediator = new Mock<ICqrsMediator>();
            mediator.GetTenantByIdOrAliasQuery(tenant);
            var featureSettingRepository = new FeatureSettingRepository(dbContext, new TestClock());
            var productSettingRepository = new ProductFeatureSettingRepository(dbContext);
            var cachingResolver = new CachingResolver(mediator.Object, tenantRepository, new ProductRepository(dbContext), featureSettingRepository, productSettingRepository);
            var productRepository = new ProductRepository(dbContext);
            var systemEventRepository = new SystemEventRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
            var portalRepository = new PortalReadModelRepository(dbContext, cachingResolver);
            var quoteRepository = new QuoteReadModelRepository(dbContext, null, new TestClock());
            var quoteVersionRepository = new QuoteVersionReadModelRepository(dbContext);
            var claimRepository = new ClaimReadModelRepository(dbContext, cachingResolver, new TestClock(), null);
            var claimVersionRepository = new ClaimVersionReadModelRepository(dbContext);
            var policyRepository = new PolicyReadModelRepository(
                dbContext,
                new Mock<IConnectionConfiguration>().Object,
                new TestClock());
            var policyTransactionRepository = new PolicyTransactionReadModelRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
            var organisationRepository = new OrganisationReadModelRepository(dbContext);
            var customerRepository = new CustomerReadModelRepository(dbContext);
            var userRepository = new UserReadModelRepository(dbContext);
            var documentRepository = new QuoteDocumentReadModelRepository(dbContext);
            var emailRepository = new EmailRepository(dbContext, new TestClock());
            var smsRepository = new SmsRepository(dbContext);

            if (tenantRepository.GetTenantById(this.tenantId) == null)
            {
                tenantRepository.Insert(tenant);
                tenantRepository.SaveChanges();
            }

            return new AutomationEntityQueryService(
                    systemEventRepository,
                    quoteRepository,
                    quoteVersionRepository,
                    claimRepository,
                    claimVersionRepository,
                    policyRepository,
                    policyTransactionRepository,
                    organisationRepository,
                    productRepository,
                    tenantRepository,
                    customerRepository,
                    userRepository,
                    documentRepository,
                    emailRepository,
                    smsRepository,
                    portalRepository);
        }
    }
}
