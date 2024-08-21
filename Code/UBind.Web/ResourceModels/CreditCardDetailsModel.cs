// <copyright file="CreditCardDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.ComponentModel.DataAnnotations;
    using UBind.Application.Services.Encryption;
    using UBind.Domain.Aggregates.Quote.Payment;

    /// <summary>
    /// Credit card details.
    /// </summary>
    public class CreditCardDetailsModel : IPaymentMethodDetailsModel<CreditCardDetails>
    {
        /// <summary>
        /// Gets or sets the encrypted credit card number.
        /// </summary>
        [Required]
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the credit card name.
        /// </summary>
        [Required]
        public string CreditCardName { get; set; }

        /// <summary>
        /// Gets or sets the credit card Expiry Date as a string.
        /// </summary>
        [Required]
        public string CreditCardExpiry { get; set; }

        /// <summary>
        /// Gets or sets the credit card CCV.
        /// </summary>
        [Required]
        public string CreditCardCCV { get; set; }

        /// <inheritdoc/>
        public CreditCardDetails Map(IAsymmetricEncryptionService encryptionService)
        {
            var expiryParts = this.CreditCardExpiry.Split('/');
            var expiryMonth = expiryParts[0];
            var expiryYear = int.Parse(expiryParts[1]);

            // decrypt here.
            var cardNumber = encryptionService.Decrypt(this.CreditCardNumber);
            var ccv = encryptionService.Decrypt(this.CreditCardCCV);
            return new CreditCardDetails(
                cardNumber,
                this.CreditCardName,
                expiryMonth,
                expiryYear,
                ccv);
        }
    }
}
