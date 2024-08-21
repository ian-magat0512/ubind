// <copyright file="IntervalPrice.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Represents the price that is applicable for a quote.
    /// </summary>
    public class IntervalPrice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalPrice"/> class.
        /// </summary>
        /// <param name="fixedAndScalablePrice">The price breakdown in fixed and scalable parts.</param>
        /// <param name="startDate">The start date of the period covered by the price.</param>
        /// <param name="endDate">The end date of the period covered by the price.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        public IntervalPrice(
            FixedAndScalablePrice fixedAndScalablePrice,
            LocalDate startDate,
            LocalDate endDate)
        {
            this.FixedAndScalablePrice = fixedAndScalablePrice;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        [JsonConstructor]
        private IntervalPrice()
        {
        }

        /// <summary>
        /// Gets the breakdown of applicable price.
        /// </summary>
        [JsonProperty]
        public FixedAndScalablePrice FixedAndScalablePrice { get; private set; }

        /// <summary>
        /// Gets the start of period covered by applicable price.
        /// </summary>
        [JsonProperty]
        public LocalDate StartDate { get; private set; }

        /// <summary>
        /// Gets the end of period covered by applicable price.
        /// </summary>
        [JsonProperty]
        public LocalDate EndDate { get; private set; }

        /// <summary>
        /// Calculate a pro-rata price, by scaling a price for a new time period.
        /// </summary>
        /// <param name="newStartDate">The new period start date.</param>
        /// <param name="newEndDate">The new period end date.</param>
        /// <returns>A new price for the new period.</returns>
        public IntervalPrice CalculateProRataPrice(
            LocalDate newStartDate,
            LocalDate newEndDate)
        {
            var originalPeriod = Period.Between(this.StartDate, this.EndDate, PeriodUnits.Days);
            var newPeriod = Period.Between(newStartDate, newEndDate, PeriodUnits.Days);
            var scaleFactor = 0.0m;

            // To ensure we don't divide by zero, the start date must be before the end date. Otherwise
            // our scale factor is 0.
            if (newStartDate < newEndDate)
            {
                scaleFactor = decimal.Divide(newPeriod.Days, originalPeriod.Days);
            }

            var scaledComponents = this.FixedAndScalablePrice.ScalableComponents * scaleFactor;
            var newPrice = new FixedAndScalablePrice(this.FixedAndScalablePrice.FixedComponents, scaledComponents);
            return new IntervalPrice(newPrice, newStartDate, newEndDate);
        }

        /// <summary>
        /// Gets the total price breakdown of fixed and scalable components.
        /// </summary>
        /// <returns>A new price breakdown including both fixed and scalable components.</returns>
        public PriceBreakdown ToPriceBreakdown()
        {
            return this.FixedAndScalablePrice.FixedComponents + this.FixedAndScalablePrice.ScalableComponents;
        }

        /// <summary>
        /// Gets an annualized price based on this interval price.
        /// </summary>
        /// <returns>The price pro-rated to one year (365 days).</returns>
        public PriceBreakdown Annualize()
        {
            var originalPeriod = Period.Between(this.StartDate, this.EndDate, PeriodUnits.Days);
            if (originalPeriod.Days == 0)
            {
                // Instead of throwing an exception here, we are going to return the same price, since some policies are not annual policies.
                // We will need a way to solve this in the future, ie mark a policy type as annual or not
                /*
                throw new ErrorException(Errors.Calculation.UnableToAnnualisePremiumFromAZeroDayPeriod(this.StartDate, this.EndDate));
                */
                return this.FixedAndScalablePrice.FixedComponents + this.FixedAndScalablePrice.ScalableComponents;
            }

            const decimal daysPerYearForAnnualization = 365m;
            var scaleFactor = decimal.Divide(daysPerYearForAnnualization, originalPeriod.Days);
            var scaledComponents = this.FixedAndScalablePrice.ScalableComponents * scaleFactor;
            return this.FixedAndScalablePrice.FixedComponents + scaledComponents;
        }
    }
}
