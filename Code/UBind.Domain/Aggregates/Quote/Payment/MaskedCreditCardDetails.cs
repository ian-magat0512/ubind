// <copyright file="MaskedCreditCardDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Payment
{
    using Newtonsoft.Json;

    public class MaskedCreditCardDetails
    {
        public MaskedCreditCardDetails(
            string maskedCardNumber,
            string name,
            int expiryMonth,
            int expiryYear,
            string cardType)
        {
            this.MaskedCardNumber = maskedCardNumber;
            this.CardHolderName = name;
            this.ExpiryMonth = expiryMonth;
            this.ExpiryYear = expiryYear;
            this.CardType = cardType;
        }

        [JsonConstructor]
        public MaskedCreditCardDetails()
        {
        }

        [JsonProperty("cardHolderName")]
        public string CardHolderName { get; protected set; }

        [JsonProperty("maskedCardNumber")]
        public string MaskedCardNumber { get; protected set; }

        [JsonProperty("expiryYear")]
        public int ExpiryYear { get; protected set; }

        [JsonProperty("expiryMonth")]
        public int ExpiryMonth { get; protected set; }

        [JsonProperty("cardType")]
        public string CardType { get; protected set; }
    }
}
