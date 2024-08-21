// <copyright file="ValidateQuoteCalculationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.QuoteCalculation
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Perform validation of all of the data before performing a calculation.
    /// </summary>
    public class ValidateQuoteCalculationCommandHandler<TRequest, TResponse>
        : IPipelineBehavior<QuoteCalculationCommand, CalculationResponseModel>
         where TRequest : QuoteCalculationCommand
    {
        private readonly IClock clock;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IAggregateLockingService aggregateLockingService;
        private readonly ICqrsMediator mediator;

        public ValidateQuoteCalculationCommandHandler(
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IQuoteAggregateRepository quoteAggregateRepository,
            IAggregateLockingService aggregateLockingService,
            IClock clock,
            ICqrsMediator mediator)
        {
            this.clock = clock;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.aggregateLockingService = aggregateLockingService;
            this.mediator = mediator;
        }

        public async Task<CalculationResponseModel> Handle(
            QuoteCalculationCommand request,
            RequestHandlerDelegate<CalculationResponseModel> next,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Guid? quoteAggregateId = null;
            if (request.QuoteId != null)
            {
                quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(request.QuoteId.Value);
            }
            else if (request.PolicyId != null)
            {
                quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForPolicyId(request.PolicyId.Value);
            }

            // permits optional aggregate locking for this command since it could be that the quote Id and policy Id are not in the request,
            // instead it can be from an input data of an automation and it can also be that the results are not to be persisted.
            using (quoteAggregateId.HasValue ? await this.aggregateLockingService.CreateLockOrThrow(request.ProductContext.TenantId, quoteAggregateId.Value, AggregateType.Quote) : null)
            {
                if (request.QuoteId != null && quoteAggregateId.HasValue)
                {
                    var quoteAggregate = this.quoteAggregateRepository.GetById(request.ProductContext.TenantId, quoteAggregateId.Value);
                    quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId.Value, "quote aggregate");
                    this.ValidateProductContext(request, quoteAggregate);
                    request.Quote = quoteAggregate.GetQuoteOrThrow(request.QuoteId.Value);

                    // Check for quote type mismatch
                    if (request.QuoteType != null && request.Quote.Type != request.QuoteType)
                    {
                        throw new ErrorException(Errors.Calculation.QuoteTypeMismatch(
                            request.QuoteType.Value, request.Quote.Type, request.Quote.ProductContext));
                    }

                    request.QuoteType = request.Quote.Type;
                    request.Policy = quoteAggregate.Policy;
                    request.ProductReleaseId = request.Quote.ProductReleaseId;
                }
                else if (request.PolicyId != null && quoteAggregateId.HasValue)
                {
                    var quoteAggregate = this.quoteAggregateRepository.GetById(request.ProductContext.TenantId, quoteAggregateId.Value);
                    quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId.Value, "quote aggregate");
                    this.ValidateProductContext(request, quoteAggregate);
                    request.Policy = quoteAggregate.Policy;
                }

                if (request.QuoteId == null && request.QuoteType == null)
                {
                    // check for a missing quote type if just a policy ID was passed in
                    if (request.Policy != null)
                    {
                        throw new ErrorException(Errors.Calculation.MissingPolicyTransactionType(
                            request.Policy.Aggregate.ProductContext, request.Policy.PolicyNumber));
                    }
                    else
                    {
                        request.QuoteType = QuoteType.NewBusiness;
                    }
                }

                // if we don't have a product release ID, let's get the right one
                request.ProductReleaseId = request.ProductReleaseId
                    ?? await this.mediator.Send(new GetProductReleaseIdQuery(
                        request.ProductContext.TenantId,
                        request.ProductContext.ProductId,
                        request.ProductContext.Environment,
                        request.QuoteType ?? QuoteType.NewBusiness,
                        request.Policy?.PolicyId ?? request.PolicyId));

                if (request.Quote != null)
                {
                    // If a quote was passed, ensure it wasn't discarded
                    if (request.Quote.IsDiscarded)
                    {
                        throw new ErrorException(Errors.Calculation.QuoteDiscarded(request.Quote.Id, request.ReleaseContext));
                    }

                    string quoteState = request.Quote.QuoteStatus;

                    // If a quote was passed, ensure it's not completed
                    if (quoteState.EqualsIgnoreCase(StandardQuoteStates.Complete))
                    {
                        throw new ErrorException(Errors.Calculation.QuoteCompleted(request.Quote.Id, request.ReleaseContext));
                    }

                    // If a quote was passed, ensure it's not declined
                    if (quoteState.EqualsIgnoreCase(StandardQuoteStates.Declined))
                    {
                        throw new ErrorException(Errors.Calculation.QuoteDeclined(request.Quote.Id, request.ReleaseContext));
                    }

                    // If a quote was passed, ensure it's not expired
                    if (request.Quote.IsExpired(this.clock.Now()))
                    {
                        throw new ErrorException(Errors.Calculation.QuoteExpired(request.Quote.Id, request.ReleaseContext));
                    }
                }

                // If we are supposed to persist results, there needs to be a quote
                if (request.PersistResults && request.Quote == null)
                {
                    throw new ErrorException(Errors.Calculation.CannotPersistResultsWithoutQuote(request.ReleaseContext));
                }

                // Check that policy has been passed when it's an adjustment, cancellation or renewal quote
                if (request.QuoteType != QuoteType.NewBusiness)
                {
                    if (request.Policy == null && request.Quote == null)
                    {
                        if (request.QuoteType.HasValue)
                        {
                            throw new ErrorException(Errors.Calculation.MisingQuoteOrPolicy(request.QuoteType.Value, request.ReleaseContext));
                        }
                    }
                }

                // infer the policy if we can
                if (request.QuoteType != QuoteType.NewBusiness && request.Policy == null && request.Quote != null)
                {
                    request.Policy = request.Quote.Aggregate.Policy;
                }

                return await next();
            }
        }

        /// <summary>
        /// Checks that the quote aggregate's product context matches the request, and throws an exception if it doesn't.
        /// </summary>
        private void ValidateProductContext(
            QuoteCalculationCommand request,
            QuoteAggregate quoteAggregate)
        {
            if (quoteAggregate.TenantId != request.ProductContext.TenantId
                || quoteAggregate.ProductId != request.ProductContext.ProductId)
            {
                throw new ErrorException(Errors.General.Forbidden($"access a quote or policy from a different tenancy or product"));
            }

            if (quoteAggregate.Environment != request.ProductContext.Environment)
            {
                throw new ErrorException(Errors.Operations.EnvironmentMisMatch("quote or policy"));
            }
        }
    }
}
