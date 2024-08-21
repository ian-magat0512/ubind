// <copyright file="BaseActionConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;

    public abstract class BaseActionConfigurationModel : IBuilder<Action>
    {
        [JsonConstructor]
        public BaseActionConfigurationModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>>? runCondition,
            IEnumerable<ErrorConditionConfigModel>? beforeRunConditions,
            IEnumerable<ErrorConditionConfigModel>? afterRunConditions,
            IEnumerable<IBuilder<Action>>? onErrorActions)
        {
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.Asynchronous = asynchronous;
            this.RunCondition = runCondition;
            this.BeforeRunErrorConditions = beforeRunConditions ?? Enumerable.Empty<ErrorConditionConfigModel>();
            this.AfterRunErrorConditions = afterRunConditions ?? Enumerable.Empty<ErrorConditionConfigModel>();
            this.OnErrorActions = onErrorActions ?? Enumerable.Empty<IBuilder<Action>>();
        }

        /// <summary>
        /// Gets or sets the name of the action.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the unique alias as identifier for the action.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Alias { get; protected set; }

        [JsonProperty]
        public string Description { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the action is executed asychronously or not .
        /// </summary>
        [JsonProperty]
        public bool Asynchronous { get; protected set; }

        /// <summary>
        /// Gets or sets the condition to be evaluated prior to execution, if available. If it evaluates to true, the action is executed.
        /// Otherwise, the action is skipped.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<bool>>>? RunCondition { get; protected set; }

        /// <summary>
        /// Gets or sets an optional list of error conditions that are evaluated in order. If any evaluate to true, an error is raised as defined, and this action will not be run.
        /// </summary>
        [JsonProperty]
        public IEnumerable<ErrorConditionConfigModel>? BeforeRunErrorConditions { get; protected set; } = Enumerable.Empty<ErrorConditionConfigModel>();

        /// <summary>
        /// Gets or sets an optional list of error conditions that are evaluated in order. If any evaluate to true, an error is raised as defined.
        /// </summary>
        [JsonProperty]
        public IEnumerable<ErrorConditionConfigModel>? AfterRunErrorConditions { get; protected set; } = Enumerable.Empty<ErrorConditionConfigModel>();

        /// <summary>
        /// Gets or sets a collection representing the list of actions to be run only if this action fails to complete successfully.
        /// </summary>
        [JsonProperty]
        public IEnumerable<IBuilder<Action>>? OnErrorActions { get; protected set; } = Enumerable.Empty<IBuilder<Action>>();

        /// <summary>
        /// Builds the Action object from the configuration model.
        /// </summary>
        public abstract Action Build(System.IServiceProvider dependencyProvider);
    }
}
