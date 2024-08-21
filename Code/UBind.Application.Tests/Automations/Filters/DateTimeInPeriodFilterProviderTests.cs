// <copyright file="DateTimeInPeriodFilterProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Filters
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
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using Xunit;

    public class DateTimeInPeriodFilterProviderTests
    {
        [Fact]
        public async Task DateTimeInPeriodFilterProvider_CorrectlyFiltersEntities_WhenUsedInFilteredCollectionProvider()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var numberOfQuotes = 100;
            var queryService = new Mock<IEntityQueryService>();
            var cachingResolver = new Mock<ICachingResolver>();
            var now = SystemClock.Instance.Now();
            var dummyQuotes = this.GetDummyQuotes(now, numberOfQuotes);
            queryService
                .Setup(s => s.QueryQuotes(tenantId, It.IsAny<DeploymentEnvironment>(), new List<string>()))
                .Returns(() => dummyQuotes);
            var collectionProvider = new EntityQueryListProvider(
                queryService.Object,
                new StaticProvider<Data<string>>("quote"),
                cachingResolver.Object);
            var dateTimeExpressionProvider = new ObjectPathLookupExpressionProvider<long>(
                new StaticProvider<Data<string>>("#/quote/createdTicksSinceEpoch"));
            var periodStart = now.Minus(Duration.FromDays(50));
            var periodEnd = now;
            var intervalProvider = new StaticProvider<Data<Interval>>(new Interval(periodStart, periodEnd));
            var sut = new DateTimeInPeriodFilterProvider(
                dateTimeExpressionProvider,
                intervalProvider);

            var itemAliasProvider = new StaticProvider<Data<string>>("quote");
            var filteredCollectionProvider = new FilterListItemsListProvider(
                collectionProvider,
                sut,
                itemAliasProvider);
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(tenantId);

            // Act
            var entityCollection = (await filteredCollectionProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Asset
            entityCollection.ToList().Should().HaveCount(50);
        }

        private IQueryable<IQuoteReadModelWithRelatedEntities> GetDummyQuotes(Instant now, int numberOfQuotes)
        {
            return Enumerable.Range(1, numberOfQuotes)
                .Select(i => new QuoteReadModelWithRelatedEntities
                {
                    Quote = new FakeNewQuoteReadModel(
                        Guid.NewGuid(), Guid.NewGuid(), now.Minus(Duration.FromDays(numberOfQuotes - (i - 1)))),
                })
                .AsQueryable();
        }
    }
}
