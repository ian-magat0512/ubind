// <copyright file="MigrateEntitiesToNewProductReleaseCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Release
{
    using System.Data.Entity;
    using Hangfire;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Finds all quotes and policy transactions associated with a given release, and updates them to now be associated with
    /// a different release. The migration operation happens in the background as a hangfire job as it will take some time
    /// to complete.
    /// Users may want to do this in the case where there is a bug with a release and it needs to be patched with a fix.
    /// </summary>
    public class MigrateEntitiesToNewProductReleaseCommandHandler : ICommandHandler<MigrateEntitiesToNewProductReleaseCommand>
    {
        private const int BatchSize = 10;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IReleaseRepository releaseRepository;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ILogger<MigrateEntitiesToNewProductReleaseCommandHandler> logger;
        private readonly ICachingResolver cachingResolver;
        private readonly IUBindDbContext dbContext;
        private CancellationToken cancellationToken;
        private List<Guid> aggregateIdsWithIssue = new List<Guid>();

        public MigrateEntitiesToNewProductReleaseCommandHandler(
            IQuoteReadModelRepository quoteReadModelRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IReleaseRepository releaseRepository,
            IBackgroundJobClient backgroundJobClient,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            ILogger<MigrateEntitiesToNewProductReleaseCommandHandler> logger,
            ICachingResolver cachingResolver,
            IUBindDbContext dbContext)
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.releaseRepository = releaseRepository;
            this.backgroundJobClient = backgroundJobClient;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.logger = logger;
            this.cachingResolver = cachingResolver;
            this.dbContext = dbContext;
        }

        public Task<Unit> Handle(MigrateEntitiesToNewProductReleaseCommand command, CancellationToken cancellationToken)
        {
            if (command.Environment == DeploymentEnvironment.Development)
            {
                throw new ErrorException(Errors.General.AccessDeniedToEnvironment(command.Environment));
            }
            Release release = this.GetReleaseOrThrow(command.TenantId, command.ReleaseId);
            Release newRelease = this.GetReleaseOrThrow(command.TenantId, command.NewReleaseId);

            string releaseVersion = $"{release.Number}.{release.MinorNumber}";
            string newReleaseVersion = $"{newRelease.Number}.{newRelease.MinorNumber}";

            var productAlias = this.cachingResolver.GetProductAliasOrThrow(command.TenantId, release.ProductId);
            var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(command.TenantId);

            this.cancellationToken = cancellationToken;
            this.backgroundJobClient.Enqueue<MigrateEntitiesToNewProductReleaseCommandHandler>(handler =>
                handler.MigrateQuotesWithAssociatedPolicyTransaction(command.TenantId, command.Environment, command.ReleaseId, command.NewReleaseId, releaseVersion, newReleaseVersion, tenantAlias, productAlias));
            return Task.FromResult(Unit.Value);
        }

        /// <summary>
        /// Migration of quotes with associated policy transactions from one release to another in a specific environment.
        /// This will move all quotes even completed, cancelled, decline and expired quotes.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="releaseId">The Id of the release were the quote originate.</param>
        /// <param name="newReleaseId">The Id of the new release were the quotes to be moved.</param>
        /// <param name="environment">The environment were the quotes belong.</param>
        [JobDisplayName("Migrate Entities to New Product Release | TENANT ALIAS: {6}, PRODUCT ALIAS:{7}, RELEASE: {4}, NEW RELEASE: {5}, ENVIRONMENT: {1}")]
        public async Task MigrateQuotesWithAssociatedPolicyTransaction(Guid tenantId, DeploymentEnvironment environment, Guid releaseId, Guid newReleaseId, string releaseVersion, string newReleaseVersion, string tenantAlias, string productAlias)
        {
            using (MiniProfiler.Current.Step($"{nameof(MigrateEntitiesToNewProductReleaseCommandHandler)}.{nameof(this.MigrateQuotesWithAssociatedPolicyTransaction)}"))
            {
                this.aggregateIdsWithIssue = new List<Guid>();
                var quoteAggregateIds = this.quoteReadModelRepository.GetQuoteAggregateIdsByProductReleaseId(tenantId, releaseId, environment);
                var policyTransactionAggregateIds = this.policyReadModelRepository.GetPolicyTransactionAggregateIdsByProductReleaseId(tenantId, releaseId, environment);

                var aggregateIds = quoteAggregateIds.Concat(policyTransactionAggregateIds).Distinct().ToList();

                this.logger.LogInformation($"Start migrating quotes and policy transactions from release {releaseVersion} to new release {newReleaseVersion}. Total QuoteAggregates: {aggregateIds.Count}.");

                var currentInstant = this.clock.GetCurrentInstant();

                var numBatches = (aggregateIds.Count + BatchSize - 1) / BatchSize;

                for (int batchIndex = 0; batchIndex < numBatches; batchIndex++)
                {
                    var startIndex = batchIndex * BatchSize;
                    var endIndex = Math.Min(startIndex + BatchSize, aggregateIds.Count);
                    var batch = aggregateIds.GetRange(startIndex, endIndex - startIndex);

                    await this.ProcessBatch(batch, tenantId, releaseId, newReleaseId, currentInstant);

                    this.dbContext.DetachTrackedEntities();
                }

                if (this.aggregateIdsWithIssue.Count > 0)
                {
                    this.logger.LogWarning($"Error migrating quotes and policy transactions to new release. Aggregates with issue: {string.Join(", ", this.aggregateIdsWithIssue)}");
                    this.logger.LogWarning($"Total Aggregates with issue: {this.aggregateIdsWithIssue.Count} - Remaining ids not yet migrated, Total QuoteAggregates: {aggregateIds.Count}");
                }

                this.logger.LogInformation($"Done migrating quotes and policy transactions to new release.");
            }
        }

        private async Task ProcessBatch(List<Guid> batch, Guid tenantId, Guid releaseId, Guid newReleaseId, Instant currentInstant)
        {
            foreach (var aggregateId in batch)
            {
                QuoteAggregate? quoteAggregate = null;
                try
                {
                    quoteAggregate = await this.quoteAggregateRepository.GetByIdAsync(tenantId, aggregateId);
                    if (quoteAggregate == null)
                    {
                        continue;
                    }

                    quoteAggregate.MigrateQuotesAndPolicyTransactionsToNewProductRelease(
                        releaseId, newReleaseId, this.httpContextPropertiesResolver.PerformingUserId, currentInstant);
                    await this.quoteAggregateRepository.Save(quoteAggregate);
                    await Task.Delay(200, this.cancellationToken);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Error migrating quote aggregate {aggregateId} to new release.");
                    this.aggregateIdsWithIssue.Add(aggregateId);
                }
                finally
                {
                    this.dbContext.RemoveContextAggregates<QuoteAggregate>();
                    quoteAggregate?.ClearUnstrippedEvents();
                }
            }
        }

        private Release GetReleaseOrThrow(Guid tenantId, Guid releaseId)
        {
            var release = this.releaseRepository.GetReleaseWithoutAssets(tenantId, releaseId);
            if (release == null)
            {
                throw new ErrorException(Errors.General.NotFound("release", releaseId));
            }

            return release;
        }
    }
}
