// <copyright file="DefaultClaimWorkflow.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Humanizer;
    using UBind.Domain.Aggregates.Quote.Workflow;

    /// <summary>
    /// Default claim workflow to use.
    /// </summary>
    public class DefaultClaimWorkflow : IClaimWorkflow
    {
        private List<WorkFlowOperation> transitions = new List<WorkFlowOperation>()
        {
            new WorkFlowOperation { Action = "Actualise", RequiredStates = new List<string>(), ResultingState = "Incomplete" },
            new WorkFlowOperation { Action = "AutoApproval", RequiredStates = new List<string>(), ResultingState = "Approved" },
            new WorkFlowOperation { Action = "Notify", RequiredStates = new List<string>(), ResultingState = "Notified" },
            new WorkFlowOperation { Action = "Acknowledge", RequiredStates = new List<string>(), ResultingState = "Acknowledged" },
            new WorkFlowOperation { Action = "Return", RequiredStates = new List<string>(), ResultingState = "Incomplete" },
            new WorkFlowOperation { Action = "Settle", RequiredStates = new List<string>(), ResultingState = "Complete" },
        };

        /// <inheritdoc/>
        public BindOptions BindOptions
        {
            get
            {
                return BindOptions.PolicyAndTransactionRecord;
            }

            set
            {
                this.BindOptions = BindOptions.PolicyAndTransactionRecord;
            }
        }

        /// <inheritdoc/>
        public bool IsSettlementRequired
        {
            get
            {
                return true;
            }

            set
            {
                this.IsSettlementRequired = true;
            }
        }

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
        public string GetResultingState(ClaimActions action, string currentState)
        {
            return this.Transitions.SingleOrDefault(x => x.Action == action.ToString())?.ResultingState ?? currentState;
        }

        /// <inheritdoc/>
        public bool IsActionPermittedByState(ClaimActions action, string currentState)
        {
            return true;
        }

        public WorkFlowOperation GetOperation(ClaimActions action)
        {
            return this.Transitions.FirstOrDefault(x => x.Action.Equals(action.Humanize(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
