// <copyright file="CardInformation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Extensions;

    public class CardInformation : ISchemaObject
    {
        public CardInformation(MaskedCreditCardDetails cardDetails)
        {
            this.ExpiryYear = cardDetails.ExpiryYear;
            this.ExpiryMonth = cardDetails.ExpiryMonth;
            this.MaskedCardNumber = cardDetails.MaskedCardNumber;
            this.CardHolderName = cardDetails.CardHolderName;
            this.CardType = cardDetails.CardType;

            var systemClock = SystemClock.Instance;
            var currentTime = systemClock.Now();
            var expiryDate = new LocalDate(this.ExpiryYear, this.ExpiryMonth, 1);
            this.Expired = expiryDate < currentTime.ToLocalDateInAet();
        }

        [JsonConstructor]
        public CardInformation()
        {
        }

        [JsonProperty("expiryYear")]
        public int ExpiryYear { get; set; }

        [JsonProperty("expiryMonth")]
        public int ExpiryMonth { get; set; }

        [JsonProperty("expired")]
        public bool Expired { get; set; }

        [JsonProperty("cardType")]
        public string CardType { get; set; }

        [JsonProperty("cardHolderName")]
        public string CardHolderName { get; set; }

        [JsonProperty("maskedCardNumber")]
        public string MaskedCardNumber { get; set; }
    }
}
