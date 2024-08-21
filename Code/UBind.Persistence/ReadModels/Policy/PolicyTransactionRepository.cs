// <copyright file="PolicyTransactionRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Linq.Expressions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class PolicyTransactionRepository : IPolicyTransactionRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransactionRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public PolicyTransactionRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public void AddOrUpdate(PolicyTransaction policyTransaction)
        {
            this.dbContext.PolicyTransactions.AddOrUpdate(policyTransaction);
        }

        /// <inheritdoc/>
        public void Delete(Expression<Func<PolicyTransaction, bool>> policyTransaction)
        {
            // delete from the db
            var transactionsToDelete = this.dbContext.PolicyTransactions.Where(policyTransaction);
            foreach (PolicyTransaction transaction in transactionsToDelete)
            {
                this.dbContext.PolicyTransactions.Remove(transaction);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<PolicyTransaction> GetByAggregateId(Guid aggregateId)
        {
            return this.dbContext.PolicyTransactions
                .OfType<PolicyTransaction>()
                .OrderBy(t => t.EffectiveTicksSinceEpoch)
                .Where(t => t.PolicyId == aggregateId)
                .AsEnumerable();
        }
    }
}
