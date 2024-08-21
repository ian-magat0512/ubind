// <copyright file="IClaimWorkflow.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim.Workflow
{
    using System.Collections.Generic;
    using UBind.Domain.Aggregates.Quote.Workflow;

    /// <summary>
    /// Configurable workflow logic for a product's quotes.
    /// </summary>
    public interface IClaimWorkflow
    {
        /// <summary>
        /// Gets a value indicating whether gets a value indicating whether settlement is required before binding.
        /// </summary>
        bool IsSettlementRequired { get; }

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
        /// <returns><code>true</code> if the operation is permitted, otherwise. <code>false</code></returns>
        bool IsActionPermittedByState(ClaimActions action, string currentState);

        /// <summary>
        /// Returns the resulting state of a given action if it is configured for the given quote.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="currentState">The quote's current state.</param>
        /// <returns>The resulting state.</returns>
        string GetResultingState(ClaimActions action, string currentState);

        WorkFlowOperation GetOperation(ClaimActions action);
    }
}
