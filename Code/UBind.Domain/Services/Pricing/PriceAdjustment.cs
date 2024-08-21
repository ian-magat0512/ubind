// <copyright file="PriceAdjustment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.Pricing
{
    using Newtonsoft.Json;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Representing the charges and refunds applicable for a mid-term adjustment.
    /// </summary>
    public class PriceAdjustment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceAdjustment"/> class.
        /// </summary>
        /// <param name="refund">The amount to refund.</param>
        /// <param name="newCharge">The new charge.</param>
        public PriceAdjustment(PriceBreakdown refund, PriceBreakdown newCharge)
        {
            this.Refund = refund;
            this.NewCharge = newCharge;
        }

        [JsonConstructor]
        private PriceAdjustment()
        {
        }

        /// <summary>
        /// Gets the amount to refund.
        /// </summary>
        [JsonProperty]
        public PriceBreakdown Refund { get; private set; }

        /// <summary>
        /// Gets the amount of the new charge.
        /// </summary>
        [JsonProperty]
        public PriceBreakdown NewCharge { get; private set; }

        /// <summary>
        /// Gets the total amount chargable or refundable, which is the difference of the new charge and the refund.
        /// </summary>
        public PriceBreakdown Difference => this.NewCharge - this.Refund;
    }
}
