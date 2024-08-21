// <copyright file="QuoteWorkflow.cs" company="uBind">
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
    using Newtonsoft.Json;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ValueTypes;

    /// <inheritdoc />
    public class QuoteWorkflow : IQuoteWorkflow
    {
        /// <summary>
        /// Gets or sets the alias for <see cref="QuoteAction.Actualise"/> operation.
        /// </summary>
        private readonly string quoteAction = "Quote";

        /// <inheritdoc />
        [JsonProperty]
        public bool IsSettlementRequired { get; set; }

        /// <inheritdoc />
        [JsonProperty]
        public BindOptions BindOptions { get; set; }

        /// <inheritdoc />
        [JsonProperty]
        public List<WorkFlowOperation> Transitions { private get; set; }

        /// <inheritdoc />
        [JsonProperty]
        public bool IsSettlementSupported { get; set; }

        /// <inheritdoc />
        [JsonProperty]
        public decimal? PremiumThresholdRequiringSettlement { get; set; }

        /// <inheritdoc />
        public bool IsActionPermittedByState(QuoteAction action, string currentState)
        {
            if (action.Humanize().Equals(WorkflowConstants.CalculationOperation, StringComparison.OrdinalIgnoreCase)
                || action.Humanize().Equals(WorkflowConstants.FormDataUpdateOperation, StringComparison.OrdinalIgnoreCase)
                || action.Humanize().Equals(WorkflowConstants.QuoteVersionOperation, StringComparison.OrdinalIgnoreCase))
            {
                return !WorkflowConstants.NoDataUpdateStates.Contains(currentState, StringComparer.OrdinalIgnoreCase);
            }

            string operation = action == QuoteAction.Actualise ? this.quoteAction : action.Humanize();
            var transition = this.Transitions.FirstOrDefault(x => x.Action.Equals(operation, StringComparison.OrdinalIgnoreCase));
            if (transition == null)
            {
                List<string> definedTransitionActions = this.Transitions.Select(t => t.Action).ToList();
                throw new ErrorException(Errors.Quote.OperationNotDefinedForWorkflow(
                    action.ToString(), currentState, definedTransitionActions));
            }

            if (WorkflowConstants.CustomerOperation.Equals(operation, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (currentState == StandardQuoteStates.Complete
                || transition.RequiredStates.Contains(currentState, StringComparer.OrdinalIgnoreCase)
                || transition.RequiredStates.Count == 0)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public string GetResultingState(QuoteAction action, string currentState)
        {
            string operation = action == QuoteAction.Actualise ? this.quoteAction : action.Humanize();
            var transition = this.Transitions.FirstOrDefault(x => x.Action.Equals(operation, StringComparison.OrdinalIgnoreCase));
            return transition == null ?
                currentState :
                transition.ResultingState;
        }

        public WorkFlowOperation GetOperation(QuoteAction action)
        {
            string operation = action == QuoteAction.Actualise ? this.quoteAction : action.Humanize();
            return this.Transitions.FirstOrDefault(x => x.Action.Equals(operation, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public bool IsResultingStateSupported(string state)
        {
            return this.Transitions.Any(x => x.ResultingState.Equals(state, StringComparison.OrdinalIgnoreCase));
        }
    }
}
