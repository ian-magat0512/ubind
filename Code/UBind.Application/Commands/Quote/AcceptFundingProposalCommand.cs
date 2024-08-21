// <copyright file="AcceptFundingProposalCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System;
    using MediatR;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    public class AcceptFundingProposalCommand : ICommand<Unit>
    {
        public AcceptFundingProposalCommand(
            ReleaseContext releaseContext,
            Guid quoteId,
            Guid fundingProposalId,
            IPaymentMethodDetails paymentDetails)
        {
            this.ReleaseContext = releaseContext;
            this.QuoteId = quoteId;
            this.PremiumFundingProposalId = fundingProposalId;
            this.PaymentMethodDetails = paymentDetails;
        }

        public ReleaseContext ReleaseContext { get; }

        public Guid QuoteId { get; }

        public Guid PremiumFundingProposalId { get; }

        public IPaymentMethodDetails PaymentMethodDetails { get; }
    }
}
