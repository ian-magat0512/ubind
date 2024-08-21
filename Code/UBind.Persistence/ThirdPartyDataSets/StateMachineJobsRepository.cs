// <copyright file="StateMachineJobsRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ThirdPartyDataSets
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Domain.Repositories;
    using UBind.Domain.ThirdPartyDataSets;

    /// <inheritdoc/>
    public class StateMachineJobsRepository : IStateMachineJobsRepository
    {
        private readonly ThirdPartyDataSetsDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateMachineJobsRepository "/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public StateMachineJobsRepository(ThirdPartyDataSetsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task<StateMachineJob> GetByIdAsync(Guid stateMachineJobId)
        {
            return await this.dbContext.StateMachineJob.FindAsync(stateMachineJobId);
        }

        /// <inheritdoc/>
        public void SaveChanges() => this.dbContext.SaveChanges();

        /// <inheritdoc/>
        public async Task SaveChangesAsync() => await this.dbContext.SaveChangesAsync();

        /// <inheritdoc/>
        public void Add(StateMachineJob stateMachineJob)
        {
            if (stateMachineJob == null)
            {
                return;
            }

            this.dbContext.StateMachineJob.Add(stateMachineJob);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<StateMachineJob>> GetListAsync()
        {
            return await this.dbContext.StateMachineJob.OrderByDescending(order => order.HangfireJobId).ToListAsync();
        }

        /// <inheritdoc/>
        public IEnumerable<StateMachineJob> GetList()
        {
            return this.dbContext.StateMachineJob.OrderByDescending(order => order.HangfireJobId).ToList();
        }

        /// <inheritdoc />
        public void UpdateStateMachineCurrentState(Guid stateMachineJobId, string state)
        {
            var currentStateMachine = this.dbContext.StateMachineJob.Find(stateMachineJobId);
            if (currentStateMachine == null)
            {
                return;
            }

            currentStateMachine.SetState(state);

            this.SaveChanges();
        }

        /// <inheritdoc/>
        public StateMachineJob GetById(Guid id)
        {
            return this.dbContext.StateMachineJob.Find(id);
        }

        /// <inheritdoc/>
        public StateMachineJob? GetByIdAndJobType(Guid id, string jobType)
        {
            return this.dbContext.StateMachineJob
                .FirstOrDefault(job => job.Id == id && job.StateMachineJobType == jobType);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<StateMachineJob>> GetListByJobTypeAsync(string jobType)
        {
            return await this.dbContext.StateMachineJob
                .Where(job => job.StateMachineJobType == jobType)
                .OrderByDescending(order => order.CreatedTicksSinceEpoch)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public IEnumerable<StateMachineJob> GetListByJobType(string jobType)
        {
            return this.dbContext.StateMachineJob
                .Where(job => job.StateMachineJobType == jobType)
                .OrderByDescending(order => order.CreatedTicksSinceEpoch)
                .ToList();
        }
    }
}
