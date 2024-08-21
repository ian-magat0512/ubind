// <copyright file="ClaimViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Report
{
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Claim view model providing data for use in liquid report templates.
    /// </summary>
    public class ClaimViewModel : ReportBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimViewModel"/> class.
        /// </summary>
        /// <param name="claimReportItem">The report summary.</param>
        public ClaimViewModel(IClaimReportItem claimReportItem)
            : base(claimReportItem)
        {
            this.ProductId = claimReportItem.ProductId;
            this.Environment = claimReportItem.Environment;
            this.PolicyId = claimReportItem.PolicyId;
            this.PolicyNumber = claimReportItem.PolicyNumber;
            this.Amount = claimReportItem.Amount;
            this.Description = claimReportItem.Description;
            this.IncidentTimestamp = claimReportItem.IncidentTimestamp;
            this.CustomerId = claimReportItem.CustomerId;
            this.PersonId = claimReportItem.PersonId;
            this.CustomerFullName = claimReportItem.CustomerFullName;
            this.CustomerPreferredName = claimReportItem.CustomerFullName;
            this.ClaimReference = claimReportItem.ClaimReference;
            this.ClaimNumber = claimReportItem.ClaimNumber;
            this.Status = claimReportItem.Status;
            this.OrganisationId = claimReportItem.OrganisationId;
            this.IsTestData = claimReportItem.IsTestData;
            this.OwnerUserId = claimReportItem.OwnerUserId;
            this.TimeZoneId = claimReportItem.TimeZone?.ToString() ?? DateTimeZone.Utc.ToString();
            this.CreatedDate = claimReportItem.CreatedTimestamp.ToLocalDateInAet().ToMMDDYYYWithSlashes();
            this.CreatedTime = claimReportItem.CreatedTimestamp.ToLocalTimeInAet().To12HrFormat();
            this.LastModifiedDate = claimReportItem.LastModifiedTimestamp.ToLocalDateInAet().ToMMDDYYYWithSlashes();
            this.LastModifiedTime = claimReportItem.LastModifiedTimestamp.ToLocalTimeInAet().To12HrFormat();
            this.TestData = claimReportItem.IsTestData ? "Yes" : "No";
            this.InvoiceNumber = claimReportItem.InvoiceNumber?.ToString() ?? string.Empty;
            this.CreditNoteNumber = claimReportItem.CreditNoteNumber?.ToString() ?? string.Empty;
            this.ProductName = claimReportItem.ProductName;
            this.ProductEnvironment = claimReportItem.Environment.ToString();
            this.Status = claimReportItem.Status;
            this.Customer = new CustomerViewModel(
                claimReportItem.CustomerPreferredName,
                claimReportItem.CustomerFullName,
                claimReportItem.CustomerEmail,
                claimReportItem.CustomerAlternativeEmail,
                claimReportItem.CustomerMobilePhone,
                claimReportItem.CustomerHomePhone,
                claimReportItem.CustomerWorkPhone);

            this.OrganisationName = claimReportItem.OrganisationName;
            this.OrganisationAlias = claimReportItem.OrganisationAlias;
            this.AgentName = claimReportItem.AgentName;
            this.LatestFormData = claimReportItem.LatestFormData;
            this.WorkflowStep = claimReportItem.WorkflowStep;

            this.Form = !string.IsNullOrEmpty(claimReportItem.LatestFormData)
                ? JToken.Parse(claimReportItem.LatestFormData).SelectToken("formModel")?.CapitalizePropertyNames().ToDictionary() : null;

            if (claimReportItem.SerializedLatestCalculationResult == null)
            {
                return;
            }

            var calculationJsonString = JToken.Parse(claimReportItem.SerializedLatestCalculationResult)?.SelectToken("Json")?.Value<string>();
            if (calculationJsonString == null)
            {
                return;
            }

            this.Calculation = JToken.Parse(calculationJsonString).CapitalizePropertyNames().ToDictionary();
        }

        /// <summary>
        /// Gets or sets the claim amount.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the claim description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the latest form data for the quote.
        /// </summary>
        public string? LatestFormData { get; set; }

        /// <summary>
        /// Gets or sets the ID of the latest calculation result.
        /// </summary>
        public Guid LatestCalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the form data used in the latest calculation result.
        /// </summary>
        public Guid LatestCalculationResultFormDataId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the product the claim relates to.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets the environment the claim belongs to.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets or sets the ID of the policy the claim pertains to.
        /// </summary>
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the owner of the claim.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the policy number of the policy the claim relates to.
        /// </summary>
        public string? PolicyNumber { get; set; }

        public CustomerViewModel Customer { get; }

        public string OrganisationName { get; private set; }

        public string OrganisationAlias { get; private set; }

        public string AgentName { get; private set; }

        public IDictionary<string, object>? Form { get; private set; }

        /// <summary>
        /// Gets or sets the ID of the customer the quote and claim are for.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person (customer) the quote and claim are for.
        /// </summary>
        public Guid? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the customer the claim is for.
        /// </summary>
        public string CustomerFullName { get; set; }

        /// <summary>
        /// Gets or sets the preferred name of the customer the claim is for.
        /// </summary>
        public string CustomerPreferredName { get; set; }

        /// <summary>
        /// Gets or sets gets the Claim number.
        /// </summary>
        public string ClaimNumber { get; set; }

        /// <summary>
        /// Gets or sets the Claim reference number.
        /// </summary>
        public string ClaimReference { get; set; }

        /// <summary>
        /// Gets or sets the claim status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the claim workflow step.
        /// </summary>
        public string WorkflowStep { get; set; }

        public long? IncidentTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the claim has been actualised.
        /// </summary>
        public bool IsActualised { get; set; }

        /// <summary>
        /// Gets or sets the Id of organisation the policy belongs to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets the claim incident date time.
        /// </summary>
        /// <remarks>Exposed for efficient queries only.</remarks>
        public LocalDateTime? IncidentDateTime
        {
            get => this.IncidentTimestamp.HasValue
                ? this.IncidentTimestamp.Value.InZone(this.TimeZone).LocalDateTime
                : (LocalDateTime?)null;
        }

        public Instant? IncidentTimestamp
        {
            get => this.IncidentTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.IncidentTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.IncidentTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public DateTimeZone TimeZone => this.TimeZoneId != null
            ? Timezones.GetTimeZoneByIdOrThrow(this.TimeZoneId)
            : Timezones.AET;

        /// <summary>
        /// Get Time Zone Id
        /// </summary>
        public string? TimeZoneId { get; set; }

        public string? CreatedDate { get; private set; }

        public string? CreatedTime { get; private set; }

        public string? LastModifiedDate { get; private set; }

        public string? LastModifiedTime { get; private set; }

        public string? TestData { get; private set; }

        public string? InvoiceNumber { get; private set; }

        public string? CreditNoteNumber { get; private set; }

        public string? ProductName { get; private set; }

        public string? ProductEnvironment { get; private set; }

        /// <summary>
        /// Gets the Lodged date as an instance of DateTime for persistance with EF.
        /// </summary>
        /// <remarks>Exposed for efficient queries only.</remarks>
        public LocalDateTime? LodgedDateTime
        {
            get => this.LodgedTimestamp.HasValue
                ? this.LodgedTimestamp.Value.InZone(this.TimeZone).LocalDateTime
                : (LocalDateTime?)null;
        }

        public Instant? LodgedTimestamp
        {
            get => this.LodgedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.LodgedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.LodgedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? LodgedTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the Settled date as an instance of DateTime for persistance with EF.
        /// </summary>
        /// <remarks>Exposed for efficient queries only.</remarks>
        public LocalDateTime? SettledDateTime
        {
            get => this.SettledTimestamp.HasValue
                ? this.SettledTimestamp.Value.InZone(this.TimeZone).LocalDateTime
                : (LocalDateTime?)null;
        }

        public Instant? SettledTimestamp
        {
            get => this.SettledTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.SettledTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.SettledTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? SettledTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets the Declined date as an instance of DateTime for persistance with EF.
        /// </summary>
        /// <remarks>Exposed for efficient queries only.</remarks>
        public LocalDateTime? DeclinedDateTime
        {
            get => this.DeclinedTimestamp.HasValue
                ? this.DeclinedTimestamp.Value.InZone(this.TimeZone).LocalDateTime
                : (LocalDateTime?)null;
        }

        public Instant? DeclinedTimestamp
        {
            get => this.DeclinedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.DeclinedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.DeclinedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? DeclinedTicksSinceEpoch { get; private set; }

        public IDictionary<string, object>? Calculation { get; private set; }
    }
}
