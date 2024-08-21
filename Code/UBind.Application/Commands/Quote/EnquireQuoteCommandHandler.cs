// <copyright file="EnquireQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    /// <summary>
    /// Represents the handler for lodging a quote enquiry.
    /// </summary>
    public class EnquireQuoteCommandHandler : ICommandHandler<EnquireQuoteCommand, Unit>
    {
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IAggregateLockingService aggregateLockingService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public EnquireQuoteCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IAggregateLockingService aggregateLockingService)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.aggregateLockingService = aggregateLockingService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(EnquireQuoteCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                quote.UpdateFormData(command.FormData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                quoteAggregate.MakeEnquiry(this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), command.QuoteId);
                await this.quoteAggregateRepository.Save(quoteAggregate);
                return Unit.Value;
            }
        }
    }
}
