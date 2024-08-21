// <copyright file="AllPaymentsByPayerQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Accounting
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Accounting;
    using UBind.Domain.ValueTypes;

    /// <summary>
    ///  A query to return all financial transactions by payer.
    /// </summary>
    public class AllPaymentsByPayerQuery : IQuery<IEnumerable<PaymentReadModel>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllPaymentsByPayerQuery"/> class.
        /// </summary>
        /// /// <param name="payerId">The payer id.</param>
        /// <param name="payerType">The payer type.</param>
        public AllPaymentsByPayerQuery(Guid payerId, TransactionPartyType payerType)
        {
            this.PayerId = payerId;
            this.PayerType = payerType;
        }

        /// <summary>
        /// Gets the payer id.
        /// </summary>
        public Guid PayerId { get; private set; }

        /// <summary>
        /// Gets the payer type.
        /// </summary>
        public TransactionPartyType PayerType { get; private set; }
    }
}
