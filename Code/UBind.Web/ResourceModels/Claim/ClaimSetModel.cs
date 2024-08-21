// <copyright file="ClaimSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Resource model for serving claim records as associated resource property.
    /// </summary>
    public class ClaimSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimSetModel"/> class.
        /// </summary>
        /// <param name="claim">The claim to be served.</param>
        public ClaimSetModel(IClaimReadModelSummary claim)
        {
            this.Id = claim.Id;
            if (claim.CustomerId.HasValue)
            {
                this.CustomerDetails = new CustomerSimpleModel(claim.CustomerId.Value, claim.CustomerFullName);
            }

            this.ProductName = claim.ProductName;
            this.ClaimNumber = claim.ClaimNumber;
            this.ClaimReference = claim.ClaimReference;
            this.Status = claim.Status.ToString();
            this.CreatedDateTime = claim.CreatedTimestamp.ToExtendedIso8601String();
            this.LastModifiedDateTime = claim.LastModifiedTimestamp.ToExtendedIso8601String();
            this.IsTestData = claim.IsTestData;
            this.OwnerUserId = claim.OwnerUserId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimSetModel"/> class.
        /// </summary>
        /// <param name="claimAggregate">The claim aggregate.</param>
        /// <param name="productName">The product name.</param>
        public ClaimSetModel(ClaimAggregate claimAggregate, string productName)
        {
            if (claimAggregate != null)
            {
                this.Id = claimAggregate.Id;
                this.ProductName = productName;
                this.ClaimReference = claimAggregate.Claim.ClaimReference;
                this.Status = claimAggregate.Claim.ClaimStatus;
                this.CreatedDateTime = claimAggregate.CreatedTimestamp.ToExtendedIso8601String();
                this.IsTestData = claimAggregate.IsTestData;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimSetModel"/> class.
        /// </summary>
        [JsonConstructor]
        public ClaimSetModel()
        {
        }

        /// <summary>
        /// Gets the Id of the Claim record.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the product the claim is for.
        /// </summary>
        [JsonProperty]
        public string ProductName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether claim is a test data.
        /// </summary>
        [JsonProperty]
        public bool IsTestData { get; private set; }

        /// <summary>
        /// Gets the reference number of the claim.
        /// </summary>
        [JsonProperty]
        public string ClaimReference { get; private set; }

        /// <summary>
        /// Gets the claim number.
        /// </summary>
        [JsonProperty]
        public string ClaimNumber { get; private set; }

        /// <summary>
        /// Gets the current status of the claim.
        /// </summary>
        [JsonProperty]
        public string Status { get; private set; }

        /// <summary>
        /// Gets the date the claim is created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the date the claim was last updated.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets the owner user id of the claim.
        /// </summary>
        [JsonProperty]
        public Guid? OwnerUserId { get; private set; }

        /// <summary>
        /// Gets the details of the Claim customer.
        /// </summary>
        [JsonProperty]
        public CustomerSimpleModel CustomerDetails { get; private set; }
    }
}
