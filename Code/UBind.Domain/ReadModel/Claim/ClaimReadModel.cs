// <copyright file="ClaimReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain;

    /// <summary>
    /// Read model for claims.
    /// </summary>
    public class ClaimReadModel : EntityReadModel<Guid>, IClaimReadModel
    {
        static ClaimReadModel()
        {
            SupportsAdditionalProperties = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the claim belongs to.</param>
        /// <param name="productId">The ID of the product the claim belongs to.</param>
        /// <param name="environment">The environment the claim belongs to.</param>
        /// <param name="organisationId">The ID of the organisation.</param>
        /// <param name="claimId">The ID of the claim.</param>
        /// <param name="policyId">The ID of the policy the claim pertains to.</param>
        /// <param name="policyNumber">The policy number of the policy the claim relates to.</param>
        /// <param name="customerId">The ID of the customer the claim belongs to.</param>
        /// <param name="ownerUserId">The ID of the agent the claim belongs to.</param>
        /// <param name="personId">The ID of the person the customer pertains to.</param>
        /// <param name="customerFullName">The full name of the customer the claim is for.</param>
        /// <param name="customerPreferredName">The preferred name of the customer the claim is for.</param>
        /// <param name="referenceNumber">The claim's reference number.</param>
        /// <param name="claimNumber">The claim number.</param>
        /// <param name="amount">The amount being claimed.</param>
        /// <param name="status">The claim status.</param>
        /// <param name="description">A description of the claim.</param>
        /// <param name="incidentTimestamp">The timestmap of the incident the claim relates to.</param>
        /// <param name="createdTimestamp">The time the claim was created.</param>
        /// <param name="isTestData">The flag indicating the claim is a test data..</param>
        public ClaimReadModel(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid organisationId,
            Guid claimId,
            Guid? policyId,
            string referenceNumber,
            string claimNumber,
            decimal? amount,
            string status,
            string description,
            Instant? incidentTimestamp,
            string policyNumber,
            Guid? customerId,
            Guid? ownerUserId,
            Guid? personId,
            string customerFullName,
            string customerPreferredName,
            Instant createdTimestamp,
            DateTimeZone timeZone,
            bool isTestData)
            : base(tenantId, claimId, createdTimestamp)
        {
            // TODO: Replace this constructor with two that take import and initialization events as parameters.
            this.ProductId = productId;
            this.Environment = environment;
            this.PolicyId = policyId;
            this.PolicyNumber = policyNumber;
            this.Amount = amount;
            this.Description = description;
            this.IncidentTimestamp = incidentTimestamp;
            this.CustomerId = customerId;
            this.PersonId = personId;
            this.CustomerFullName = customerFullName;
            this.CustomerPreferredName = customerPreferredName;
            this.ClaimReference = referenceNumber;
            this.ClaimNumber = claimNumber;
            this.Status = status;
            this.OrganisationId = organisationId;
            this.IsTestData = isTestData;
            this.OwnerUserId = ownerUserId;
            this.TimeZoneId = timeZone.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimReadModel"/> class.
        /// public constructor for claimsReadModel.
        /// </summary>
        public ClaimReadModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimReadModel"/> class.
        /// </summary>
        /// <param name="id">The claim id.</param>
        protected ClaimReadModel(Guid id)
        {
            this.Id = id;
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
        public string LatestFormData { get; set; }

        /// <summary>
        /// Gets or sets the ID of the latest calculation result.
        /// </summary>
        public Guid LatestCalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the form data used in the latest calculation result.
        /// </summary>
        public Guid LatestCalculationResultFormDataId { get; set; }

        /// <summary>
        /// Gets or sets the latest calculation result if any, otherwise null.
        /// </summary>
        [NotMapped]
        public ClaimCalculationResultReadModel LatestCalculationResult { get; set; }

        /// <summary>
        /// Gets  the latest calculation result if any, otherwise null.
        /// </summary>
        [NotMapped]
        IClaimCalculationResultReadModel IClaimReadModel.LatestCalculationResult => this.LatestCalculationResult;

        /// <summary>
        /// Gets a string containing the serialized representation of the calculation result for persistence in EF.
        /// </summary>
        public string SerializedLatestCalcuationResult
        {
            get => this.LatestCalculationResult != null
                ? JsonConvert.SerializeObject(this.LatestCalculationResult)
                : "{}";

            private set => this.LatestCalculationResult = value != null
                ? JsonConvert.DeserializeObject<ClaimCalculationResultReadModel>(value, CustomSerializerSetting.JsonSerializerSettings)
                : null;
        }

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

        /// <summary>
        /// Gets the incident date as an instance of DateTime for persistance with EF.
        /// </summary>
        /// <remarks>Exposed for efficient queries only.</remarks>
        [NotMapped]
        public LocalDateTime? IncidentDateTime
        {
            get => this.IncidentTimestamp.HasValue
                ? this.IncidentTimestamp.Value.InZone(this.TimeZone).LocalDateTime
                : (LocalDateTime?)null;
        }

        [NotMapped]
        public Instant? IncidentTimestamp
        {
            get => this.IncidentTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.IncidentTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.IncidentTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

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

        [NotMapped]
        public DateTimeZone TimeZone => this.TimeZoneId != null
            ? Timezones.GetTimeZoneByIdOrThrow(this.TimeZoneId)
            : Timezones.AET;

        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets the Lodged date as an instance of DateTime for persistance with EF.
        /// </summary>
        /// <remarks>Exposed for efficient queries only.</remarks>
        [NotMapped]
        public LocalDateTime? LodgedDateTime
        {
            get => this.LodgedTimestamp.HasValue
                ? this.LodgedTimestamp.Value.InZone(this.TimeZone).LocalDateTime
                : (LocalDateTime?)null;
        }

        [NotMapped]
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
        [NotMapped]
        public LocalDateTime? SettledDateTime
        {
            get => this.SettledTimestamp.HasValue
                ? this.SettledTimestamp.Value.InZone(this.TimeZone).LocalDateTime
                : (LocalDateTime?)null;
        }

        [NotMapped]
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
        [NotMapped]
        public LocalDateTime? DeclinedDateTime
        {
            get => this.DeclinedTimestamp.HasValue
                ? this.DeclinedTimestamp.Value.InZone(this.TimeZone).LocalDateTime
                : (LocalDateTime?)null;
        }

        [NotMapped]
        public Instant? DeclinedTimestamp
        {
            get => this.DeclinedTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.DeclinedTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.DeclinedTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? DeclinedTicksSinceEpoch { get; private set; }
    }
}
