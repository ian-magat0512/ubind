// <copyright file="IPaymentGateway.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment
{
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Entities;
    using UBind.Domain.Payment;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Service allowing payments to be made.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Gets a value indicating whether this gateway can calculate merchant fees in advance.
        /// </summary>
        /// <returns>True if the gateway supports calculating merchant fees in advance.</returns>
        bool CanCalculateMerchantFees();

        /// <summary>
        /// Gets the merchant fees for a given transaction.
        /// </summary>
        /// <param name="payableAmount">The amount payable, excluding any pre-calculated merchant fees.</param>
        /// <param name="currencyCode">The currency code.</param>
        /// <param name="paymentData">The credit card details.</param>
        /// <returns>The merchant fees.</returns>
        Task<MerchantFees> CalculateMerchantFees(
            decimal payableAmount,
            string currencyCode,
            PaymentData paymentData);

        /// <summary>
        /// Make a payment via the gateway using credit card details.
        /// </summary>
        /// <param name="quote">The quote the payment is for (only required for unique number generation).</param>
        /// <param name="priceBreakdown">The details of what needs paying.</param>
        /// <param name="cardDetails">Card details to use.</param>
        /// <param name="reference">A reference.</param>
        /// <returns>A task that will return the response from the payment gateway.</returns>
        Task<PaymentGatewayResult> MakePayment(Quote quote, PriceBreakdown priceBreakdown, CreditCardDetails cardDetails, string reference);

        /// <summary>
        /// Make a payment via the gateway using tokenised credit card details.
        /// </summary>
        /// <param name="priceBreakdown">The details of what needs paying.</param>
        /// <param name="tokenId">ID of the token to use.</param>
        /// <param name="reference">A reference.</param>
        /// <returns>A task that will return the response from the payment gateway.</returns>
        Task<PaymentGatewayResult> MakePayment(PriceBreakdown priceBreakdown, string tokenId, string reference);

        /// <summary>
        /// Make a payment via the gateway using a saved payment method.
        /// </summary>
        /// <param name="quote">The quote the payment is for.</param>
        /// <param name="priceBreakdown">The details of what needs paying.</param>
        /// <param name="savedPaymentMethod">The saved payment method details to be used.</param>
        /// <returns>A task that will return the response from the payment gateway.</returns>
        Task<PaymentGatewayResult> MakePayment(Quote quote, PriceBreakdown priceBreakdown, SavedPaymentMethod savedPaymentMethod);
    }
}
