// <copyright file="ReferenceNumberConsumptionResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// The result of consuming a reference number (policy number, invoice number etc.
    /// </summary>
    public struct ReferenceNumberConsumptionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceNumberConsumptionResult"/> struct.
        /// </summary>
        /// <param name="referenceNumber">The reference number consumed.</param>
        /// <param name="remainingCount">The count of remaining available reference numbers after consumption.</param>
        public ReferenceNumberConsumptionResult(string referenceNumber, int remainingCount)
        {
            this.ReferenceNumber = referenceNumber;
            this.RemainingCount = remainingCount;
        }

        /// <summary>
        /// Gets the resource number consumed.
        /// </summary>
        public string ReferenceNumber { get; }

        /// <summary>
        /// Gets the count of remaining available reference numbers after consumption.
        /// </summary>
        public int RemainingCount { get; }
    }
}
