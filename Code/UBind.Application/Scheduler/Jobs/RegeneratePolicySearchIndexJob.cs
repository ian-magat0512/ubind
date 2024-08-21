// <copyright file="RegeneratePolicySearchIndexJob.cs" company="uBind">
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
    public class RegeneratePolicySearchIndexJob : IJob
    {
        private readonly ILogger<RegeneratePolicySearchIndexJob> logger;
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegeneratePolicySearchIndexJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="provider">THe service provider.</param>
        public RegeneratePolicySearchIndexJob(
            ILogger<RegeneratePolicySearchIndexJob> logger,
            IServiceProvider provider)
        {
            this.logger = logger;
            this.provider = provider;
        }

        /// <summary>
        /// Execute the regeneration of policies search indexes.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = this.provider.CreateScope())
            {
                this.logger.LogDebug("Initializing service for RegeneratePolicySearchIndexJob.");
                var service = scope.ServiceProvider
                    .GetRequiredService<ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters>>();
                var cachingResolver = scope.ServiceProvider.GetRequiredService<ICachingResolver>();
                if (LuceneIndexJobState.PolicyIndexRegenerationStatus == TaskStatus.WaitingForActivation &&
                    LuceneIndexJobState.PolicyIndexGenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.RegenerateLuceneIndexes(
                        service,
                        "INITIAL Regenerate of Policy Lucene Indexes Done.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if ((LuceneIndexJobState.PolicyIndexRegenerationStatus == TaskStatus.Faulted ||
                    LuceneIndexJobState.PolicyIndexRegenerationStatus == TaskStatus.Canceled) &&
                    LuceneIndexJobState.PolicyIndexGenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.RegenerateLuceneIndexes(
                        service,
                        "RETRYING Regenerate of Policy Lucene indexes.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if (LuceneIndexJobState.PolicyIndexRegenerationStatus == TaskStatus.RanToCompletion &&
                    LuceneIndexJobState.PolicyIndexGenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.RegenerateLuceneIndexes(
                        service,
                        "RECURRING job for Regenerate Policy Lucene Indexes.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if (LuceneIndexJobState.PolicyIndexRegenerationStatus == TaskStatus.Running)
                {
                    this.logger.LogInformation("Cannot execute RegeneratePolicySearchIndexJob for the previous job is not yet finished.");
                }
                else if (LuceneIndexJobState.PolicyIndexGenerationStatus == TaskStatus.Running)
                {
                    this.logger.LogInformation("Cannot execute RegeneratePolicySearchIndexJob because UpdatePolicySearchIndexJob is running.");

                    // Re Execute check if the ProcessPoliciesSearchIndexesJob is done so that the regenerate lucene index will perform..
                    await this.Execute(context);
                }
                else
                {
                    this.logger.LogInformation("Cannot execute RegeneratePolicySearchIndexJob. Initial regenerate of lucene indexes not yet executed.");
                }
            }
        }

        private void RegenerateLuceneIndexes(
            ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> service,
            string logMessage,
            IEnumerable<Product> activeProducts,
            CancellationToken cancellationToken)
        {
            try
            {
                LuceneIndexJobState.PolicyIndexRegenerationStatus = TaskStatus.Running;
                service.RegenerateLuceneIndexes(DeploymentEnvironment.Production, activeProducts, cancellationToken);
                service.RegenerateLuceneIndexes(DeploymentEnvironment.Staging, activeProducts, cancellationToken);
                service.RegenerateLuceneIndexes(DeploymentEnvironment.Development, activeProducts, cancellationToken);

                this.logger.LogInformation(logMessage);
                LuceneIndexJobState.PolicyIndexRegenerationStatus = TaskStatus.RanToCompletion;
            }
            catch (Exception exception)
            {
                LuceneIndexJobState.PolicyIndexRegenerationStatus = TaskStatus.Faulted;
                this.logger.LogError(exception, exception.Message);
            }
        }
    }
}
