// <copyright file="SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <summary>
    /// Command handler for generating default organisation to existing quotes and its related records based from tenancy.
    /// </summary>
    public class SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommandHandler
        : ICommandHandler<SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommand, Unit>
    {
        private readonly ILogger<SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommandHandler> logger;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommandHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The repository for tenants.</param>
        /// <param name="quoteAggregateRepository">The repository for quote aggregates.</param>
        /// <param name="quoteReadModelRepository">The repository for quote read models.</param>
        /// <param name="quoteAggregateResolverService">The resolver service for quote aggregates.</param>
        /// <param name="httpContextPropertiesResolver">The resolver for performing user.</param>
        /// <param name="clock">Represents the clock which can return the time as <see cref="Instant"/>.</param>
        public SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommandHandler(
            ITenantRepository tenantRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            ILogger<SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommandHandler> logger,
            IClock clock)
        {
            this.logger = logger;
            this.policyReadModelRepository = policyReadModelRepository;
            this.tenantRepository = tenantRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.SetForQuotes(cancellationToken);
            await this.SetForPolicies(cancellationToken);

            return Unit.Value;
        }

        private async Task SetForQuotes(CancellationToken cancellationToken)
        {
            var tenants = this.tenantRepository.GetTenants();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Set Default Organisation for Quotes of tenant {tenant.Id}");

                // we leave no policy unturned.
                var quotes = this.quoteReadModelRepository.ListQuotesForOrganisationMigration(tenant.Id).ToList();

                int quoteCount = 0;
                foreach (var quote in quotes)
                {
                    quoteCount++;
                    if (quoteCount % 1000 == 0)
                    {
                        // log only every 1000 records.
                        this.logger.LogInformation($"Processing Quotes: {quoteCount}/{quotes.Count}");
                    }

                    int retryTimes = 0;
                    async Task AssignDefaultOrganisationIdToQuote()
                    {
                        try
                        {
                            // this is a defect in the records where aggregate ids sometimes are empty.
                            var aggregateId = quote.AggregateId != default ?
                                quote.AggregateId :
                                (quote.PolicyId != null ?
                                    quote.PolicyId.Value : default);

                            if (aggregateId == default)
                            {
                                return;
                            }

                            var quoteAggregate = this.quoteAggregateRepository.GetById(tenant.Id, aggregateId);

                            // if either one has no organisationId.
                            if (quoteAggregate != null && (quoteAggregate.OrganisationId == default || quote.OrganisationId == default))
                            {
                                var defaultOrganisationId = quote.OrganisationId;
                                if (defaultOrganisationId == default)
                                {
                                    defaultOrganisationId = tenant.Details.DefaultOrganisationId;
                                }

                                var currentTimestamp = this.clock.GetCurrentInstant();

                                quoteAggregate.RecordOrganisationMigration(
                                    defaultOrganisationId,
                                    quote.Id,
                                    performingUserId,
                                    currentTimestamp);

                                this.logger.LogInformation($"Set Default Organisation to Quote {quote.Id} for tenant {tenant.Id}, retryTimes = " + retryTimes);
                                await this.quoteAggregateRepository.Save(quoteAggregate);

                                await Task.Delay(150, cancellationToken);
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError($"ERROR: Quote {quote.Id} for tenant {tenant.Id}, retryTimes = {retryTimes}, errorMessage: {e.Message}-{e.InnerException?.Message}");
                            throw;
                        }

                        await Task.Delay(50, cancellationToken);
                    }

                    await RetryPolicyHelper.ExecuteAsync<Exception>(() => AssignDefaultOrganisationIdToQuote(), maxJitter: 1500);
                }
            }
        }

        private async Task SetForPolicies(CancellationToken cancellationToken)
        {
            var tenants = this.tenantRepository.GetTenants();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

            foreach (var tenant in tenants)
            {
                this.logger.LogInformation($"Set Default Organisation for Policies of tenant {tenant.Id}");

                // we leave no policy unturned.
                var policies = this.policyReadModelRepository.ListPoliciesForOrganisationMigration(tenant.Id).ToList();

                int policyCount = 0;
                foreach (var policy in policies)
                {
                    policyCount++;
                    if (policyCount % 1000 == 0)
                    {
                        // log only every 1000 records.
                        this.logger.LogInformation($"Processing Policies: {policyCount}/{policies.Count}");
                    }

                    int retryTimes = 0;

                    async Task AssignDefaultOrganisationIdToPolicy()
                    {
                        try
                        {
                            var quoteAggregate = this.quoteAggregateRepository.GetById(tenant.Id, policy.Id);

                            // if either one has no organisationId.
                            if (quoteAggregate != null && (quoteAggregate.OrganisationId == default || policy.OrganisationId == default))
                            {
                                var defaultOrganisationId = policy.OrganisationId;
                                if (defaultOrganisationId == default)
                                {
                                    defaultOrganisationId = tenant.Details.DefaultOrganisationId;
                                }

                                var currentTimestamp = this.clock.GetCurrentInstant();

                                quoteAggregate.RecordOrganisationMigration(
                                    defaultOrganisationId,
                                    policy.QuoteId,
                                    performingUserId,
                                    currentTimestamp);

                                this.logger.LogInformation($"Set Default Organisation to Policy {policy.Id} for tenant {tenant.Id}, retryTimes = " + retryTimes);
                                await this.quoteAggregateRepository.Save(quoteAggregate);

                                await Task.Delay(150, cancellationToken);
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError($"ERROR: Policy {policy.Id} for tenant {tenant.Id}, retryTimes = {retryTimes}, errorMessage: {e.Message}-{e.InnerException?.Message}");
                            throw;
                        }

                        await Task.Delay(50, cancellationToken);
                    }

                    await RetryPolicyHelper.ExecuteAsync<Exception>(() => AssignDefaultOrganisationIdToPolicy(), maxJitter: 1500);
                }
            }
        }
    }
}
