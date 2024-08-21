// <copyright file="PaymentData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Payment;

using System.Text.Json.Serialization;

/// <summary>
/// Data which is used to calculate fees and surcharges for the given payment type.
/// </summary>
public class PaymentData
{

    private const int CardBinLength = 8;
    private string? zeroPaddedBin;

    [JsonConstructor]
    public PaymentData()
    {
    }

    public PaymentData(string cardNumber, int cardNumberLength, string paymentMethod, bool singlePayment)
    {
        this.CardBin = this.GenerateCardBin(cardNumber);
        this.CardNumberLength = cardNumberLength;
        this.PaymentMethod = paymentMethod;
        this.SinglePayment = singlePayment;
    }

    [JsonPropertyName("cardNumberLength")]
    public int CardNumberLength { get; set; }

    /// <summary>
    /// Gets or sets the Card BIN, which is the Bank Identification Number,
    /// which is the first eight numbers of the credit card.
    /// </summary>
    /// <remarks>
    /// This has been updated from max of 6 to 8 numbers with latest BIN update. 08/03.
    /// </remarks>
    [JsonPropertyName("cardBin")]
    public string? CardBin { get; set; }

    /// <summary>
    /// Gets or sets the payment method, one of "CARD" or "DIRECT_DEBIT".
    /// </summary>
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a once off or recurring payment.
    /// </summary>
    [JsonPropertyName("singlePayment")]
    public bool SinglePayment { get; set; }

    [JsonIgnore]
    public string? ZeroPaddedCardBin
    {
        get
        {
            if (this.zeroPaddedBin != null)
            {
                return this.zeroPaddedBin;
            }
            if (!string.IsNullOrEmpty(this.CardBin) && this.CardNumberLength > 0)
            {
                this.zeroPaddedBin = this.CardBin
                    .PadRight(this.CardNumberLength, '0');
                return this.zeroPaddedBin;
            }
            return null;
        }
    }

    // Use to trim down card number passed down to the constructor
    private string GenerateCardBin(string cardNumber)
    {
        return cardNumber.PadRight(CardBinLength, '0')
                    .Substring(0, CardBinLength);
    }
}
