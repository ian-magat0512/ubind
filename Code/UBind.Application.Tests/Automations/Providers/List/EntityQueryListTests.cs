// <copyright file="EntityQueryListTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.List
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Automations.Filters;
    using UBind.Application.Tests.Fakes;
    using Xunit;

    public class EntityQueryListTests
    {
        [Fact]
        public void EntityCollection_CanBeEnumerated()
        {
            // Arrange
            var query = Enumerable.Range(1, 10)
                .Select(i => new TestEntity(i))
                .AsQueryable();
            var sut = new EntityQueryList<TestEntity>(query, q => q.OrderBy(e => e.X));

            // Act
            var enumerationCount = 0;
            foreach (var entity in sut)
            {
                ++enumerationCount;
            }

            // Asset
            enumerationCount.Should().Be(10);
        }

        [Fact]
        public void Count_CountsItemsCorrectly()
        {
            // Arrange
            var query = Enumerable.Range(1, 10)
                .Select(i => new TestEntity(i))
                .AsQueryable();
            var sut = new EntityQueryList<TestEntity>(query, q => q.OrderBy(e => e.X));

            // Act
            int count = sut.Count();

            // Asset
            count.Should().Be(10);
        }

        [Fact]
        public async Task Where_FiltersItemsCorrectly()
        {
            // Arrange
            var query = Enumerable.Range(1, 10)
                .Select(i => new TestEntity(i))
                .AsQueryable();
            var filterProvider = new FakeFilterProvider<TestEntity>(e => e.X > 3);
            var systemEvent = FakeEventFactory.Create();
            var dataContext = AutomationData.CreateFromSystemEvent(
                systemEvent, null, MockAutomationData.GetDefaultServiceProvider());
            var sut = new EntityQueryList<TestEntity>(query, q => q.OrderBy(e => e.X));

            // Act
            var filteredCollection = await sut.Where("myItemAlias", filterProvider, new ProviderContext(dataContext));

            // Asset
            filteredCollection.Should().HaveCount(7);
        }

        [Fact]
        public async Task FirstOrDefault_ReturnsFirstMatchingItem_WhenOneExists()
        {
            // Arrange
            var query = Enumerable.Range(1, 10)
                .Select(i => new TestEntity(i))
                .AsQueryable();
            var filterProvider = new FakeFilterProvider<TestEntity>(e => e.X > 3);
            var systemEvent = FakeEventFactory.Create();
            var dataContext = AutomationData.CreateFromSystemEvent(
                systemEvent, null, MockAutomationData.GetDefaultServiceProvider());
            var sut = new EntityQueryList<TestEntity>(query, q => q.OrderBy(e => e.X));

            // Act
            var item = await sut.FirstOrDefault("myItemAlias", filterProvider, new ProviderContext(dataContext));

            // Asset
            item.X.Should().Be(4);
        }

        [Fact]
        public async Task FirstOrDefault_ReturnsNull_WhenNoMatchingItemExistsAsync()
        {
            // Arrange
            var query = Enumerable.Range(1, 10)
                .Select(i => new TestEntity(i))
                .AsQueryable();
            var filterProvider = new FakeFilterProvider<TestEntity>(e => e.X > 10);
            var systemEvent = FakeEventFactory.Create();
            var dataContext = AutomationData.CreateFromSystemEvent(
                systemEvent, null, MockAutomationData.GetDefaultServiceProvider());
            var sut = new EntityQueryList<TestEntity>(query, q => q.OrderBy(e => e.X));

            // Act
            var item = await sut.FirstOrDefault("myItemAlias", filterProvider, new ProviderContext(dataContext));

            // Asset
            item.Should().BeNull();
        }
    }
}
