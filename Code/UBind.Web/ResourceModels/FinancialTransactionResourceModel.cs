// <copyright file="FinancialTransactionResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Accounting.Enums;

    /// <summary>
    /// For representing a financial transaction such as Payments and Refunds.
    /// </summary>
    public class FinancialTransactionResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionResourceModel"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for model binding.</remarks>
        [JsonConstructor]
        public FinancialTransactionResourceModel()
        {
        }

        /// <summary>
        /// Gets or sets the customer Id.
        /// </summary>
        [JsonProperty]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        [JsonProperty]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        [JsonProperty]
        public string TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the financial transaction type.
        /// </summary>
        [JsonProperty]
        public FinancialTransactionType Type { get; set; }
    }
}
