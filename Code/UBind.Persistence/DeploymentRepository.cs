// <copyright file="DeploymentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class DeploymentRepository : ReleaseRepository, IDeploymentRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public DeploymentRepository(IUBindDbContext dbContext)
            : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public Deployment? GetCurrentDeployment(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            using (MiniProfiler.Current.Step($"{nameof(DeploymentRepository)}.{nameof(this.GetCurrentDeployment)}"))
            {
                return this.dbContext.Deployments
                    .Where(d => d.TenantId == tenantId
                        && d.Environment == environment
                        && d.ProductId == productId)
                    .OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .Take(1)
                    .IncludeMostProperties()
                    .SingleOrDefault();
            }
        }

        public Deployment? GetCurrentDeploymentWithoutAssets(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            using (MiniProfiler.Current.Step($"{nameof(DeploymentRepository)}.{nameof(this.GetCurrentDeployment)}"))
            {
                return this.dbContext.Deployments
                    .Where(d => d.ProductId == productId)
                    .Where(d => d.TenantId == tenantId)
                    .Where(d => d.Environment == environment)
                    .OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .Take(1)
                    .IncludeMinimalProperties()
                    .SingleOrDefault();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Deployment> GetAllCurrentDeployments(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            var releases = this.dbContext.Releases
                  .Where(r => r.TenantId == tenantId
                  && r.ProductId == productId);

            var deployments = releases.Join(
                    this.dbContext.Deployments, // the source table of the inner join
                    release => release.Id,        // Select the primary key (the first part of the "on" clause in an sql "join" statement)
                    d => d.Release.Id,   // Select the foreign key (the second part of the "on" clause)
                    (release, d) => new { deployments = d })
                    .Select(s => s.deployments)
                    .Where(d => d.Environment == environment); // selection

            return deployments.ToList();
        }

        /// <inheritdoc/>
        public Guid? GetDefaultReleaseId(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            using (MiniProfiler.Current.Step($"{nameof(DeploymentRepository)}.{nameof(this.GetDefaultReleaseId)}"))
            {
                // A record is currently considered the default if it is the latest record for the tenant/product/environment
                // Another approach could be that we have an isDefault flag on the record, but this would require us to update
                // the flag on every new deployment, which is not ideal. Thus the approach below.
                return this.dbContext.Deployments
                        .Where(d => d.TenantId == tenantId
                             && d.ProductId == productId
                             && d.Environment == environment)
                        .OrderByDescending(d => d.CreatedTicksSinceEpoch)
                        .Select(d => (Guid?)d.Release.Id)
                        .FirstOrDefault();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Deployment> PurgeDeployments(Guid? tenantId, int daysRetention, Guid? productid = null)
        {
            if (daysRetention < 30)
            {
                throw new InvalidOperationException("DaysRetention should be more than 30 days.");
            }

            Duration retentionDuration = Duration.FromDays(daysRetention);

            var productPerTenantList = this.dbContext.Releases
                .Select(r => new { r.TenantId, r.ProductId });

            if (tenantId != null && productid != null)
            {
                productPerTenantList = productPerTenantList
                    .Where(t => t.TenantId == tenantId && t.ProductId == productid);
            }

            List<Deployment> deploymentsToBeDeleted = new List<Deployment>();

            foreach (var product in productPerTenantList.Distinct().ToList())
            {
                var latestRelease = this.dbContext.Releases
                     .Where(r => r.TenantId == product.TenantId
                     && r.ProductId == product.ProductId)
                     .OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault();

                var retentionCreatedTimestampLimit = latestRelease.CreatedTicksSinceEpoch - retentionDuration.TotalTicks;

                var releasesToBePurged = this.dbContext.Releases
                      .Where(r => r.TenantId == product.TenantId
                      && r.ProductId == product.ProductId && r.CreatedTicksSinceEpoch < retentionCreatedTimestampLimit);

                var deploymentsToBePurgedFromReleases = releasesToBePurged.Join(
                      this.dbContext.Deployments, // the source table of the inner join
                      release => release.Id,        // Select the primary key (the first part of the "on" clause in an sql "join" statement)
                      d => d.Release.Id,   // Select the foreign key (the second part of the "on" clause)
                      (release, d) => new { deployments = d })
                      .Select(s => s.deployments); // selection

                deploymentsToBeDeleted.AddRange(deploymentsToBePurgedFromReleases);
            }

            this.dbContext.Deployments.RemoveRange(deploymentsToBeDeleted);
            return deploymentsToBeDeleted;
        }

        /// <inheritdoc/>
        public IEnumerable<Deployment> GetCurrentDeployments(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment = DeploymentEnvironment.None)
        {
            var deployments = this.dbContext.Deployments
                .Where(d => d.ProductId == productId && d.TenantId == tenantId);
            if (environment != DeploymentEnvironment.None)
            {
                deployments = deployments.Where(d => d.Environment == environment);
            }

            var deploymentsList = deployments
                .GroupBy(d => new { d.ProductId, d.Environment })
                .Select(g => g.OrderByDescending(m => m.CreatedTicksSinceEpoch).FirstOrDefault())
                .Include(d => d.Release);
            return deploymentsList.ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<Deployment> GetCurrentDeploymentsForRelease(Guid tenantId, Guid releaseId, Guid productId)
        {
            return this.dbContext.Deployments
                .Where(d => d.TenantId == tenantId
                    && d.ProductId == productId
                    && d.Release.Id == releaseId)
                .Include(d => d.Release)
                .GroupBy(d => new { d.Release.ProductId, d.Environment })
                .Select(g => g.OrderByDescending(m => m.CreatedTicksSinceEpoch).FirstOrDefault());
        }

        /// <inheritdoc/>
        public IEnumerable<Deployment> GetDeploymentHistory(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.dbContext.Deployments
                .Where(d => d.TenantId == tenantId
                    && d.Release.ProductId == productId
                    && d.Environment == environment)
                .Include(d => d.Release)
                .OrderByDescending(d => d.CreatedTicksSinceEpoch);
        }

        /// <inheritdoc/>
        public void Insert(Deployment deployment)
        {
            // The release already exists and since we pull it out from the db without using EF6, we need to tell it
            // that it's not new, and it's unchanged. Otherwise, EF6 will try to insert it as a new record.
            if (deployment.Release != null)
            {
                this.dbContext.Releases.Attach(deployment.Release);
                this.dbContext.Entry(deployment.Release).State = EntityState.Unchanged;
            }
            this.dbContext.Deployments.Add(deployment);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }
    }
}
