// <copyright file="FixedAndScalablePrice.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a price in terms of its fixed and scalable components.
    /// Fixed components are those which do not change regardless of the policy period (e.g. merchant fees).
    /// Scalable componnents are those which scale according to the policy period (e.g. premium).
    /// </summary>
    public class FixedAndScalablePrice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedAndScalablePrice"/> class.
        /// </summary>
        /// <param name="priceBreakdown">The price.</param>
        /// <param name="fixedComponentFilter">A filter for selecting fixed components of the price.</param>
        public FixedAndScalablePrice(PriceBreakdown priceBreakdown, PriceComponentFilter fixedComponentFilter)
        {
            this.FixedComponents = priceBreakdown.Filter(fixedComponentFilter);
            this.ScalableComponents = priceBreakdown.FilterInverse(fixedComponentFilter);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedAndScalablePrice"/> class.
        /// </summary>
        /// <param name="fixedComponents">The fixed components of the price.</param>
        /// <param name="scalableComponents">The scalable components of the price.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        public FixedAndScalablePrice(PriceBreakdown fixedComponents, PriceBreakdown scalableComponents)
        {
            this.FixedComponents = fixedComponents;
            this.ScalableComponents = scalableComponents;
        }

        [JsonConstructor]
        private FixedAndScalablePrice()
        {
        }

        /// <summary>
        /// Gets the fixed components of the price.
        /// </summary>
        [JsonProperty]
        public PriceBreakdown FixedComponents { get; private set; }

        /// <summary>
        /// Gets the scalable components of the price.
        /// </summary>
        [JsonProperty]
        public PriceBreakdown ScalableComponents { get; private set; }
    }
}
