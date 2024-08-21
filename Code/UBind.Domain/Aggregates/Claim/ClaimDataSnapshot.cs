// <copyright file="ClaimDataSnapshot.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a snapshot of the state of a claims' data.
    /// </summary>
    public class ClaimDataSnapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimDataSnapshot"/> class.
        /// </summary>
        /// <param name="formData">A specific update of the quote's form data.</param>
        /// <param name="calculationResult">A specific calculation result.</param>
        public ClaimDataSnapshot(
            ClaimDataUpdate<ClaimFormData> formData,
            ClaimDataUpdate<ClaimCalculationResult> calculationResult)
        {
            this.FormData = formData;
            this.CalculationResult = calculationResult;
        }

        [JsonConstructor]
        private ClaimDataSnapshot()
        {
        }

        /// <summary>
        /// Gets a specific update of the quote's form data.
        /// </summary>
        [JsonProperty]
        public ClaimDataUpdate<ClaimFormData> FormData { get; private set; }

        /// <summary>
        /// Gets a specific update of the quote's form data.
        /// </summary>
        [JsonProperty]
        public ClaimDataUpdate<ClaimCalculationResult> CalculationResult { get; private set; }

        /// <summary>
        /// Gets the IDs of the data in the snapshot.
        /// </summary>
        public ClaimDataSnapshotIds Ids => new ClaimDataSnapshotIds(
            this.FormData?.Id ?? default,
            this.CalculationResult?.Id ?? default);
    }
}
