// <copyright file="ClaimWorkflow.cs" company="uBind">
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
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote.Workflow;

    /// <inheritdoc />
    public class ClaimWorkflow : IClaimWorkflow
    {
        /// <inheritdoc />
        [JsonProperty]
        public bool IsSettlementRequired { get; set; }

        /// <inheritdoc />
        [JsonProperty]
        public BindOptions BindOptions { get; set; }

        /// <inheritdoc />
        [JsonProperty]
        public List<WorkFlowOperation> Transitions { get; set; }

        /// <inheritdoc />
        public bool IsActionPermittedByState(ClaimActions action, string currentState)
        {
            if (action.Humanize().Equals(WorkflowConstants.CalculationOperation, StringComparison.OrdinalIgnoreCase)
                || action.Humanize().Equals(WorkflowConstants.FormDataUpdateOperation, StringComparison.OrdinalIgnoreCase)
                || action.Humanize().Equals(WorkflowConstants.QuoteVersionOperation, StringComparison.OrdinalIgnoreCase))
            {
                return !WorkflowConstants.NoDataUpdateStates.Contains(currentState, StringComparer.OrdinalIgnoreCase);
            }

            var transition = this.Transitions.FirstOrDefault(x => x.Action.Equals(action.Humanize(), StringComparison.OrdinalIgnoreCase));
            if (transition == null)
            {
                throw new InvalidOperationException($"Action {action.Humanize()} is not configured for this claim.");
            }

            if (WorkflowConstants.CustomerOperation.Equals(action.Humanize(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (transition.RequiredStates.Contains(currentState, StringComparer.OrdinalIgnoreCase) || transition.RequiredStates.Count == 0)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public string GetResultingState(ClaimActions action, string currentState)
        {
            var transition = this.Transitions.FirstOrDefault(x => x.Action.Equals(action.Humanize(), StringComparison.OrdinalIgnoreCase));
            return transition == null ?
                currentState :
                transition.ResultingState;
        }

        public WorkFlowOperation GetOperation(ClaimActions action)
        {
            return this.Transitions.FirstOrDefault(x => x.Action.Equals(action.Humanize(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
