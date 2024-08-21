// <copyright file="OrFilterProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Filters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class OrFilterProviderTests
    {
        [Fact]
        public async Task Resolve_GivesPositiveResult_WhenSingleFiltersGivePositiveResult()
        {
            // Arrange
            var filters = new List<IFilterProvider>
            {
                new FakeFilterProvider<Data<int>>(_ => false),
                new FakeFilterProvider<Data<int>>(_ => true),
                new FakeFilterProvider<Data<int>>(_ => false),
            };

            var sut = new OrFilterProvider(filters);
            var items = new List<Data<int>> { 1, 2, 3 };

            // Act
            var filter = await sut.Resolve<Data<int>>(
                new ProviderContext(MockAutomationData.CreateWithEventTrigger()),
                new ExpressionScope("foo", Expression.Parameter(typeof(Data<int>))));

            // Assert
            var filteredItems = items.AsQueryable().Where(filter);
            filteredItems.Should().HaveCount(3);
        }

        [Fact]
        public async Task Resolve_GivesNegativeResult_WhenAllFiltersGiveNegativeResult()
        {
            // Arrange
            var filters = new List<IFilterProvider>
            {
                new FakeFilterProvider<Data<int>>(_ => false),
                new FakeFilterProvider<Data<int>>(_ => false),
                new FakeFilterProvider<Data<int>>(_ => false),
            };

            var sut = new AndFilterProvider(filters);
            var items = new List<Data<int>> { 1, 2, 3 };

            // Act
            var filter = await sut.Resolve<Data<int>>(
                new ProviderContext(MockAutomationData.CreateWithEventTrigger()),
                new ExpressionScope("foo", Expression.Parameter(typeof(Data<int>))));

            // Assert
            var filteredItems = items.AsQueryable().Where(filter);
            filteredItems.Should().BeEmpty();
        }
    }
}
