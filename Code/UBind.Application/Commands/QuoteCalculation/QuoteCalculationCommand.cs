// <copyright file="QuoteCalculationCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.QuoteCalculation
{
    using System;
    using System.Collections.Generic;
    using UBind.Application.FlexCel;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;
    using UBind.Domain.Json;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// This class is need for sending the quote calculation to command handler.
    /// </summary>
    public class QuoteCalculationCommand : ICommand<CalculationResponseModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCalculationCommand"/> class.
        /// </summary>
        /// <param name="productContext">The product context.</param>
        /// <param name="quoteId">The ID of the quote, or null.</param>
        /// <param name="policyId">The ID of the policy, otherwise null.
        /// This is needed for adjustment or cancellation quote types.</param>
        /// <param name="calculationDataModel">The calculation data model.</param>
        /// <param name="paymentData">The payment data.</param>
        /// <param name="persistResults">If true, the calculation will be persisted against the quote and can be used
        /// for policy transactions.</param>
        public QuoteCalculationCommand(
             IProductContext productContext,
             Guid? quoteId,
             Guid? policyId,
             QuoteType? quoteType,
             Guid? productReleaseId,
             CalculationDataModel calculationDataModel,
             PaymentData? paymentData,
             bool hasFundingProposal,
             bool persistResults,
             Guid? organisationId)
        {
            this.ProductContext = productContext;
            this.QuoteId = quoteId;
            this.PolicyId = policyId;
            this.QuoteType = quoteType;
            this.ProductReleaseId = productReleaseId;
            this.CalculationDataModel = calculationDataModel;
            this.PaymentData = paymentData;
            this.PersistResults = persistResults;
            this.AdditionalFormData = new Dictionary<string, string>();
            this.HasFundingProposal = hasFundingProposal;
            this.OrganisationId = organisationId;
        }

        /// <summary>
        /// Gets the product context.
        /// </summary>
        public IProductContext ProductContext { get; private set; }

        public Guid? QuoteId { get; }

        /// <summary>
        /// Gets or sets the quote to calculate.
        /// </summary>
        public Quote? Quote { get; set; }

        public Guid? PolicyId { get; }

        /// <summary>
        /// Gets or sets the policy to consider for the calculation.
        /// </summary>
        public Policy? Policy { get; set; }

        /// <summary>
        /// Gets the calculation data model.
        /// </summary>
        public CalculationDataModel CalculationDataModel { get; private set; }

        /// <summary>
        /// Gets the payment data.
        /// </summary>
        public PaymentData? PaymentData { get; private set; }

        /// <summary>
        /// Gets or sets the quote type.
        /// </summary>
        public QuoteType? QuoteType { get; set; }

        /// <summary>
        /// Gets a value indicating whether the calculation result should be persisted and stored
        /// against the quote.
        /// </summary>
        public bool PersistResults { get; }

        /// <summary>
        /// Gets a value indicating whether is test data.
        /// </summary>
        public bool IsTestData { get; private set; }

        /// <summary>
        /// Gets a value indicating whether it has funding proposal.
        /// </summary>
        public bool HasFundingProposal { get; private set; }

        /// <summary>
        /// Gets or sets the calculation result data.
        /// This property is used to pass the calculation result data to the next handler in the pipeline.
        /// </summary>
        public CachingJObjectWrapper? CalculationResultData { get; set; }

        /// <summary>
        /// Gets or sets the price breakdown.
        /// This property is used to pass the price breakdown to the next handler in the pipeline.
        /// </summary>
        public PriceBreakdown? PriceBreakdown { get; set; }

        /// <summary>
        /// Gets or sets the funding proposal.
        /// This property is used to pass the funding proposal to the next handler in the pipeline.
        /// </summary>
        public FundingProposal? FundingProposal { get; set; }

        /// <summary>
        /// Gets or sets the product configuration.
        /// This property is used to pass the product configuration to the next handler in the pipeline.
        /// </summary>
        public IProductConfiguration? ProductConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the release calculation output.
        /// This property is used to pass the calculation output to the next handler in the pipeline.
        /// </summary>
        public ReleaseCalculationOutput? ReleaseCalculationOutput { get; set; }

        /// <summary>
        /// Gets or sets additional form data.
        /// This is important if you want additional form model to be applied to the quote for future references.
        /// </summary>
        public Dictionary<string, string> AdditionalFormData { get; set; }

        /// <summary>
        /// Gets or sets the final form data, which includes additional form data.
        /// </summary>
        public FormData? FinalFormData { get; set; }

        /// <summary>
        /// Gets or sets the ID of the form data update, if any.
        /// If we are persisting, we will record the ID of the form data update here.
        /// </summary>
        public Guid? FormDataUpdateId { get; set; }

        public Guid? ProductReleaseId { get; set; }

        public Guid? OrganisationId { get; set; }

        public ReleaseContext ReleaseContext => this.ProductReleaseId == null
            ? throw new InvalidOperationException("Logic error - we should have established the ProductReleaseId by now, but it's null")
            : new ReleaseContext(
                this.ProductContext.TenantId,
                this.ProductContext.ProductId,
                this.ProductContext.Environment,
                this.ProductReleaseId.Value);
    }
}
