// <copyright file="RaiseErrorActionConfigModel.cs" company="uBind">
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
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class is needed to create an instance of <see cref="RaiseErrorAction"/>.
    /// </summary>
    public class RaiseErrorActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public RaiseErrorActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            ErrorProviderConfigModel error)
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
            name.ThrowIfArgumentNullOrEmpty(nameof(name));
            error.ThrowIfArgumentNull(nameof(error));
            this.Error = error;
        }

        /// <summary>
        /// Gets the error that will be raised by the action.
        /// </summary>
        public ErrorProviderConfigModel Error { get; private set; }

        /// <inheritdoc/>
        public override Action Build(IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions.Select(br => br.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions.Select(ar => ar.Build(dependencyProvider));
            var errorActions = this.OnErrorActions.Select(oa => oa.Build(dependencyProvider));
            return new RaiseErrorAction(
               this.Name,
               this.Alias,
               this.Description,
               this.RunCondition?.Build(dependencyProvider),
               beforeRunConditions,
               afterRunConditions,
               errorActions,
               this.Error.Build(dependencyProvider),
               dependencyProvider.GetRequiredService<IClock>());
        }
    }
}
