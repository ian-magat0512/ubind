// <copyright file="IClaimVersionReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Details of a claim version.
    /// </summary>
    public interface IClaimVersionReadModelDetails : IEntityReadModel<Guid>
    {
        /// <summary>
        /// Gets the ID of the aggregate the claim is for.
        /// </summary>
        Guid AggregateId { get; }

        /// <summary>
        /// Gets the latest form data for this claim version.
        /// </summary>
        string LatestFormData { get; }

        /// <summary>
        /// Gets the ID of the claim this is a version of.
        /// </summary>
        Guid ClaimId { get; }

        /// <summary>
        /// Gets the claim number of this claim this version is for.
        /// </summary>
        string ClaimReferenceNumber { get; }

        /// <summary>
        /// Gets the version number of this claim version.
        /// </summary>
        int ClaimVersionNumber { get; }

        /// <summary>
        /// Gets the calculation result string.
        /// </summary>
        string SerializedCalculationResult { get; }

        /// <summary>
        /// Gets or sets the documents associated with this claim version.
        /// </summary>
        IEnumerable<ClaimAttachmentReadModel> Documents { get; set; }
    }
}
