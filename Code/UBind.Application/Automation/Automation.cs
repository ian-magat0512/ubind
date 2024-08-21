// <copyright file="Automation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Triggers;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// An automation that can be run.
    /// </summary>
    public class Automation
    {
        private readonly IActionRunner runner;

        /// <summary>
        /// Initializes a new instance of the <see cref="Automation"/> class.
        /// </summary>
        /// <param name="name">The name of the automation.</param>
        /// <param name="alias">The ID of the automation.</param>
        /// <param name="description">The description for the automation.</param>
        /// <param name="runCondition">The run condition for the automation, if any.</param>
        /// <param name="triggers">The triggers to be used by the automation.</param>
        /// <param name="actions">The actions to be done by the automation.</param>
        /// <param name="actionRunner">The handler for running automation actions.</param>
        public Automation(
            string name,
            string alias,
            string description,
            IProvider<Data<bool>> runCondition,
            IEnumerable<Trigger> triggers,
            IEnumerable<Action> actions,
            IActionRunner actionRunner)
        {
            Contract.Assert(name.IsNotNullOrWhitespace());
            Contract.Assert(alias.IsNotNullOrWhitespace());

            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.RunCondition = runCondition;
            this.Triggers = triggers;
            this.Actions = actions;
            this.runner = actionRunner;
        }

        /// <summary>
        /// Gets the automation name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the automation ID.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Gets the description for the automation.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the run condition for the automation.
        /// </summary>
        public IProvider<Data<bool>> RunCondition { get; }

        /// <summary>
        /// Gets the list of triggers for the automation.
        /// </summary>
        public IEnumerable<Trigger> Triggers { get; }

        /// <summary>
        /// Gets the list of actions for the automation to run.
        /// </summary>
        public IEnumerable<Action> Actions { get; }

        public bool IsReadOnly()
        {
            return this.Actions.All(a => a.IsReadOnly());
        }

        /// <summary>
        /// Finds which triggers match the trigger data in the automation data.
        /// </summary>
        /// <returns>The matching triggers.</returns>
        public async Task<IEnumerable<Trigger>> GetMatchingTriggers(AutomationData data)
        {
            var matchingTriggers = new List<Trigger>();
            foreach (var trigger in this.Triggers)
            {
                if (await trigger.DoesMatch(data))
                {
                    matchingTriggers.Add(trigger);
                }
            }

            return matchingTriggers;
        }

        public async Task<Trigger?> GetFirstMatchingTrigger(AutomationData data)
        {
            return (await this.GetMatchingTriggers(data)).FirstOrDefault();
        }

        /// <summary>
        /// Checks if one of the automation's triggers matches the trigger data in the automation data.
        /// </summary>
        /// <returns>true if there is a match, otherwise false.</returns>
        public async Task<bool> DoesMatch(AutomationData data)
        {
            foreach (var trigger in this.Triggers)
            {
                if (await trigger.DoesMatch(data))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get all periodic triggers in the automation.
        /// </summary>
        /// <returns>all periodic triggers for this automation.</returns>
        public IEnumerable<PeriodicTrigger> GetPeriodicTriggers()
        {
            return this.Triggers.Where(c => c.GetType() == typeof(PeriodicTrigger)).Select(c => (PeriodicTrigger)c);
        }

        /// <summary>
        /// Executes the actions as part of this automation.
        /// </summary>
        /// <param name="data">The automation data.</param>
        /// <returns>An awaitable task containing execution of action/s.</returns>
        public async Task Execute(AutomationData data, Trigger? matchingTrigger = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (MiniProfiler.Current.Step(nameof(Automation) + "." + nameof(this.Execute)))
            {
                matchingTrigger = matchingTrigger ?? await this.ResolveMatchingTrigger(data);
                if (matchingTrigger == null)
                {
                    return;
                }

                data.RecordAutomationAlias(this.Alias);
                data.UpdateTriggerData(matchingTrigger.Alias);
                var providerContext = new ProviderContext(data, cancellationToken);

                var runTriggerCondition = await this.ResolveRunTriggerCondition(matchingTrigger, providerContext);
                if (!runTriggerCondition)
                {
                    return;
                }

                var shouldExecuteActions = await this.ResolveRunAutomationCondition(providerContext);
                if (shouldExecuteActions)
                {
                    await this.ExecuteActions(data, cancellationToken);
                }

                bool hasActionsWithUnhandledErrors = data.Actions.Any(x => x.Value.Error != null);

                // Skip response generation if error is raised.
                if (hasActionsWithUnhandledErrors)
                {
                    var latestUnhandledError = data.Actions.LastOrDefault(err => err.Value.Error != null).Value.Error;
                    var triggerAlias = data.Trigger?.TriggerAlias;
                    data.Automation.TryGetValue("automation", out object? automationAlias);
                    data.Context.TryGetValue("quote", out object? quoteObject);
                    if (data.Error != null)
                    {
                        data.Error.Data ??= new JObject();

                        if (quoteObject is Domain.SerialisedEntitySchemaObject.Quote quote)
                        {
                            data.Error.Data["quoteId"] = quote.Id;
                        }

                        data.Error.Data["automationAlias"] = automationAlias?.ToString();
                        data.Error.Data["triggerAlias"] = triggerAlias;
                    }
                    data.SetError(latestUnhandledError);
                    throw new ErrorException(latestUnhandledError);
                }
                else
                {
                    // Only Http, portal page and extension point triggers expect a return response.
                    await matchingTrigger.GenerateCompletionResponse(providerContext);
                }
            }
        }

        private async Task<Trigger?> ResolveMatchingTrigger(AutomationData data)
        {
            var resolvedTriggers = await this.Triggers.SelectAsync(async t => await t.DoesMatch(data) ? t : null);
            return resolvedTriggers.FirstOrDefault(t => t != null);
        }

        private async Task<bool> ResolveRunTriggerCondition(Trigger matchingTrigger, ProviderContext providerContext)
        {
            if (matchingTrigger is ConditionalTrigger conditionalTrigger && conditionalTrigger.RunCondition != null)
            {
                bool resolveTriggerCondition = (await conditionalTrigger.RunCondition.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                return resolveTriggerCondition;
            }

            return true;
        }

        private async Task<bool> ResolveRunAutomationCondition(ProviderContext providerContext)
        {
            if (this.RunCondition != null)
            {
                bool resolveRunAutomation = (await this.RunCondition.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                return resolveRunAutomation;
            }

            return true;
        }

        private async Task ExecuteActions(AutomationData data, CancellationToken cancellationToken)
        {
            foreach (var action in this.Actions)
            {
                var actionData = data.Actions.FirstOrDefault(x => x.Key.Equals(action.Alias)).Value
                    ?? action.CreateActionData();
                data.AddActionData(actionData);

                if (!action.Asynchronous)
                {
                    await this.runner.HandleAction(data, action, actionData, cancellationToken);
                    if (actionData.Error != null)
                    {
                        data.SetError(actionData.Error);
                        break;
                    }
                }
                else
                {
                    actionData.ToggleStatusValuesForAsyncActions();
                    await this.runner.HandleAsyncAction(data, action, this.Alias, actionData);
                }
            }
        }
    }
}
