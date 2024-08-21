// <copyright file="StripePaymentGateway.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Stripe
{
    using System;
    using System.Threading.Tasks;
    using global::Stripe;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Payment;
    using UBind.Domain.ReadWriteModel;
    using Quote = Domain.Aggregates.Quote.Quote;

    /// <summary>
    /// Payment gateway for Stripe.
    /// </summary>
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly IStripeConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StripePaymentGateway"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public StripePaymentGateway(IStripeConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public bool CanCalculateMerchantFees()
        {
            return false;
        }

        /// <inheritdoc/>
        public Task<MerchantFees> CalculateMerchantFees(
            decimal payableAmount,
            string currencyCode,
            PaymentData paymentData)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<PaymentGatewayResult> MakePayment(
            Quote quote, PriceBreakdown priceBreakdown, CreditCardDetails cardDetails, string reference)
        {
            throw new NotSupportedException("Stripe only supports payment using tokenized credit card details.");
        }

        /// <inheritdoc/>
        public async Task<PaymentGatewayResult> MakePayment(PriceBreakdown priceBreakdown, string tokenId, string reference)
        {
            var options = new ChargeCreateOptions
            {
                Amount = (int)(priceBreakdown.TotalPayable * 100),
                Currency = priceBreakdown.CurrencyCode,
                Description = reference,
                Source = tokenId, // obtained with Stripe.js,
            };

            // ChargeCreateOptions has conflicting JSON property names specified in attributes so cannot be serialized directly.
            var capturedPayload = new
            {
                options.Amount,
                options.Currency,
                options.Description,
                options.Source,
            };

            var requestJson = JsonConvert.SerializeObject(capturedPayload);
            var service = new ChargeService(new StripeClient(this.configuration.PrivateApiKey));
            try
            {
                var charge = await service.CreateAsync(options);
                var responseJson = JsonConvert.SerializeObject(charge);
                var details = new PaymentDetails(PaymentGatewayName.Stripe, ((decimal)options.Amount) / 100m, options.Description, requestJson, responseJson);
                return PaymentGatewayResult.CreateSuccessResponse(details);
            }
            catch (StripeException exception)
            {
                var responseJson = JsonConvert.SerializeObject(exception.StripeError);
                var additionalDetails = new List<string>
                {
                    "Message : " + exception.Message,
                    !string.IsNullOrEmpty(exception.StripeError?.Code) ? "Error Code : " + exception.StripeError.Code : null,
                    !string.IsNullOrEmpty(exception.StripeError?.DeclineCode) ? "Decline Code : " + exception.StripeError.DeclineCode : null,
                };
                var errorAdditionalDetails = additionalDetails.Where(detail => detail != null).ToArray();

                var details = new PaymentDetails(PaymentGatewayName.Stripe, ((decimal)options.Amount) / 100m, options.Description, requestJson, responseJson);
                return PaymentGatewayResult.CreateFailureResponse(details, errorAdditionalDetails);
            }
        }

        /// <inheritdoc />
        public Task<PaymentGatewayResult> MakePayment(UBind.Domain.Aggregates.Quote.Quote quote, PriceBreakdown priceBreakdown, SavedPaymentMethod savedPaymentMethod)
        {
            throw new NotImplementedException("The Stripe payment gateway does not support using saved payment methods yet.");
        }
    }
}
