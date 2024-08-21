// <copyright file="IQueryableExtensionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    public class IQueryableExtensionTests
    {
        private readonly Guid fakeOrganisationId = Guid.NewGuid();
        private readonly Guid fakeTenantId = new Guid("ccae2079-2ebc-4200-879d-866fc82e6afa");
        private readonly Guid fakeProductId = new Guid("ccae2079-2ebc-4200-879d-866fc82e6afb");

        private readonly Guid? performingUserId = Guid.NewGuid();
        private List<NewQuoteReadModel> quoteReadModels;

        public IQueryableExtensionTests()
        {
            this.quoteReadModels = new List<NewQuoteReadModel>();
            for (int i = 0; i < 1002; i++)
            {
                var id = Guid.NewGuid();
                var initializedEvent = new QuoteInitializedEvent(
                    this.fakeTenantId,
                    id,
                    id,
                    this.fakeOrganisationId,
                    this.fakeProductId,
                    DeploymentEnvironment.Development,
                    QuoteType.NewBusiness,
                    this.performingUserId,
                    Instant.MinValue,
                    Timezones.AET,
                    false,
                    null,
                    false,
                    Guid.NewGuid());

                var quote = new NewQuoteReadModel(initializedEvent);
                this.quoteReadModels.Add(quote);
            }
        }

        [Fact]
        public void Paginate_ReturnsCorrectNumberOfItemsForPageSize_WhenOnlyPageSizeIsProvided()
        {
            // Arrange
            EntityListFilters fakeFilter = new EntityListFilters
            {
                PageSize = 37,
            };

            // Act
            var pagedItems = this.quoteReadModels.AsQueryable().Paginate(fakeFilter).ToList();

            // Assert
            Assert.Equal(37, pagedItems.Count);
            Assert.Equal(pagedItems[0].Id, this.quoteReadModels[0].Id);
            Assert.Equal(pagedItems[36].Id, this.quoteReadModels[36].Id);
        }

        [Fact]
        public void Paginate_ReturnsItemsCappedAtOneThousand_WhenThereIsNoPageSizeProvided()
        {
            // Arrange
            EntityListFilters fakeFilter = new EntityListFilters();

            // Act
            var pagedItems = this.quoteReadModels.AsQueryable().Paginate(fakeFilter).ToList();

            // Assert
            Assert.Equal(1000, pagedItems.Count);
            Assert.Equal(pagedItems[0].Id, this.quoteReadModels[0].Id);
            Assert.Equal(pagedItems[999].Id, this.quoteReadModels[999].Id);
        }

        [Fact]
        public void Paginate_ReturnsTheCorrectSetOfItems_WhenTherePageSizeAndPageNumberAreBothProvided()
        {
            // Arrange
            EntityListFilters fakeFilter = new EntityListFilters
            {
                PageSize = 7,
                Page = 3,
            };

            // Act
            var pagedItems = this.quoteReadModels.AsQueryable().Paginate(fakeFilter).ToList();

            // Assert
            Assert.Equal(pagedItems.Count, fakeFilter.PageSize);
            Assert.Equal(pagedItems[0].Id, this.quoteReadModels[14].Id);
            Assert.Equal(pagedItems[6].Id, this.quoteReadModels[20].Id);
        }

        [Fact]
        public void Paginate_ReturnsTheNormalPageSize_WhenShowingTheFirstPage()
        {
            // Arrange
            EntityListFilters fakeFilter = new EntityListFilters
            {
                Page = 1,
            };

            // Act
            var pagedItems = this.quoteReadModels.AsQueryable().Paginate(fakeFilter).ToList();

            // Assert
            Assert.Equal(pagedItems.Count, (int)PageSize.Normal);
            Assert.Equal(pagedItems[0].Id, this.quoteReadModels[0].Id);
            Assert.Equal(pagedItems[49].Id, this.quoteReadModels[49].Id);
        }

        [Fact]
        public void Paginate_ReturnsTheDefaultSize_WhenPageSizeGivenIsMoreThanTheDefault()
        {
            // Arrange
            EntityListFilters fakeFilter = new EntityListFilters
            {
                PageSize = (int)PageSize.Default + 1,
            };

            // Act
            var pagedItems = this.quoteReadModels.AsQueryable().Paginate(fakeFilter).ToList();

            // Assert
            Assert.Equal(pagedItems.Count, (int)PageSize.Default);
        }

        [Fact]
        public void Paginate_ReturnsTheCorrentNumberOfRemainingItems_WhenShowingTheVeryLastPage()
        {
            // Arrange
            EntityListFilters fakeFilter = new EntityListFilters
            {
                Page = 21,
            };

            // Act
            var pagedItems = this.quoteReadModels.AsQueryable().Paginate(fakeFilter).ToList();

            // Assert
            Assert.Equal(2, pagedItems.Count);
            Assert.Equal(pagedItems[0].Id, this.quoteReadModels[1000].Id);
            Assert.Equal(pagedItems[1].Id, this.quoteReadModels[1001].Id);
        }
    }
}
