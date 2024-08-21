// <copyright file="BindRequirements.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Workflow
{
    /// <summary>
    /// Represents the configured requirements for binding a quote.
    /// </summary>
    public enum BindRequirements
    {
        /// <summary>
        /// No requirements, beyond standard quote invariants (e.g. must have a calculation result, etc.).
        /// </summary>
        None = 0,

        /// <summary>
        /// The quote must be paid for before being bound.
        /// </summary>
        Payment = 1,

        /// <summary>
        /// The quote must be funded before being bound.
        /// </summary>
        Funding = 2,

        /// <summary>
        /// The quote must be either paid for or funded before being bound.
        /// </summary>
        PaymentOrFunding,
    }
}
