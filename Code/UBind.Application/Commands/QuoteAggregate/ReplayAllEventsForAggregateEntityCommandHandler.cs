// <copyright file="ReplayAllEventsForAggregateEntityCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.QuoteAggregate
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Application.Commands.Entity;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class ReplayAllEventsForAggregateEntityCommandHandler : ICommandHandler<ReplayAllEventsForAggregateEntityCommand, Unit>
    {
        private readonly IAggregateRepositoryResolver resolver;
        private readonly IUBindDbContext dbContext;

        public ReplayAllEventsForAggregateEntityCommandHandler(
            IAggregateRepositoryResolver resolver,
            IUBindDbContext dbContext)
        {
            this.resolver = resolver;
            this.dbContext = dbContext;
        }

        public async Task<Unit> Handle(ReplayAllEventsForAggregateEntityCommand command, CancellationToken cancellationToken)
        {
            List<Type> observerTypes = EventObserverHelper.GetObserverTypesForDispatchFlags(
                command.DispatchToAllObservers,
                command.DispatchToReadModelWriters,
                command.DispatchToSystemEventEmitters);
            var repository = this.resolver.Resolve(command.EntityType);
            await repository.ReplayAllEventsByAggregateId(
                command.TenantId,
                command.AggregateId,
                observerTypes,
                null);
            this.dbContext.SaveChanges();
            return Unit.Value;
        }
    }
}
