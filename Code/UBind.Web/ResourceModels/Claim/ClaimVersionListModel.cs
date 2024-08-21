// <copyright file="ClaimVersionListModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Resource model for serving claim version records as associated resource property.
    /// </summary>
    public class ClaimVersionListModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimVersionListModel"/> class.
        /// </summary>
        /// <param name="claimVersionReadModelDetails">The claim version details.</param>
        public ClaimVersionListModel(IClaimVersionReadModelDetails claimVersionReadModelDetails)
        {
            this.ClaimId = claimVersionReadModelDetails.ClaimId;
            this.ClaimVersionId = claimVersionReadModelDetails.Id;
            this.VersionNumber = claimVersionReadModelDetails.ClaimVersionNumber.ToString();
            this.ClaimReference = claimVersionReadModelDetails.ClaimReferenceNumber;
            this.CreatedDateTime = claimVersionReadModelDetails.CreatedTimestamp.ToString();
            this.LastModifiedDateTime = claimVersionReadModelDetails.LastModifiedTimestamp.ToString();
        }

        /// <summary>
        /// Gets the Id of the Claim version record.
        /// </summary>
        [JsonProperty]
        public Guid ClaimId { get; private set; }

        /// <summary>
        /// Gets the Id of the Claim version record.
        /// </summary>
        [JsonProperty]
        public Guid ClaimVersionId { get; private set; }

        /// <summary>
        /// Gets the version number of the claim version.
        /// </summary>
        [JsonProperty]
        public string VersionNumber { get; private set; }

        /// <summary>
        /// Gets the reference number of the claim version.
        /// </summary>
        [JsonProperty]
        public string ClaimReference { get; private set; }

        /// <summary>
        /// Gets the created date and time in ISO8601 format.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the last modified date and time in ISO8601 format.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }
    }
}
