// <copyright file="ClaimCalculationResultReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    /// <summary>
    /// Read model for claim calculation results.
    /// </summary>
    public class ClaimCalculationResultReadModel : IClaimCalculationResultReadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimCalculationResultReadModel"/> class.
        /// </summary>
        /// <param name="json">The json from the calculation engine.</param>
        public ClaimCalculationResultReadModel(string json)
        {
            this.Json = json;
        }

        /// <summary>
        /// Gets the JSON output from the calculation engine.
        /// </summary>
        public string Json { get; private set; }
    }
}
