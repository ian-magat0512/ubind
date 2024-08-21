// <copyright file="IDifferentialPriceCalculationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.Pricing
{
    using NodaTime;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Service for calculating differential prices for mid-term adjustments.
    /// </summary>
    public interface IDifferentialPriceCalculationService
    {
        /// <summary>
        /// Calculate a differential prices.
        /// </summary>
        /// <param name="originalPrice">The previously calculated price for the policy.</param>
        /// <param name="originalRefundableComponentFilter">A filter specifying which parts of the previous price are refundable.</param>
        /// <param name="newPrice">The newly calculated price for the adjusted policy.</param>
        /// <param name="newRefundableComponentFilter">A filter specifying which parts of the new price are refundable (pro-rata-able).</param>
        /// <param name="originalEffectiveDate">The effective date for the previously calculated price.</param>
        /// <param name="expiryDate">The expiry date for the policy.</param>
        /// <param name="newEffectiveDate">The effective date for the adjustment.</param>
        /// <returns>A new instance of <see cref="PriceAdjustment"/> detailing the refundable and chargable components of the adjustment.</returns>
        PriceAdjustment CalculateDifferentialPrice(IPriceBreakdown originalPrice, PriceComponentFilter originalRefundableComponentFilter, IPriceBreakdown newPrice, PriceComponentFilter newRefundableComponentFilter, LocalDate originalEffectiveDate, LocalDate expiryDate, LocalDate newEffectiveDate);
    }
}
