// <copyright file="TextFilterProviderConfigModelIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Application.Automations.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.Claim;
    using UBind.Persistence.ReadModels.Portal;
    using UBind.Persistence.ReadModels.Quote;

    public abstract class TextFilterProviderConfigModelIntegrationTests
    {
        protected async Task VerifyTextFilterBehaviourAsync(string filterName, string text, string parameterName, string secondParameter, bool expectedFilterResult)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var tenantRepository = new TenantRepository(dbContext);
                var tagRepository = new TagRepository(dbContext);
                var systemEventRepository = new SystemEventRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                tenantRepository.Insert(tenant);
                tenantRepository.SaveChanges();
                var tags = new List<string> { text };
                var systemEvent = SystemEvent.CreateCustom(
                    tenantId,
                    default,
                    ProductFactory.DefaultId,
                    DeploymentEnvironment.None,
                    "fooAlias",
                    "payload",
                    SystemClock.Instance.Now());
                systemEvent.SetTags(tags);
                systemEventRepository.Insert(systemEvent);
                tagRepository.AddRange(systemEvent.Tags);
                dbContext.SaveChanges();
            }

            var json = $@"{{
    ""filterListItemsList"": {{
        ""list"": {{
            ""entityQueryList"": {{
                ""entityType"": ""event""
            }},
        }},
        ""itemAlias"": ""event"",
        ""condition"": {{
            ""listCondition"": {{
                ""list"": {{
                    ""objectPathLookupList"": ""#/event/tags""
                }},
                ""condition"": {{
                    ""{filterName}"": {{
                        ""text"": {{
                            ""objectPathLookupText"": ""#/item""
                        }},
                        ""{parameterName}"": ""{secondParameter}""
                    }}
                }},
                ""matchType"": ""any""
            }}
        }}
    }}
}}";

            var sut = JsonConvert.DeserializeObject<IBuilder<IDataListProvider<object>>>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var mockCachingResolver = new Mock<ICachingResolver>();
                var productRepository = new ProductRepository(dbContext);
                var tenantRepository = new TenantRepository(dbContext);
                var mediator = new Mock<ICqrsMediator>();
                mediator.Setup(x => x.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);
                var featureSettingRepository = new FeatureSettingRepository(dbContext, new TestClock());
                var productSettingRepository = new ProductFeatureSettingRepository(dbContext);
                var cachingResolver = new CachingResolver(mediator.Object, tenantRepository, productRepository, featureSettingRepository, productSettingRepository);
                var systemEventRepository = new SystemEventRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                var portalRepository = new PortalReadModelRepository(dbContext, cachingResolver);
                var quoteRepository = new QuoteReadModelRepository(dbContext, null, new TestClock());
                var quoteVersionRepository = new QuoteVersionReadModelRepository(dbContext);
                var claimRepository = new ClaimReadModelRepository(dbContext, mockCachingResolver.Object, new TestClock(), null);
                var claimVersionRepository = new ClaimVersionReadModelRepository(dbContext);
                var policyRepository = new PolicyReadModelRepository(dbContext, new Mock<IConnectionConfiguration>().Object, new TestClock());
                var policyTransactionRepository = new PolicyTransactionReadModelRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                var organisationRepository = new OrganisationReadModelRepository(dbContext);
                var customerRepository = new CustomerReadModelRepository(dbContext);
                var userRepository = new UserReadModelRepository(dbContext);
                var documentRepository = new QuoteDocumentReadModelRepository(dbContext);
                var emailRepository = new EmailRepository(dbContext, new TestClock());
                var smsRepository = new SmsRepository(dbContext);
                var queryService = new AutomationEntityQueryService(
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
                var dependencyProvider = new Mock<IServiceProvider>();
                dependencyProvider
                    .Setup(dp => dp.GetService(typeof(IEntityQueryService)))
                    .Returns(queryService);
                var automationData = MockAutomationData.CreateWithEventTrigger(tenantId: tenantId);

                // Act
                var provider = sut.Build(dependencyProvider.Object);

                // Assert
                var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
                result.ToList().Should().HaveCount(expectedFilterResult ? 1 : 0);
            }
        }
    }
}
