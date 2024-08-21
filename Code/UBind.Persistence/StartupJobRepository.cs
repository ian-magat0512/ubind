// <copyright file="StartupJobRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class StartupJobRepository : IStartupJobRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupJobRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public StartupJobRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public StartupJob GetStartupJobByAlias(string alias)
        {
            return this.dbContext.StartupJobs.SingleOrDefault(j => j.Alias == alias);
        }

        /// <inheritdoc/>
        public StartupJob GetIncompleteByAlias(string alias)
        {
            return this.dbContext.StartupJobs.SingleOrDefault(j => j.Alias == alias && !j.Complete);
        }

        /// <inheritdoc/>
        public IEnumerable<StartupJob> GetIncompleteStartupJobs()
        {
            return this.dbContext.StartupJobs.Where(j => !j.Complete).ToList();
        }

        public IEnumerable<StartupJob> GetJobsDependentOn(string startupJobAlias)
        {
            return this.dbContext.StartupJobs
                .Where(j => j.PrecedingStartupJobAliasesCommaSeparated.Contains(startupJobAlias)).ToList();
        }

        public void RecordHangfireJobId(string startupJobAlias, string hangfireJobId)
        {
            var job = this.dbContext.StartupJobs.SingleOrDefault(j => j.Alias == startupJobAlias);
            if (job != null)
            {
                job.HangfireJobId = hangfireJobId;
            }

            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void StartJobByAlias(string alias)
        {
            var job = this.dbContext.StartupJobs.SingleOrDefault(j => j.Alias == alias);
            if (job != null)
            {
                job.Started = true;
            }

            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void CompleteJobByAlias(string alias)
        {
            var job = this.dbContext.StartupJobs.SingleOrDefault(j => j.Alias == alias);
            if (job != null)
            {
                job.CompleteJob();
            }

            this.dbContext.SaveChanges();
        }
    }
}
