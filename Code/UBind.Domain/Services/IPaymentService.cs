// <copyright file="IPaymentService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System.Threading.Tasks;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;

    /// <summary>
    /// Service for handling quote payment attempts.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Gets a value indicating whether this gateway can calculate merchant fees in advance.
        /// </summary>
        /// <param name="releaseContext">The product context.</param>
        /// <returns>True if the gateway supports calculating merchant fees in advance.</returns>
        Task<bool> CanCalculateMerchantFees(ReleaseContext releaseContext);

        /// <summary>
        /// Gets the merchant fees for a given transaction.
        /// </summary>
        /// <param name="releaseContext">The product context.</param>
        /// <param name="payableAmount">The amount payable, excluding any pre-calculated merchant fees.</param>
        /// <param name="currencyCode">The currency code.</param>
        /// <param name="paymentData">The payment data.</param>
        /// <returns>The merchant fees.</returns>
        Task<MerchantFees> CalculateMerchantFees(
            ReleaseContext releaseContext,
            decimal payableAmount,
            string currencyCode,
            PaymentData paymentData);
    }
}
