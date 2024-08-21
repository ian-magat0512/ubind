// <copyright file="SetVariableActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;

    public class SetVariableActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public SetVariableActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            IBuilder<IProvider<Data<string>>> variableName,
            IBuilder<IProvider<Data<string>>> propertyName,
            IBuilder<IProvider<IData>> value,
            IBuilder<IProvider<Data<string>>> path,
            IBuilder<IObjectProvider> properties)
             : base(
              name,
              alias,
              description,
              asynchronous,
              runCondition,
              beforeRunErrorConditions,
              afterRunErrorConditions,
              onErrorActions)
        {
            this.VariableNameProvider = variableName;
            this.PropertyNameProvider = propertyName;
            this.ValueProvider = value;
            this.Properties = properties;
            this.Path = path;
        }

        /// <summary>
        /// Gets or sets variable name provider.
        /// Note: This is only used for backward compatibility, should be removed after deploying of UB-8060.
        /// </summary>
        [JsonProperty("variableName")]
        public IBuilder<IProvider<Data<string>>> VariableNameProvider { get; set; }

        /// <summary>
        /// Gets or sets property name provider.
        /// Note: mark this as json required after removing 'variableName' JsonProeprty.
        /// </summary>
        [JsonProperty("propertyName")]
        public IBuilder<IProvider<Data<string>>> PropertyNameProvider { get; set; }

        [JsonProperty("value")]
        public IBuilder<IProvider<IData>> ValueProvider { get; private set; }

        /// <summary>
        /// Gets or sets the json pointer of the variable in the automation data.
        /// </summary>
        [JsonProperty("path")]
        public IBuilder<IProvider<Data<string>>> Path { get; set; }

        /// <summary>
        /// Gets or sets the properties object.
        /// </summary>
        [JsonProperty("properties")]
        public IBuilder<IObjectProvider> Properties { get; set; }

        public override Action Build(IServiceProvider dependencyProvider)
        {
            var propertyName = this.VariableNameProvider != null
                ? this.VariableNameProvider?.Build(dependencyProvider)
                : this.PropertyNameProvider?.Build(dependencyProvider);
            var path = this.Path?.Build(dependencyProvider);
            var properties = this.Properties?.Build(dependencyProvider);

            return new SetVariableAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                this.BeforeRunErrorConditions?.Select(bc => bc.Build(dependencyProvider)),
                this.AfterRunErrorConditions?.Select(ac => ac.Build(dependencyProvider)),
                this.OnErrorActions?.Select(ea => ea.Build(dependencyProvider)),
                propertyName,
                this.ValueProvider?.Build(dependencyProvider),
                dependencyProvider.GetRequiredService<IClock>(),
                path,
                properties,
                dependencyProvider);
        }
    }
}
