// <copyright file="QuoteStateChangeEventExporterCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Factory for quote state event condition.
    /// </summary>
    public class QuoteStateChangeEventExporterCondition : EventExporterCondition
    {
        private readonly ITextProvider operationNameProvider;
        private readonly ITextProvider originalStateProvider;
        private readonly ITextProvider resultingStateProvider;
        private readonly ITextProvider userTypeProvider;
        private readonly ITextProvider currentWorkflowStepProvider;
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteStateChangeEventExporterCondition"/> class.
        /// </summary>
        /// <param name="operationNameProvider">Factory for generating a string containing the required operation name.</param>
        /// <param name="originalStateProvider">Factory for generating a string containing the original state.</param>
        /// <param name="resultingStateProvider">Factory for generating a string containing the resulting state.</param>
        /// <param name="userTypeProvider">Factory for generating a string containing the user type.</param>
        /// <param name="currentWorkflowStepProvider">Factory for generating a string containing the current workflow step.</param>
        /// <param name="userService">The user service.</param>
        public QuoteStateChangeEventExporterCondition(
            ITextProvider operationNameProvider,
            ITextProvider originalStateProvider,
            ITextProvider resultingStateProvider,
            ITextProvider userTypeProvider,
            ITextProvider currentWorkflowStepProvider,
            IUserService userService)
        {
            this.operationNameProvider = operationNameProvider;
            this.originalStateProvider = originalStateProvider;
            this.resultingStateProvider = resultingStateProvider;
            this.userTypeProvider = userTypeProvider;
            this.currentWorkflowStepProvider = currentWorkflowStepProvider;
            this.userService = userService;
        }

        /// <inheritdoc/>
        public override async Task<bool> Evaluate(ApplicationEvent applicationEvent)
        {
            ITextProvider[] textProviders =
            {
                this.operationNameProvider,
                this.originalStateProvider,
                this.resultingStateProvider,
                this.userTypeProvider,
                this.currentWorkflowStepProvider,
            };

            var texts = textProviders.Select(async p => await this.GetTextProviderValueOrNull(p, applicationEvent));
            var operationName = await texts.ElementAt(0);
            var originalState = await texts.ElementAt(1);
            var resultingState = await texts.ElementAt(2);
            var userType = await texts.ElementAt(3);
            var currentWorkflowStep = await texts.ElementAt(4);
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            var @event = quote.LatestQuoteStateChange;
            bool userTypeMatches = true;
            if (userType.IsNotNullOrEmpty() && @event.UserId.HasValue)
            {
                var user = this.userService.GetUser(quote.Aggregate.TenantId, @event.UserId.Value);
                if (user != null)
                {
                    var userModel = this.userService.GetUserRoles(
                        quote.Aggregate.TenantId,
                        @event.UserId.Value);
                    userTypeMatches = userModel.Any(item => item.Type.Humanize().Equals(userType, StringComparison.OrdinalIgnoreCase));
                }
            }

            this.DebugInfo = "When checking whether quote state has changed from one state to another for a given operation and workflow step: \n";
            bool operationMatches = operationName.IsNullOrEmpty() ? true : operationName.Equals(@event.OperationName, StringComparison.OrdinalIgnoreCase);
            this.DebugInfo += operationName.IsNullOrEmpty()
                ? "The operation name was not specified so we didn't require it match. \n"
                : $"The operation name \"{operationName}\" " + (operationMatches ? "MATCHED" : "DID NOT MATCH")
                    + $" the operation in progress, \"{@event.OperationName}\". \n";

            bool originalStateMatches = originalState.IsNullOrEmpty() ? true : originalState.Equals(@event.OriginalState, StringComparison.OrdinalIgnoreCase);
            this.DebugInfo += originalState.IsNullOrEmpty()
                ? "The original state was not specified so we didn't require it match. \n"
                : $"The original state \"{originalState}\" " + (originalStateMatches ? "MATCHED" : "DID NOT MATCH")
                    + $" the current original state, \"{@event.OriginalState}\"";

            bool resultingStateMatches = resultingState.IsNullOrEmpty() ? true : resultingState.Equals(@event.ResultingState, StringComparison.OrdinalIgnoreCase);
            this.DebugInfo += originalState.IsNullOrEmpty()
                ? "The resulting state was not specified so we didn't require it match. \n"
                : $"The resulting state \"{resultingState}\" " + (resultingStateMatches ? "MATCHED" : "DID NOT MATCH")
                    + $" the current resulting state, \"{@event.ResultingState}\". \n";

            bool workflowStepMatches = currentWorkflowStep.IsNullOrEmpty() ? true : currentWorkflowStep.Equals(quote.WorkflowStep);
            this.DebugInfo += originalState.IsNullOrEmpty()
                ? "The workflow step was not specified so we didn't require it match. \n"
                : $"The workflow step \"{currentWorkflowStep}\" " + (workflowStepMatches ? "MATCHED" : "DID NOT MATCH")
                    + $" the current workflow step, \"{quote.WorkflowStep}\". \n";

            bool matches = operationMatches && originalStateMatches && resultingStateMatches && userTypeMatches && workflowStepMatches;
            this.DebugInfo += matches
                ? $"Since all of the properties specified matched, the condition evaluated to TRUE. \n"
                : $"Since at least one of the properties specified did not match, the condition evaluated to FALSE. \n";

            return matches;
        }

        private async Task<string> GetTextProviderValueOrNull(ITextProvider provider, ApplicationEvent applicationEvent)
        {
            return provider != null
                ? await provider?.Invoke(applicationEvent)
                : null;
        }
    }
}
