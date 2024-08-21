// <copyright file="CreditCardPaymentModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Credit card payment request POSTed from client.
    /// </summary>
    public class CreditCardPaymentModel : QuoteFormDataUpdateModel
    {
        /// <summary>
        /// Gets or sets the ID of the calculation result that specifies the payment amount.
        /// </summary>
        [Required]
        public Guid CalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets the credit card details.
        /// </summary>
        public CreditCardDetailsModel CreditCardDetails { get; set; }

        /// <summary>
        /// Gets or sets the saved payment method Id to use.
        /// </summary>
        public Guid SavedPaymentMethodId { get; set; }
    }
}
