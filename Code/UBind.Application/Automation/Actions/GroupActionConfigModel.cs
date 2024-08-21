// <copyright file="GroupActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Processing;

    /// <summary>
    /// Model for building an instance of <see cref="GroupActionConfigModel"/>.
    /// </summary>
    public class GroupActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public GroupActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            IEnumerable<IBuilder<Action>> actions,
            bool parallel)
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
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.Asynchronous = asynchronous;
            this.RunCondition = runCondition;
            this.BeforeRunErrorConditions = beforeRunErrorConditions ?? Enumerable.Empty<ErrorConditionConfigModel>();
            this.AfterRunErrorConditions = afterRunErrorConditions ?? Enumerable.Empty<ErrorConditionConfigModel>();
            this.OnErrorActions = onErrorActions ?? Enumerable.Empty<IBuilder<Action>>();
            this.Actions = actions ?? Enumerable.Empty<IBuilder<Action>>();
            this.Parallel = parallel;
        }

        /// <summary>
        /// Gets the list of actions that belong to this group.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public IEnumerable<IBuilder<Action>> Actions { get; private set; } = Enumerable.Empty<IBuilder<Action>>();

        /// <summary>
        /// Gets a value indicating whether the actions in this group will run in parallel. Default is false.
        /// </summary>
        public bool Parallel { get; private set; }

        /// <inheritdoc/>
        public override Action Build(System.IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions.Select(brc => brc.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions.Select(arc => arc.Build(dependencyProvider));
            var onErrorActions = this.OnErrorActions.Select(oea => oea.Build(dependencyProvider));
            var actions = this.Actions?.Select(act => act.Build(dependencyProvider));

            return new GroupAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                beforeRunConditions,
                afterRunConditions,
                onErrorActions,
                actions,
                this.Parallel,
                dependencyProvider.GetService<IActionRunner>(),
                dependencyProvider.GetService<IJobClient>(),
                dependencyProvider.GetRequiredService<IClock>());
        }
    }
}
