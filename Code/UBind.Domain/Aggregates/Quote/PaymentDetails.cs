// <copyright file="PaymentDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
namespace UBind.Domain.Aggregates.Quote
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using UBind.Domain.Enums;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// For capturing details of a payment.
    /// </summary>
    public class PaymentDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetails"/> class.
        /// </summary>
        /// <param name="paymentGateway">The gateway used for payment.</param>
        /// <param name="totalAmount">The total amount paid.</param>
        /// <param name="reference">A reference identifying the payment.</param>
        /// <param name="request">The request that was used to make the payment.</param>
        /// <param name="response">The response that was received.</param>
        public PaymentDetails(
            PaymentGatewayName paymentGateway,
            decimal totalAmount,
            string? reference,
            string? request,
            string? response)
        {
            this.PaymentGateway = paymentGateway;
            this.TotalAmount = totalAmount;
            this.Reference = reference;
            this.Request = request;
            this.Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetails"/> class.
        /// </summary>
        /// <param name="reference">A reference identifying the payment.</param>
        /// <remarks>Only for use in importing existing payments that did not capture amount.</remarks>
        public PaymentDetails(string reference)
            : this(PaymentGatewayName.None, 0m, reference, string.Empty, string.Empty)
        {
        }

        [JsonConstructor]
        private PaymentDetails()
        {
        }

        /// <summary>
        /// Gets the total amount paid.
        /// </summary>
        [JsonProperty("paymentType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentGatewayName PaymentGateway { get; private set; }

        /// <summary>
        /// Gets the total amount paid.
        /// </summary>
        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; private set; }

        /// <summary>
        /// Gets a reference identifying the payment.
        /// </summary>
        [JsonProperty("reference")]
        public string? Reference { get; private set; }

        /// <summary>
        /// Gets the response received from the payment gateway (serialized Json).
        /// </summary>
        [JsonProperty("request")]
        [JsonConverter(typeof(RawJsonConverter))]
        public string? Request { get; private set; }

        /// <summary>
        /// Gets the response received from the payment gateway (serialized Json).
        /// </summary>
        [JsonProperty("response")]
        [JsonConverter(typeof(RawJsonConverter))]
        public string? Response { get; private set; }
    }
}
