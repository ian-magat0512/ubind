// <copyright file="AccountingTransactionHistoryItemModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Accounting.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using NodaMoney;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Resource model for serving the list of accounting transactions i.e. payments, refunds, credit notes, and invoices.
    /// </summary>
    public class AccountingTransactionHistoryItemModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountingTransactionHistoryItemModel"/> class.
        /// </summary>
        /// <param name="id">The transaction id.</param>
        /// <param name="transactionDate">The transaction Date.</param>
        /// <param name="type">The accounting documentType.</param>
        /// <param name="amount">THe payment amount.</param>
        /// <param name="referenceNumber">The reference Number.</param>
        /// <param name="isAmountNegative">Is Amount displayed as negative.</param>
        public AccountingTransactionHistoryItemModel(Guid id, Instant transactionDate, AccountingDocumentType type, Money amount, string referenceNumber, bool isAmountNegative = false)
        {
            this.Id = id;
            this.TransactionDateTime = transactionDate.ToExtendedIso8601String();
            this.TransactionHistoryType = (int)type;
            this.Amount = amount;
            this.DisplayedAmount = isAmountNegative ? $"- {amount.Amount.ToDollarsAndCents()}" : amount.Amount.ToDollarsAndCents();
            this.ReferenceNumber = referenceNumber;
        }

        /// <summary>
        /// Gets the ID of the quote.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the total amount in dollars and cents.
        /// </summary>
        [JsonProperty]
        public string DisplayedAmount { get; private set; }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        [JsonProperty]
        public Money Amount { get; private set; }

        /// <summary>
        /// Gets the date the quote was created in Iso 8601 format.
        /// </summary>
        [JsonProperty]
        public string TransactionDateTime { get; private set; }

        /// <summary>
        /// Gets the Transaction History Type.
        /// </summary>
        [JsonProperty]
        public int TransactionHistoryType { get; private set; }

        /// <summary>
        /// Gets the Reference Number.
        /// </summary>
        [JsonProperty]
        public string ReferenceNumber { get; private set; }
    }
}
