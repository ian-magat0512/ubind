// <copyright file="ExtensionPointTriggerConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for extension point trigger base class.
    /// </summary>
    public abstract class ExtensionPointTriggerConfigModel : IBuilder<Trigger>
    {
        [JsonConstructor]
        public ExtensionPointTriggerConfigModel(
            string name,
            string alias,
            string description,
            ExtensionPointType extensionPoint,
            IBuilder<IProvider<Data<bool>>>? runCondition)
        {
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.RunCondition = runCondition;
            this.ExtensionPoint = extensionPoint;
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
        /// Gets the type of the event that invokes this trigger.
        /// </summary>
        [JsonProperty]
        public ExtensionPointType ExtensionPoint { get; private set; }

        /// <summary>
        /// Gets the running condition.
        /// </summary>
        [JsonProperty]
        public IBuilder<IProvider<Data<bool>>>? RunCondition { get; private set; }

        public virtual Trigger Build(IServiceProvider dependencyProvider)
        {
            throw new NotImplementedException();
        }
    }
}
