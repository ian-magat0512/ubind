// <copyright file="RollbackQuoteAggregateCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.QuoteAggregate
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class RollbackQuoteAggregateCommandHandler : ICommandHandler<RollbackQuoteAggregateCommand, Unit>
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IAggregateSnapshotService<QuoteAggregate> aggregateSnapshotService;
        private readonly IClock clock;

        public RollbackQuoteAggregateCommandHandler(
            IQuoteAggregateRepository quoteAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IAggregateSnapshotService<QuoteAggregate> aggregateSnapshotService,
            IClock clock)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.aggregateSnapshotService = aggregateSnapshotService;
            this.clock = clock;
        }

        public async Task<Unit> Handle(RollbackQuoteAggregateCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            QuoteAggregate quoteAggregate = await this.quoteAggregateRepository.GetByIdWithoutUsingSnapshot(command.TenantId, command.AggregateId);
            EntityHelper.ThrowIfNotFound(quoteAggregate, command.AggregateId, "Quote Aggregate");
            if (command.SequenceNumber >= quoteAggregate.PersistedEventCount)
            {
                throw new ErrorException(Errors.General.NotFound("QuoteAggregate Sequence Number", command.SequenceNumber));
            }

            quoteAggregate.RollbackTo(command.SequenceNumber, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());

            // Don't bother handling concurrency issues here, as if the policy has been modified we probably
            // want to re-inspect it before rolling back.
            await this.quoteAggregateRepository.Save(quoteAggregate);

            // Add a snapshot after the rollback. then load the aggregate after the rollback so we can get the updated aggregate to be snapshotted.
            quoteAggregate = await this.quoteAggregateRepository.GetByIdWithoutUsingSnapshot(command.TenantId, command.AggregateId);
            await this.aggregateSnapshotService.AddAggregateSnapshot(command.TenantId, quoteAggregate, quoteAggregate.PersistedEventCount);

            return Unit.Value;
        }
    }
}
