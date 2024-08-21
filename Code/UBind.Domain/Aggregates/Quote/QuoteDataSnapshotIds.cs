// <copyright file="QuoteDataSnapshotIds.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents IDs specifying a snapshot of the state of a quote's data.
    /// </summary>
    public class QuoteDataSnapshotIds
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDataSnapshotIds"/> class.
        /// </summary>
        /// <param name="formDataId">The ID of a specific update of the quote's form data.</param>
        /// <param name="calculationResultId">The ID of a specific calculation result.</param>
        /// <param name="customerDetailsId">The ID of a specific update of the quote's customer details.</param>
        public QuoteDataSnapshotIds(
            Guid formDataId,
            Guid? calculationResultId,
            Guid? customerDetailsId)
        {
            this.FormDataId = formDataId;
            this.CalculationResultId = calculationResultId;
            this.CustomerDetailsId = customerDetailsId;
        }

        [JsonConstructor]
        private QuoteDataSnapshotIds()
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
        public Guid? CalculationResultId { get; private set; }

        /// <summary>
        /// Gets a specific update of the quote's form data.
        /// </summary>
        [JsonProperty]
        public Guid? CustomerDetailsId { get; private set; }
    }
}
