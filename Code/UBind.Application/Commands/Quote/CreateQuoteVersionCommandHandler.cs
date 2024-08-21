// <copyright file="CreateQuoteVersionCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    /// <summary>
    /// Represents the handler for creating new quote version.
    /// </summary>
    public class CreateQuoteVersionCommandHandler : ICommandHandler<CreateQuoteVersionCommand, NewQuoteReadModel>
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly IAggregateLockingService aggregateLockingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQuoteVersionCommandHandler"/> class.
        /// </summary>
        /// <param name="quoteAggregateRepository">The application repository.</param>
        /// <param name="httpContextPropertiesResolver">The current identification.</param>
        /// <param name="clock">A clock for obtianing the current time.</param>
        /// <param name="quoteAggregateResolverService">The service for resolving a quote aggregate from a quote ID.</param>
        /// <param name="productFeatureSettingService">The product feature setting service.</param>
        /// <param name="mediator"><see cref="ICqrsMediator"/>.</param>
        public CreateQuoteVersionCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IAggregateLockingService aggregateLockingService,
            IProductFeatureSettingService productFeatureSettingService)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.productFeatureSettingService = productFeatureSettingService;
            this.aggregateLockingService = aggregateLockingService;
        }

        /// <inheritdoc/>
        public async Task<NewQuoteReadModel> Handle(CreateQuoteVersionCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                var timestamp = this.clock.Now();
                quote.UpdateFormData(command.FormData, this.httpContextPropertiesResolver.PerformingUserId, timestamp);
                quoteAggregate.CreateVersion(this.httpContextPropertiesResolver.PerformingUserId, timestamp, quote.Id);
                await this.quoteAggregateRepository.Save(quoteAggregate);
                return quote.ReadModel;
            }
        }
    }
}
