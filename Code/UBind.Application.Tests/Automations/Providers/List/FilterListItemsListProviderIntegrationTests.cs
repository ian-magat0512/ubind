// <copyright file="FilterListItemsListProviderIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Collections
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using MorseCode.ITask;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Extensions;
    using Xunit;

    public class FilterListItemsListProviderIntegrationTests
    {
        [Fact]
        public async Task Resolve_ReturnsCorrectList_WhenConditionsIncludeNestedListConditionsWithDefaultAliases()
        {
            // Arrange
            var listProvider = new Mock<IDataListProvider<object>>();
            listProvider
                .Setup(p => p.Resolve(It.IsAny<ProviderContext>()))
                .Returns(Task.FromResult(ProviderResult<EntityQueryList<Foo>>.Success(this.GenerateDummyList())).AsITask());
            var filterProvider = new ListConditionFilterProvider(
                new ObjectPathLookupExpressionProvider<IEnumerable>(new StaticProvider<Data<string>>("#/item/bars")),
                null,
                new BinaryExpressionFilterProvider(
                    (a, b) => Expression.LessThan(a, b),
                    new ObjectPathLookupExpressionProvider<int>(
                        new StaticProvider<Data<string>>("#/item1/y")),
                    new ConstantExpressionProvider(new StaticProvider<Data<int>>(15)),
                    "integerIsLessThanCondition"),
                ListConditionMatchType.All);

            var sut = new FilterListItemsListProvider(listProvider.Object, filterProvider);
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();

            // Act
            var entityCollection = (await sut.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Asset
            entityCollection.ToList().Should().HaveCount(5);
        }

        private EntityQueryList<Foo> GenerateDummyList()
        {
            var foos = Enumerable.Range(1, 10).Select(i => new Foo(i)).AsQueryable();
            return new EntityQueryList<Foo>(foos, fs => fs.OrderBy(f => f.X));
        }

        public class Foo
        {
            public Foo(int x)
            {
                this.X = x;
                this.Bars = Enumerable.Range(x, 10).Select(i => new Bar(i)).ToList();
            }

            public int X { get; }

            public List<Bar> Bars { get; }
        }

        public class Bar
        {
            public Bar(int y)
            {
                this.Y = y;
                this.Bazzes = Enumerable.Range(y, 10).Select(i => new Baz(i)).ToList();
            }

            public int Y { get; }

            public List<Baz> Bazzes { get; }
        }

        public class Baz
        {
            public Baz(int z)
            {
                this.Z = z;
            }

            public int Z { get; }
        }
    }
}
