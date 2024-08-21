// <copyright file="EntityQueryListProviderTests.cs" company="uBind">
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
    using DotLiquid.Util;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using Xunit;

    public class EntityQueryListProviderTests
    {
        [Fact]
        public async Task Resolve_ShouldReturnEntityCollectionWithAppropriateData()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var numberOfQuotes = 100;
            var cachingResolver = new Mock<ICachingResolver>();
            var queryService = new Mock<IEntityQueryService>();
            queryService
                .Setup(s => s.QueryQuotes(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), new List<string>()))
                .Returns(() => this.GetDummyQuotes(numberOfQuotes));

            var sut = new EntityQueryListProvider(
                queryService.Object,
                new StaticProvider<Data<string>>("quote"),
                cachingResolver.Object);
            var dummySystemEvent = FakeEventFactory.Create(tenantId);
            var automationData = AutomationData.CreateFromSystemEvent(
                dummySystemEvent, null, MockAutomationData.GetDefaultServiceProvider());

            // Act
            var entityQueryList = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Asset
            entityQueryList.Should().HaveCount(numberOfQuotes);
        }

        [Theory]
        [InlineData("c79644c1-f4c8-44f8-b742-bfe2058b1d49", DeploymentEnvironment.Development, 1)]
        [InlineData("c79644c1-f4c8-44f8-b742-bfe2058b1d49", DeploymentEnvironment.Staging, 2)]
        [InlineData("c79644c1-f4c8-44f8-b742-bfe2058b1d49", DeploymentEnvironment.Production, 3)]
        public async Task Resolve_ShouldReturnEntityList_ForSpecifiedTenantAndEnvironment(
            string tenantString, DeploymentEnvironment environment, int count)
        {
            // Arrange
            var tenantId = new Guid(tenantString);
            var cachingResolver = new Mock<ICachingResolver>();
            var queryService = new Mock<IEntityQueryService>();
            queryService
                .Setup(s => s.QueryQuotes(It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>(), new List<string>()))
                .Returns(() => this.GetDummyQuotesWithEnvironment(tenantId)
                .Where(q => q.Quote.TenantId == tenantId && q.Quote.Environment == environment));

            var sut = new EntityQueryListProvider(
                queryService.Object,
                new StaticProvider<Data<string>>("quote"),
                cachingResolver.Object);
            var dummySystemEvent = FakeEventFactory.Create(tenantId, environment: environment);
            var automationData = AutomationData.CreateFromSystemEvent(
                dummySystemEvent, null, MockAutomationData.GetDefaultServiceProvider());

            // Act
            var entityQueryList = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            var list = entityQueryList.ToList();
            list.Should().HaveCount(count);

            // Assert that list contains items from the correct tenancy & environment.
            foreach (QuoteReadModelWithRelatedEntities item in list)
            {
                item.Quote.TenantId.Should().Be(tenantId);
                item.Quote.Environment.Should().Be(environment);
            }
        }

        [Theory]
        [InlineData(5, 1)]
        [InlineData(6, 2)]
        [InlineData(8, 2)]
        public void Should_Return_Paginaged_Result(int? pageSize, int? pageNumber)
        {
            // Arrange
            var queryService = new Mock<IEntityQueryService>();
            var dummyQuoteList = this.GetDummyQuotesWithEnvironment(Guid.NewGuid());
            Func<IQueryable<IQuoteReadModelWithRelatedEntities>,
                IOrderedQueryable<IQuoteReadModelWithRelatedEntities>> order = (q) => q.OrderBy(eq => eq.Quote.CreatedTicksSinceEpoch);

            // Act
            var paginatedListFirstPage = new EntityQueryList<IQuoteReadModelWithRelatedEntities>(
                dummyQuoteList,
                order,
                pageSize,
                pageNumber).ToList();

            // Assert
            (paginatedListFirstPage.Count == pageSize).IsTruthy();
        }

        [Fact]
        public void Should_Not_Throw_Error_If_Result_Smaller_Than_Page_Size()
        {
            // Arrange
            var dummyQuoteList = this.GetDummyQuotesWithEnvironment(Guid.NewGuid());
            Func<IQueryable<IQuoteReadModelWithRelatedEntities>,
                IOrderedQueryable<IQuoteReadModelWithRelatedEntities>> order = (q) => q.OrderBy(eq => eq.Quote.CreatedTicksSinceEpoch);

            // Act
            Action action = () =>
            {
                var paginatedListFirstPage = new EntityQueryList<IQuoteReadModelWithRelatedEntities>(dummyQuoteList, order, 22, 1).ToList();
            };

            // Assert
            action.Should().NotThrow<ErrorException>();
        }

        private IQueryable<IQuoteReadModelWithRelatedEntities> GetDummyQuotesWithEnvironment(Guid tenantId)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var quotes = new List<IQuoteReadModelWithRelatedEntities>
            {
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(tenantId, Guid.NewGuid(), now, DeploymentEnvironment.Development) },
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(tenantId, Guid.NewGuid(), now, DeploymentEnvironment.Staging) },
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(tenantId, Guid.NewGuid(), now, DeploymentEnvironment.Staging) },
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(tenantId, Guid.NewGuid(), now, DeploymentEnvironment.Production) },
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(tenantId, Guid.NewGuid(), now, DeploymentEnvironment.Production) },
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(tenantId, Guid.NewGuid(), now, DeploymentEnvironment.Production) },

                // Add quotes in different tenancy. These should be excluded in the entity query list result.
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(Guid.NewGuid(), Guid.NewGuid(), now, DeploymentEnvironment.Development) },
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(Guid.NewGuid(), Guid.NewGuid(), now, DeploymentEnvironment.Staging) },
                new QuoteReadModelWithRelatedEntities { Quote = new FakeNewQuoteReadModel(Guid.NewGuid(), Guid.NewGuid(), now, DeploymentEnvironment.Production) },
            };

            return quotes.AsQueryable();
        }

        private IQueryable<IQuoteReadModelWithRelatedEntities> GetDummyQuotes(int numberOfQuotes)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            return Enumerable.Range(1, numberOfQuotes)
                .Select(i => new QuoteReadModelWithRelatedEntities
                {
                    Quote = new FakeNewQuoteReadModel(Guid.NewGuid(), Guid.NewGuid(), now.Minus(Duration.FromDays(numberOfQuotes - (i - 1)))),
                })
                .AsQueryable();
        }
    }
}
