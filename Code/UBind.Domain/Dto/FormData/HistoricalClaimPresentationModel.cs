// <copyright file="HistoricalClaimPresentationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Dto.FormData
{
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Presentation model for past claims.
    /// </summary>
    public class HistoricalClaimPresentationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricalClaimPresentationModel"/> class.
        /// </summary>
        /// <param name="claim">The claimReadmodel.</param>
        public HistoricalClaimPresentationModel(IClaimReadModel claim)
        {
            this.DateOfClaim = claim.IncidentDateTime.HasValue
                ? claim.IncidentDateTime.Value.Date.ToDMMMYYYY()
                : string.Empty;
            this.ClaimNumber = claim.ClaimNumber;
            this.ClaimStatus = claim.Status.ToString();
            this.TotalClaimInsurer = claim.Amount != null ? (decimal)claim.Amount : 0;
            this.DetailsOfLoss = claim.Description;
        }

        [JsonConstructor]
        private HistoricalClaimPresentationModel()
        {
        }

        /// <summary>
        /// Gets the date of claim.
        /// </summary>
        [JsonProperty]
        public string DateOfClaim { get; private set; }

        /// <summary>
        /// Gets claim number.
        /// </summary>
        [JsonProperty]
        public string ClaimNumber { get; private set; }

        /// <summary>
        /// Gets the details of loss.
        /// </summary>
        [JsonProperty]
        public string DetailsOfLoss { get; private set; }

        /// <summary>
        /// Gets the total amount of claim.
        /// </summary>
        [JsonProperty]
        public decimal TotalClaimInsurer { get; private set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        [JsonProperty]
        public string ClaimStatus { get; private set; }
    }
}
