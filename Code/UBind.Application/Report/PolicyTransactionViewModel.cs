// <copyright file="PolicyTransactionViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Policy transaction view model providing data for use in liquid report templates.
    /// </summary>
    public class PolicyTransactionViewModel : ReportBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransactionViewModel"/> class.
        /// </summary>
        /// <param name="policyTransactionReadModelSummary">The policy transaction summary.</param>
        public PolicyTransactionViewModel(IPolicyTransactionReadModelSummary policyTransactionReadModelSummary)
            : base(policyTransactionReadModelSummary)
        {
            this.TransactionType = policyTransactionReadModelSummary.TransactionType;
            this.ProductName = policyTransactionReadModelSummary.ProductName;
            this.CreatedDate = policyTransactionReadModelSummary.CreatedDate.ToMMDDYYYWithSlashes();
            this.CreatedTime = policyTransactionReadModelSummary.CreatedTimestamp.ToLocalTimeInAet().To12HrFormat();
            this.EffectiveDate = policyTransactionReadModelSummary.EffectiveDateTime.Date.ToMMDDYYYWithSlashes() ?? string.Empty;
            this.EffectiveTime = policyTransactionReadModelSummary.EffectiveDateTime.TimeOfDay.To12HrFormat() ?? string.Empty;
            this.ExpiryDate = policyTransactionReadModelSummary.ExpiryDateTime?.Date.ToMMDDYYYWithSlashes() ?? string.Empty;
            this.ExpiryTime = policyTransactionReadModelSummary.ExpiryDateTime?.TimeOfDay.To12HrFormat() ?? string.Empty;
            this.InceptionDate = policyTransactionReadModelSummary.InceptionDateTime?.Date.ToMMDDYYYWithSlashes() ?? string.Empty;
            this.InceptionTime = policyTransactionReadModelSummary.InceptionDateTime?.TimeOfDay.To12HrFormat() ?? string.Empty;
            this.AdjustmentEffectiveDate = policyTransactionReadModelSummary.AdjustmentEffectiveDateTime?.Date.ToMMDDYYYWithSlashes() ?? string.Empty;
            this.AdjustmentEffectiveTime = policyTransactionReadModelSummary.AdjustmentEffectiveDateTime?.TimeOfDay.To12HrFormat() ?? string.Empty;
            this.CancellationEffectiveDate = policyTransactionReadModelSummary.CancellationEffectiveDateTime?.Date.ToMMDDYYYWithSlashes() ?? string.Empty;
            this.CancellationEffectiveTime = policyTransactionReadModelSummary.CancellationEffectiveDateTime?.TimeOfDay.To12HrFormat() ?? string.Empty;
            this.PolicyNumber = policyTransactionReadModelSummary.PolicyNumber;
            this.QuoteReference = policyTransactionReadModelSummary.QuoteReference;
            this.InvoiceNumber = policyTransactionReadModelSummary.InvoiceNumber;
            this.CreditNoteNumber = policyTransactionReadModelSummary.CreditNoteNumber;
            this.CustomerFullName = policyTransactionReadModelSummary.CustomerFullName;
            this.CustomerEmail = policyTransactionReadModelSummary.CustomerEmail;
            this.OrganisationName = policyTransactionReadModelSummary.OrganisationName;
            this.OrganisationAlias = policyTransactionReadModelSummary.OrganisationAlias;
            this.AgentName = policyTransactionReadModelSummary.AgentName;

            this.Form = JToken.Parse(policyTransactionReadModelSummary.FormData).SelectToken("formModel").CapitalizePropertyNames().ToDictionary();

            if (policyTransactionReadModelSummary.CalculationResult != null)
            {
                var calculationJsonString = JToken.Parse(policyTransactionReadModelSummary.CalculationResult).SelectToken("Json").Value<string>();
                this.Calculation = JToken.Parse(calculationJsonString).CapitalizePropertyNames().ToDictionary();
            }
        }

        /// <summary>
        /// Gets the transaction type.
        /// </summary>
        public string TransactionType { get; }

        /// <summary>
        /// Gets product name.
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// Gets created date.
        /// </summary>
        public string CreatedDate { get; }

        /// <summary>
        /// Gets created time.
        /// </summary>
        public string CreatedTime { get; }

        /// <summary>
        /// Gets created date.
        /// </summary>
        public string CreationDate => this.CreatedDate;

        /// <summary>
        /// Gets created time.
        /// </summary>
        public string CreationTime => this.CreatedTime;

        public string EffectiveDate { get; }

        public string EffectiveTime { get; }

        public string ExpiryDate { get; }

        public string ExpiryTime { get; }

        /// <summary>
        /// Gets policy inception date.
        /// </summary>
        public string InceptionDate { get; }

        /// <summary>
        /// Gets policy inception time.
        /// </summary>
        public string InceptionTime { get; }

        /// <summary>
        /// Gets policy adjustment date.
        /// </summary>
        public string AdjustmentEffectiveDate { get; }

        /// <summary>
        /// Gets policy adjustment time.
        /// </summary>
        public string AdjustmentEffectiveTime { get; }

        /// <summary>
        /// Gets policy adjustment date.
        /// </summary>
        public string AdjustmentDate => this.AdjustmentEffectiveDate;

        /// <summary>
        /// Gets policy adjustment time.
        /// </summary>
        public string AdjustmentTime => this.AdjustmentEffectiveTime;

        /// <summary>
        /// Gets policy adjustment date.
        /// </summary>
        public string CancellationEffectiveDate { get; }

        /// <summary>
        /// Gets policy adjustment time.
        /// </summary>
        public string CancellationEffectiveTime { get; }

        /// <summary>
        /// Gets policy number.
        /// </summary>
        public string PolicyNumber { get; }

        /// <summary>
        /// Gets quote reference.
        /// </summary>
        public string QuoteReference { get; }

        /// <summary>
        /// Gets ivoice number.
        /// </summary>
        public string InvoiceNumber { get; }

        /// <summary>
        /// Gets credit note number.
        /// </summary>
        public string CreditNoteNumber { get; }

        /// <summary>
        /// Gets customer full name.
        /// </summary>
        public string CustomerFullName { get; }

        /// <summary>
        /// Gets the customer's email address.
        /// </summary>
        public string CustomerEmail { get; }

        /// <summary>
        /// Gets the form data.
        /// </summary>
        public IDictionary<string, object> Form { get; }

        /// <summary>
        /// Gets the calculation data.
        /// </summary>
        public IDictionary<string, object> Calculation { get; }

        /// <summary>
        /// Gets the Organisation name.
        /// </summary>
        public string OrganisationName { get; }

        /// <summary>
        /// Gets the Organisation alias.
        /// </summary>
        public string OrganisationAlias { get; }

        /// <summary>
        /// Gets the Agent name.
        /// </summary>
        public string AgentName { get; }
    }
}
