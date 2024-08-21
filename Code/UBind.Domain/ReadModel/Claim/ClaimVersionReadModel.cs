// <copyright file="ClaimVersionReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Read model for claim versions.
    /// </summary>
    public class ClaimVersionReadModel : EntityReadModel<Guid>
    {
        private Guid aggregateId;

        /// <summary>
        /// Initializes static properties.
        /// </summary>
        static ClaimVersionReadModel()
        {
            SupportsAdditionalProperties = true;
        }

        /// <summary>
        /// Gets or sets the ID of the aggregate the claim belongs to.
        /// </summary>
        public Guid AggregateId
        {
            get => this.aggregateId != default ? this.aggregateId : this.ClaimId;
            set => this.aggregateId = value;
        }

        /// <summary>
        /// Gets or sets the claim's ID.
        /// </summary>
        public Guid ClaimId { get; set; }

        /// <summary>
        /// Gets or sets the version number for the current quote.
        /// </summary>
        public int ClaimVersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the latest form data for the claim.
        /// </summary>
        public string LatestFormData { get; set; }

        /// <summary>
        /// Gets or sets gets the Claim number.
        /// </summary>
        public string ClaimNumber { get; set; }

        /// <summary>
        /// Gets or sets the claim reference number.
        /// </summary>
        public string ClaimReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the serialized calculation result.
        /// </summary>
        public string SerializedCalculationResult { get; set; }

        /// <summary>
        /// Gets or sets the calculation result for the claim version.
        /// </summary>
        [NotMapped]
        public IClaimCalculationResultReadModel CalculationResult
        {
            get => this.SerializedCalculationResult != null
                ? JsonConvert.DeserializeObject<IClaimCalculationResultReadModel>(
                    this.SerializedCalculationResult)
                : null;

            set => this.SerializedCalculationResult = JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Gets or sets the ID of the policy the claim version pertains to.
        /// </summary>
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the environment where the claim version is created.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the Id of the claim version customer.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the Id of claim version owner.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the Id of product the claim version belongs to.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the Id of organisation the claim version belongs to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; set; }
    }
}
