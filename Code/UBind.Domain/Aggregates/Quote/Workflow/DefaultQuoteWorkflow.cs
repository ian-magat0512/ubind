// <copyright file="DefaultQuoteWorkflow.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Humanizer;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Default quote workflow to use.
    /// </summary>
    public class DefaultQuoteWorkflow : IQuoteWorkflow
    {
        private List<WorkFlowOperation> transitions = new List<WorkFlowOperation>()
        {
            new WorkFlowOperation
            {
                Action = QuoteAction.Quote.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Nascent },
                ResultingState = StandardQuoteStates.Incomplete,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.ReviewReferral.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Incomplete },
                ResultingState = StandardQuoteStates.Review,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.ReviewApproval.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Review },
                ResultingState = StandardQuoteStates.Approved,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.AutoApproval.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Incomplete },
                ResultingState = StandardQuoteStates.Approved,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.Return.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Review, StandardQuoteStates.Endorsement, StandardQuoteStates.Approved },
                ResultingState = StandardQuoteStates.Incomplete,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.EndorsementReferral.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Incomplete, StandardQuoteStates.Review },
                ResultingState = StandardQuoteStates.Endorsement,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.EndorsementReferral.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Incomplete, StandardQuoteStates.Review },
                ResultingState = StandardQuoteStates.Endorsement,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.EndorsementApproval.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Endorsement },
                ResultingState = StandardQuoteStates.Approved,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.Decline.Humanize(),
                RequiredStates = new List<string>(),
                ResultingState = StandardQuoteStates.Declined,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.Bind.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Approved },
                ResultingState = StandardQuoteStates.Complete,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.Submit.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Approved },
                ResultingState = StandardQuoteStates.Complete,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.Policy.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Approved },
                ResultingState = StandardQuoteStates.Complete,
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.Invoice.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Approved },
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.Payment.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Approved },
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.Fund.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Approved },
            },
            new WorkFlowOperation
            {
                Action = QuoteAction.CreditNote.Humanize(),
                RequiredStates = new List<string>() { StandardQuoteStates.Approved },
            },
        };

        /// <inheritdoc/>
        public BindOptions BindOptions
        {
            get
            {
                return BindOptions.PolicyAndTransactionRecord;
            }
        }

        /// <inheritdoc/>
        public bool IsSettlementRequired
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc/>
        public bool IsSettlementSupported
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc/>
        public decimal? PremiumThresholdRequiringSettlement => 2000.00M;

        /// <inheritdoc />
        public List<WorkFlowOperation> Transitions
        {
            get
            {
                return this.transitions;
            }

            set
            {
                this.Transitions = this.transitions;
            }
        }

        /// <summary>
        /// Returns the resulting state of the given operation based on configured transition.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="currentState">The current state of the quote.</param>
        /// <returns>Returns the current state of the quote, by default.</returns>
        public string GetResultingState(QuoteAction action, string currentState)
        {
            string? resultingState = this.Transitions.SingleOrDefault(x => x.Action == action.ToString())?.ResultingState;
            return string.IsNullOrEmpty(resultingState)
                ? currentState
                : resultingState;
        }

        /// <inheritdoc/>
        public bool IsActionPermittedByState(QuoteAction action, string currentState)
        {
            return true;
        }

        public WorkFlowOperation GetOperation(QuoteAction action)
        {
            return this.Transitions.FirstOrDefault(x => x.Action.Equals(action.Humanize(), StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public bool IsResultingStateSupported(string state)
        {
            return this.Transitions.Any(x => x.ResultingState.Equals(state, StringComparison.OrdinalIgnoreCase));
        }
    }
}
