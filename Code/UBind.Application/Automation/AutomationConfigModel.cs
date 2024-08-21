// <copyright file="AutomationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Triggers;

    /// <summary>
    /// Builder model for building automations from deserialized json.
    /// Models #automation within the automations schema.
    /// </summary>
    public class AutomationConfigModel : IBuilder<Automation>
    {
        /// <summary>
        /// Gets or sets the name for the automation.
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the  ID for the automation.
        /// </summary>
        [JsonProperty]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the description for the automation.
        /// </summary>
        [JsonProperty]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the run condition for the automation.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<bool>>> RunCondition { get; set; }

        /// <summary>
        /// Gets or sets the list of triggers for the automation.
        /// </summary>
        [JsonProperty]
        public IEnumerable<IBuilder<Trigger>> Triggers { get; set; } = Enumerable.Empty<IBuilder<Trigger>>();

        /// <summary>
        /// Gets or sets the list of actions for the automation to run.
        /// </summary>
        [JsonProperty]
        public IEnumerable<IBuilder<Actions.Action>> Actions { get; set; } = Enumerable.Empty<IBuilder<Actions.Action>>();

        /// <summary>
        /// Builds an instance of <see cref="Automation"/> to be used for running the automation.
        /// </summary>
        /// <param name="dependencyProvider">Container providing dependencies required in automation building.</param>
        /// <returns>A new automation handler.</returns>
        public Automation Build(IServiceProvider dependencyProvider)
        {
            var actions = this.Actions.Select(a => a.Build(dependencyProvider)).ToList();
            var triggers = this.Triggers.Select(t => t.Build(dependencyProvider)).ToList();
            var runner = dependencyProvider.GetService<IActionRunner>();
            return new Automation(this.Name, this.Alias, this.Description, this.RunCondition?.Build(dependencyProvider), triggers, actions, runner);
        }
    }
}
