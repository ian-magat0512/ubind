// <copyright file="SavedPaymentMethod.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using NodaTime;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Refers to the a persisted payment method for a customer.
    /// </summary>
    public class SavedPaymentMethod : MutableEntity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavedPaymentMethod"/> class.
        /// </summary>
        public SavedPaymentMethod(
            Guid tenantId,
            Guid customerId,
            Guid paymentMethodId,
            LocalDate? expiryDate,
            string identificationData,
            string paymentGatewayUsageData,
            Instant creationTimestamp)
            : base(Guid.NewGuid(), creationTimestamp)
        {
            this.TenantId = tenantId;
            this.PaymentMethodId = paymentMethodId;
            this.CustomerId = customerId;
            this.ExpiryDate = expiryDate;
            this.IdentificationDataJson = identificationData;
            this.AuthenticationDataJson = paymentGatewayUsageData;
        }

        /// <summary>
        /// Parameterless constructor for EF.
        /// </summary>
        private SavedPaymentMethod()
            : base(default, default)
        {
        }

        /// <summary>
        /// Gets or sets the ID of the tenant the customer owning this saved payment method is under.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the customer that owns this saved payment method.
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the payment method being used.
        /// </summary>
        public Guid PaymentMethodId { get; set; }

        /// <summary>
        /// Gets or sets the expiry date for the payment method, if applicable.
        /// </summary>
        public LocalDate? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the details required to identify this Customer Payment Method in JSON form.
        /// The information stored here can be presented to the user to help them to identify which
        /// of their saved payment methods this one is. This information is presented to them in user
        /// interfaces, and it typically contains things like a masked card number, the name on the card etc.
        /// </summary>
        public string IdentificationDataJson { get; set; }

        /// <summary>
        /// Gets or sets the details required to actually use this Payment Method, if applicable.
        /// This is information we need to store to use this payment method with the payment gateway provider.
        /// Typically this will consist of a saved card token which was generated the first time the card
        /// was entered.
        /// </summary>
        /// <remarks>
        /// E.g of these are payment tokens.
        /// </remarks>
        public string AuthenticationDataJson { get; set; }

        /// <summary>
        /// Returns whether this payment method is expired or not.
        /// </summary>
        /// <returns>True if expired, otherwise false.</returns>
        /// <remarks>If payment method does not have an expiry date, this returns false.</remarks>
        public bool IsExpired(Instant currentTime)
        {
            return this.ExpiryDate.HasValue && this.ExpiryDate.Value >= currentTime.ToLocalDateInAet();
        }

        public void ClearSensitiveInformation()
        {
            this.AuthenticationDataJson = null;
        }
    }
}
