// <copyright file="PremiumFundingPaymentModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Application.Services.Encryption;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Premium funding payment request POSTed from client.
    /// </summary>
    public class PremiumFundingPaymentModel : IQuoteResourceModel
    {
        public Guid? ProductReleaseId { get; set; }

        /// <inheritdoc/>
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets the ID of the calculation result that specifies the payment amount.
        /// </summary>
        [Required]
        [JsonProperty]
        public Guid CalculationResultId { get; private set; }

        /// <summary>
        /// Gets this ID of the premium funding proposal that is to be paid for.
        /// </summary>
        [Required]
        [JsonProperty]
        public Guid PremiumFundingProposalId { get; private set; }

        /// <summary>
        /// Gets or sets the credit card details.
        /// </summary>
        public CreditCardDetailsModel CreditCardDetails { get; set; }

        /// <summary>
        /// Gets or sets the credit card details.
        /// </summary>
        public BankAccountDetailsModel BankAccountDetails { get; set; }

        /// <summary>
        /// Gets the payment method details for the request.
        /// </summary>
        /// <param name="decryptor">The encryption/decryption service.</param>
        /// <returns>Payment method details of credit card or bank account.</returns>
        public IPaymentMethodDetails GetPaymentMethodDetails(IAsymmetricEncryptionService decryptor)
        {
            return this.CreditCardDetails != null
                ? (IPaymentMethodDetails)this.CreditCardDetails.Map(decryptor)
                : this.BankAccountDetails != null
                    ? this.BankAccountDetails.Map(decryptor)
                    : throw new InvalidOperationException("One of CreditCardDetails or BankAccountDetails must be provided.");
        }
    }
}
