// <copyright file="BankAccountDetailsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Application.Services.Encryption;
    using UBind.Domain.Aggregates.Quote.Payment;

    /// <summary>
    /// Resource model for bank account details.
    /// </summary>
    public class BankAccountDetailsModel : IPaymentMethodDetailsModel<BankAccountDetails>
    {
        /// <summary>
        /// Gets or sets the credit card number.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "bankAccountNumber")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the credit card name.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "bankAccountName")]
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the Bank-State-Branch (BSB) number of the account.
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "bankAccountBsb")]
        public string BsbNumber { get; set; }

        public BankAccountDetails Map(IAsymmetricEncryptionService encryptionService)
        {
            var bsbDigits = new string(this.BsbNumber.Where(c => char.IsDigit(c)).ToArray());
            return new BankAccountDetails(this.AccountName, this.AccountNumber, bsbDigits);
        }
    }
}
