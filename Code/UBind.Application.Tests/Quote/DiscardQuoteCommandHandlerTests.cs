// <copyright file="DiscardQuoteCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Quote
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.Quote;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class DiscardQuoteCommandHandlerTests
    {
        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepositoryMock
            = new Mock<IQuoteAggregateRepository>();

        private readonly Mock<IQuoteAggregateResolverService> quoteAggregateResolverService
            = new Mock<IQuoteAggregateResolverService>();

        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolverMock
            = new Mock<IHttpContextPropertiesResolver>();

        private IClock clock;

        public DiscardQuoteCommandHandlerTests()
        {
            this.clock = new TestClock();
        }

        [Fact]
        public async Task DiscardQuoteCommand_MakesQuoteDiscarded()
        {
            // Arrange
            var adjustmentQuote = QuoteFactory.CreateNewPolicy().WithAdjustmentQuote();
            var quoteAggregate = adjustmentQuote.Aggregate;
            quoteAggregate.GetLatestQuote().Should().NotBeNull();
            this.quoteAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(quoteAggregate);
            this.quoteAggregateResolverService.Setup(s => s.GetQuoteAggregateForQuote(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(quoteAggregate);
            var command = new DiscardQuoteCommand(
                TenantFactory.DefaultId,
                adjustmentQuote.Id);
            var handler = new DiscardQuoteCommandHandler(
                this.quoteAggregateRepositoryMock.Object,
                this.quoteAggregateResolverService.Object,
                this.httpContextPropertiesResolverMock.Object,
                this.clock);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            quoteAggregate.GetLatestQuote().IsDiscarded.Should().BeTrue();
        }
    }
}
