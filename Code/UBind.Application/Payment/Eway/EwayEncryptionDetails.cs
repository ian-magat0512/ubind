// <copyright file="EwayEncryptionDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Eway
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote.Payment;

    /// <summary>
    /// Resource model for serving credit card details to EWAY Encryption Service.
    /// </summary>
    public class EwayEncryptionDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EwayEncryptionDetails"/> class.
        /// </summary>
        /// <param name="cardDetails">The card details to be encrypted.</param>
        public EwayEncryptionDetails(CreditCardDetails cardDetails)
        {
            this.Method = "eCrypt";
            this.Items = this.GetValues(cardDetails);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwayEncryptionDetails"/> class.
        /// </summary>
        /// <param name="method">The encryption method used/to be used.</param>
        /// <param name="items">The items values encrypted.</param>
        public EwayEncryptionDetails(string method, IEnumerable<EncryptionItemValues> items)
        {
            this.Items = items;
            this.Method = method;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EwayEncryptionDetails"/> class.
        /// </summary>
        [JsonConstructor]
        public EwayEncryptionDetails()
        {
        }

        /// <summary>
        /// Gets or sets the method of encryption to be used.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets the values to be encrypted.
        /// </summary>
        public IEnumerable<EncryptionItemValues> Items { get; private set; }

        /// <summary>
        /// Generate new instance of credit card details from encrypted details and unencrypted details.
        /// </summary>
        /// <param name="expiryMonth">The credit card expiry month.</param>
        /// <param name="expiryYear">The credit card expiry year.</param>
        /// <param name="cardName">The credit card name.</param>
        /// <returns>A new instance of <see cref="CreditCardDetails"/> with encrypted card number and CVN.</returns>
        public CreditCardDetails Map(string expiryMonth, int expiryYear, string cardName)
        {
            var number = this.Items.Where(x => x.Name == "card").Select(x => x.Value).FirstOrDefault();
            var cvv = this.Items.Where(x => x.Name == "CVN").Select(x => x.Value).FirstOrDefault();
            return new CreditCardDetails(number, cardName, expiryMonth, expiryYear, cvv);
        }

        private IEnumerable<EncryptionItemValues> GetValues(CreditCardDetails cardDetails)
        {
            var card = new EncryptionItemValues("card", cardDetails.Number);
            var cvn = new EncryptionItemValues("CVN", cardDetails.Cvv);
            return new[] { card, cvn };
        }
    }
}
