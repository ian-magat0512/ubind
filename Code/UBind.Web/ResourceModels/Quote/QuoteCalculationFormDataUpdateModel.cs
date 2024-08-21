// <copyright file="QuoteCalculationFormDataUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;
    using UBind.Application;
    using UBind.Domain.Payment;

    /// <summary>
    /// Form and and calculation data for an application POSTed from client.
    /// </summary>
    public class QuoteCalculationFormDataUpdateModel : IQuoteResourceModel
    {
        /// <inheritdoc/>
        [Required]
        public Guid QuoteId { get; set; }

        public Guid? ProductReleaseId { get; set; }

        /// <summary>
        /// Gets or sets form data json.
        /// </summary>
        [JsonPropertyName("formDataJson")]
        [Required]
        public CalculationDataModel FormDataJson { get; set; } = new CalculationDataModel();

        /// <summary>
        /// Gets or sets data to assist with the calculation of merchant fees and surcharges.
        /// </summary>
        [JsonPropertyName("paymentData")]
        public PaymentData? PaymentData { get; set; }
    }
}
