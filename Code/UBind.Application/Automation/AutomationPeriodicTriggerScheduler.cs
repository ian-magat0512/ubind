// <copyright file="AutomationPeriodicTriggerScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Linq;
    using System.Threading;
    using Hangfire;
    using Hangfire.Storage;
    using Microsoft.Extensions.Logging;
    using StackExchange.Profiling;
    using UBind.Application.Releases;
    using UBind.Application.Services;
    using UBind.Application.Services.Email;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;

    public class AutomationPeriodicTriggerScheduler : IAutomationPeriodicTriggerScheduler
    {
        private readonly IProductRepository productRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IRecurringJobManager recurringJob;
        private readonly IStorageConnection storageConnection;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IAutomationConfigurationProvider automationConfigurationProvider;
        private readonly IErrorNotificationService errorNotificationService;
        private readonly ILogger<AutomationPeriodicTriggerScheduler> logger;

        public AutomationPeriodicTriggerScheduler(
            IProductRepository productRepository,
            ITenantRepository tenantRepository,
            IRecurringJobManager recurringJob,
            IStorageConnection storageConnection,
            IReleaseQueryService releaseQueryService,
            IAutomationConfigurationProvider automationConfigurationProvider,
            IErrorNotificationService errorNotificationService,
            ILoggerFactory loggerFactory)
        {
            this.productRepository = productRepository;
            this.tenantRepository = tenantRepository;
            this.recurringJob = recurringJob;
            this.storageConnection = storageConnection;
            this.releaseQueryService = releaseQueryService;
            this.automationConfigurationProvider = automationConfigurationProvider;
            this.errorNotificationService = errorNotificationService;
            this.logger = loggerFactory.CreateLogger<AutomationPeriodicTriggerScheduler>();
        }

        [JobDisplayName("Register Periodic Triggers Jobs | All tenants and products")]
        public async Task RegisterPeriodicTriggerJobs()
        {
            try
            {
                // this lock stops 2 of these jobs from running at the same time.
                using (JobStorage.Current.GetConnection().AcquireDistributedLock(
                    "Register Periodic Triggers Jobs | All tenants and products", TimeSpan.FromSeconds(1)))
                {
                    var products = this.productRepository.GetAllProducts().ToList();
                    var tenants = this.tenantRepository.GetActiveTenants();

                    // Remove all existing periodic trigger jobs before re-registering them.
                    // This is to ensure that we don't delete recreated jobs from products with the same alias
                    foreach (var product in products)
                    {
                        this.RemovePeriodicTriggerJobs(product);
                    }

                    foreach (var product in products)
                    {
                        var tenant = tenants.FirstOrDefault(c => c.Id == product.TenantId);
                        if (tenant != null)
                        {
                            await this.ExecuteMultiJobRegistration(tenant, product);
                        }
                    }
                }
            }
            catch (DistributedLockTimeoutException)
            {
                // A job is already running and hasn't completed within the 5 minute lock timeout.
                // In this case, we just catch the exception and return.
                this.logger.LogInformation($"The previous \"Register Periodic Triggers Jobs\" job is still running, so not executing another.");
            }
        }

        public async Task RegisterPeriodicTriggerJobs(Guid tenantId)
        {
            var tenant = this.tenantRepository.GetTenantById(tenantId);
            var products = this.productRepository.GetAllProductsForTenant(tenantId).ToList();
            foreach (var product in products)
            {
                this.RemovePeriodicTriggerJobs(product);
                await this.ExecuteMultiJobRegistration(tenant, product);
            }
        }

        public async Task RegisterPeriodicTriggerJobs(Guid tenantId, Guid productId)
        {
            var product = this.productRepository.GetProductById(tenantId, productId);
            var tenant = this.tenantRepository.GetTenantById(tenantId);

            this.RemovePeriodicTriggerJobs(product);
            await this.ExecuteMultiJobRegistration(tenant, product);
        }

        public async Task RegisterPeriodicTriggerJobs(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            using (MiniProfiler.Current.Step($"{nameof(AutomationPeriodicTriggerScheduler)}.{nameof(this.RegisterPeriodicTriggerJobs)}"))
            {
                var product = this.productRepository.GetProductById(tenantId, productId);
                var tenant = this.tenantRepository.GetTenantById(tenantId);
                this.RemoveScheduledJobs(product, environment);
                if (product.Details.Disabled || product.Details.Deleted)
                {
                    return;
                }

                await this.CreateScheduledJobs(tenant, product, environment);
            }
        }

        public async Task RegisterPeriodicTriggerJobs(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, ActiveDeployedRelease release)
        {
            using (MiniProfiler.Current.Step($"{nameof(AutomationPeriodicTriggerScheduler)}.{nameof(this.RegisterPeriodicTriggerJobs)}"))
            {
                var product = this.productRepository.GetProductById(tenantId, productId);
                var tenant = this.tenantRepository.GetTenantById(tenantId);
                this.RemoveScheduledJobs(product, environment);
                if (product.Details.Disabled || product.Details.Deleted)
                {
                    return;
                }

                await this.CreateScheduledJobs(tenant, product, environment, release);
            }
        }

        public void RemovePeriodicTriggerJobs(Guid tenantId)
        {
            var products = this.productRepository.GetAllProductsForTenant(tenantId, true).ToList();
            foreach (var product in products)
            {
                this.RemovePeriodicTriggerJobs(product);
            }
        }

        public void RemovePeriodicTriggerJobs(Product product)
        {
            this.RemoveScheduledJobs(product, DeploymentEnvironment.Development);
            this.RemoveScheduledJobs(product, DeploymentEnvironment.Staging);
            this.RemoveScheduledJobs(product, DeploymentEnvironment.Production);
        }

        public void RemovePeriodicTriggerJobs(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            var product = this.productRepository.GetProductById(tenantId, productId, true);
            this.RemoveScheduledJobs(product, environment);
        }

        private void RemoveScheduledJobs(Product product, DeploymentEnvironment environment)
        {
            if (this.storageConnection == null)
            {
                return;
            }

            var productAlias = product.Details.Alias;
            var recurringJobs = this.storageConnection.GetRecurringJobs();
            var jobs = recurringJobs.Where(p => p.Id.StartsWith($"{productAlias}-{environment}", StringComparison.InvariantCultureIgnoreCase));
            foreach (var job in jobs)
            {
                this.recurringJob.RemoveIfExists(job.Id);
            }
        }

        private async Task CreateScheduledJobs(Tenant tenant, Product product, DeploymentEnvironment environment)
        {
            var releaseContext = this.releaseQueryService.GetDefaultReleaseContextOrNull(product.TenantId, product.Id, environment);
            if (releaseContext == null)
            {
                return;
            }

            var release = this.releaseQueryService.GetReleaseWithoutCachingOrAssets(releaseContext.Value);
            await this.CreateScheduledJobs(tenant, product, environment, release);
        }

        private Task CreateScheduledJobs(Tenant tenant, Product product, DeploymentEnvironment environment, ActiveDeployedRelease release)
        {
            var productAlias = product.Details.Alias;
            var config = this.automationConfigurationProvider.GetAutomationConfiguration(release.AutomationsConfigurationModel);
            if (config == null)
            {
                return Task.CompletedTask;
            }

            var organisationId = tenant.Details?.DefaultOrganisationId ?? Guid.Empty;
            foreach (Automation automation in config.Automations)
            {
                var periodicTriggers = automation.GetPeriodicTriggers().ToList();
                foreach (var periodicTrigger in periodicTriggers)
                {
                    var recurringOption = new RecurringJobOptions { TimeZone = periodicTrigger.TimeZone };
                    this.recurringJob.AddOrUpdate<IAutomationService>(
                        $"{productAlias}-{environment}-{periodicTrigger.Alias}",
                        (triggerService) => triggerService.TriggerPeriodicAutomation(
                            product.TenantId,
                            organisationId,
                            product.Id,
                            environment,
                            automation.Alias,
                            CancellationToken.None),
                        periodicTrigger.GetCronSchedule(),
                        recurringOption);
                }
            }

            return Task.CompletedTask;
        }

        private async Task ExecuteMultiJobRegistration(Tenant tenant, Product product)
        {
            if (product.Details.Disabled || product.Details.Deleted)
            {
                return;
            }

            // The cause of the error is the parallel execution of accessing the database, specifically the
            // Deployment and DevRelease table. As per Chat GPT, parallel execution should be avoided. It should
            // only be done if it is really necessary and if it is, we're going to need to use Distributed Transaction
            // Coordinator (DTC) to handle the transaction which could make things more complex. In this case, it would
            // be better to just execute the CreateScheduledJobs method sequentially.
            var environments = new List<DeploymentEnvironment>()
            {
                DeploymentEnvironment.Development,
                DeploymentEnvironment.Staging,
                DeploymentEnvironment.Production,
            };

            var exceptions = new List<ErrorException>();
            foreach (var environment in environments)
            {
                try
                {
                    await this.CreateScheduledJobs(tenant, product, environment);
                }
                catch (ErrorException ex)
                {
                    ex.Error.Data.Add($"Tenant Alias:", tenant.Details.Alias);
                    ex.Error.Data.Add($"Product Alias:", product.Details.Alias);
                    ex.Error.Data.Add($"Environment:", environment.ToString());
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                var originalException = exceptions.First();
                throw new ErrorException(originalException.Error, originalException);
            }
        }
    }
}
