// <copyright file="AcceptFundingProposalCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.CustomPipelines.BindPolicy
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using MediatR;
    using NodaTime;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Funding;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.CustomPipelines.BindPolicy;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Services;

    public class AcceptFundingProposalCommandHandler
        : ICommandHandler<AcceptFundingProposalCommand, Unit>,
            IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IFundingConfigurationProvider fundingConfigurationProvider;
        private readonly FundingServiceFactory fundingServiceFactory;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IAggregateLockingService aggregateLockingService;

        public AcceptFundingProposalCommandHandler(
            ICachingResolver cachingResolver,
            FundingServiceFactory fundingServiceFactory,
            IFundingConfigurationProvider fundingConfigurationProvider,
            IQuoteAggregateResolverService aggregateResolver,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteWorkflowProvider quoteWorkflowProvider,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IAggregateLockingService aggregateLockingService,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.fundingServiceFactory = fundingServiceFactory;
            this.fundingConfigurationProvider = fundingConfigurationProvider;
            this.quoteAggregateResolverService = aggregateResolver;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteWorkflowProvider = quoteWorkflowProvider;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.aggregateLockingService = aggregateLockingService;
        }

        public async Task<Unit> Handle(AcceptFundingProposalCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);

            // Since this is not called as part of the BindPolicyCommand pipeline and it's called directly from
            // `PremiumFundingController.AcceptProposalWithPayment`, then it won't have called `ValidateBindPolicyCommandHandler`
            // first, so no aggregate lock would have been attained yet. So we need to get a lock now:
            using (await this.aggregateLockingService.CreateLockOrThrow(command.ReleaseContext.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.ReleaseContext.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var acceptedFundingProposal = await this.AcceptProposal(
                    command.ReleaseContext,
                    quoteAggregate,
                    command.QuoteId,
                    command.PremiumFundingProposalId,
                    command.PaymentMethodDetails,
                    quoteAggregate.IsTestData,
                    cancellationToken);
                quoteAggregate.RecordFundingProposalAccepted(
                    command.PremiumFundingProposalId,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    this.clock.Now(),
                    command.QuoteId);
                await this.quoteAggregateRepository.Save(quoteAggregate);
            }
            return Unit.Value;
        }

        public async Task<ValueTuple<NewQuoteReadModel, PolicyReadModel>> Handle(
            BindPolicyCommand command,
            RequestHandlerDelegate<ValueTuple<NewQuoteReadModel, PolicyReadModel>> next,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Guid quoteId = EntityHelper.ThrowIfNotFound(command.QuoteId ?? Guid.Empty, "QuoteId");

            if (command.QuoteAggregate != null)
            {
                var quoteAggregate = command.QuoteAggregate;
                var quote = quoteAggregate.GetQuoteOrThrow(quoteId);

                // already paid or funded in previous operation.
                if (quote.IsFunded || quote.IsPaidFor)
                {
                    return await next();
                }

                // Since 'HaveTriedPersistingCommandBefore' flag is set to true, then this command has been
                // handled initially for persisting by the SaveBindCommandHandler.
                // We are skipping acceptance of funding proposal here because it has already been done.
                if (command.HaveTriedPersistingCommandBefore && command.AcceptedFundingProposal != null)
                {
                    return await next();
                }

                // no funding proposal id, thus does not need funding
                if (!command.FundingProposalId.HasValue)
                {
                    return await next();
                }

                var paymentMethodDetails = EntityHelper.ThrowIfNotFound(command.PaymentMethodDetails, "PaymentMethodDetails");

                command.AcceptedFundingProposal = await this.AcceptProposal(
                    command.ReleaseContext,
                    quoteAggregate,
                    quoteId,
                    command.FundingProposalId.Value,
                    paymentMethodDetails,
                    quoteAggregate.IsTestData);

                return await next();
            }

            return await next();
        }

        private async Task<FundingProposal> AcceptProposal(
            ReleaseContext releaseContext,
            QuoteAggregate quoteAggregate,
            Guid quoteId,
            Guid premiumFundingProposalId,
            IPaymentMethodDetails paymentDetails,
            bool isTestData = false,
            CancellationToken cancellationToken = default)
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            if (!quote.IsFundingAcceptanceAdmissible(quoteWorkflow))
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.Fund);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quote.QuoteStatus,
                    operation.ResultingState ?? string.Empty,
                    operation.RequiredStates));
            }

            var maybeConfiguration = await this.fundingConfigurationProvider.GetConfigurationAsync(releaseContext);
            if (maybeConfiguration.HasNoValue)
            {
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(quoteAggregate.TenantId);
                var isMutual = TenantHelper.IsMutual(tenantAlias);
                throw new ErrorException(Errors.Payment.Funding.NotConfigured(
                    quote.ProductContext.Environment,
                    quote.ProductContext.ProductId,
                    isMutual));
            }

            var fundingConfiguration = maybeConfiguration.Value;
            var fundingService = this.fundingServiceFactory.Create(fundingConfiguration, releaseContext);
            return await fundingService.AcceptFundingProposal(
                quote, premiumFundingProposalId, paymentDetails, isTestData, cancellationToken);
        }
    }
}
