// <copyright file="DeftPaymentRequest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;
    using NodaTime;
    using Org.BouncyCastle.Asn1;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Security;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// For generating a DEFT payment request payload.
    /// </summary>
    public class DeftPaymentRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeftPaymentRequest"/> class.
        /// </summary>
        /// <param name="configuration">Configuration for the DEFT account to use.</param>
        /// <param name="priceBreakdown">The payment details.</param>
        /// <param name="cardDetails">The credit card details.</param>
        /// <param name="customerReferenceNumber">The DEFT CRN (a 10 digit number).</param>
        /// <param name="reference">A private reference for tracking/reconciliation purposes.</param>
        /// <param name="drn">The DEFT reference number. This should have been requested via API prior.</param>
        /// <param name="billerCode">The biller code to be used.</param>
        /// <param name="cardScheme">The card scheme/type to be used for the request. This is used for creating the card mask number.</param>
        /// <param name="timestamp">The current time.</param>
        public DeftPaymentRequest(
            IDeftConfiguration configuration,
            PriceBreakdown priceBreakdown,
            CreditCardDetails cardDetails,
            string customerReferenceNumber,
            string reference,
            string drn,
            string billerCode,
            string cardScheme,
            Instant timestamp)
        {
            this.CustomerReference = reference;
            this.Amount = priceBreakdown.TotalPayable - (priceBreakdown.MerchantFees + priceBreakdown.MerchantFeesGst);
            this.Card = new CardDetails(
                cardDetails.Name,
                cardDetails.Number,
                cardDetails.ExpiryMMyy,
                cardDetails.Cvv,
                configuration.SecurityKey,
                configuration.PaymentUrl.Contains("/v2/"),
                cardScheme,
                timestamp);
            this.CurrencyCode = priceBreakdown.CurrencyCode;

            if (configuration.PaymentUrl.Contains("/v3/"))
            {
                this.Drn = drn;
            }
            else
            {
                this.CustomerReferenceNumber = ulong.Parse(customerReferenceNumber);
                this.BillerCode = ulong.Parse(billerCode);
                this.Drn = drn;
            }
        }

        /// <summary>
        /// Gets the DEFT reference number for the transaction. The DRN is generated from the Deft biller code and the CRN.
        /// </summary>
        [JsonProperty("drn")]
        public string Drn { get; private set; }

        /// <summary>
        /// Gets the currency code.
        /// </summary>
        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; } = PriceBreakdown.DefaultCurrencyCode;

        /// <summary>
        /// Gets the amount being paid (not including surcharge).
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; }

        /// <summary>
        /// Gets the email address of the notification recipient.
        /// </summary>
        [JsonProperty("recipientEmail")]
        public string RecipientEmail { get; }

        /// <summary>
        /// Gets the payment frequency.
        /// </summary>
        [JsonProperty("paymentFrequency")]
        public string PaymentFrequency => "ONE_OFF";

        /// <summary>
        /// Gets the ECOM indicator.
        /// </summary>
        [JsonProperty("ecomIndicator")]
        public string EcomIndicator => "07";

        /// <summary>
        /// Gets a reference tha UBind can make up.
        /// </summary>
        [JsonProperty("customerReference")]
        public string CustomerReference { get; }

        /// <summary>
        /// Gets a value indicating whether to inject fees and surcharge.
        /// </summary>
        [JsonProperty("injectFeeAndSurcharge")]
        public bool InjectFeeAndSurcharge => true;

        /// <summary>
        /// Gets the customer reference number (up to 10 digits).
        /// </summary>
        [JsonProperty("crn")]
        public ulong CustomerReferenceNumber { get; }

        /// <summary>
        /// Gets the Biller code to use in payment requests.
        /// </summary>
        [JsonProperty("dbc")]
        public ulong BillerCode { get; private set; }

        /// <summary>
        /// Gets the card details.
        /// </summary>
        [JsonProperty("cardDetails")]
        public CardDetails Card { get; }

        /// <summary>
        /// Clear sensitive data.
        /// </summary>
        public void ClearSensitiveData()
        {
            this.Card.ClearSensitiveData();
        }

        /// <summary>
        /// For card details that are sent to DEFT.
        /// </summary>
        public class CardDetails
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CardDetails"/> class.
            /// </summary>
            /// <param name="name">The name on the card.</param>
            /// <param name="number">The encrypted card number.</param>
            /// <param name="expiryDateMMyy">THe card expiry date in MMYY format.</param>
            /// <param name="cvv">The card CVV.</param>
            /// <param name="passKey">The password key to use for encrypting the data.</param>
            /// <param name="isV2">Flag to depict if configuration is for v2 of the payment provider.</param>
            /// <param name="cardScheme">The card scheme/type being used.</param>
            /// <param name="timestamp">The current time.</param>
            public CardDetails(string name, string number, string expiryDateMMyy, string cvv, string passKey, bool isV2, string cardScheme, Instant timestamp)
            {
                if (isV2)
                {
                    this.CardNumber = new string(number.Where(c => char.IsDigit(c)).ToArray());
                    this.Holder = name;
                    this.Cvv = cvv;
                }
                else
                {
                    var cardNumber = new string(number.Where(c => char.IsDigit(c)).ToArray());
                    this.EncodedNumber = this.EncryptValue(this.AppendWithTimestamp(cardNumber, timestamp), passKey);
                    this.HolderName = name;
                    this.EncodedCvv = this.EncryptValue(this.AppendWithTimestamp(cvv, timestamp), passKey);
                }

                if (this.EncodedNumber?.Length < 13 || this.CardNumber?.Length < 13)
                {
                    throw new ArgumentException($"Credit card number must be at least 13 digits.");
                }

                this.ExpiryDate = expiryDateMMyy;
                string bin = cardScheme.EqualsIgnoreCase("MASTERCARD") || cardScheme.EqualsIgnoreCase("VISA") ?
                    number.Substring(0, 8) :
                    number.Substring(0, 6);
                this.MaskedNumber = $"{bin}****{number.Substring(number.Length - 4)}";
            }

            /// <summary>
            /// Gets the expiry date in MMYY format.
            /// </summary>
            [JsonProperty("cardExpiryDate")]
            public string ExpiryDate { get; }

            /// <summary>
            /// Gets the masked card number in format BIN-****-LAST4DIGITS.
            /// </summary>
            [JsonProperty("cardNumberMask")]
            public string MaskedNumber { get; }

            /// <summary>
            /// Gets the encrypted card number.
            /// </summary>
            /// <remarks>Used for v3 of payment requests.</remarks>
            [JsonProperty("encodedCardNumber", NullValueHandling = NullValueHandling.Ignore)]
            public string EncodedNumber { get; private set; }

            /// <summary>
            /// Gets the card number.
            /// </summary>
            /// <remarks>Used for v2 of payment requests.</remarks>
            [JsonProperty("cardNumber", NullValueHandling = NullValueHandling.Ignore)]
            public string CardNumber { get; private set; }

            /// <summary>
            /// Gets the card holder name.
            /// </summary>
            /// <remarks>Used for v3 of payment requests.</remarks>
            [JsonProperty("cardHolderName", NullValueHandling = NullValueHandling.Ignore)]
            public string HolderName { get; }

            /// <summary>
            /// Gets the card holder name.
            /// </summary>
            /// <remarks>Used for v2 of payment requests.</remarks>
            [JsonProperty("cardHolder", NullValueHandling = NullValueHandling.Ignore)]
            public string Holder { get; }

            /// <summary>
            /// Gets the CVV.
            /// </summary>
            [JsonProperty("cardCVV", NullValueHandling = NullValueHandling.Ignore)]
            public string Cvv { get; private set; }

            /// <summary>
            /// Gets the encoded CCV.
            /// </summary>
            [JsonProperty("encodedCardCVV", NullValueHandling = NullValueHandling.Ignore)]
            public string EncodedCvv { get; private set; }

            /// <summary>
            /// Obscure CVV and all but last 4 digit of card number.
            /// </summary>
            public void ClearSensitiveData()
            {
                if (this.EncodedNumber.IsNotNullOrEmpty())
                {
                    this.EncodedNumber = this.CardNumber = $"************{this.MaskedNumber.Substring(this.MaskedNumber.Length - 4)}";
                }

                if (this.CardNumber.IsNotNullOrEmpty())
                {
                    this.CardNumber = $"************{this.MaskedNumber.Substring(this.MaskedNumber.Length - 4)}";
                }

                this.Cvv = "***";
                this.EncodedCvv = "***";
            }

            private string AppendWithTimestamp(string value, Instant timestamp)
            {
                var formattedTimestamp = timestamp.ToDateTimeUtc().ToString("ddMMyyyyHHmmss");
                return $"{value}|||{formattedTimestamp}";
            }

            private string EncryptValue(string input, string passPhrase)
            {
                Asn1Object obj = Asn1Object.FromByteArray(Convert.FromBase64String(passPhrase));
                DerSequence publicKeySequence = (DerSequence)obj;
                DerBitString encodedPublicKey = (DerBitString)publicKeySequence[1];
                DerSequence publicKey = (DerSequence)Asn1Object.FromByteArray(encodedPublicKey.GetBytes());

                DerInteger modulues = (DerInteger)publicKey[0];
                DerInteger exponent = (DerInteger)publicKey[1];
                RsaKeyParameters keyParams = new RsaKeyParameters(false, modulues.PositiveValue, exponent.PositiveValue);
                RSAParameters parameters = DotNetUtilities.ToRSAParameters(keyParams);

                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(parameters);
                    byte[] dataToEncrypt = Encoding.UTF8.GetBytes(input);
                    byte[] encryptedData = rsa.Encrypt(dataToEncrypt, false);
                    var securedString = Convert.ToBase64String(encryptedData);
                    return securedString;
                }
            }
        }
    }
}
