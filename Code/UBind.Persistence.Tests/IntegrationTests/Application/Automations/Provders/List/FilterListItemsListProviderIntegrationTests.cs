// <copyright file="FilterListItemsListProviderIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Application.Automations.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Queries.Services;
    using UBind.Application.Tests;
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
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection("Database collection")]
    public class FilterListItemsListProviderIntegrationTests
    {
        private Guid tenantId;

        public FilterListItemsListProviderIntegrationTests()
        {
            this.tenantId = Guid.NewGuid();
        }

        [Fact]
        public async Task Resolve_ReturnsEntityCollectionWithExpectedEvents()
        {
            // Arrange
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var systemEventRepository = new SystemEventRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                var tagRepository = new TagRepository(dbContext);
                for (int i = 0; i < 10; ++i)
                {
                    var tags = i % 2 == 0
                        ? new List<string> { "baz" }
                        : new List<string> { "qux" };
                    var systemEvent = SystemEvent.CreateCustom(
                        this.tenantId,
                        default,
                        ProductFactory.DefaultId,
                        DeploymentEnvironment.None,
                        "foo",
                        "payload",
                        SystemClock.Instance.Now());
                    systemEvent.SetTags(tags);
                    systemEventRepository.Insert(systemEvent);
                    tagRepository.AddRange(systemEvent.Tags);
                }

                dbContext.SaveChanges();
            }

            var tenant = TenantFactory.Create(this.tenantId);
            var entityTypeProvider = new StaticProvider<Data<string>>("Event");
            var filterProvider = new ListConditionFilterProvider(
                new ObjectPathLookupExpressionProvider<IEnumerable>(new StaticProvider<Data<string>>("#/event/tags")),
                new StaticProvider<Data<string>>("tag"),
                new BinaryExpressionFilterProvider(
                    (a, b) => Expression.Equal(a, b),
                    new PropertyExpressionProvider(new StaticProvider<Data<string>>("$")),
                    new ConstantExpressionProvider((IProvider<IData>)new StaticProvider<Data<string>>("baz")),
                    "textIsEqualToCondition"),
                ListConditionMatchType.Any);
            var logMessages = new List<string>();
            using (var dbContext = new TestUBindDbContext(
                DatabaseFixture.TestConnectionString,
                message => logMessages.Add(message)))
            {
                Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();
                mediator.GetTenantByIdOrAliasQuery(tenant);
                var tenantRepository = new TenantRepository(dbContext);
                var featureSettingRepository = new FeatureSettingRepository(dbContext, new TestClock());
                var productSettingRepository = new ProductFeatureSettingRepository(dbContext);
                var cachingResolver = new CachingResolver(mediator.Object, tenantRepository, new ProductRepository(dbContext), featureSettingRepository, productSettingRepository);
                var productRepository = new ProductRepository(dbContext);
                var systemEventRepository = new SystemEventRepository(dbContext, null);
                var portalRepository = new PortalReadModelRepository(dbContext, cachingResolver);
                var quoteRepository = new QuoteReadModelRepository(dbContext, null, new TestClock());
                var quoteVersionRepository = new QuoteVersionReadModelRepository(dbContext);
                var claimRepository = new ClaimReadModelRepository(dbContext, cachingResolver, new TestClock(), null);
                var claimVersionRepository = new ClaimVersionReadModelRepository(dbContext);
                var policyRepository = new PolicyReadModelRepository(dbContext, new Mock<IConnectionConfiguration>().Object, new TestClock());
                var policyTransactionRepository = new PolicyTransactionReadModelRepository(dbContext, new Mock<IConnectionConfiguration>().Object);
                var organisationRepository = new OrganisationReadModelRepository(dbContext);
                var customerRepository = new CustomerReadModelRepository(dbContext);
                var userRepository = new UserReadModelRepository(dbContext);
                var documentRepository = new QuoteDocumentReadModelRepository(dbContext);
                var emailRepository = new EmailRepository(dbContext, new TestClock());
                var smsRepository = new SmsRepository(dbContext);
                var entityQueryService = new AutomationEntityQueryService(
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
                var entityCollectionProvider = new EntityQueryListProvider(entityQueryService, entityTypeProvider, cachingResolver);
                var sut = new FilterListItemsListProvider(
                    entityCollectionProvider,
                    filterProvider,
                    new StaticProvider<Data<string>>("event"));

                tenantRepository.Insert(tenant);
                tenantRepository.SaveChanges();
                var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(tenantId: this.tenantId);

                // Act
                var eventCollection = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

                // Assert
                eventCollection.ToList().Should().HaveCount(5);
            }
        }
    }
}
