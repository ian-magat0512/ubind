// <copyright file="ApplicationFundingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Funding;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.Services;

    /// <inheritdoc />
    public class ApplicationFundingService : IApplicationFundingService, IDomainFundingService
    {
        private readonly IFundingConfigurationProvider fundingConfigurationProvider;
        private readonly FundingServiceFactory fundingServiceFactory;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationFundingService"/> class.
        /// </summary>
        /// <param name="fundingConfigurationProvider">The payment configuration provider for the product.</param>
        /// <param name="fundingServiceFactory">A factory for creating the payment gateway.</param>
        /// <param name="quoteAggregateRepository">The quote aggregate repository.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="clock">A clock for obtianing the current time.</param>
        /// <param name="quoteAggregateResolverService">The service to resolve the quote aggregate for a given quote ID.</param>
        public ApplicationFundingService(
            IFundingConfigurationProvider fundingConfigurationProvider,
            FundingServiceFactory fundingServiceFactory,
            IQuoteAggregateRepository quoteAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IQuoteAggregateResolverService quoteAggregateResolverService)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.clock = clock;
            this.fundingConfigurationProvider = fundingConfigurationProvider;
            this.fundingServiceFactory = fundingServiceFactory;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
        }

        /// <inheritdoc/>
        public async Task<IFundingService> GetFundingService(ReleaseContext releaseContext)
        {
            var maybeConfiguration = await this.fundingConfigurationProvider.GetConfigurationAsync(releaseContext);
            if (maybeConfiguration.HasNoValue)
            {
                return null;
            }

            var fundingService = this.fundingServiceFactory.Create(maybeConfiguration.Value, releaseContext);
            return fundingService;
        }

        /// <inheritdoc/>
        public async Task<QuoteAggregate> RecordExternalAcceptanceByQuote(Guid tenantId, Guid quoteId, Guid internalUBindProposalId)
        {
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
            return await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                async () => await this.RecordFundingProposalAsAccepted(quoteAggregate, internalUBindProposalId, quoteId),
                () => quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId));
        }

        private async Task<QuoteAggregate> RecordFundingProposalAsAccepted(
            QuoteAggregate quoteAggregate, Guid internalUBindProposalId, Guid quoteId)
        {
            quoteAggregate.RecordFundingProposalAccepted(
                internalUBindProposalId, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quoteId);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quoteAggregate;
        }
    }
}
