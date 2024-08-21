// <copyright file="RecordFundingProposalCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Commands.Quote
{
    using System;
    using MediatR;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Patterns.Cqrs;

    public class RecordFundingProposalCommand : ICommand<Unit>
    {
        public RecordFundingProposalCommand(Guid tenantId, Guid quoteId, FundingProposal fundingProposal, bool accepted = false)
        {
            this.TenantId = tenantId;
            this.QuoteId = quoteId;
            this.FundingProposal = fundingProposal;
            this.Accepted = accepted;
        }

        public Guid TenantId { get; }

        public Guid QuoteId { get; }

        public FundingProposal FundingProposal { get; }

        /// <summary>
        /// Gets a value indicating whether the funding proposal was accepted externally, and therefore we should
        /// also record the acceptance.
        /// Funding providers which use an IFrame or redirect will handle externally.
        /// </summary>
        public bool Accepted { get; }
    }
}
