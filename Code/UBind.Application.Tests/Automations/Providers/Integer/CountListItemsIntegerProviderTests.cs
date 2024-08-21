// <copyright file="CountListItemsIntegerProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Integer
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Integer;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class CountListItemsIntegerProviderTests
    {
        [Fact]
        public async Task Resolve_ReturnsCorrectNumber_ForCollectionWithItems()
        {
            // Arrange
            var fakeEntities = Enumerable.Range(1, 3).Select(i => new FakeEntity(i));
            var collectionProvider = new Mock<IDataListProvider<object>>();
            var items = new EntityQueryList<FakeEntity>(fakeEntities.AsQueryable(), entities => entities.OrderBy(e => e.X));
            collectionProvider
                .Setup(p => p.Resolve(It.IsAny<ProviderContext>()))
                .Returns(Task.FromResult(ProviderResult<EntityQueryList<FakeEntity>>.Success(items)).AsITask());
            var sut = new CountListItemsIntegerProvider(collectionProvider.Object);

            // Act
            long count = (await sut.Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            count.Should().Be(3);
        }

        private class FakeEntity
        {
            public FakeEntity(int ix) => this.X = ix;

            public int X { get; }
        }
    }
}
