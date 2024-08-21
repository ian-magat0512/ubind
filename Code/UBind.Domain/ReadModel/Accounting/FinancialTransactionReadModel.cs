// <copyright file="FinancialTransactionReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Base class for financial transactions read models, like Payments and Refunds.
    /// </summary>
    /// <typeparam name="TAllocation">The allocation type.</typeparam>
    public abstract class FinancialTransactionReadModel<TAllocation> : EntityReadModel<Guid>, IFinancialTransactionReadModel<TAllocation>, IReadModel<Guid>
        where TAllocation : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionReadModel{TAllocation}"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="id">The transaction id.</param>
        /// <param name="amount">The transaction amount.</param>
        /// <param name="referenceNumber">The reference number.</param>
        /// <param name="payerId">The payer id.</param>
        /// <param name="payerType">The payer type.</param>
        /// <param name="payeeId">The payee id.</param>
        /// <param name="payeeType">The payee type.</param>
        /// <param name="createdTimestamp">The created time for audit purposes.</param>
        /// <param name="transactionTime">The time of transaction, as entered by the user.</param>
        public FinancialTransactionReadModel(
            Guid tenantId,
            Guid id,
            Money amount,
            string referenceNumber,
            Guid payerId,
            TransactionPartyType payerType,
            Guid? payeeId,
            TransactionPartyType? payeeType,
            Instant createdTimestamp,
            Instant transactionTime)
            : base(tenantId, id, createdTimestamp)
        {
            this.Amount = amount.Amount;
            this.Currency = amount.Currency.Code;
            this.ReferenceNumber = referenceNumber;
            this.PayerId = payerId;
            this.PayerType = payerType;
            this.PayeeId = payeeId;
            this.PayeeType = payeeType;
            this.TransactionTimestamp = transactionTime;
            this.IsDeleted = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionReadModel{TAllocation}"/> class.
        /// Parameterless Constructor for FinancialTransactionReadModel.
        /// </summary>
        protected FinancialTransactionReadModel()
        {
        }

        /// <inheritdoc/>
        public decimal Amount { get; private set; }

        /// <inheritdoc/>
        public string Currency { get; private set; }

        /// <inheritdoc/>
        public Guid? PayeeId { get; set; }

        /// <inheritdoc/>
        public TransactionPartyType? PayeeType { get; set; }

        /// <inheritdoc/>
        public Guid PayerId { get; set; }

        /// <inheritdoc/>
        public TransactionPartyType PayerType { get; set; }

        /// <inheritdoc/>
        public string ReferenceNumber { get; private set; }

        /// <inheritdoc/>
        public bool IsDeleted { get; set; }

        /// <inheritdoc/>
        public virtual ICollection<TAllocation> Allocations { get; set; } = new Collection<TAllocation>();

        /// <inheritdoc/>
        public Instant TransactionTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.TransactionTicksSinceEpoch);
            }

            private set
            {
                this.TransactionTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        /// <summary>
        /// Gets representation of <see cref="FinancialTransactionReadModel{TAllocation}.TransactionTimestamp"/> in ticks since epoch to allow persisting via EF.
        /// </summary>
        public long TransactionTicksSinceEpoch { get; private set; }
    }
}
