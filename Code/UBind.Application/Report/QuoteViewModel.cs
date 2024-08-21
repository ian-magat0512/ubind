// <copyright file="QuoteViewModel.cs" company="uBind">
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
    /// Quote view model providing data for use in liquid report templates.
    /// </summary>
    public class QuoteViewModel : ReportBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteViewModel"/> class.
        /// </summary>
        /// <param name="quoteReportItem">The report summary.</param>
        public QuoteViewModel(IQuoteReportItem quoteReportItem)
            : base(quoteReportItem)
        {
            this.QuoteType = quoteReportItem.QuoteType.ToString();
            this.QuoteReference = quoteReportItem.QuoteNumber?.ToString();
            this.QuoteStatus = quoteReportItem.QuoteState.ToString();
            this.CreatedDate = quoteReportItem.CreatedTimestamp.ToLocalDateInAet().ToMMDDYYYWithSlashes();
            this.CreatedTime = quoteReportItem.CreatedTimestamp.ToLocalTimeInAet().To12HrFormat();
            this.LastModifiedDate = quoteReportItem.LastModifiedTimestamp.ToLocalDateInAet().ToMMDDYYYWithSlashes();
            this.LastModifiedTime = quoteReportItem.LastModifiedTimestamp.ToLocalTimeInAet().To12HrFormat();
            this.TestData = quoteReportItem.IsTestData ? "Yes" : "No";
            this.InvoiceNumber = quoteReportItem.InvoiceNumber?.ToString() ?? string.Empty;
            this.CreditNoteNumber = quoteReportItem.CreditNoteNumber?.ToString() ?? string.Empty;
            this.ProductName = quoteReportItem.ProductName;
            this.ProductEnvironment = quoteReportItem.Environment.ToString();
            this.PolicyNumber = quoteReportItem.PolicyNumber?.ToString();
            this.Customer = new CustomerViewModel(
                quoteReportItem.CustomerPreferredName,
                quoteReportItem.CustomerFullName,
                quoteReportItem.CustomerEmail,
                quoteReportItem.CustomerAlternativeEmail,
                quoteReportItem.CustomerMobilePhone,
                quoteReportItem.CustomerHomePhone,
                quoteReportItem.CustomerWorkPhone);

            this.OrganisationName = quoteReportItem.OrganisationName;
            this.OrganisationAlias = quoteReportItem.OrganisationAlias;
            this.AgentName = quoteReportItem.AgentName;

            this.Form = quoteReportItem.LatestFormData != null
                ? JToken.Parse(quoteReportItem.LatestFormData).SelectToken("formModel").CapitalizePropertyNames().ToDictionary() : null;

            if (quoteReportItem.SerializedLatestCalculationResult != null)
            {
                var calculationJsonString = JToken.Parse(quoteReportItem.SerializedLatestCalculationResult).SelectToken("Json").Value<string>();
                this.Calculation = JToken.Parse(calculationJsonString).CapitalizePropertyNames().ToDictionary();
            }
        }

        /// <summary>
        /// Gets the quote type.
        /// </summary>
        public string QuoteType { get; }

        /// <summary>
        /// Gets the product name.
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// Gets the product environment.
        /// </summary>
        public string ProductEnvironment { get; }

        /// <summary>
        /// Gets the created date.
        /// </summary>
        public string CreatedDate { get; }

        /// <summary>
        /// Gets the created time.
        /// </summary>
        public string CreatedTime { get; }

        /// <summary>
        /// Gets the created date.
        /// </summary>
        public string CreationDate => this.CreatedDate;

        /// <summary>
        /// Gets the created time.
        /// </summary>
        public string CreationTime => this.CreatedTime;

        /// <summary>
        /// Gets the test data.
        /// </summary>
        public string TestData { get; }

        /// <summary>
        /// Gets the policy number.
        /// </summary>
        public string PolicyNumber { get; }

        /// <summary>
        /// Gets the quote reference.
        /// </summary>
        public string QuoteReference { get; }

        /// <summary>
        /// Gets the quote status.
        /// </summary>
        public string QuoteStatus { get; }

        /// <summary>
        /// Gets the last modified date.
        /// </summary>
        public string LastModifiedDate { get; }

        /// <summary>
        /// Gets the last modified time.
        /// </summary>
        public string LastModifiedTime { get; }

        /// <summary>
        /// Gets the invoice number.
        /// </summary>
        public string InvoiceNumber { get; }

        /// <summary>
        /// Gets the credit note number.
        /// </summary>
        public string CreditNoteNumber { get; }

        /// <summary>
        /// Gets the organisation name.
        /// </summary>
        public string OrganisationName { get; }

        /// <summary>
        /// Gets the organisation alias.
        /// </summary>
        public string OrganisationAlias { get; }

        /// <summary>
        /// Gets the agent name.
        /// </summary>
        public string AgentName { get; }

        /// <summary>
        /// Gets the customer record.
        /// </summary>
        public CustomerViewModel Customer { get; }

        /// <summary>
        /// Gets the form data.
        /// </summary>
        public IDictionary<string, object> Form { get; }

        /// <summary>
        /// Gets the calculation data.
        /// </summary>
        public IDictionary<string, object> Calculation { get; }
    }
}
