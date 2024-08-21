// <copyright file="CustomerPaymentMethod.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Entities;

    public class CustomerPaymentMethod : BaseEntity<SavedPaymentMethod>
    {
        public CustomerPaymentMethod(Guid id)
            : base(id)
        {
        }

        public CustomerPaymentMethod(SavedPaymentMethod model)
            : base(model.Id, model.CreatedTicksSinceEpoch, model.LastModifiedTicksSinceEpoch)
        {
            this.TenantId = model.TenantId.ToString();
            this.PaymentMethodId = model.PaymentMethodId.ToString();
            var maskedCardDetails = JsonConvert.DeserializeObject<MaskedCreditCardDetails>(model.IdentificationDataJson);
            this.CardDetails = new CardInformation(maskedCardDetails);
        }

        [JsonConstructor]
        public CustomerPaymentMethod()
        {
        }

        /// <summary>
        /// Gets or sets the tenant id of the customer owning the payment method.
        /// </summary>
        [JsonProperty(PropertyName = "tenantId", Order = 21)]
        [Required]
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant of the customer owning the payment method.
        /// </summary>
        [JsonProperty(PropertyName = "tenant", Order = 22)]
        public Tenant Tenant { get; set; }

        /// <summary>
        ///  Gets or sets the ID of the payment method for which this saved payment can be used.
        /// </summary>
        [JsonProperty(PropertyName = "paymentMethodId", Order = 23)]
        public string PaymentMethodId { get; set; }

        /// <summary>
        ///  Gets or sets the payment method for which this saved payment can be used.
        /// </summary>
        [JsonProperty(PropertyName = "paymentMethod", Order = 24)]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the card/bank details for this payment method.
        /// </summary>
        /// <remarks>For now, this only outputs card details.</remarks>
        [JsonProperty(PropertyName = "cardDetails", Order = 25)]
        public CardInformation CardDetails { get; set; }
    }
}
