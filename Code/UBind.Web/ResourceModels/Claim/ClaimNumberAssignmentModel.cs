// <copyright file="ClaimNumberAssignmentModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    /// <summary>
    /// Resource model for creating a claim.
    /// </summary>
    public class ClaimNumberAssignmentModel
    {
        /// <summary>
        /// Gets or sets the reference number of the claim.
        /// </summary>
        public string ClaimNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether old claim numbers can be used or not.
        /// </summary>
        public bool IsRestoreToList { get; set; }

        /// <summary>
        /// Gets or sets the claim status.
        /// </summary>
        public string Status { get; set; }
    }
}
