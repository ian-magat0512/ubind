// <copyright file="DiscardQuoteCommandHandler.cs" company="uBind">
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
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class DiscardQuoteCommandHandler : ICommandHandler<DiscardQuoteCommand>
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public DiscardQuoteCommandHandler(
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<Unit> Handle(DiscardQuoteCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(request.TenantId, request.QuoteId);
            quoteAggregate.Discard(request.QuoteId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return await Task.FromResult(Unit.Value);
        }
    }
}
