// <copyright file="CompletePolicyTransactionCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Policy
{
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Commands.Policy;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    /// <summary>
    /// Completes the quote transaction and issues a policy.
    /// </summary>
    /// <remarks>When used during the bind operation, persistence is left with the SaveBindCommandHandler pipeline.</remarks>
    public class CompletePolicyTransactionCommandHandler : ICommandHandler<CompletePolicyTransactionCommand, NewQuoteReadModel>
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IPolicyService policyService;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IClock clock;
        private readonly IAggregateLockingService aggregateLockingService;

        public CompletePolicyTransactionCommandHandler(
            IHttpContextPropertiesResolver contextPropertiesResolver,
            IQuoteAggregateResolverService quotequoteAggregateResolver,
            IQuoteAggregateRepository quotequoteAggregateRepository,
            IPolicyService policyService,
            IReleaseQueryService releaseQueryService,
            IClock clock,
            IAggregateLockingService aggregateLockingService)
        {
            this.httpContextPropertiesResolver = contextPropertiesResolver;
            this.quoteAggregateResolverService = quotequoteAggregateResolver;
            this.quoteAggregateRepository = quotequoteAggregateRepository;
            this.policyService = policyService;
            this.releaseQueryService = releaseQueryService;
            this.clock = clock;
            this.aggregateLockingService = aggregateLockingService;
        }

        /// <summary>
        /// Handles the completion of a policy transaction.
        /// </summary>
        public async Task<NewQuoteReadModel> Handle(CompletePolicyTransactionCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                var now = this.clock.Now();
                if (quote.IsExpired(now))
                {
                    throw new ErrorException(Errors.Policy.Issuance.InvalidStateDetected(quote.QuoteStatus));
                }

                var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
                quote.UpdateFormData(command.LatestFormData, performingUserId, now);
                var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                    command.TenantId,
                    quote.Aggregate.ProductId,
                    quote.Aggregate.Environment,
                    quote.ProductReleaseId);
                await this.policyService.CompletePolicyTransaction(
                    releaseContext,
                    quote,
                    command.CalculationResultId,
                    cancellationToken,
                    command.LatestFormData,
                   progressQuoteState: command.ProgressQuoteState);
                await this.quoteAggregateRepository.Save(quoteAggregate);
                return quote.ReadModel;
            }
        }
    }
}
