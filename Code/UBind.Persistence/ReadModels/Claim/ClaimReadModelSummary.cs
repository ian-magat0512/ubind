// <copyright file="ClaimReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Claim
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Services;

    /// <summary>
    /// Data transfer object for quotereadmodel
    /// for a specific product.
    /// </summary>
    public class ClaimReadModelSummary : EntityReadModel<Guid>, IClaimReadModelSummary
    {
        /// <summary>
        /// Gets or sets the product details.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the organisation the claim relates to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether claim is a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the ID of the product the claim relates to.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the environment the claim belongs to.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the ID of the quote/policy the claim pertains to.
        /// </summary>
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the policy number of the policy the claim relates to.
        /// </summary>
        public string PolicyNumber { get; set; }

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
        /// Gets or sets the ID of the user who owns the claim.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets gets the Claim reference number.
        /// </summary>
        public string ClaimNumber { get; set; }

        /// <summary>
        /// Gets or sets gets the Claim reference number.
        /// </summary>
        public string ClaimReference { get; set; }

        /// <summary>
        /// Gets or sets the claim amount.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the claim description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the claim status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the claim workflow step.
        /// </summary>
        public string WorkflowStep { get; set; }

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

        public long? IncidentTicksSinceEpoch { get; set; }

        /// <inheritdoc/>
        public Guid LatestCalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets a string containing the serialized calculation result, if known, otherwise null.</summary>
        public string SerializedLatestCalculationResult { get; set; }

        /// <inheritdoc/>
        public IClaimCalculationResultReadModel LatestCalculationResult =>
            this.SerializedLatestCalculationResult != null
                ? JsonConvert.DeserializeObject<ClaimCalculationResultReadModel>(this.SerializedLatestCalculationResult, CustomSerializerSetting.JsonSerializerSettings)
                : null;

        /// <inheritdoc/>
        public Guid LatestCalculationResultFormDataId { get; set; }

        /// <inheritdoc/>
        public string LatestFormData { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ClaimAttachmentReadModel> Documents { get; set; }

        /// <inheritdoc/>
        public Guid? CustomerOwnerUserId { get; set; }

        public DateTimeZone TimeZone => this.TimeZoneId != null
            ? Timezones.GetTimeZoneByIdOrThrow(this.TimeZoneId)
            : Timezones.AET;

        public string TimeZoneId { get; set; }

        /// <summary>
        /// Returns the generated form data on demand.
        /// </summary>
        /// <returns>The form data JSON string.</returns>
        public string GetFormData()
        {
            if (this.LatestFormData != null)
            {
                return this.LatestFormData;
            }

            // Imported quotes will not have form data, so we generate it on demand from claim data.
            var emptyFormModel = new JObject();
            var claimData = new ClaimData(this.Amount, this.Description, this.IncidentTimestamp, this.TimeZone);
            var syncedFormModel = new DefaultClaimDataMapper(new DefaultPolicyTransactionTimeOfDayScheme())
                .SyncFormdata(emptyFormModel, claimData);
            var formData = new JObject();
            formData.PatchProperty(new JsonPath("formModel"), syncedFormModel);
            return formData.ToString();
        }
    }
}
