// <copyright file="ReportBodyViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Report
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using DotLiquid;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Report content view model providing data for use in liquid report templates.
    /// </summary>
    public class ReportBodyViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportBodyViewModel"/> class.
        /// </summary>
        /// <param name="policyTransactions">The policy transactions for report.</param>
        /// <param name="quoteSummaries">The quotes for report.</param>
        /// <param name="emails">The emails for report.</param>
        public ReportBodyViewModel(
            IEnumerable<PolicyTransactionViewModel>? policyTransactions = null,
            IEnumerable<QuoteViewModel>? quoteSummaries = null,
            IEnumerable<EmailViewModel>? emails = null,
            IEnumerable<ClaimViewModel>? claimSummaries = null)
        {
            this.PolicyTransactions = policyTransactions ?? new List<PolicyTransactionViewModel>();
            this.Quotes = quoteSummaries ?? new List<QuoteViewModel>();
            this.Emails = emails ?? new List<EmailViewModel>();
            this.Claims = claimSummaries ?? new List<ClaimViewModel>();
            this.CreateSummaries();
        }

        /// <summary>
        /// Gets the policy transactions.
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{PolicyTransactions}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{PolicyTransaction.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<PolicyTransactionViewModel> PolicyTransactions { get; }

        /// <summary>
        /// Gets the quotes.
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{Quotes}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{Quotes.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<QuoteViewModel> Quotes { get; }

        /// <summary>
        /// Gets the emails.
        /// </summary>
        /// <remarks>
        /// This property is nested inside the quotes temaplate source in order to achieve
        /// {{Emails}} list variable inside the dot liquid template and each item
        /// in the list variable can be define as {{Email.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<EmailViewModel> Emails { get; }

        /// <summary>
        /// Gets the claims.
        /// </summary>
        /// <remarks>
        /// This property is nested inside the quotes template source in order to achieve
        /// {{Claims}} list variable inside the dot liquid template and each item
        /// in the list variable can be define as {{Claims.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<ClaimViewModel> Claims { get; }

        /// <summary>
        /// Gets or sets the quotes monthly summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{QuotesMonthlySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{QuoteSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<QuoteSummaryViewModel>? QuotesMonthlySummaries { get; set; }

        /// <summary>
        /// Gets or sets the quotes daily summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{QuotesDailySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{QuoteSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<QuoteSummaryViewModel>? QuotesDailySummaries { get; set; }

        /// <summary>
        ///  Gets or sets  the quotes weekly summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{QuotesWeeklySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{QuoteSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<QuoteSummaryViewModel>? QuotesWeeklySummaries { get; set; }

        /// <summary>
        /// Gets or sets the emails monthly summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{EmailsMonthlySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{EmailSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<EmailSummaryViewModel>? EmailsMonthlySummaries { get; set; }

        /// <summary>
        /// Gets or sets the quotes daily summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{EmailsDailySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{EmailSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<EmailSummaryViewModel>? EmailsDailySummaries { get; set; }

        /// <summary>
        ///  Gets or sets the emails weekly summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{EmailsWeeklySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{EmailSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<EmailSummaryViewModel>? EmailsWeeklySummaries { get; set; }

        /// <summary>
        /// Gets or sets the claims monthly summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{ClaimsMonthlySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{ClaimSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<ClaimSummaryViewModel>? ClaimsMonthlySummaries { get; set; }

        /// <summary>
        /// Gets or sets the claims daily summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{ClaimsDailySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{ClaimSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<ClaimSummaryViewModel>? ClaimsDailySummaries { get; set; }

        /// <summary>
        ///  Gets or sets  the claims weekly summaries .
        /// </summary>
        /// <remarks>
        /// This property is nested inside the report content view model in order to achieve
        /// {{ClaimsWeeklySummaries}} list variable inside the liquid template and each item
        /// in the list variable can be define as {{ClaimSummary.[Property]}} in the template.
        /// </remarks>
        public IEnumerable<ClaimSummaryViewModel>? ClaimsWeeklySummaries { get; set; }

        private void CreateSummaries()
        {
            // quotes summaries
            if (this.Quotes != null)
            {
                this.QuotesMonthlySummaries = this.GenerateSummariesFromQuotes(SummaryReportType.Monthly);
                this.QuotesDailySummaries = this.GenerateSummariesFromQuotes(SummaryReportType.Daily);
                this.QuotesWeeklySummaries = this.GenerateSummariesFromQuotes(SummaryReportType.Weekly);
            }

            // emails summaries
            if (this.Emails != null)
            {
                this.EmailsMonthlySummaries = this.GenerateSummariesFromEmails(SummaryReportType.Monthly);
                this.EmailsDailySummaries = this.GenerateSummariesFromEmails(SummaryReportType.Daily);
                this.EmailsWeeklySummaries = this.GenerateSummariesFromEmails(SummaryReportType.Weekly);
            }

            // claims summaries
            if (this.Claims != null)
            {
                this.ClaimsMonthlySummaries = this.GenerateSummariesFromClaims(SummaryReportType.Monthly);
                this.ClaimsDailySummaries = this.GenerateSummariesFromClaims(SummaryReportType.Daily);
                this.ClaimsWeeklySummaries = this.GenerateSummariesFromClaims(SummaryReportType.Weekly);
            }
        }

        private IEnumerable<QuoteSummaryViewModel> GenerateSummariesFromQuotes(SummaryReportType summaryType)
        {
            var summaries = new List<QuoteSummaryViewModel>();
            var quoteSummaries = (from q in this.Quotes
                                  select new
                                  {
                                      MonthYear = LocalDate.FromDateTime(DateTime.Parse(q.LastModifiedDate)).ToMMYYYY(),
                                      Description = this.GenerateDateDescription(summaryType, q.LastModifiedDate),
                                      status = q.QuoteStatus,
                                      BasePremium = q.BasePremium.Replace("$", string.Empty).Replace(",", string.Empty),
                                      TotalPremium = q.TotalPremium.Replace("$", string.Empty).Replace(",", string.Empty),
                                  }).ToList();

            var descriptions = quoteSummaries.Select(a => new { a.MonthYear, a.Description })
                .Distinct()
                .OrderBy(o => DateTime.Parse(o.MonthYear))
                .ToList();

            foreach (var desc in descriptions)
            {
                var summaryCompleteQuotes = quoteSummaries.Where(w => w.Description == desc.Description && w.status.ToLower() == "complete");
                var summaryIncompleteQuotes = quoteSummaries.Where(w => w.Description == desc.Description && w.status.ToLower() == "incomplete");
                var totalComplete = summaryCompleteQuotes.Count();
                var totalIncomplete = summaryIncompleteQuotes.Count();
                var totalBasePremiumCompleteQuotes = summaryCompleteQuotes.Sum(s => decimal.Parse(s.BasePremium)).ToString("C", CultureInfo.CurrentCulture);
                var totalTotalPremiumCompleteQuotes = summaryCompleteQuotes.Sum(s => decimal.Parse(s.TotalPremium)).ToString("C", CultureInfo.CurrentCulture);
                var summary = new QuoteSummaryViewModel(
                    desc.Description,
                    desc.Description,
                    totalIncomplete,
                    totalComplete,
                    totalComplete + totalIncomplete,
                    totalBasePremiumCompleteQuotes,
                    totalTotalPremiumCompleteQuotes);
                summaries.Add(summary);
            }

            if (summaryType == SummaryReportType.Weekly)
            {
                summaries = summaries.OrderBy(o => o.Description).ToList();
            }

            if (summaryType == SummaryReportType.Daily)
            {
                summaries = summaries.OrderBy(o => DateTime.Parse(o.Description)).ToList();
            }

            return summaries;
        }

        private IEnumerable<EmailSummaryViewModel> GenerateSummariesFromEmails(SummaryReportType summaryType)
        {
            var summaries = new List<EmailSummaryViewModel>();
            var emailSummaries = (from e in this.Emails
                                  select new
                                  {
                                      MonthYear = LocalDate.FromDateTime(DateTime.Parse(e.SentDate)).ToMMYYYY(),
                                      e.SentDate,
                                      Description = this.GenerateDateDescription(summaryType, e.SentDate),
                                      Customer = e.Customer != null && e.To.Contains(e.Customer.Email) ? 1 : 0,
                                      Client = (e.Customer != null && !e.To.Contains(e.Customer.Email)) || e.Customer == null ? 1 : 0,
                                  }).ToList();

            var descriptions = emailSummaries.Select(a => new { a.MonthYear, a.Description })
                .Distinct()
                .OrderBy(o => DateTime.Parse(o.MonthYear))
                .ToList();

            foreach (var desc in descriptions)
            {
                var summaryEmailsSent = emailSummaries.Where(w => w.Description == desc.Description);
                var totalEmailsSent = summaryEmailsSent.Count();
                var totalEmailsSentToCustomer = summaryEmailsSent.Sum(s => s.Customer);
                var totalEmailsSentToClient = summaryEmailsSent.Sum(s => s.Client);
                var summary = new EmailSummaryViewModel(
                    desc.Description,
                    desc.Description,
                    totalEmailsSent,
                    totalEmailsSentToCustomer,
                    totalEmailsSentToClient);
                summaries.Add(summary);
            }

            if (summaryType == SummaryReportType.Weekly)
            {
                summaries = summaries.OrderBy(o => o.Description).ToList();
            }

            if (summaryType == SummaryReportType.Daily)
            {
                summaries = summaries.OrderBy(o => DateTime.Parse(o.Description)).ToList();
            }

            return summaries;
        }

        private IEnumerable<ClaimSummaryViewModel> GenerateSummariesFromClaims(SummaryReportType summaryType)
        {
            var summaries = new List<ClaimSummaryViewModel>();
            var claimSummaries = (from c in this.Claims
                                  select new
                                  {
                                      MonthYear = LocalDate.FromDateTime(DateTime.Parse(c.LastModifiedDate!)).ToMMYYYY(),
                                      Description = this.GenerateDateDescription(summaryType, c.LastModifiedDate!),
                                      status = c.Status,
                                      Amount = c.Amount?.ToString().Replace("$", string.Empty).Replace(",", string.Empty),
                                  }).ToList();

            var descriptions = claimSummaries.Select(a => new { a.MonthYear, a.Description })
                .Distinct()
                .OrderBy(o => DateTime.Parse(o.MonthYear))
                .ToList();

            foreach (var desc in descriptions)
            {
                var summaryCompleteClaims = claimSummaries.Where(w => w.Description == desc.Description && w.status.ToLower() == "complete");
                var summaryIncompleteClaims = claimSummaries.Where(w => w.Description == desc.Description && w.status.ToLower() == "incomplete");
                var totalComplete = summaryCompleteClaims.Count();
                var totalIncomplete = summaryIncompleteClaims.Count();
                var totalAmountOfCompleteClaims = summaryCompleteClaims.Sum(s => decimal.Parse(s.Amount));
                var totalAmountOfIncompleteClaims = summaryIncompleteClaims.Sum(s => decimal.Parse(s.Amount));
                var totalAmountOfCompleteClaimsFormatted = totalAmountOfCompleteClaims.ToString("C", CultureInfo.CurrentCulture);
                var totalAmountOfClaimsFormatted = (totalAmountOfCompleteClaims + totalAmountOfIncompleteClaims).ToString("C", CultureInfo.CurrentCulture);

                var summary = new ClaimSummaryViewModel(
                    desc.Description,
                    desc.Description,
                    totalIncomplete,
                    totalComplete,
                    totalComplete + totalIncomplete,
                    totalAmountOfCompleteClaimsFormatted,
                    totalAmountOfClaimsFormatted);
                summaries.Add(summary);
            }

            if (summaryType == SummaryReportType.Weekly)
            {
                return summaries.OrderBy(o => o.Description).ToList();
            }

            if (summaryType == SummaryReportType.Daily)
            {
                return summaries.OrderBy(o => DateTime.Parse(o.Description)).ToList();
            }

            return summaries;
        }

        private string GenerateDateDescription(SummaryReportType summaryType, string dateString)
        {
            switch (summaryType)
            {
                case SummaryReportType.Daily:
                    return LocalDate.FromDateTime(DateTime.Parse(dateString)).ToMMDDYYYWithSlashes();
                case SummaryReportType.Monthly:
                    return LocalDate.FromDateTime(DateTime.Parse(dateString)).ToMMMMYYYY();
                case SummaryReportType.Weekly:
                    return "Week " + DateTime.Parse(dateString)
                                          .ToWeekOfYear()
                                          .ToString()
                                          .PadLeft(2, '0');
                default:
                    return dateString;
            }
        }
    }
}
