// <copyright file="PaymentDetailsViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// A view model for representing payment details.
    /// </summary>
    public class PaymentDetailsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentDetailsViewModel"/> class.
        /// </summary>
        /// <param name="paymentAttemptResult">The payment to expose.</param>
        public PaymentDetailsViewModel(Domain.Aggregates.Quote.PaymentAttemptResult paymentAttemptResult)
        {
            var paymentDetails = paymentAttemptResult.PaymentDetails;
            this.Succeeded = paymentAttemptResult.IsSuccess;
            this.CreatedTimestamp = paymentAttemptResult.CreatedTimestamp;
            this.Reference = paymentDetails.Reference;
            this.TotalAmount = paymentDetails.TotalAmount.ToDollarsAndCents();
            this.Request = new JsonViewModel(paymentDetails.Request);
            this.Response = new JsonViewModel(paymentDetails.Response);
        }

        /// <summary>
        /// Gets a value indicating whether the payment succeeded.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets the payment created time.
        /// </summary>
        public Instant CreatedTimestamp { get; }

        /// <summary>
        /// Gets the payment created time.
        /// </summary>
        public Instant CreationTime => this.CreatedTimestamp;

        /// <summary>
        /// Gets the payment reference.
        /// </summary>
        public string Reference { get; }

        /// <summary>
        /// Gets a string representation of the total amount paid in dollars and cents.
        /// </summary>
        public string TotalAmount { get; }

        /// <summary>
        /// Gets the json representation of the request that was sent to the payment gateway.
        /// </summary>
        public JsonViewModel Request { get; }

        /// <summary>
        /// Gets the json representation of the response that was received from the payment gateway.
        /// </summary>
        public JsonViewModel Response { get; }

        /// <summary>
        /// Gets a form property value as a string.
        /// </summary>
        /// <param name="propertyName">The name .</param>
        /// <returns>A string representing the value of the property, or null if the property is not found.</returns>
        /// <remarks>Provided for backwards compatibility with old payment view model.</remarks>
        public string this[string propertyName] =>
            propertyName == "reference" ? this.Reference :
            propertyName == "totalAmount" ? this.TotalAmount :
            propertyName == "success" ? (this.Succeeded.ToString() == "True" ? "Yes" : "No") :
            propertyName == "creationTime" ? this.CreationTime
                .InZone(Timezones.AET).LocalDateTime.ToString() :
            null;
    }
}
