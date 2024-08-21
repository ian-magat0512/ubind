// <copyright file="EntityQueryListProviderIntegrationTests.cs" company="uBind">
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
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.Claim;
    using UBind.Persistence.ReadModels.Portal;
    using UBind.Persistence.ReadModels.Quote;
    using Xunit;

    [Collection("Database collection")]
    public class EntityQueryListProviderIntegrationTests
    {
        private Guid tenantId;

        public EntityQueryListProviderIntegrationTests()
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
                var tags = new List<string> { "baz" };
                for (int i = 0; i < 10; ++i)
                {
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

            var entityTypeProvider = new StaticProvider<Data<string>>("Event");
            var tenant = TenantFactory.Create(this.tenantId);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
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
                var sut = new EntityQueryListProvider(entityQueryService, entityTypeProvider, cachingResolver);

                tenantRepository.Insert(tenant);
                tenantRepository.SaveChanges();
                var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(tenantId: this.tenantId);

                // Act
                var eventCollection = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
                var userQueryActions = () =>
                {
                    var filters = new UserReadModelFilters();
                    filters.Statuses = filters.Statuses.Append("new").Append("active").Append("deactivated").Append("disabled").Append("invited");
                    userRepository.GetUsers(this.tenantId, filters);
                };

                // Assert
                var events = eventCollection.ToList();
                events.Should().HaveCount(10);
                userQueryActions.Should().NotThrow();
            }
        }
    }
}
