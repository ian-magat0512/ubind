// <copyright file="IPolicyTransactionRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Repository for storing policy transactions.
    /// </summary>
    public interface IPolicyTransactionRepository
    {
        /// <summary>
        /// Add or update a policy transaction to the database.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        void AddOrUpdate(PolicyTransaction transaction);

        /// <summary>
        /// Deletes the policy transaction, in the case of a rollback.
        /// </summary>
        /// <param name="predicate">an expression which when matches, will delete the transaction.</param>
        void Delete(Expression<Func<PolicyTransaction, bool>> predicate);

        /// <summary>
        /// Get all the policy transactions by aggregate Id.
        /// </summary>
        /// <param name="aggregateId">The policy aggregate ID.</param>
        /// <returns>Enumerable of policy transactions.</returns>
        IEnumerable<PolicyTransaction> GetByAggregateId(Guid aggregateId);
    }
}
