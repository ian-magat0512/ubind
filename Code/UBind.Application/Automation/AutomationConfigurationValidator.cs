// <copyright file="AutomationConfigurationValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Triggers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    public class AutomationConfigurationValidator : IAutomationConfigurationValidator
    {
        private readonly IServiceProvider dependencyProvider;

        public AutomationConfigurationValidator(IServiceProvider dependencyProvider)
        {
            this.dependencyProvider = dependencyProvider;
        }

        public void Validate(AutomationsConfigurationModel configModel)
        {
            this.ThrowIfDuplicateAutomationAlias(configModel);
            this.ThrowIfDuplicateTriggerAlias(configModel);
            this.ThrowIfDuplicateActionAlias(configModel);
            this.ThrowIfHttpTriggersWithTheSameHttpVerbHaveTheSamePathValues(configModel);
        }

        private void ThrowIfDuplicateAutomationAlias(AutomationsConfigurationModel configModel)
        {
            var automationAliases = configModel.AutomationModels.Select(m => m.Alias);
            if (automationAliases.HasDuplicate(out string duplicate))
            {
                throw new ErrorException(Errors.Automation.HasDuplicateAutomationAlias(duplicate));
            }
        }

        private void ThrowIfDuplicateTriggerAlias(AutomationsConfigurationModel configurationModel)
        {
            foreach (var automationModel in configurationModel.AutomationModels)
            {
                var triggerAliasList = automationModel.Triggers.Select(t => t.Build(this.dependencyProvider).Alias);
                if (triggerAliasList.HasDuplicate(out string firstDuplicate))
                {
                    throw new ErrorException(
                        Errors.Automation.HasDuplicateTrggerAlias(automationModel.Alias, firstDuplicate));
                }
            }
        }

        private void ThrowIfHttpTriggersWithTheSameHttpVerbHaveTheSamePathValues(AutomationsConfigurationModel configurationModel)
        {
            foreach (var automationModel in configurationModel.AutomationModels)
            {
                var triggers = automationModel.Triggers.Select(t => t.Build(this.dependencyProvider)).ToList();
                var uniquePathAndHttpVerb = new HashSet<KeyValuePair<string, string>>();

                foreach (var trigger in triggers)
                {
                    if (trigger is HttpTrigger httpTrigger)
                    {
                        Regex regex = new Regex(@"\{(.*?)\}");
                        var addResult = uniquePathAndHttpVerb
                            .Add(new KeyValuePair<string, string>(httpTrigger.Endpoint.HttpVerb, regex.Replace(httpTrigger.Endpoint.Path, "{segment}")));
                        if (!addResult)
                        {
                            var additionalDetails = new List<string>();
                            additionalDetails.Add($"Automation Name: {automationModel.Name}");
                            additionalDetails.Add($"Automation Alias: {automationModel.Alias}");
                            additionalDetails.Add($"Automation Description: {automationModel.Description}");
                            throw new ErrorException(
                                Errors.Automation.HttpTriggersWithTheSameHttpVerbMustHaveUniquePathValues(new Newtonsoft.Json.Linq.JObject(), additionalDetails));
                        }
                    }
                }
            }
        }

        private void ThrowIfDuplicateActionAlias(AutomationsConfigurationModel configurationModel)
        {
            foreach (var automationModel in configurationModel.AutomationModels)
            {
                this.ThrowIfDuplicateActionAlias(automationModel.Actions, automationModel.Alias);
            }
        }

        private void ThrowIfDuplicateActionAlias(IEnumerable<IBuilder<Actions.Action>> actionConfigs, string automationAlias)
        {
            List<string> actionAliases = new List<string>();
            var actions = actionConfigs.Select(t => t.Build(this.dependencyProvider));
            actionAliases.AddRange(actions.SelectMany(a => this.GetActionAliasAndDescendentActionAliases(a)));
            if (actionAliases.HasDuplicate(out string firstDuplicate))
            {
                throw new ErrorException(
                    Errors.Automation.HasDuplicateActionAlias(automationAlias, firstDuplicate));
            }
        }

        private IEnumerable<string> GetActionAliasAndDescendentActionAliases(IRunnableAction action)
        {
            List<string> actionAliases = new List<string>();
            actionAliases.Add(action.Alias);
            if (action is IRunnableParentAction parentAction)
            {
                actionAliases.AddRange(parentAction.ChildActions
                    .SelectMany(a => this.GetActionAliasAndDescendentActionAliases(a)));
            }

            actionAliases.AddRange(
                action.OnErrorActions.SelectMany(a => this.GetActionAliasAndDescendentActionAliases(a)));
            return actionAliases;
        }
    }
}
