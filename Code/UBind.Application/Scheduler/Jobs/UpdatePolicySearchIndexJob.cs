// <copyright file="UpdatePolicySearchIndexJob.cs" company="uBind">
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
    /// The job for creating/updating policy search indexes from the latest data in DB.
    /// </summary>
    [DisallowConcurrentExecution]
    public class UpdatePolicySearchIndexJob : IJob
    {
        private readonly ILogger<UpdatePolicySearchIndexJob> logger;
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePolicySearchIndexJob"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="provider">THe service provider.</param>
        public UpdatePolicySearchIndexJob(
            ILogger<UpdatePolicySearchIndexJob> logger,
            IServiceProvider provider)
        {
            this.logger = logger;
            this.provider = provider;
        }

        /// <summary>
        /// Execute the processing of policies search indexes.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = this.provider.CreateScope())
            {
                this.logger.LogDebug("Initializing service for UpdatePolicySearchIndexJob.");

                var service = scope.ServiceProvider
                    .GetRequiredService<ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters>>();
                var cachingResolver = scope.ServiceProvider.GetRequiredService<ICachingResolver>();

                if (LuceneIndexJobState.PolicyIndexGenerationStatus == TaskStatus.WaitingForActivation &&
                   LuceneIndexJobState.PolicyIndexRegenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.GenerateIndexes(
                        service,
                        "INITIAL Generation of Policy Search Indexes Done.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if ((LuceneIndexJobState.PolicyIndexGenerationStatus == TaskStatus.Faulted ||
                    LuceneIndexJobState.PolicyIndexGenerationStatus == TaskStatus.Canceled) &&
                    LuceneIndexJobState.PolicyIndexRegenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.GenerateIndexes(
                        service,
                        "RETRYING Generation of Search indexes for Policy DONE.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if (LuceneIndexJobState.PolicyIndexGenerationStatus == TaskStatus.RanToCompletion &&
                    LuceneIndexJobState.PolicyIndexRegenerationStatus != TaskStatus.Running)
                {
                    var activeProducts = await cachingResolver.GetActiveProducts();
                    this.GenerateIndexes(
                        service,
                        "RECURRING job for Updating Search Indexes for Policy DONE.",
                        activeProducts,
                        context.CancellationToken);
                }
                else if (LuceneIndexJobState.PolicyIndexGenerationStatus == TaskStatus.Running)
                {
                    this.logger.LogInformation("Cannot execute UpdatePolicySearchIndexJob for the previous job is not yet finished.");
                }
                else if (LuceneIndexJobState.PolicyIndexRegenerationStatus == TaskStatus.Running)
                {
                    this.logger.LogInformation("Cannot execute UpdatePolicySearchIndexJob because RegeneratePolicySearchIndexJob is running.");
                }
                else
                {
                    this.logger.LogInformation("Cannot execute UpdatePolicySearchIndexJob. Initial generation of search indexes not yet executed.");
                }
            }
        }

        private void GenerateIndexes(
            ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> service,
            string logMessage,
            IEnumerable<Product> activeProducts,
            CancellationToken cancellationToken)
        {
            try
            {
                LuceneIndexJobState.PolicyIndexGenerationStatus = TaskStatus.Running;
                service.ProcessIndexUpdates(DeploymentEnvironment.Production, activeProducts, cancellationToken);
                service.ProcessIndexUpdates(DeploymentEnvironment.Staging, activeProducts, cancellationToken);
                service.ProcessIndexUpdates(DeploymentEnvironment.Development, activeProducts, cancellationToken);
                this.logger.LogInformation(logMessage);
                LuceneIndexJobState.PolicyIndexGenerationStatus = TaskStatus.RanToCompletion;
            }
            catch (Exception exception)
            {
                LuceneIndexJobState.PolicyIndexGenerationStatus = TaskStatus.Faulted;
                this.logger.LogError(exception, exception.Message);
            }
        }
    }
}
