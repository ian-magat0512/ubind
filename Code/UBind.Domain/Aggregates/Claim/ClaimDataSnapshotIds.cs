// <copyright file="ClaimDataSnapshotIds.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents IDs specifying a snapshot of the state of a claim's data.
    /// </summary>
    public class ClaimDataSnapshotIds
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimDataSnapshotIds"/> class.
        /// </summary>
        /// <param name="formDataId">The ID of a specific update of the claim's form data.</param>
        /// <param name="calculationResultId">The ID of a specific calculation result.</param>
        public ClaimDataSnapshotIds(
            Guid formDataId,
            Guid calculationResultId)
        {
            this.FormDataId = formDataId;
            this.CalculationResultId = calculationResultId;
        }

        [JsonConstructor]
        private ClaimDataSnapshotIds()
        {
        }

        /// <summary>
        /// Gets a specific update of the quote's form data.
        /// </summary>
        [JsonProperty]
        public Guid FormDataId { get; private set; }

        /// <summary>
        /// Gets a specific update of the quote's form data.
        /// </summary>
        [JsonProperty]
        public Guid CalculationResultId { get; private set; }
    }
}
