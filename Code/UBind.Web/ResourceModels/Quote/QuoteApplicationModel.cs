// <copyright file="QuoteApplicationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Dto;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Resource model for an application.
    /// </summary>
    public class QuoteApplicationModel : IQuoteResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteApplicationModel"/> class.
        /// </summary>
        /// <param name="quote">The quote.</param>
        /// <param name="quoteVersion">The version of the quote aggregate.</param>
        /// <param name="customerReadModel">The customer read model.</param>
        /// <param name="currentUser">The current logged in user.</param>
        /// <param name="calculationResultModelPaid">The calculationResultModelFromBind that was paid for.</param>
        public QuoteApplicationModel(
            NewQuoteReadModel quote,
            QuoteVersionReadModelDto? quoteVersion = null,
            ICustomerReadModelSummary? customerReadModel = null,
            UserResourceModel? currentUser = null,
            QuoteCalculationResultModel? calculationResultModelPaid = null)
        {
            this.IsTestData = quote.IsTestData;
            if (calculationResultModelPaid != null)
            {
                this.PriceBreakdown = calculationResultModelPaid.PriceBreakdown;
                this.RefundBreakdown = calculationResultModelPaid.RefundBreakdown;
                this.AmountPayable = calculationResultModelPaid.AmountPayable;
                this.CalculationResultId = calculationResultModelPaid.CalculationResultId;
                this.CalculationResult = calculationResultModelPaid.CalculationResult;
                this.PremiumFundingProposal = calculationResultModelPaid.PremiumFundingProposal;
            }
            else if (quote.LatestCalculationResult != null)
            {
                QuoteCalculationResultModel calculationResultModel
                    = new QuoteCalculationResultModel(quote);
                this.PriceBreakdown = calculationResultModel.PriceBreakdown;
                this.RefundBreakdown = calculationResultModel.RefundBreakdown;
                this.AmountPayable = calculationResultModel.AmountPayable;
                this.CalculationResultId = quote.LatestCalculationResultId;
                this.CalculationResult = quote.LatestCalculationResult.JObject;
                this.PremiumFundingProposal = calculationResultModel.PremiumFundingProposal;
            }

            this.PolicyId = quote.PolicyId;
            this.QuoteId = quote.Id;
            this.QuoteReference = quote.QuoteNumber;
            this.QuoteVersion = quoteVersion?.QuoteVersionNumber;
            if (quote.CustomerId != default && quote.CustomerId != null)
            {
                this.CustomerId = quote.CustomerId;
                this.HadCustomerOnCreation = quote.HadCustomerOnCreation;
            }

            this.CustomerHasAccount = customerReadModel?.UserHasBeenActivated;
            this.QuoteType = quote.Type;

            FormData? formData = null;
            if (quoteVersion != null && quoteVersion.LatestFormDataJson != null)
            {
                formData = new FormData(quoteVersion.LatestFormDataJson);
            }
            else if (quote.LatestFormData != null)
            {
                formData = new FormData(quote.LatestFormData);
            }

            if (formData != null)
            {
                this.FormModel = formData.FormModel;
            }

            this.PolicyNumber = quote.PolicyNumber;
            this.WorkflowStep = quoteVersion?.WorkflowStep ?? quote.WorkflowStep;
            this.QuoteState = quote.QuoteState.ToLower();
            this.CurrentUser = currentUser;
            this.ProductReleaseId = quote.ProductReleaseId;
        }

        /// <summary>
        /// Gets or sets the application ID.
        /// </summary>
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the Current Quote ID.
        /// </summary>
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the quote reference assigned to the application.
        /// </summary>
        public string QuoteReference { get; set; }

        /// <summary>
        /// Gets or sets the number of the latest quote version.
        /// </summary>
        public int? QuoteVersion { get; set; }

        /// <summary>
        /// Gets or sets the ID of the latest form data.
        /// </summary>
        public Guid? FormDataId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the latest calculation result.
        /// </summary>
        public Guid? CalculationResultId { get; set; }

        /// <summary>
        /// Gets the price breakdown for the quote.
        /// </summary>
        public PriceBreakdownModel PriceBreakdown { get; }

        /// <summary>
        /// Gets the refund breakdown for the quote.
        /// </summary>
        public PriceBreakdownModel RefundBreakdown { get; }

        /// <summary>
        /// Gets or sets the form model.
        /// </summary>
        public JObject FormModel { get; set; }

        /// <summary>
        /// Gets or sets the calculation result.
        /// </summary>
        public JObject CalculationResult { get; set; }

        /// <summary>
        /// Gets or sets the Id of the customer the application is assigned to.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets indicating whether the customer has account or not.
        /// </summary>
        public bool? CustomerHasAccount { get; set; }

        /// <summary>
        /// Gets or sets the policy number.
        /// </summary>
        public string? PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the application ID.
        /// </summary>
        public QuoteType QuoteType { get; set; }

        /// <summary>
        /// Gets or sets the workflow step.
        /// </summary>
        public string WorkflowStep { get; set; }

        /// <summary>
        /// Gets or sets the current status/state of the quote.
        /// </summary>
        public string QuoteState { get; set; }

        /// <summary>
        /// Gets the amount payable for the quote.
        /// </summary>
        public string AmountPayable { get; }

        /// <summary>
        /// Gets a value indicating whether this application is considered to be test data.
        /// </summary>
        public bool IsTestData { get; }

        /// <summary>
        /// Gets the current logged in user.
        /// </summary>
        public UserResourceModel CurrentUser { get; }

        /// <summary>
        /// Gets the details of the premium funding proposal for this calculation result, if one exists, otherwise null.
        /// </summary>
        public PremiumFundingProposalModel PremiumFundingProposal { get; }

        /// <summary>
        /// Gets a value indicating whether this quote was initially created against an existing customer.
        /// </summary>
        public bool HadCustomerOnCreation { get; }

        public Guid? ProductReleaseId { get; set; }
    }
}
