// <copyright file="MoveQuoteAggregateToOrganisationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    public class MoveQuoteAggregateToOrganisationCommandHandler : ICommandHandler<MoveQuoteAggregateToOrganisationCommand>
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public MoveQuoteAggregateToOrganisationCommandHandler(
            IQuoteAggregateRepository quoteAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<Unit> Handle(MoveQuoteAggregateToOrganisationCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregate = this.quoteAggregateRepository.GetById(request.TenantId, request.QuoteAggregateId);
            if (quoteAggregate == null)
            {
                throw new ErrorException(Errors.General.NotFound("quote aggregate", request.QuoteAggregateId));
            }

            quoteAggregate.RecordOrganisationMigration(
                request.OrganisationId,
                null,
                this.httpContextPropertiesResolver.PerformingUserId,
                this.clock.Now());
            await this.quoteAggregateRepository.Save(quoteAggregate);

            return Unit.Value;
        }
    }
}
