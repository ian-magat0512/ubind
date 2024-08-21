// <copyright file="IQuoteWorkflow.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Workflow
{
    using System.Collections.Generic;

    /// <summary>
    /// Configurable workflow logic for a product's quotes.
    /// </summary>
    public interface IQuoteWorkflow
    {
        /// <summary>
        /// Gets a value indicating whether settlement is required before binding.
        /// Settlement means payment - it requires someone to make payment or establish a
        /// premium funding contract.
        /// </summary>
        bool IsSettlementRequired { get; }

        /// <summary>
        /// Gets a value indicating whether settlement is supported.
        /// Settlement means payment. This specified whether we support online payment.
        /// </summary>
        bool IsSettlementSupported { get; }

        /// <summary>
        /// Gets the amount of premium above which settlement is not required.
        /// The idea here is that if the premium amount is a lot, we don't want to force
        /// people to pay via credit card as the card fees would be astronomical.
        /// </summary>
        decimal? PremiumThresholdRequiringSettlement { get; }

        /// <summary>
        /// Gets the optional actions that should be triggered on binding a quote.
        /// </summary>
        BindOptions BindOptions { get; }

        /// <summary>
        /// Sets the list of operations available for the quote.
        /// </summary>
        List<WorkFlowOperation> Transitions { set; }

        /// <summary>
        /// Check whether a given action is permited by the quote workflow, given the current state of the quote.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="currentState">The quote's current state.</param>
        /// <returns><c>true</c> if the operation is permitted, otherwise <c>false</c>.</returns>
        bool IsActionPermittedByState(QuoteAction action, string currentState);

        /// <summary>
        /// Returns the resulting state of a given action if it is configured for the given quote.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="currentState">The quote's current state.</param>
        /// <returns>The resulting state.</returns>
        string GetResultingState(QuoteAction action, string currentState);

        WorkFlowOperation GetOperation(QuoteAction action);

        /// <summary>
        /// The value whether the resulting state is supported by the quote workflow.
        /// </summary>
        bool IsResultingStateSupported(string state);
    }
}
