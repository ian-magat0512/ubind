// <copyright file="QuoteExportModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using Newtonsoft.Json;
    using StackExchange.Profiling.Internal;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.JsonConverters;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Quote model for export service.
    /// </summary>
    public class QuoteExportModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteExportModel"/> class.
        /// </summary>
        /// <param name="quote">Quote read model.</param>
        public QuoteExportModel(IQuoteReadModelSummary quote)
        {
            this.QuoteId = quote.QuoteId;
            this.OrganisationId = quote.OrganisationId;
            this.TenantId = quote.TenantId;
            this.ProductId = quote.ProductId;
            this.Environment = quote.Environment;
            this.InvoiceNumber = quote.InvoiceNumber;
            this.PolicyNumber = quote.PolicyNumber;
            this.QuoteNumber = quote.QuoteNumber;
            this.CreatedDateTime = quote.CreatedTimestamp.ToExtendedIso8601String();
            this.InvoiceDateTime = quote.InvoiceTimestamp.ToExtendedIso8601String();
            this.SubmissionDateTime = quote.SubmissionTimestamp.ToExtendedIso8601String();
            this.PolicyInceptionDateTime = quote.PolicyInceptionTimestamp?.ToExtendedIso8601String();
            this.PolicyIssued = quote.PolicyIssued;
            this.IsInvoiced = quote.IsInvoiced;
            this.IsPaidFor = quote.IsPaidFor;
            this.IsSubmitted = quote.IsSubmitted;
            this.QuoteState = quote.QuoteState;
            this.QuoteType = quote.QuoteType.ToString();
            this.LatestFormData = quote.LatestFormData.ToJson();
            this.LatestCalculationResultJson = quote.LatestCalculationResult.ToJson();
        }

        /// <summary>
        /// Gets the application ID.
        /// </summary>
        public Guid QuoteId { get; private set; }

        /// <summary>
        /// Gets the ID of the organisation this quote was created under.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the product ID.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the invoice number.
        /// </summary>
        public string InvoiceNumber { get; private set; }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        public string PolicyNumber { get; private set; }

        /// <summary>
        /// Gets the quote state.
        /// </summary>
        public string QuoteState { get; }

        /// <summary>
        /// Gets the quote type.
        /// </summary>
        public string QuoteType { get; }

        /// <summary>
        /// Gets the quote number.
        /// </summary>
        public string QuoteNumber { get; private set; }

        /// <summary>
        /// Gets the created time.
        /// </summary>
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the invoice time.
        /// </summary>
        public string InvoiceDateTime { get; private set; }

        /// <summary>
        /// Gets the submission time.
        /// </summary>
        public string SubmissionDateTime { get; private set; }

        /// <summary>
        /// Gets the policy inception time.
        /// </summary>
        public string PolicyInceptionDateTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy is issued.
        /// </summary>
        public bool PolicyIssued { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote is invoiced.
        /// </summary>
        public bool IsInvoiced { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quotes has payment.
        /// </summary>
        public bool IsPaidFor { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote is submitted.
        /// </summary>
        public bool IsSubmitted { get; private set; }

        /// <summary>
        /// Gets  the form model.
        /// </summary>
        [JsonConverter(typeof(RawJsonConverter))]
        public string LatestFormData { get; private set; }

        /// <summary>
        /// Gets the calculation result.
        /// </summary>
        [JsonProperty(PropertyName = "latestCalculationResult")]
        [JsonConverter(typeof(RawJsonConverter))]
        public string LatestCalculationResultJson { get; private set; }
    }
}
