// <copyright file="CardPaymentCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System;
    using MediatR;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    /// <summary>
    /// This class is needed to create a command to settle a quote premium via card payment.
    /// </summary>
    public class CardPaymentCommand : ICommand<Unit>
    {
        public CardPaymentCommand(
              ReleaseContext releaseContext,
              Guid quoteId,
              Guid calculationResultId,
              FormData latestFormData,
              string paymentTokenId = null,
              CreditCardDetails cardDetails = null,
              Guid savedPaymentMethodId = default)
        {
            this.ReleaseContext = releaseContext;
            this.QuoteId = quoteId;
            this.CalculationResultId = calculationResultId;
            this.LatestFormData = latestFormData;
            this.PaymentTokenId = paymentTokenId;
            this.CreditCardDetails = cardDetails;
            this.SavedPaymentMethodId = savedPaymentMethodId;
        }

        /// <summary>
        /// Gets the ID of the tenant.
        /// </summary>
        public ReleaseContext ReleaseContext { get; }

        /// <summary>
        /// Gets the ID of the quote to be paid.
        /// </summary>
        public Guid QuoteId { get; }

        /// <summary>
        /// Gets the calculation result to base payable amount on.
        /// </summary>
        public Guid CalculationResultId { get; }

        /// <summary>
        /// Gets the latest formdata.
        /// </summary>
        public FormData LatestFormData { get; }

        /// <summary>
        /// Gets a payment token ID - if using token-based payment gateways.
        /// </summary>
        public string PaymentTokenId { get; }

        /// <summary>
        /// Gets the card details to be used for payments - if using card payments.
        /// </summary>
        public CreditCardDetails CreditCardDetails { get; }

        /// <summary>
        /// Gets the ID of the saved payment method to be used - if using a saved payment method for settlement.
        /// </summary>
        public Guid? SavedPaymentMethodId { get; }
    }
}
