// <copyright file="ListConditionFilterProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Filters
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
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
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class ListConditionFilterProviderTests
    {
        [Fact]
        public async Task ListConditionFilterProvider_CorrectlyFiltersList_BaseOnFilterOfUnrelatedList()
        {
            // Arrange
            var fakeEntities = Enumerable.Range(1, 3).Select(i => new FakeEntity(i));
            var collectionProvider = new Mock<IExpressionProvider>();
            var listExpression = new EntityQueryList<FakeEntity>(
                fakeEntities.AsQueryable(), entities => entities.OrderBy(e => e.X))
                .Query.Expression;
            collectionProvider
                .Setup(p => p.Invoke(It.IsAny<ProviderContext>(), It.IsAny<ExpressionScope>()))
                .Returns(Task.FromResult(listExpression));
            var filterProvider = new FakeFilterProvider<FakeEntity>(e => e.X > 2);
            var sut = new ListConditionFilterProvider(
                collectionProvider.Object,
                new StaticProvider<Data<string>>("foo"),
                filterProvider,
                ListConditionMatchType.Any);
            var fakeEntityCollection = new EntityQueryList<Data<int>>(Enumerable.Range(1, 5).Select(i => new Data<int>(i)).AsQueryable(), ints => ints.OrderBy(i => i.DataValue));
            var automationContext = MockAutomationData.CreateWithEventTrigger();

            // Act
            var filteredCollection = await fakeEntityCollection.Where("myItemAlias", sut, new ProviderContext(automationContext));

            // Assert
            filteredCollection.ToList().Should().HaveCount(5);
        }

        [Fact]
        public async Task ListConditionFilterProvider_CorrectlyFiltersList_BasedOnfilterOfSubList()
        {
            // Arrange
            var fakeEntities = Enumerable
                .Range(1, 5)
                .Select(i => new FakeParentEntity
                {
                    X = i,
                    Children = new Collection<FakeChildEntity> { new FakeChildEntity { X = i }, new FakeChildEntity { X = i + 1 } },
                });
            var collectionProvider = new Mock<IDataListProvider<object>>();
            var collection = new EntityQueryList<FakeParentEntity>(
                fakeEntities.AsQueryable(), entities => entities.OrderBy(e => e.X));
            collectionProvider
                .Setup(p => p.Resolve(It.IsAny<ProviderContext>()))
                .Returns(Task.FromResult(ProviderResult<EntityQueryList<FakeParentEntity>>.Success(collection)).AsITask());
            var childFilterProvider = new FakeFilterProvider<FakeChildEntity>(e => e.X > 3);
            var listExpressionProvider = new ObjectPathLookupExpressionProvider<IEnumerable>(
                new StaticProvider<Data<string>>("#/parent/children"));
            var sut = new ListConditionFilterProvider(
                listExpressionProvider,
                new StaticProvider<Data<string>>("child"),
                childFilterProvider,
                ListConditionMatchType.Any);
            var automationContext = MockAutomationData.CreateWithEventTrigger();
            var filterCollectionProvider = new FilterListItemsListProvider(
                collectionProvider.Object,
                sut,
                new StaticProvider<Data<string>>("parent"));

            // Act
            var filteredCollection = (await filterCollectionProvider.Resolve(new ProviderContext(automationContext))).GetValueOrThrowIfFailed();

            // Assert
            filteredCollection.Should().HaveCount(3);
        }

        [Fact]
        public async Task ListConditionFilterProvider_CorrectlyFiltersList_WhenMatchTypeIsAll()
        {
            // Arrange
            var fakeEntities = Enumerable
                .Range(1, 5)
                .Select(i => new FakeParentEntity
                {
                    X = i,
                    Children = new Collection<FakeChildEntity> { new FakeChildEntity { X = i }, new FakeChildEntity { X = i + 1 } },
                });
            var collectionProvider = new Mock<IDataListProvider<object>>();
            var collection = new EntityQueryList<FakeParentEntity>(
                fakeEntities.AsQueryable(), entities => entities.OrderBy(e => e.X));
            collectionProvider
                .Setup(p => p.Resolve(It.IsAny<ProviderContext>()))
                .Returns(Task.FromResult(ProviderResult<EntityQueryList<FakeParentEntity>>.Success(collection)).AsITask());
            var childFilterProvider = new FakeFilterProvider<FakeChildEntity>(e => e.X > 3);
            var listExpressionProvider = new ObjectPathLookupExpressionProvider<IEnumerable>(
                new StaticProvider<Data<string>>("#/parent/children"));
            var sut = new ListConditionFilterProvider(
                listExpressionProvider,
                new StaticProvider<Data<string>>("child"),
                childFilterProvider,
                ListConditionMatchType.All);
            var automationContext = MockAutomationData.CreateWithEventTrigger();
            var filterCollectionProvider = new FilterListItemsListProvider(
                collectionProvider.Object,
                sut,
                new StaticProvider<Data<string>>("parent"));

            // Act
            var filteredCollection = (await filterCollectionProvider.Resolve(new ProviderContext(automationContext))).GetValueOrThrowIfFailed();

            // Assert
            filteredCollection.Should().HaveCount(2);
        }

        public class FakeEntity
        {
            public FakeEntity(int ix) => this.X = ix;

            public int X { get; }
        }

        public class FakeParentEntity
        {
            public int X { get; set; }

            public ICollection<FakeChildEntity> Children { get; set; }
        }

        public class FakeChildEntity
        {
            public int X { get; set; }
        }
    }
}
