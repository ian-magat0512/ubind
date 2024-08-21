// <copyright file="FilterListItemsListProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Collections
{
    using System;
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
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using Xunit;

    public class FilterListItemsListProviderTests
    {
        [Fact]
        public async Task Resolve_ShouldReturnEntityCollectionWithAppropriateData()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var numberOfQuotes = 100;
            var queryService = new Mock<IEntityQueryService>();
            var cachingResolver = new Mock<ICachingResolver>();
            var now = SystemClock.Instance.Now();
            queryService
                .Setup(s => s.QueryQuotes(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), new List<string>()))
                .Returns(() => this.GetDummyQuotes(now, numberOfQuotes));
            var collectionProvider = new EntityQueryListProvider(
                queryService.Object,
                new StaticProvider<Data<string>>("quote"),
                cachingResolver.Object);
            var itemAlias = new StaticProvider<Data<string>>("quote");
            var firstOperandExpressionProvider = new PropertyExpressionProvider(new StaticProvider<Data<string>>("quote.createdTicksSinceEpoch"));
            var fiftyDaysAgo = now.Minus(Duration.FromDays(50));
            var secondOperandExpressionProvider = new ConstantExpressionProvider(new StaticProvider<Data<long>>(fiftyDaysAgo.ToUnixTimeTicks()));
            var lessThanFilterProvider = new BinaryExpressionFilterProvider(
                (a, b) => Expression.LessThan(a, b),
                firstOperandExpressionProvider,
                secondOperandExpressionProvider,
                "integerIsLessThanCondition");
            var sut = new FilterListItemsListProvider(collectionProvider, lessThanFilterProvider, itemAlias);
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(tenantId);

            // Act
            var entityCollection = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Asset
            entityCollection.ToList().Should().HaveCount(50);
        }

        private IQueryable<IQuoteReadModelWithRelatedEntities> GetDummyQuotes(Instant now, int numberOfQuotes)
        {
            return Enumerable.Range(1, numberOfQuotes)
                .Select(i => new QuoteReadModelWithRelatedEntities
                {
                    Quote = new FakeNewQuoteReadModel(Guid.NewGuid(), Guid.NewGuid(), now.Minus(Duration.FromDays(numberOfQuotes - (i - 1)))),
                })
                .AsQueryable();
        }
    }
}
