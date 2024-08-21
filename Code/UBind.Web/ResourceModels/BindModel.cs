// <copyright file="BindModel.cs" company="uBind">
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
    using UBind.Domain.Dto;

    /// <summary>
    /// Bind data for a quote POSTed from client.
    /// </summary>
    public class BindModel : IBindDataModel
    {
        /// <inheritdoc />
        [Required]
        public Guid QuoteId { get; set; }

        /// <inheritdoc/>
        [Required]
        public Guid CalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the funding proposal.
        /// </summary>
        public Guid? PremiumFundingProposalId { get; set; }

        /// <summary>
        /// Gets or sets the credit card details, if any, otherwise null.
        /// </summary>
        public CreditCardDetailsModel CreditCardDetails { get; set; }

        /// <summary>
        /// Gets or sets the credit card details.
        /// </summary>
        public BankAccountDetailsModel BankAccountDetails { get; set; }

        /// <summary>
        /// Gets or sets the Id of the saved payment method to use.
        /// </summary>
        public Guid SavedPaymentMethodId { get; set; }

        /// <summary>
        /// Gets or sets the payment token, if any, otherwise null.
        /// </summary>
        public string TokenId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IP address is whitelisted.
        /// </summary>
        public bool IsWhitelisted { get; set; }

        /// <summary>
        /// Gets the payment method details for the request.
        /// </summary>
        /// <param name="decryptor">The encryption/decryption service.</param>
        /// <returns>Payment method details of credit card or bank account.</returns>
        public IPaymentMethodDetails GetPaymentMethodDetails(IAsymmetricEncryptionService decryptor)
        {
            // TODO: revisit the requirement for payment details. Sometimes we may want to bind without the need for immediate payment - it could be paid manually later.???????
            return this.CreditCardDetails != null
                ? (IPaymentMethodDetails)this.CreditCardDetails.Map(decryptor)
                : this.BankAccountDetails != null
                    ? this.BankAccountDetails.Map(decryptor)
                    : throw new InvalidOperationException("One of CreditCardDetails or BankAccountDetails must be provided.");
        }

        /// <summary>
        /// Returns the binding requirements for processig.
        /// </summary>
        /// <returns>A bind requirement dto.</returns>
        public BindRequirementDto ToDto()
        {
            return new BindRequirementDto(this.CalculationResultId);
        }
    }
}
