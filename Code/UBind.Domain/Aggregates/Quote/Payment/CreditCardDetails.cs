// <copyright file="CreditCardDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Payment
{
    /// <summary>
    /// Credit card details required for making a purchase.
    /// </summary>
    public class CreditCardDetails : IPaymentMethodDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardDetails"/> class.
        /// </summary>
        /// <param name="number">The credit card number.</param>
        /// <param name="name">The name on the credit card.</param>
        /// <param name="expiryMonth">The month of the expiry date on the credit card.</param>
        /// <param name="expiryYear">The year of the expiry date on the credit card.</param>
        /// <param name="cvv">The card verification value (3 digit number on back).</param>
        public CreditCardDetails(
            string number,
            string name,
            string expiryMonth,
            int expiryYear,
            string cvv)
        {
            this.Number = number.Replace(" ", string.Empty);
            this.Name = name;
            this.ExpiryMonth = expiryMonth;
            this.ExpiryYear = expiryYear < 100 ? (2000 + expiryYear).ToString() : expiryYear.ToString();
            this.Cvv = cvv;
        }

        /// <summary>
        /// Gets the credit card number.
        /// </summary>
        public string Number { get; }

        /// <summary>
        /// Gets the name on the credit card.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the month of the expiry date of the credit card as an integer.
        /// </summary>
        public string ExpiryMonth { get; }

        /// <summary>
        /// Gets the year of the expiry date on the credit card as a 4 digit integer.
        /// </summary>
        public string ExpiryYear { get; }

        /// <summary>
        /// Gets the card verification value.
        /// </summary>
        public string Cvv { get; }

        /// <summary>
        /// Gets the month of the expiry date of the credit card in format MM.
        /// </summary>
        public string ExpiryMonthMM => $"{this.ExpiryMonth:D2}";

        /// <summary>
        /// Gets the year of the expiry date of the credit card in format yy.
        /// </summary>
        public string ExpiryYearyyyy => $"{this.ExpiryYear:D4}";

        /// <summary>
        /// Gets the year of the expiry date of the credit card in format yyyy.
        /// </summary>
        public string ExpiryYearyy => this.ExpiryYearyyyy.Substring(2);

        /// <summary>
        /// Gets the expiry date in MMyy format.
        /// </summary>
        public string ExpiryMMyy => $"{this.ExpiryMonthMM}{this.ExpiryYearyy}";

        /// <summary>
        /// Gets the expiry date in MMyyyy format.
        /// </summary>
        public string ExpiryMMyyyy => $"{this.ExpiryMonthMM}{this.ExpiryYearyyyy}";
    }
}
