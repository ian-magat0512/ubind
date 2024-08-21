// <copyright file="RegenerateQuoteSearchIndexJob.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler.Jobs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using UBind.Application.Services.Search;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;

    /// <summary>
    /// The job for backuping index and resetting search indexes from the latest data in DB.
    /// </summary>
    [DisallowConcurrentExecution]
    public class RegenerateQuoteSearchIndexJob : IJob
    {
        private readonly ILogger<RegenerateQuoteSearchIndexJob> logger;
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegenerateQuoteSearchIndexJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="provider">THe service provider.</param>
        public RegenerateQuoteSearchIndexJob(
            ILogger<RegenerateQuoteSearchIndexJob> logger,
            IServiceProvider provider)
        {
            this.logger = logger;
            this.provider = provider;
        }

        /// <summary>
        /// Execute the regeneration of quotes search indexes.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = this.provider.CreateScope())
            {
                this.logger.LogDebug("Initializing service for RegenerateQuoteSearchIndexJob.");
                var service = scope.ServiceProvider
                    .GetRequiredService<ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters>>();
                var cachingResolver = scope.ServiceProvider.GetRequiredService<ICachingResolver>();
                if (LuceneIndexJobState.QuoteIndexRegenerationStatus == TaskStatus.WaitingForActivation &&
                                    LuceneIndexJobState.QuoteIndexGenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.RegenerateLuceneIndexes(
                        service,
                        "INITIAL Regenerate of Quotes Lucene Indexes Done.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if ((LuceneIndexJobState.QuoteIndexRegenerationStatus == TaskStatus.Faulted ||
                    LuceneIndexJobState.QuoteIndexRegenerationStatus == TaskStatus.Canceled) &&
                    LuceneIndexJobState.QuoteIndexGenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.RegenerateLuceneIndexes(
                        service,
                        "RETRYING Regenerate of Quotes Lucene indexes.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if (LuceneIndexJobState.QuoteIndexRegenerationStatus == TaskStatus.RanToCompletion &&
                    LuceneIndexJobState.QuoteIndexGenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.RegenerateLuceneIndexes(
                        service,
                        "RECURRING job for Regenerate Quotes Lucene Indexes.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if (LuceneIndexJobState.QuoteIndexRegenerationStatus == TaskStatus.Running)
                {
                    this.logger.LogInformation("Cannot execute RegenerateQuoteSearchIndexJob for the previous job is not yet finished.");
                }
                else if (LuceneIndexJobState.QuoteIndexGenerationStatus == TaskStatus.Running)
                {
                    this.logger.LogInformation("Cannot execute RegenerateQuoteSearchIndexJob because UpdateQuoteSearchIndexJob is running.");

                    // Re Execute check if the ProcessQuotesSearchIndexesJob is done so that the regenerate lucene index will perform..
                    await this.Execute(context);
                }
                else
                {
                    this.logger.LogInformation("Cannot execute RegenerateQuoteSearchIndexJob. Initial regenerate of lucene indexes not yet executed.");
                }
            }
        }

        private void RegenerateLuceneIndexes(
            ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters> service,
            string logMessage,
            IEnumerable<Product> activeProducts,
            CancellationToken cancellationToken)
        {
            try
            {
                LuceneIndexJobState.QuoteIndexRegenerationStatus = TaskStatus.Running;
                service.RegenerateLuceneIndexes(DeploymentEnvironment.Production, activeProducts, cancellationToken);
                service.RegenerateLuceneIndexes(DeploymentEnvironment.Staging, activeProducts, cancellationToken);
                service.RegenerateLuceneIndexes(DeploymentEnvironment.Development, activeProducts, cancellationToken);

                this.logger.LogInformation(logMessage);
                LuceneIndexJobState.QuoteIndexRegenerationStatus = TaskStatus.RanToCompletion;
            }
            catch (Exception exception)
            {
                LuceneIndexJobState.QuoteIndexRegenerationStatus = TaskStatus.Faulted;
                this.logger.LogError(exception, exception.Message);
            }
        }
    }
}
