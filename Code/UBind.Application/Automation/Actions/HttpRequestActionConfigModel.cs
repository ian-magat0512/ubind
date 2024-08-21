// <copyright file="HttpRequestActionConfigModel.cs" company="uBind">
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
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Model for http request action.
    /// </summary>
    public class HttpRequestActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestActionConfigModel"/> class.
        /// </summary>
        /// <param name="name">The action name.</param>
        /// <param name="alias">The action alias.</param>
        /// <param name="description">The action description.</param>
        /// <param name="asynchronous">The action if asynchronous.</param>
        /// <param name="runCondition">An optional condition.</param>
        /// <param name="beforeRunErrorConditions">The validation rules before the action.</param>
        /// <param name="afterRunErrorConditions">The validation rules after the action.</param>
        /// <param name="onErrorActions">The list of non successful actions.</param>
        /// <param name="httpRequest">The http request.</param>
        [JsonConstructor]
        public HttpRequestActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            HttpRequestConfigurationConfigModel httpRequest)
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
            httpRequest.ThrowIfArgumentNull(nameof(httpRequest));
            this.HttpRequest = httpRequest;
        }

        /// <summary>
        /// Gets the httpRequest that the action will trigger.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public HttpRequestConfigurationConfigModel HttpRequest { get; private set; }

        /// <inheritdoc/>
        public override Action Build(IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions.Select(br => br.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions.Select(ar => ar.Build(dependencyProvider));
            var errorActions = this.OnErrorActions.Select(oa => oa.Build(dependencyProvider));
            return new HttpRequestAction(
               this.Name,
               this.Alias,
               this.Description,
               this.Asynchronous,
               this.RunCondition?.Build(dependencyProvider),
               beforeRunConditions,
               afterRunConditions,
               errorActions,
               this.HttpRequest.Build(dependencyProvider),
               dependencyProvider.GetRequiredService<IClock>());
        }
    }
}
