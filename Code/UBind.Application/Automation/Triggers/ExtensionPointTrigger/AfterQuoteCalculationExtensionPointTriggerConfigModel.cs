// <copyright file="AfterQuoteCalculationExtensionPointTriggerConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;

    /// <summary>
    /// Model for the after quote calculation extension point trigger.
    /// </summary>
    public class AfterQuoteCalculationExtensionPointTriggerConfigModel : ExtensionPointTriggerConfigModel
    {
        [JsonConstructor]
        public AfterQuoteCalculationExtensionPointTriggerConfigModel(
            string name,
            string alias,
            string description,
            ExtensionPointType extensionPoint,
            IBuilder<IObjectProvider> returnInputData,
            IBuilder<IObjectProvider> returnCalculationResult,
            IBuilder<IProvider<Data<bool>>> runCondition)
            : base(name, alias, description, extensionPoint, runCondition)
        {
            this.ReturnInputData = returnInputData;
            this.ReturnCalculationResult = returnCalculationResult;
        }

        public IBuilder<IObjectProvider>? ReturnInputData { get; set; }

        public IBuilder<IObjectProvider>? ReturnCalculationResult { get; set; }

        public sealed override Trigger Build(IServiceProvider dependencyProvider)
        {
            return new AfterQuoteCalculationExtensionPointTrigger(
                this.Name,
                this.Alias,
                this.Description,
                this.ExtensionPoint,
                this.ReturnInputData?.Build(dependencyProvider),
                this.ReturnCalculationResult?.Build(dependencyProvider),
                this.RunCondition?.Build(dependencyProvider));
        }
    }
}
