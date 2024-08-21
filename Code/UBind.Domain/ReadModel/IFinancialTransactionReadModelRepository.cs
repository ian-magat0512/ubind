// <copyright file="IFinancialTransactionReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// The payment readmodel repository.
    /// </summary>
    /// <typeparam name="TReadModel">The readmodel Type.</typeparam>
    public interface IFinancialTransactionReadModelRepository<TReadModel>
        where TReadModel : class
    {
        /// <summary>
        /// Get transactions by payer.
        /// </summary>
        /// <param name="payerId">The payer Id.</param>
        /// <param name="payerType">The payerType.</param>
        /// <returns>List of financial transactions(payments or refunds).</returns>
        IEnumerable<TReadModel> GetAllForPayer(Guid payerId, TransactionPartyType payerType);

        /// <summary>
        /// Get the payment/refund by Id.
        /// </summary>
        /// <param name="id">The transaction (payment/refund) Id.</param>
        /// <returns>The readmodel.</returns>
        TReadModel GetById(Guid id);
    }
}
