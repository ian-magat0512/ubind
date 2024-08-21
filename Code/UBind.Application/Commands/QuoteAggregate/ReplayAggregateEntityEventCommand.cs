// <copyright file="ReplayAggregateEntityEventCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.QuoteAggregate
{
    using System;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Patterns.Cqrs;

    public class ReplayAggregateEntityEventCommand : ICommand
    {
        public ReplayAggregateEntityEventCommand(
            Guid tenantId,
            Guid aggregateId,
            AggregateEntityType entityType,
            int sequenceNumber,
            bool dispatchToAllObservers = true,
            bool dispatchToReadModelWriters = true,
            bool dispatchToSystemEventEmitters = true)
        {
            this.TenantId = tenantId;
            this.AggregateId = aggregateId;
            this.EntityType = entityType;
            this.SequenceNumber = sequenceNumber;
            this.DispatchToAllObservers = dispatchToAllObservers;
            this.DispatchToReadModelWriters = dispatchToReadModelWriters;
            this.DispatchToSystemEventEmitters = dispatchToSystemEventEmitters;
        }

        public Guid TenantId { get; }

        public Guid AggregateId { get; }

        public AggregateEntityType EntityType { get; }

        public int SequenceNumber { get; }

        public bool DispatchToAllObservers { get; }

        public bool DispatchToReadModelWriters { get; }

        public bool DispatchToSystemEventEmitters { get; }
    }
}
