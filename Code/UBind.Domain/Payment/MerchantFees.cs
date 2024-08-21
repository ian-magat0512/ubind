// <copyright file="MerchantFees.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Payment
{
    /// <summary>
    /// Represents the merchant fees associated with card payment transaction.
    /// </summary>
    public class MerchantFees
    {
        /// <summary>
        /// Gets or sets the card scheme used for the merchant fee request.
        /// </summary>
        public string SchemeName { get; set; }

        /// <summary>
        /// Gets or sets the merchant request in the gateway.
        /// </summary>
        public string MerchantRequest { get; set; }

        /// <summary>
        /// Gets or sets the flat fee (in the currency specified) charged to the Payer for the service.
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Gets or sets the total surcharge amount to be charged for the transaction.
        /// </summary>
        public decimal SurchargeAmount { get; set; }

        /// <summary>
        /// Gets the total merchant fees.
        /// </summary>
        public decimal Total => this.Fee + this.SurchargeAmount;
    }
}
