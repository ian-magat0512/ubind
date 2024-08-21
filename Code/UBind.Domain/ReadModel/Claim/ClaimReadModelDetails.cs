// <copyright file="ClaimReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Services;

    /// <summary>
    /// The claim details.
    /// </summary>
    public class ClaimReadModelDetails : EntityReadModel<Guid>, IClaimReadModelDetails
    {
        /// <inheritdoc/>
        public string OrganisationName { get; set; }

        /// <inheritdoc/>
        public string ProductName { get; set; }

        /// <inheritdoc/>
        public bool IsTestData { get; set; }

        /// <inheritdoc/>
        public Guid OrganisationId { get; set; }

        /// <inheritdoc/>
        public Guid ProductId { get; set; }

        /// <inheritdoc/>
        public DeploymentEnvironment Environment { get; set; }

        /// <inheritdoc/>
        public Guid? PolicyId { get; set; }

        /// <inheritdoc/>
        public string PolicyNumber { get; set; }

        /// <inheritdoc/>
        public Guid? CustomerId { get; set; }

        /// <inheritdoc/>
        public Guid? PersonId { get; set; }

        /// <inheritdoc/>
        public string CustomerFullName { get; set; }

        /// <inheritdoc/>
        public Guid? OwnerUserId { get; set; }

        /// <inheritdoc/>
        public string CustomerPreferredName { get; set; }

        /// <inheritdoc/>
        public string ClaimReference { get; set; }

        /// <inheritdoc/>
        public string ClaimNumber { get; set; }

        /// <inheritdoc/>
        public decimal? Amount { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public LocalDateTime? IncidentDateTime
        {
            get => this.IncidentTimestamp?.InZone(this.TimeZone).LocalDateTime;
        }

        /// <inheritdoc/>
        public string Status { get; set; }

        /// <inheritdoc/>
        public string WorkflowStep { get; set; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IClaimCalculationResultReadModel LatestCalculationResult =>
            this.SerializedLatestCalculationResult != null
                ? JsonConvert.DeserializeObject<ClaimCalculationResultReadModel>(this.SerializedLatestCalculationResult, CustomSerializerSetting.JsonSerializerSettings)
                : null;

        /// <summary>
        /// Gets or sets a string containing the serialized calculation result, if known, otherwise null.</summary>
        public string SerializedLatestCalculationResult { get; set; }

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

        /// <inheritdoc/>
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
