// <copyright file="FinancialTransactionAllocationReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Accounting
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using NodaMoney;
    using NodaTime;

    /// <inheritdoc/>
    public abstract class FinancialTransactionAllocationReadModel<TFinancialTransactionReadModel, TCommercialDocumentReadModel> :
        IFinancialTransactionAllocationReadModel<TFinancialTransactionReadModel, TCommercialDocumentReadModel>,
        IReadModel<Guid>
        where TFinancialTransactionReadModel : class
        where TCommercialDocumentReadModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAllocationReadModel{TFinancialTransactionReadModel, TCommercialDocumentReadModel}"/> class.
        /// </summary>
        /// <param name="id">The readmodel id.</param>
        /// <param name="amount">The amount of allocation.</param>
        /// <param name="transaction">The financial transaction.</param>
        /// <param name="commercialDocument">The commercial document.</param>
        /// <param name="createdTimestamp">The created time of this allocation.</param>
        protected FinancialTransactionAllocationReadModel(
            Guid tenantId,
            Guid id,
            Money amount,
            TFinancialTransactionReadModel transaction,
            TCommercialDocumentReadModel commercialDocument,
            Instant createdTimestamp)
        {
            this.TenantId = tenantId;
            this.Id = id;
            this.Amount = amount.Amount;
            this.FinancialTransaction = transaction;
            this.CommercialDocument = commercialDocument;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionAllocationReadModel{TFinancialTransactionReadModel, TCommercialDocumentReadModel}"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        protected FinancialTransactionAllocationReadModel()
        {
        }

        /// <inheritdoc/>
        [Key]
        public Guid Id { get; private set; }

        /// <inheritdoc/>
        public TFinancialTransactionReadModel FinancialTransaction { get; private set; }

        /// <inheritdoc/>
        public TCommercialDocumentReadModel CommercialDocument { get; private set; }

        /// <inheritdoc/>
        public decimal Amount { get; private set; }

        /// <inheritdoc/>
        public Instant CreatedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);
            }

            private set
            {
                this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        /// <inheritdoc/>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <inheritdoc/>
        public bool IsDeleted { get; set; }

        /// <inheritdoc/>
        public Guid TenantId { get; set; }
    }
}
