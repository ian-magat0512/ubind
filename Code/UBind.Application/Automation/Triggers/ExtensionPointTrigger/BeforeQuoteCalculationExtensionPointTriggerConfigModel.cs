// <copyright file="BeforeQuoteCalculationExtensionPointTriggerConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Model for the before quote calculation extension point trigger.
    /// </summary>
    public class BeforeQuoteCalculationExtensionPointTriggerConfigModel : ExtensionPointTriggerConfigModel
    {
        [JsonConstructor]
        public BeforeQuoteCalculationExtensionPointTriggerConfigModel(
            string name,
            string alias,
            string description,
            ExtensionPointType extensionPoint,
            IBuilder<IObjectProvider> returnInputData,
            IBuilder<IProvider<Data<bool>>> runCondition)
            : base(name, alias, description, extensionPoint, runCondition)
        {
            this.ReturnInputData = returnInputData;
        }

        public IBuilder<IObjectProvider> ReturnInputData { get; set; }

        public sealed override Trigger Build(IServiceProvider dependencyProvider)
        {
            return new BeforeQuoteCalculationExtensionPointTrigger(
                this.Name,
                this.Alias,
                this.Description,
                this.ExtensionPoint,
                this.ReturnInputData.Build(dependencyProvider),
                this.RunCondition?.Build(dependencyProvider));
        }
    }
}
