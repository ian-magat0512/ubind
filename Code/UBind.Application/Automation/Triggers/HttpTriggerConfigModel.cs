// <copyright file="HttpTriggerConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for http trigger.
    /// </summary>
    public class HttpTriggerConfigModel : IBuilder<Trigger>
    {
        [JsonConstructor]
        public HttpTriggerConfigModel(
            string name,
            string alias,
            string description,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IBuilder<IProvider<Data<string>>> context,
            TriggerRequestEndpointConfigModel endpoint,
            HttpResponseConfigModel httpResponse)
        {
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.RunCondition = runCondition;
            this.Context = context;
            this.Endpoint = endpoint;
            this.HttpResponse = httpResponse;
        }

        /// <summary>
        /// Gets the name of the trigger.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the unique trigger ID or alias.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Alias { get; private set; }

        /// <summary>
        /// Gets the description of the trigger.
        /// </summary>
        [JsonProperty]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the run condition of the trigger.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<bool>>> RunCondition { get; private set; }

        /// <summary>
        /// Gets the optional object containing a list of entities that will be set as the automation context.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<string>>> Context { get; private set; }

        /// <summary>
        /// Gets the definition of the API endpoint that the HTTP trigger will be invoked through.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public TriggerRequestEndpointConfigModel Endpoint { get; private set; }

        /// <summary>
        /// Gets the definition of the HTTP Response that will be generated if the automation successfully completes.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public HttpResponseConfigModel HttpResponse { get; private set; }

        /// <inheritdoc/>
        public Trigger Build(IServiceProvider dependencyProvider)
        {
            var httpResponse = this.HttpResponse?.Build(dependencyProvider);
            return new HttpTrigger(
                this.Name,
                this.Alias,
                this.Description,
                this.RunCondition?.Build(dependencyProvider),
                this.Endpoint.Build(dependencyProvider),
                this.Context?.Build(dependencyProvider),
                httpResponse);
        }
    }
}
