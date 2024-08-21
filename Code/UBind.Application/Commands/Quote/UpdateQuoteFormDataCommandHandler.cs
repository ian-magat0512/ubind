// <copyright file="UpdateQuoteFormDataCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    public class UpdateQuoteFormDataCommandHandler : ICommandHandler<UpdateQuoteFormDataCommand, Unit>
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IClock clock;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IAggregateLockingService aggregateLockingService;

        public UpdateQuoteFormDataCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IProductConfigurationProvider productConfigurationProvider,
            IQuoteAggregateRepository quoteAggregateRepository,
            IReleaseQueryService releaseQueryService,
            IAggregateLockingService aggregateLockingService,
            IClock clock)
        {
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.productConfigurationProvider = productConfigurationProvider;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.releaseQueryService = releaseQueryService;
            this.aggregateLockingService = aggregateLockingService;
        }

        public async Task<Unit> Handle(UpdateQuoteFormDataCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                    quote.Aggregate.TenantId,
                    quote.Aggregate.ProductId,
                    quote.Aggregate.Environment,
                    quote.ProductReleaseId);
                var productConfiguration = await this.productConfigurationProvider.GetProductConfiguration(releaseContext, WebFormAppType.Quote);
                var quoteDataRetriever = quote.LatestCalculationResult == null
                    ? null
                    : new StandardQuoteDataRetriever(productConfiguration, command.FormData, quote.LatestCalculationResult.Data);
                quote.UpdateFormData(command.FormData, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                var quoteTitle = quoteDataRetriever?.Retrieve(StandardQuoteDataField.QuoteTitle);
                if (quoteTitle != null && quote.QuoteTitle != quoteTitle)
                {
                    quote.SetQuoteTitle(quoteTitle, this.httpContextPropertiesResolver.PerformingUserId.GetValueOrDefault(), this.clock.Now());
                }

                await this.quoteAggregateRepository.Save(quoteAggregate);
                return Unit.Value;
            }
        }
    }
}
