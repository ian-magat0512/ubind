// <copyright file="QuoteService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using StackExchange.Redis;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence.Configuration;
    using UBind.Persistence.Redis.Repositories;

    /// <inheritdoc/>
    public class QuoteService : IQuoteService
    {
        private const int BatchSize = 10;
        private const int HashExpiryDays = 7;
        private const string UpdatedExpiryDateQuoteIdsKey = "updateExpiryDateQuoteIds";
        private const string RemovedExpiryDateQuoteIdsKey = "removeExpiryDateQuoteIds";
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IQuoteVersionReadModelRepository quoteVersionRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IClock clock;
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<QuoteService> logger;
        private readonly IRedisConfiguration redisConfiguration;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteService"/> class.
        /// </summary>
        /// <param name="quoteReadModelRepository">The quote repository.</param>
        /// <param name="quoteVersionRepository">The quote versions repository.</param>
        /// <param name="quoteAggregateRepository">The quote aggregate repository.</param>
        /// <param name="httpContextPropertiesResolver">The performing user resolver.</param>
        /// <param name="quoteAggregateResolverService">The service to resolve the quote aggregate for a given quote ID.</param>
        /// <param name="clock">The clock representing time today.</param>
        public QuoteService(
            IQuoteReadModelRepository quoteReadModelRepository,
            IQuoteVersionReadModelRepository quoteVersionRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IClock clock,
            IUBindDbContext dbContext,
            ILogger<QuoteService> logger,
            IRedisConfiguration redisConfiguration,
            IConnectionMultiplexer connectionMultiplexer,
            ICachingResolver cachingResolver)
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.quoteVersionRepository = quoteVersionRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.clock = clock;
            this.dbContext = dbContext;
            this.logger = logger;
            this.redisConfiguration = redisConfiguration;
            this.connectionMultiplexer = connectionMultiplexer;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteReadModelSummary> GetQuotes(Guid tenantId, QuoteReadModelFilters filters)
        {
            return this.quoteReadModelRepository.ListQuotes(tenantId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> GetQuoteIdsFromPolicy(Guid tenantId, Guid policyId, DeploymentEnvironment environment)
        {
            return this.quoteReadModelRepository.ListQuoteIdsFromPolicy(tenantId, policyId, environment);
        }

        /// <inheritdoc/>
        public IQuoteReadModelSummary GetQuoteSummary(Guid tenantId, Guid quoteId)
        {
            var quote = this.quoteReadModelRepository.GetQuoteSummary(tenantId, quoteId);
            if (quote == null)
            {
                throw new ErrorException(Errors.Quote.NotFound(quoteId));
            }

            return quote;
        }

        /// <inheritdoc/>
        public IQuoteReadModelDetails GetQuoteDetails(Guid tenantId, Guid quoteId)
        {
            var quote = this.quoteReadModelRepository.GetQuoteDetails(tenantId, quoteId);
            if (quote == null)
            {
                throw new ErrorException(Errors.Quote.NotFound(quoteId));
            }

            return quote;
        }

        /// <inheritdoc/>
        public QuoteVersionReadModelDto? GetQuoteVersionForQuote(Guid tenantId, Guid quoteId, int version)
        {
            var quoteVersionReadModelDetails
                = this.quoteVersionRepository.GetVersionDetailsByVersionNumber(tenantId, quoteId, version);
            return quoteVersionReadModelDetails != null
                ? new QuoteVersionReadModelDto(quoteVersionReadModelDetails)
                : null;
        }

        /// <inheritdoc/>
        public IQuoteVersionReadModelDetails GetQuoteVersionDetail(Guid tenantId, Guid quoteVersionId)
        {
            var quoteVersion = this.quoteVersionRepository.GetVersionDetailsById(tenantId, quoteVersionId);
            if (quoteVersion == null)
            {
                throw new ErrorException(Errors.General.NotFound("quote version", quoteVersionId));
            }

            return quoteVersion;
        }

        /// <inheritdoc/>
        public bool HasQuotesForCustomer(QuoteReadModelFilters filters, IEnumerable<Guid> excludedQuoteIds)
        {
            return this.quoteReadModelRepository.HasQuotesForCustomer(filters, excludedQuoteIds);
        }

        /// <inheritdoc/>
        public async Task UpdateExpiryDates(
            Guid tenantId,
            Guid productId,
            QuoteExpirySettings settings,
            ProductQuoteExpirySettingUpdateType updateType,
            CancellationToken cancellationToken)
        {
            if (!settings.Enabled)
            {
                return;
            }

            IEnumerable<(Guid QuoteAggregateId, Guid QuoteId)> quoteIds = new List<(Guid QuoteAggregateId, Guid QuoteId)>();
            switch (updateType)
            {
                case ProductQuoteExpirySettingUpdateType.UpdateNone:
                    break;
                case ProductQuoteExpirySettingUpdateType.UpdateAllExceptExplicitSet:
                case ProductQuoteExpirySettingUpdateType.UpdateAllQuotes:
                    quoteIds = this.quoteReadModelRepository
                        .GetIncompleteQuotesIds(tenantId, productId).ToList();
                    break;
                case ProductQuoteExpirySettingUpdateType.UpdateAllWithoutExpiryOnly:
                    quoteIds = this.quoteReadModelRepository
                        .GetIncompleteQuoteIdsWithoutExpiryDates(tenantId, productId).ToList();
                    break;
            }

            if (!quoteIds.Any())
            {
                this.logger.LogInformation("There are no quotes to process.");
                return;
            }

            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);
            var redisGuidSetRepository = new RedisHashSetRepository<Guid>(
                this.redisConfiguration,
                this.connectionMultiplexer,
                tenantId,
                $"{productAlias}:{UpdatedExpiryDateQuoteIdsKey}",
                TimeSpan.FromDays(HashExpiryDays));
            int quoteIndex = 0;
            var savedQuoteIdsCount = await redisGuidSetRepository.GetCount();
            this.logger.LogInformation($"Started quotes expiry update. Total number of quotes : {quoteIds.Count()} | "
                + $"Number of quotes already updated: {savedQuoteIdsCount}");

            try
            {
                // iterate each and update expiry date.
                foreach (var quoteId in quoteIds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.logger.LogWarning("The task was cancelled.");
                        this.MemoryCleanup();
                        PerformanceHelper.ThrowIfCancellationRequested(cancellationToken);
                    }

                    // skip if this quote was already processed in the previous run of this job
                    if (await redisGuidSetRepository.Contains(quoteId.QuoteId))
                    {
                        continue;
                    }

                    // TODO: revert back original code once https://jira.aptiture.com/browse/UB-9307 is addressed
                    var quoteAggregate = await this.quoteAggregateRepository.GetByIdAsync(tenantId, quoteId.QuoteAggregateId);
                    if (quoteAggregate == null)
                    {
                        this.logger.LogWarning($"The quote aggregate with ID {quoteId.QuoteAggregateId} was not found");
                        continue;
                    }

                    var quote = quoteAggregate.GetQuote(quoteId.QuoteId);
                    if (quote == null)
                    {
                        this.logger.LogWarning($"The quote with ID {quoteId.QuoteId} was not found");
                        continue;
                    }

                    if (updateType == ProductQuoteExpirySettingUpdateType.UpdateAllQuotes
                        || updateType != ProductQuoteExpirySettingUpdateType.UpdateAllExceptExplicitSet
                        || quote.ExpiryReason == QuoteExpiryReason.Automatic)
                    {
                        try
                        {
                            quoteAggregate.SetQuoteExpiryFromSettings(
                                quote.Id,
                                this.httpContextPropertiesResolver.PerformingUserId,
                                this.clock.Now(),
                                settings);
                            await this.quoteAggregateRepository.Save(quoteAggregate);
                            await redisGuidSetRepository.Add(quoteId.QuoteId);
                            await Task.Delay(5000, cancellationToken);
                        }
                        catch (TaskCanceledException)
                        {
                            this.logger.LogWarning("The task was cancelled during delay.");
                        }
                        finally
                        {
                            this.dbContext.RemoveContextAggregates<QuoteAggregate>();
                            quoteAggregate.ClearUnstrippedEvents();
                        }

                        // detach tracked entries after a number of quotes determined by 'BatchSize'
                        quoteIndex++;
                        if ((quoteIndex % BatchSize) == 0)
                        {
                            this.dbContext.DetachTrackedEntities();
                        }
                    }
                }

                savedQuoteIdsCount = await redisGuidSetRepository.GetCount();
                this.logger.LogInformation($"Finished updating quotes expiry: Updated {savedQuoteIdsCount} of {quoteIds.Count()} quotes.");
                await redisGuidSetRepository.DeleteSet();
            }
            finally
            {
                // detach tracked entries on remaining quotes
                if ((quoteIndex % BatchSize) != 0)
                {
                    this.dbContext.DetachTrackedEntities();
                }
            }
        }

        /// <inheritdoc/>
        public async Task RemoveExpiryDates(
            Guid tenantId,
            Guid productId,
            CancellationToken cancellationToken)
        {
            var quoteIds = this.quoteReadModelRepository
                .GetIncompleteQuotesIds(tenantId, productId).ToList();

            if (!quoteIds.Any())
            {
                this.logger.LogInformation("There are no quotes to process.");
                return;
            }

            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);
            var redisGuidSetRepository = new RedisHashSetRepository<Guid>(
                this.redisConfiguration,
                this.connectionMultiplexer,
                tenantId,
                $"{productAlias}:{RemovedExpiryDateQuoteIdsKey}",
                TimeSpan.FromDays(HashExpiryDays));
            int quoteIndex = 0;
            var savedQuoteIdsCount = await redisGuidSetRepository.GetCount();
            this.logger.LogInformation($"Started removing expiry date of quotes. Total number of quotes : {quoteIds.Count} | "
                + $"Number of quotes already updated: {savedQuoteIdsCount}");

            try
            {
                // iterate each and update expiry date.
                foreach (var quoteId in quoteIds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.logger.LogWarning("The task was cancelled.");
                        this.MemoryCleanup();
                        PerformanceHelper.ThrowIfCancellationRequested(cancellationToken);
                    }

                    // skip if this quote was already processed in the previous run of this job
                    if (await redisGuidSetRepository.Contains(quoteId.QuoteId))
                    {
                        continue;
                    }

                    var quoteAggregate = await this.quoteAggregateRepository.GetByIdAsync(tenantId, quoteId.QuoteAggregateId);
                    if (quoteAggregate == null)
                    {
                        this.logger.LogWarning($"The quote aggregate with ID {quoteId.QuoteAggregateId} was not found");
                        continue;
                    }

                    var quote = quoteAggregate.GetQuote(quoteId.QuoteId);
                    if (quote == null)
                    {
                        this.logger.LogWarning($"The quote with ID {quoteId.QuoteId} was not found");
                        continue;
                    }

                    try
                    {
                        quoteAggregate.RemoveQuoteExpiryTime(
                            quote.Id,
                            this.httpContextPropertiesResolver.PerformingUserId,
                            this.clock.Now());
                        await this.quoteAggregateRepository.Save(quoteAggregate);
                        await redisGuidSetRepository.Add(quoteId.QuoteId);
                        await Task.Delay(5000, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        this.logger.LogWarning("The task was cancelled during delay.");
                    }
                    finally
                    {
                        this.dbContext.RemoveContextAggregates<QuoteAggregate>();
                        quoteAggregate.ClearUnstrippedEvents();
                    }

                    // detach tracked entries after a number of quotes determined by 'BatchSize'
                    quoteIndex++;
                    if ((quoteIndex % BatchSize) == 0)
                    {
                        this.dbContext.DetachTrackedEntities();
                    }
                }

                savedQuoteIdsCount = await redisGuidSetRepository.GetCount();
                this.logger.LogInformation($"Finished removing expiry date of quotes: Updated {savedQuoteIdsCount} of {quoteIds.Count} quotes.");
                await redisGuidSetRepository.DeleteSet();
            }
            finally
            {
                // detach tracked entries on remaining quotes
                if ((quoteIndex % BatchSize) != 0)
                {
                    this.dbContext.DetachTrackedEntities();
                }
            }
        }

        /// <inheritdoc/>
        public void VerifyQuoteCustomerAssociation(Guid tenantId, Guid associationInvitationId, Guid quoteId, Guid customerUserId)
        {
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
            if (quoteAggregate == null)
            {
                throw new ErrorException(Errors.Quote.AssociationInvitation.NotFound(associationInvitationId));
            }

            quoteAggregate
                .VerifyCustomerAssociationInvitation(associationInvitationId, customerUserId, this.clock.Now());
        }

        /// <inheritdoc/>
        public string GetQuoteNumber(Guid tenantId, Guid quoteId)
        {
            var quoteSummary = this.quoteReadModelRepository.GetQuoteSummary(tenantId, quoteId);
            return quoteSummary.QuoteNumber;
        }

        /// <inheritdoc/>
        public string GetQuoteState(Guid tenantId, Guid quoteId)
        {
            var quoteSummary = this.quoteReadModelRepository.GetQuoteSummary(tenantId, quoteId);
            if (quoteSummary == null)
            {
                throw new ErrorException(Errors.Quote.NotFound(quoteId));
            }

            return quoteSummary.QuoteState;
        }

        private void MemoryCleanup()
        {
            // GC used as temporary fix for https://jira.aptiture.com/browse/UB-12654.
            // It will be removed when the root cause of memory leak was properly fixed at https://jira.aptiture.com/browse/UB-12674.
            var memBefore = BytesToGb(GC.GetTotalMemory(false));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true, true);
            GC.WaitForPendingFinalizers();
            var memAfter = BytesToGb(GC.GetTotalMemory(true));

            this.logger.LogInformation($"GC starts with {memBefore:N} GB, ends with {memAfter:N} GB");

            double BytesToGb(long bytes)
            {
                return bytes * 1E-9;
            }
        }
    }
}
