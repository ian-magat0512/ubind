// <copyright file="QuoteDataSnapshot.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Represents a snapshot of the state of a quote's data.
    /// </summary>
    public class QuoteDataSnapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDataSnapshot"/> class.
        /// </summary>
        /// <param name="formData">A specific update of the quote's form data.</param>
        /// <param name="calculationResult">A specific calculation result.</param>
        /// <param name="customerDetails">A specific update of the quote's customer details.</param>
        public QuoteDataSnapshot(
            QuoteDataUpdate<FormData> formData,
            QuoteDataUpdate<ReadWriteModel.CalculationResult> calculationResult,
            QuoteDataUpdate<IPersonalDetails> customerDetails)
        {
            this.FormData = formData;
            this.CalculationResult = calculationResult;
            this.CustomerDetails = customerDetails;
        }

        [JsonConstructor]
        private QuoteDataSnapshot()
        {
        }

        /// <summary>
        /// Gets a specific update of the quote's form data.
        /// </summary>
        [JsonProperty]
        public QuoteDataUpdate<FormData> FormData { get; private set; }

        /// <summary>
        /// Gets a specific update of the quote's form data.
        /// </summary>
        [JsonProperty]
        public QuoteDataUpdate<ReadWriteModel.CalculationResult> CalculationResult { get; private set; }

        /// <summary>
        /// Gets a specific update of the quote's customer detail.
        /// </summary>
        [JsonProperty]
        public QuoteDataUpdate<IPersonalDetails> CustomerDetails { get; private set; }

        /// <summary>
        /// Gets the IDs of the data in the snapshot.
        /// </summary>
        public QuoteDataSnapshotIds Ids => new QuoteDataSnapshotIds(
            this.FormData?.Id ?? default,
            this.CalculationResult?.Id ?? default,
            this.CustomerDetails?.Id ?? default);
    }
}
