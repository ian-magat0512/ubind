// <copyright file="BaseEntityScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hangfire;
    using Hangfire.Storage;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Handles registering scheduled jobs meant for updating existing entities.
    /// </summary>
    public abstract class BaseEntityScheduler
    {
        private readonly ITenantRepository tenantRepository;

        public BaseEntityScheduler(
            ITenantRepository tenantRepository,
            IStorageConnection storageConnection,
            IRecurringJobManager recurringJobManager,
            ICqrsMediator mediator)
        {
            this.tenantRepository = tenantRepository;
            this.StorageConnection = storageConnection;
            this.RecurringJobManager = recurringJobManager;
            this.Mediator = mediator;
        }

        protected IStorageConnection StorageConnection { get; }

        protected IRecurringJobManager RecurringJobManager { get; }

        protected ICqrsMediator Mediator { get; }

        protected List<DeploymentEnvironment> Environments
        {
            get
            {
                return new List<DeploymentEnvironment>
                {
                    DeploymentEnvironment.Development,
                    DeploymentEnvironment.Staging,
                    DeploymentEnvironment.Production,
                };
            }
        }

        public abstract void CreateStateChangeJob();

        public abstract string GetRecurringJobId();

        public void RegisterStateChangeJob()
        {
            this.RemoveScheduledJob();
            this.CreateStateChangeJob();
        }

        protected List<Guid> RetrieveTenantIdsForUpdate()
        {
            return this.tenantRepository.GetActiveTenants()
                .Select(x => x.Id).ToList();
        }

        private void RemoveScheduledJob()
        {
            if (this.StorageConnection == null)
            {
                return;
            }

            var recurringJobs = this.StorageConnection.GetRecurringJobs();
            var jobs = recurringJobs.Where(p => p.Id.Equals(this.GetRecurringJobId(), StringComparison.InvariantCultureIgnoreCase));
            foreach (var job in jobs)
            {
                this.RecurringJobManager.RemoveIfExists(job.Id);
            }
        }
    }
}
