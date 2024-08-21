// <copyright file="ClaimApplicationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.JsonConverters;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Resource model for serving a claim application.
    /// </summary>
    public class ClaimApplicationModel : IClaimResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimApplicationModel"/> class.
        /// </summary>
        /// <param name="claim">The claim aggregate.</param>
        /// <param name="claimVersion">The claim version.</param>
        /// <param name="currentUser">The current logged in user.</param>
        public ClaimApplicationModel(
            ClaimReadModel claim,
            IClaimVersionReadModelDetails? claimVersion = null,
            UserResourceModel? currentUser = null)
        {
            this.ClaimReference = claim.ClaimReference;
            this.ClaimId = claim.Id;
            this.ClaimState = claim.Status;
            this.FormModelJson = claim.LatestFormData != null
                ? JObject.Parse(claim.LatestFormData)["formModel"]?.ToString()
                : null;
            this.ClaimVersion = claimVersion != null
                ? claimVersion.ClaimVersionNumber
                : 0;

            if (claim.LatestCalculationResult != null)
            {
                this.CalculationResultId = claim.LatestCalculationResultId;
                this.CalculationResultJson = claim.LatestCalculationResult.Json;
            }

            this.WorkflowStep = claim.WorkflowStep;
            this.CurrentUser = currentUser;
        }

        /// <summary>
        /// Gets or sets the Id of the claim aggregate.
        /// </summary>
        public Guid ClaimId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the latest calculation result.
        /// </summary>
        public Guid? CalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets the state of the claim.
        /// </summary>
        public string ClaimState { get; set; }

        /// <summary>
        /// Gets or sets the current version number loaded.
        /// </summary>
        public int ClaimVersion { get; set; }

        /// <summary>
        /// Gets or sets the claim reference.
        /// </summary>
        public string ClaimReference { get; set; }

        /// <summary>
        /// Gets or sets the form model.
        /// </summary>
        [JsonProperty(PropertyName = "formModel")]
        [JsonConverter(typeof(RawJsonConverter))]
        public string FormModelJson { get; set; }

        /// <summary>
        /// Gets or sets the calculation result.
        /// </summary>
        [JsonProperty(PropertyName = "calculationResult")]
        [JsonConverter(typeof(RawJsonConverter))]
        public string CalculationResultJson { get; set; }

        /// <summary>
        /// Gets or sets the workflow step.
        /// </summary>
        public string WorkflowStep { get; set; }

        /// <summary>
        /// Gets the current logged in user.
        /// </summary>
        public UserResourceModel CurrentUser { get; }
    }
}
