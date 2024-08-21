// <copyright file="QuoteAggregateEventReplayTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using UBind.Application.SystemEvents;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Events;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.SystemEvents;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    [SystemEventTypeExtensionInitialize]
    public class QuoteAggregateEventReplayTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly IUBindDbContext dbContext;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteSystemEventEmitter quoteSystemEventEmitter;
        private readonly Mock<ISystemEventService> mockSystemEventService = new Mock<ISystemEventService>();
        private readonly Mock<IEventPayloadFactory> mockEventPayloadFactory = new Mock<IEventPayloadFactory>();
        private readonly Mock<IAggregateSnapshotService<QuoteAggregate>> aggregateSnapshotService;

        public QuoteAggregateEventReplayTests()
        {
            this.dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var connectionConfig = new ConnectionStrings();
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            connectionConfig.UBind = config.GetConnectionString(DatabaseFixture.TestConnectionStringName);
            this.quoteSystemEventEmitter = new QuoteSystemEventEmitter(this.mockSystemEventService.Object, this.mockEventPayloadFactory.Object, this.clock);
            this.aggregateSnapshotService = new Mock<IAggregateSnapshotService<QuoteAggregate>>();
            var eventRecordRepository = new EventRecordRepository(this.dbContext, connectionConfig);
            this.quoteAggregateRepository = new QuoteAggregateRepository(
                this.dbContext,
                eventRecordRepository,
                this.quoteSystemEventEmitter,
                this.aggregateSnapshotService.Object,
                this.clock,
                NullLogger<QuoteAggregateRepository>.Instance,
                new Mock<IServiceProvider>().AddLoggers().Object);
        }

        [Fact]
        public async Task Replaying_aggregate_event_triggers_system_event_to_be_emitted()
        {
            // Arrange
            var quoteAggregate = QuoteFactory.CreateNewPolicy(TenantFactory.DefaultId);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            this.mockSystemEventService
                .Setup(s => s.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce<QuoteAggregate>(
                    It.IsAny<QuoteAggregate>(),
                    It.IsAny<List<SystemEvent>>()));
            this.mockSystemEventService.Invocations.Clear();

            // Act
            await this.quoteAggregateRepository.ReplayEventByAggregateId(
                    quoteAggregate.TenantId,
                    quoteAggregate.Id,
                    5,
                    new List<Type> { typeof(ISystemEventEmitter) });

            // Assert
            this.mockSystemEventService.Verify(
                v => v.OnAggregateSavedPersistAndEmitSystemEventsInBackgroundOnce<QuoteAggregate>(
                    It.IsAny<QuoteAggregate>(),
                    It.IsAny<List<SystemEvent>>()),
                Times.Once);
        }
    }
}
