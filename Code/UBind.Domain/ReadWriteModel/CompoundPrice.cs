// <copyright file="CompoundPrice.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// For representing collecion of prices applicable over consecutive time periods.
    /// </summary>
    [JsonObject]
    public class CompoundPrice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundPrice"/> class.
        /// </summary>
        /// <param name="initialIntervalPrice">The initial interval price.</param>
        public CompoundPrice(IntervalPrice initialIntervalPrice)
        {
            this.IntervalPrices = new List<IntervalPrice>
            {
                initialIntervalPrice,
            };
        }

        [System.Text.Json.Serialization.JsonConstructor]
        public CompoundPrice(List<IntervalPrice> intervalPrices)
        {
            for (var i = 0; i < intervalPrices.Count - 1; ++i)
            {
                if (intervalPrices[i].EndDate != intervalPrices[i + 1].StartDate)
                {
                    throw new InvalidOperationException("Intervals in compound price must be contiguous.");
                }
            }

            this.IntervalPrices = new List<IntervalPrice>(intervalPrices);
        }

        [JsonConstructor]
        private CompoundPrice()
        {
        }

        [JsonProperty]
        public List<IntervalPrice> IntervalPrices { get; private set; }

        /// <summary>
        /// Calculate the refund from a compound price for a given new end date.
        /// </summary>
        /// <param name="newEndDate">The new end date.</param>
        /// <returns>The refund.</returns>
        public PriceBreakdown CalculateRefund(LocalDate newEndDate)
        {
            var currencyCode = this.IntervalPrices.First().FixedAndScalablePrice.FixedComponents.CurrencyCode;
            var fullyRefundablePartsTotal = this.IntervalPrices
                .Where(p => p.StartDate >= newEndDate)
                .Select(p => p.FixedAndScalablePrice.ScalableComponents)
                .Aggregate(PriceBreakdown.Zero(currencyCode), (total, item) => total + item);
            var partiallyRefundableChunk = this.IntervalPrices
                .SingleOrDefault(p => p.StartDate < newEndDate && p.EndDate > newEndDate);
            if (partiallyRefundableChunk == null)
            {
                return fullyRefundablePartsTotal;
            }

            var proRataPartiallyRefundableChunk = partiallyRefundableChunk.CalculateProRataPrice(
                    partiallyRefundableChunk.StartDate, newEndDate);
            var additionalPartialRefund =
                partiallyRefundableChunk.FixedAndScalablePrice.ScalableComponents -
                proRataPartiallyRefundableChunk.FixedAndScalablePrice.ScalableComponents;
            return fullyRefundablePartsTotal + additionalPartialRefund;
        }

        /// <summary>
        /// Update the compound price with a new part.
        /// </summary>
        /// <param name="applicablePrice">The new price for a particular interval.</param>
        /// <returns>A new updated compound price.</returns>
        public CompoundPrice Update(IntervalPrice applicablePrice)
        {
            var newIntervalPrices = new List<IntervalPrice>();
            foreach (var intervalPrice in this.IntervalPrices)
            {
                if (intervalPrice.EndDate == applicablePrice.StartDate)
                {
                    newIntervalPrices.Add(intervalPrice);
                    break;
                }
                else if (intervalPrice.EndDate > applicablePrice.StartDate)
                {
                    var truncatedInterval = intervalPrice.CalculateProRataPrice(intervalPrice.StartDate, applicablePrice.StartDate);
                    newIntervalPrices.Add(truncatedInterval);
                    break;
                }
                else
                {
                    newIntervalPrices.Add(intervalPrice);
                }
            }

            if (newIntervalPrices.LastOrDefault()?.EndDate != applicablePrice.StartDate)
            {
                throw new InvalidOperationException("New interval's start date must be on or before exising compound price end date.");
            }

            newIntervalPrices.Add(applicablePrice);
            return new CompoundPrice(newIntervalPrices);
        }

        /// <summary>
        /// Get last premium from the list.
        /// </summary>
        /// <returns>The total of fixed and scalable components' premium .</returns>
        public decimal GetLastPremium()
        {
            var lastFixedAndScalablePricePremium = this.IntervalPrices.LastOrDefault().FixedAndScalablePrice.FixedComponents.BasePremium
                + this.IntervalPrices.LastOrDefault().FixedAndScalablePrice.ScalableComponents.BasePremium;
            return lastFixedAndScalablePricePremium;
        }
    }
}
