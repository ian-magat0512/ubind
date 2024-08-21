// <copyright file="ClaimVersionReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Claim
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// For representing the details of the claim version.
    /// </summary>
    public class ClaimVersionReadModelDetails : EntityReadModel<Guid>, IClaimVersionReadModelDetails
    {
        private Guid aggregateId;

        /// <summary>
        /// Gets or sets the ID of the claim aggregate.
        /// </summary>
        public Guid AggregateId
        {
            get => this.aggregateId != default ? this.aggregateId : this.ClaimId;
            set => this.aggregateId = value;
        }

        /// <summary>
        /// Gets or sets the ID of the claim this version is for.
        /// </summary>
        public Guid ClaimId { get; set; }

        /// <summary>
        /// Gets or sets the latest form data for the claim.
        /// </summary>
        public string LatestFormData { get; set; }

        /// <summary>
        /// Gets or sets the claim reference number.
        /// </summary>
        public string ClaimReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the version number for the current version.
        /// </summary>
        public int ClaimVersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the string containing the serialized calculation result.
        /// </summary>
        public string SerializedCalculationResult { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ClaimAttachmentReadModel> Documents { get; set; }

        /// <summary>
        /// Gets the calculation result.
        /// </summary>
        public ClaimCalculationResult CalculationResult
        {
            get => this.SerializedCalculationResult != null
                ? JsonConvert.DeserializeObject<ClaimCalculationResult>(this.SerializedCalculationResult, CustomSerializerSetting.JsonSerializerSettings)
                : null;
        }
    }
}
